using System.Collections.Generic;
using UnityEngine;

public class TilemapChunkSplitter : MonoBehaviour
{
    [Header("Tile Parent")]
    public Transform tileParent;

    [Header("Chunk System Reference")]
    public ChunkManager chunkManager;

    // temp storage
    private Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        BuildChunks();
    }

    void BuildChunks()
    {
        foreach (Transform tile in tileParent)
        {
            Vector3 pos = tile.position;

            Vector2Int chunkCoord = GetChunkCoord(pos);

            GameObject chunk = GetOrCreateChunk(chunkCoord);

            tile.SetParent(chunk.transform);
        }

        chunkManager.RegisterChunks();
    }

    GameObject GetOrCreateChunk(Vector2Int coord)
    {
        if (chunks.ContainsKey(coord))
            return chunks[coord];

        GameObject chunk = new GameObject($"Chunk_{coord.x}_{coord.y}");

        chunk.transform.position = new Vector3(coord.x * chunkManager.chunkSize, coord.y * chunkManager.chunkSize, 0);

        chunk.transform.parent = chunkManager.chunkParent;

        Chunk c = chunk.AddComponent<Chunk>();
        c.chunkCoord = coord;

        chunks.Add(coord, chunk);

        chunkManager.chunkPrefabs.Add(chunk);

        return chunk;
    }

    Vector2Int GetChunkCoord(Vector3 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x / chunkManager.chunkSize), Mathf.FloorToInt(pos.y / chunkManager.chunkSize));
    }
}