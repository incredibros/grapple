using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Chunk Settings")]
    public int chunkSize = 16;
    public int renderDistance = 2;

    [Header("Chunk Prefabs")]
    public List<GameObject> chunkPrefabs;

    [Header("Chunk Parent")]
    public Transform chunkParent;

    // Loaded chunks in scene
    private Dictionary<Vector2Int, GameObject> loadedChunks =
        new Dictionary<Vector2Int, GameObject>();

    // All available chunk prefabs
    private Dictionary<Vector2Int, GameObject> chunkLookup =
        new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        RegisterChunks();
        UpdateChunks();
    }

    void Update()
    {
        UpdateChunks();
    }

    // Register all chunk prefabs
    void RegisterChunks()
    {
        foreach (GameObject prefab in chunkPrefabs)
        {
            Chunk chunkData = prefab.GetComponent<Chunk>();

            if (chunkData == null)
            {
                Debug.LogError(
                    prefab.name +
                    " is missing Chunk.cs!"
                );

                continue;
            }

            if (!chunkLookup.ContainsKey(chunkData.chunkCoord))
            {
                chunkLookup.Add(
                    chunkData.chunkCoord,
                    prefab
                );
            }
        }
    }

    // Load/unload chunks around player
    void UpdateChunks()
    {
        Vector2Int playerChunk =
            GetChunkCoord(player.position);

        HashSet<Vector2Int> neededChunks =
            new HashSet<Vector2Int>();

        // Load nearby chunks
        for (int x = -renderDistance;
             x <= renderDistance;
             x++)
        {
            for (int y = -renderDistance;
                 y <= renderDistance;
                 y++)
            {
                Vector2Int coord =
                    new Vector2Int(
                        playerChunk.x + x,
                        playerChunk.y + y
                    );

                neededChunks.Add(coord);

                if (!loadedChunks.ContainsKey(coord))
                {
                    LoadChunk(coord);
                }
            }
        }

        // Unload far chunks
        List<Vector2Int> chunksToUnload =
            new List<Vector2Int>();

        foreach (var chunk in loadedChunks)
        {
            if (!neededChunks.Contains(chunk.Key))
            {
                Destroy(chunk.Value);

                chunksToUnload.Add(chunk.Key);
            }
        }

        foreach (Vector2Int coord in chunksToUnload)
        {
            loadedChunks.Remove(coord);
        }
    }

    // Convert world position to chunk coordinate
    Vector2Int GetChunkCoord(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / chunkSize),
            Mathf.FloorToInt(position.y / chunkSize)
        );
    }

    // Spawn chunk
    void LoadChunk(Vector2Int coord)
    {
        // Does this chunk exist?
        if (!chunkLookup.ContainsKey(coord))
            return;

        GameObject prefab =
            chunkLookup[coord];

        Vector3 worldPosition =
            new Vector3(
                coord.x * chunkSize,
                coord.y * chunkSize,
                0
            );

        GameObject chunk =
            Instantiate(
                prefab,
                worldPosition,
                Quaternion.identity,
                chunkParent
            );

        chunk.name =
            "Chunk_" +
            coord.x +
            "_" +
            coord.y;

        loadedChunks.Add(coord, chunk);
    }
}