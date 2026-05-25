using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ChunkEditorScript : MonoBehaviour
{
    [SerializeField] ChunkManager chunkManager;
    [SerializeField] List<GameObject> grids;

    // Chunks(Vector2, Grids(Name, tileMaps(Name, List(tiles))))
    public List<ChunkData> chunks = new List<ChunkData>();

    #region Revert Chunks
    [ContextMenu("Revert Chunks")]
    void RevertChunks()
    {
        List<Transform> groundTiles = new List<Transform>();
        List<Transform> semiSolidTiles = new List<Transform>();
        List<Transform> extraTiles = new List<Transform>();

        List<Transform> halfGroundTiles = new List<Transform>();
        List<Transform> halfSemiSolidTiles = new List<Transform>();

        foreach (Transform chunk in transform)
        {
            foreach (Transform grid in chunk)
            {
                foreach (Transform tilemap in grid)
                {
                    foreach (Transform tile in tilemap)
                    {
                        if (tile.name == "Ground")
                        {
                            groundTiles.Add(tile);
                        }
                        else if (tile.name == "SemiSolid")
                        {
                            semiSolidTiles.Add(tile);
                        }
                        else if (tile.name == "HalfGround")
                        {
                            halfGroundTiles.Add(tile);
                        }
                        else if (tile.name == "HalfSemiSolid")
                        {
                            halfSemiSolidTiles.Add(tile);
                        }
                        else
                        {
                            extraTiles.Add(tile);
                        }
                    }
                }
            }
        }
        Transform platformsTilemap = grids[0].transform.Find("PlatformTilemap");
        Transform semiSolidTilemap = grids[0].transform.Find("SemiSolidTilemap");
        Transform extraTilemap = grids[0].transform.Find("ExtraTilemap");

        Transform halfPlatformsTilemap = grids[1].transform.Find("HalfPlatformTilemap");
        Transform halfSemiSolidTilemap = grids[1].transform.Find("HalfSemiSolidTilemap");

        foreach (var tile in groundTiles)
            Undo.SetTransformParent(tile, platformsTilemap, "Reparent Tile");
        foreach (var tile in semiSolidTiles)
            Undo.SetTransformParent(tile, semiSolidTilemap, "Reparent Tile");
        foreach (var tile in extraTiles)
            Undo.SetTransformParent(tile, extraTilemap, "Reparent Tile");
        foreach (var tile in halfGroundTiles)
            Undo.SetTransformParent(tile, halfPlatformsTilemap, "Reparent Tile");
        foreach (var tile in halfSemiSolidTiles)
            Undo.SetTransformParent(tile, halfSemiSolidTilemap, "Reparent Tile");
        
        chunks.Clear();
        RemoveEmptyChunks();
        
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
    #endregion
    
    #region Execute Chunking
    [ContextMenu("Execute Chunks")]
    void ExecuteChunking()
    {
        UpdateChunksList();
        RemoveEmptyChunks();
        InstantiateChunks();
        
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
    #endregion

    #region Update Chunks List
    void UpdateChunksList()
    {
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

    void AddTile(Vector2Int chunkPos, GameObject gridObj, GameObject tilemapObj, GameObject tileObj)
    {
        ChunkData chunk = chunks.SingleOrDefault(c => c.chunkCoord == chunkPos);
        if (chunk == null)
        {
            chunk = new ChunkData(chunkPos);
            chunks.Add(chunk);
        }

        GridData grid = chunk.grids.SingleOrDefault(g => g.gridObject == gridObj);
        if (grid == null)
        {
            grid = new GridData(gridObj);
            chunk.grids.Add(grid);
        }

        TilemapData tilemap = grid.tilemaps.SingleOrDefault(t => t.tilemapObject == tilemapObj);
        if (tilemap == null)
        {
            tilemap = new TilemapData(tilemapObj);
            grid.tilemaps.Add(tilemap);
        }

        if (!tilemap.tiles.Any(t => t.tileObject == tileObj))
        {
            tilemap.tiles.Add(new TileData(tileObj));
        }
    }

    Vector2Int GetChunkCoord(Vector3 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x / chunkManager.chunkSize), Mathf.FloorToInt(pos.y / chunkManager.chunkSize));
    }
    #endregion

    #region Instantiate Chunks
    void InstantiateChunks()
    {
        foreach (var chunk in chunks)
        {
            Vector2Int chunkPos = chunk.chunkCoord;
            string chunkName = $"Chunk_{chunkPos.x}_{chunkPos.y}";
            Transform existingChunk = chunkManager.transform.Find(chunkName);
            GameObject chunkObject;

            if (existingChunk != null)
            {
                chunkObject = existingChunk.gameObject;
            }
            else
            {
                chunkObject = new GameObject(chunkName);
                chunkObject.transform.SetParent(chunkManager.transform);
                Undo.RegisterCreatedObjectUndo(chunkObject, "Create Chunk");

                Chunk c = chunkObject.AddComponent<Chunk>();
                c.chunkCoord = chunkPos;
            }

            foreach (var grid in chunk.grids)
            {
                Transform existingGrid = chunkObject.transform.Find(grid.gridObject.name);
                GameObject gridObject;

                if (existingGrid != null)
                {
                    gridObject = existingGrid.gameObject;
                }
                else
                {
                    gridObject = new GameObject($"{grid.gridObject.name}_{chunkPos.x}_{chunkPos.y}");
                    gridObject.transform.SetParent(chunkObject.transform);
                    Undo.RegisterCreatedObjectUndo(gridObject, "Create Chunk Grid");

                    if (grid.gridObject.TryGetComponent<Grid>(out Grid gridComponent))
                    {
                        Grid g = gridObject.AddComponent<Grid>();
                        g.cellSize = gridComponent.cellSize;
                    }
                }

                foreach (var tileMap in grid.tilemaps)
                {
                    Transform existingTileMap = gridObject.transform.Find(tileMap.tilemapObject.name);
                    GameObject tileMapObject;

                    if (existingTileMap != null)
                    {
                        tileMapObject = existingTileMap.gameObject;
                    }
                    else
                    {
                        tileMapObject = new GameObject($"{tileMap.tilemapObject.name}_{chunkPos.x}_{chunkPos.y}");
                        tileMapObject.transform.SetParent(gridObject.transform);
                        Undo.RegisterCreatedObjectUndo(tileMapObject, "Create Chunk Tilemap");

                        tileMapObject.layer = tileMap.tilemapObject.layer;

                        if (tileMap.tilemapObject.TryGetComponent<Tilemap>(out Tilemap _))
                        {
                            tileMapObject.AddComponent<Tilemap>();
                        }

                        if (tileMap.tilemapObject.TryGetComponent<Rigidbody2D>(out Rigidbody2D originalRb))
                        {
                            Rigidbody2D rb = tileMapObject.AddComponent<Rigidbody2D>();
                            rb.bodyType = originalRb.bodyType;
                        }

                        if (tileMap.tilemapObject.TryGetComponent<CompositeCollider2D>(out CompositeCollider2D originalCC))
                        {
                            CompositeCollider2D cc = tileMapObject.AddComponent<CompositeCollider2D>();
                            cc.usedByEffector = originalCC.usedByEffector;
                            cc.sharedMaterial = originalCC.sharedMaterial;
                        }

                        if (tileMap.tilemapObject.TryGetComponent<PlatformEffector2D>(out PlatformEffector2D originalPE))
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
                    }

                    foreach (TileData tileData in tileMap.tiles)
                    {
                        if (tileData.tileObject.transform.parent != tileMapObject.transform)
                        {
                            Undo.SetTransformParent(tileData.tileObject.transform, tileMapObject.transform, "Reparent Tile");
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Remove Empty Chunks
    void RemoveEmptyChunks()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var chunk = transform.GetChild(i);

            for (int g = chunk.childCount - 1; g >= 0; g--)
            {
                var grid = chunk.GetChild(g);

                for (int t = grid.childCount - 1; t >= 0; t--)
                {
                    var tilemap = grid.GetChild(t);
                    if (tilemap.childCount == 0)
                    {
                        Undo.DestroyObjectImmediate(tilemap.gameObject);
                    }
                }

                if (grid.childCount == 0)
                {
                    Undo.DestroyObjectImmediate(grid.gameObject);
                }
            }

            if (chunk.childCount == 0)
            {
                Undo.DestroyObjectImmediate(chunk.gameObject);
            }
        }

        for (int i = chunks.Count - 1; i >= 0; i--)
        {
            ChunkData chunk = chunks[i];

            for (int g = chunk.grids.Count - 1; g >= 0; g--)
            {
                GridData grid = chunk.grids[g];

                if (grid.gridObject == null)
                {
                    chunk.grids.RemoveAt(g);
                    continue;
                }

                for (int t = grid.tilemaps.Count - 1; t >= 0; t--)
                {
                    TilemapData tilemap = grid.tilemaps[t];

                    if (tilemap.tilemapObject == null)
                    {
                        grid.tilemaps.RemoveAt(t);
                        continue;
                    }

                    tilemap.tiles.RemoveAll(ti => ti.tileObject == null);

                    if (tilemap.tiles.Count == 0)
                    {
                        grid.tilemaps.RemoveAt(t);
                    }
                }

                if (grid.tilemaps.Count == 0)
                {
                    chunk.grids.RemoveAt(g);
                }
            }

            if (chunk.grids.Count == 0)
            {
                chunks.RemoveAt(i);
            }
        }
    }
    #endregion
}

[Serializable]
public class ChunkData
{
    public Vector2Int chunkCoord;
    public List<GridData> grids = new List<GridData>();

    public ChunkData(Vector2Int c)
    {
        chunkCoord = c;
    }
}

[Serializable]
public class GridData
{
    public GameObject gridObject;
    public List<TilemapData> tilemaps = new List<TilemapData>();

    public GridData(GameObject g)
    {
        gridObject = g;
    }
}

[Serializable]
public class TilemapData
{
    public GameObject tilemapObject;
    public List<TileData> tiles = new List<TileData>();

    public TilemapData(GameObject t)
    {
        tilemapObject = t;
    }

    /*public bool type;
    public List<GameObject> tileObjects = new List<GameObject>();
    public List<TileBase> tilesSprites = new List<TileBase>();*/
}

[Serializable]
public class TileData
{
    public GameObject tileObject;

    public TileData(GameObject t)
    {
        tileObject = t;
    }

    //public TileBase tileBase;
}