using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    GameObject player;

    ChunkEditorScript chunkEditorScript;

    [Header("Settings")]
    public int chunkSize = 16;
    public int renderDistance = 2;

    // Loaded chunks in scene
    Dictionary<Vector2Int, GameObject> loadedChunks = new Dictionary<Vector2Int, GameObject>();
    
    // All available chunk prefabs
    Dictionary<Vector2Int, GameObject> chunkLookup = new Dictionary<Vector2Int, GameObject>();

    void Awake()
    {
        player = GameObject.FindWithTag("Player");
        chunkEditorScript = GetComponent<ChunkEditorScript>();
    }

    void Start()
    {
        DisableChildren();
        RegisterExistingChunks();
    }

    void Update()
    {
        UpdateChunks();
    }

    #region Register Existing Chunks
    void RegisterExistingChunks()
    {
        foreach (Transform child in transform)
        {
            Chunk chunk = child.GetComponent<Chunk>();

            if (chunk != null)
            {
                chunkLookup[chunk.chunkCoord] = child.gameObject;
            }
        }
    }
    #endregion

    #region Disable Children
    void DisableChildren()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
    #endregion


    #region Update Chunks
    void UpdateChunks()
    {
        Vector2Int playerChunk = GetChunkCoord(player.transform.position);

        HashSet<Vector2Int> neededChunks = new HashSet<Vector2Int>();

        // Load Neaby Chunks
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int y = -renderDistance; y <= renderDistance; y++)
            {
                Vector2Int coord = new Vector2Int(playerChunk.x + x, playerChunk.y + y);

                neededChunks.Add(coord);

                if (!loadedChunks.ContainsKey(coord))
                {
                    LoadChunk(coord);
                }
            }
        }

        // Unload far chunks
        List<Vector2Int> chunksToUnload = new List<Vector2Int>();

        foreach (var chunk in loadedChunks)
        {
            if (!neededChunks.Contains(chunk.Key))
            {
                chunk.Value.SetActive(false);
                chunksToUnload.Add(chunk.Key);
            }
        }

        foreach (Vector2Int coord in chunksToUnload)
        {
            loadedChunks.Remove(coord);
        }
    }

    // Convert world position to chunk coordinate
    Vector2Int GetChunkCoord(Vector3 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x / chunkSize), Mathf.FloorToInt(pos.y / chunkSize));
    }

    // Spawn chunk
    void LoadChunk(Vector2Int coord)
    {
        if (loadedChunks.ContainsKey(coord))
            return;

        if (!chunkLookup.ContainsKey(coord))
            return;

        GameObject chunk = chunkLookup[coord];
        chunk.SetActive(true);

        loadedChunks.Add(coord, chunk);
    }
    #endregion
}