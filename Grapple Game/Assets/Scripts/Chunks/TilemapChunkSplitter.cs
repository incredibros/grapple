using System.Collections.Generic;
using UnityEngine;

public class TilemapChunkSplitter : MonoBehaviour
{
    public ChunkManager chunkManager;
    public List<Transform> grids;

    private Dictionary<Vector2Int, List<GameObject>> chunks = new Dictionary<Vector2Int, List<GameObject>>();

    void Start()
    {
        BuildChunks();
    }

    void BuildChunks()
    {
        if (grids == null)
            { return; }

        chunks.Clear();
        chunkManager.chunkPrefabs.Clear();

        foreach (Transform grid in grids)
        {
            foreach (Transform tileMap in grid)
            {
                foreach (Transform tile in tileMap)
                {
                    AddOrCreateChunk(GetChunkCoord(tile.position), tile.gameObject);
                }
            }
        }
        
        

        foreach (KeyValuePair<Vector2Int, List<GameObject>> chunk in chunks)
        {
            Vector2Int chunkPos = chunk.Key;
            List<GameObject> tiles = chunk.Value;

            GameObject chunkObject = new GameObject($"Chunk_{chunkPos.x}_{chunkPos.y}");
            chunkObject.transform.SetParent(chunkManager.transform);
            chunkObject.SetActive(false);

            Chunk c = chunkObject.AddComponent<Chunk>();
            c.chunkCoord = new Vector2Int(chunkPos.x, chunkPos.y);

            foreach (GameObject tile in tiles)
            {
                tile.transform.SetParent(chunkObject.transform);
            }

            chunkManager.chunkPrefabs.Add(chunkObject);
        }

        chunkManager.RegisterChunks();
    }

    void AddOrCreateChunk(Vector2Int coord, GameObject tile)
    {
        if (!chunks.ContainsKey(coord))
        {
            chunks[coord] = new List<GameObject>();
        }

        chunks[coord].Add(tile);
    }

    Vector2Int GetChunkCoord(Vector3 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x / chunkManager.chunkSize), Mathf.FloorToInt(pos.y / chunkManager.chunkSize));
    }
}