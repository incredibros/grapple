using System.Collections.Generic;
using UnityEngine;

public class TilemapChunkSplitter : MonoBehaviour
{
    [Header("Tile Parent")]
    public Transform tileParent;

    [Header("Chunk System Reference")]
    public ChunkManager chunkManager;

    private Dictionary<Vector2Int, List<GameObject>> chunks = new Dictionary<Vector2Int, List<GameObject>>();

    void Start()
    {
        BuildChunks();
    }

    void BuildChunks()
    {
        chunks.Clear();
        chunkManager.chunkPrefabs.Clear();

        List<GameObject> tiles = new List<GameObject>();

        // Store all tiles first
        foreach (GameObject tile in tileParent)
        {
            tiles.Add(tile);

            tile.gameObject.SetActive(false);
        }

        // Create chunks
        foreach (GameObject tile in tiles)
        {
            Vector3 pos = tile.position;

            Vector2Int chunkCoord = GetChunkCoord(pos);

            AddOrCreateChunk(tile, chunkCoord);
        }

        chunkManager.RegisterChunks();
    }

    GameObject AddOrCreateChunk(GameObject tile, Vector2Int coord)
    {
        if (chunks.Contains(coord))
        {
            findChunk[coord].Add(tile);
            return;
        }

        //GameObject chunk = new GameObject($"Chunk_{coord.x}_{coord.y}");

        //Chunk c = chunk.AddComponent<Chunk>();
        //c.chunkCoord = new Vector3(coord.x * chunkManager.chunkSize, coord.y * chunkManager.chunkSize, 0);;

        chunks[coord].Add(tile);

        return chunk;
    }

    Vector2Int GetChunkCoord(Vector3 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x / chunkManager.chunkSize), Mathf.FloorToInt(pos.y / chunkManager.chunkSize));
    }
}