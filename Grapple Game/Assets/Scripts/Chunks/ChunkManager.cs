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

    [HideInInspector] public List<GameObject> chunkPrefabs;

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
        RegisterChunks();
    }

    void Update()
    {
        UpdateChunks();
    }

    void RegisterChunks()
    {
        if (chunkPrefabs == null)
            { return; }

        foreach (GameObject prefab in chunkPrefabs)
        {
            Chunk chunkData = prefab.GetComponent<Chunk>();

            if (!chunkLookup.ContainsKey(chunkData.chunkCoord))
            {
                chunkLookup.Add(chunkData.chunkCoord, prefab);
            }
        }
    }

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
                //Destroy(chunk.Value);
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

        GameObject prefab = chunkLookup[coord];

        GameObject chunk = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
        
        chunk.SetActive(true);

        chunk.name = "Chunk_" + coord.x + "_" + coord.y;

        loadedChunks.Add(coord, chunk);
    }
    #endregion
}