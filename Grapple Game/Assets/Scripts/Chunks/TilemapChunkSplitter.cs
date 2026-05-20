using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapChunkSplitter : MonoBehaviour
{
    public ChunkManager chunkManager;
    List<GameObject> grids = new List<GameObject>();

    // Chunks(Vector2, Grids(Name, tileMaps(Name, List(tiles))))
    private Dictionary<Vector2Int, Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>>> chunks = new Dictionary<Vector2Int, Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>>>();

    void Awake()
    {
        grids.Clear();
        grids.Add(GameObject.Find("Grid"));
        grids.Add(GameObject.Find("HalfGrid"));
    }

    void Start()
    {
        BuildChunks();
        MakeGameObjects();
        chunkManager.RegisterChunks();
    }

    void BuildChunks()
    {
        if (grids == null)
            return;

        chunks.Clear();
        chunkManager.chunkPrefabs.Clear();

        foreach (GameObject grid in grids)
        {
            foreach (Transform tileMap in grid.transform)
            {
                foreach (Transform tile in tileMap.transform)
                {
                    AddTile(GetChunkCoord(tile.position), grid, tileMap.gameObject, tile.gameObject);
                }
            }
        }
    }

    void AddTile(Vector2Int chunkPos, GameObject grid, GameObject tileMap, GameObject tile)
    {
        if (!chunks.ContainsKey(chunkPos))
        {
            chunks[chunkPos] = new Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>>();
        }

        if (!chunks[chunkPos].ContainsKey(grid))
        {
            chunks[chunkPos][grid] = new Dictionary<GameObject, List<GameObject>>();
        }

        if (!chunks[chunkPos][grid].ContainsKey(tileMap))
        {
            chunks[chunkPos][grid][tileMap] = new List<GameObject>();
        }

        chunks[chunkPos][grid][tileMap].Add(tile);
    }

    Vector2Int GetChunkCoord(Vector3 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x / chunkManager.chunkSize), Mathf.FloorToInt(pos.y / chunkManager.chunkSize));
    }

    void MakeGameObjects()
    {
        foreach (var chunk in chunks)
        {
            Vector2Int chunkPos = chunk.Key;
            var grids = chunk.Value;

            GameObject chunkObject = new GameObject($"Chunk_{chunkPos.x}_{chunkPos.y}");
            chunkObject.transform.SetParent(chunkManager.transform);
            chunkObject.SetActive(false);

            Chunk c = chunkObject.AddComponent<Chunk>();
            c.chunkCoord = chunkPos;

            foreach (var grid in grids)
            {
                GameObject gridObject = new GameObject(grid.Key.name);
                gridObject.transform.SetParent(chunkObject.transform);

                if (grid.Key.TryGetComponent<Grid>(out Grid gridComponent))
                {
                    Grid g = gridObject.AddComponent<Grid>();
                    g.cellSize = gridComponent.cellSize;
                }

                foreach (var tileMap in grid.Value)
                {
                    GameObject tileMapObject = new GameObject(tileMap.Key.name);
                    tileMapObject.transform.SetParent(gridObject.transform);

                    tileMapObject.layer = tileMap.Key.layer;

                    if (tileMap.Key.TryGetComponent<Tilemap>(out Tilemap tilemapComponent))
                    {
                        Tilemap t = tileMapObject.AddComponent<Tilemap>();
                    }
                    
                    if (tileMap.Key.TryGetComponent<Rigidbody2D>(out Rigidbody2D originalRb))
                    {
                        Rigidbody2D rb = tileMapObject.AddComponent<Rigidbody2D>();

                        rb.bodyType = originalRb.bodyType;
                    }

                    if (tileMap.Key.TryGetComponent<CompositeCollider2D>(out CompositeCollider2D originalCC))
                    {
                        CompositeCollider2D cc = tileMapObject.AddComponent<CompositeCollider2D>();

                        cc.usedByEffector = originalCC.usedByEffector;

                        cc.sharedMaterial = originalCC.sharedMaterial;
                    }

                    if (tileMap.Key.TryGetComponent<PlatformEffector2D>(out PlatformEffector2D originalPE))
                    {
                        PlatformEffector2D pe = tileMapObject.AddComponent<PlatformEffector2D>();

                        pe.rotationalOffset = originalPE.rotationalOffset;
                        pe.useOneWay = originalPE.useOneWay;
                        pe.useOneWayGrouping = originalPE.useOneWayGrouping;
                        pe.useSideFriction = originalPE.useSideFriction;
                        pe.useSideBounce = originalPE.useSideBounce;

                        pe.surfaceArc = originalPE.surfaceArc;
                        pe.sideArc = originalPE.sideArc;
                    }

                    foreach (GameObject tile in tileMap.Value)
                    {
                        tile.transform.SetParent(tileMapObject.transform);
                    }
                }
            }

            chunkManager.chunkPrefabs.Add(chunkObject);
        }
    }
}