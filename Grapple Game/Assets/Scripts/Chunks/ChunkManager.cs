using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkManager : MonoBehaviour
{
    GameObject player;
    ChunkSplitter chunkSplitter;

    [SerializeField] List<Tilemap> tilemaps;

    [Header("Settings")]
    public int chunkSize = 16;
    public Vector2Int renderDistance = new Vector2Int(2, 1);

    Vector2Int lastPlayerChunk = new Vector2Int(int.MaxValue, int.MaxValue);

    void Awake()
    {
        player = GameObject.FindWithTag("Player");
        chunkSplitter = GetComponent<ChunkSplitter>();
    }

    void Start()
    {
        chunkSplitter.InitializeChunkCache(tilemaps);
        
        Vector2Int currentPlayerChunk = chunkSplitter.GetChunkCoord(player.transform.position);
        RenderVisibleChunks(currentPlayerChunk);
    }

    void Update()
    {
        Vector2Int currentPlayerChunk = chunkSplitter.GetChunkCoord(player.transform.position);

        if (currentPlayerChunk != lastPlayerChunk)
        {
            lastPlayerChunk = currentPlayerChunk;
            RenderVisibleChunks(currentPlayerChunk);
        }
    }

    void RenderVisibleChunks(Vector2Int centerChunk)
    {
        ClearAllTilemaps();

        for (int x = -renderDistance.x; x <= renderDistance.x; x++)
        {
            for (int y = -renderDistance.y; y <= renderDistance.y; y++)
            {
                Vector2Int targetChunkCoord = new Vector2Int(centerChunk.x + x, centerChunk.y + y);
                
                List<TileData> tilesInChunk = chunkSplitter.GetTilesInChunk(targetChunkCoord);
                if (tilesInChunk != null)
                {
                    foreach (TileData data in tilesInChunk)
                    {
                        data.originTilemap.SetTile((Vector3Int)data.gridPos, data.tile);
                    }
                }
                
        }
        }
    }

    void ClearAllTilemaps()
    {
        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap != null)
            {
                tilemap.ClearAllTiles();
            }
        }
    }
}