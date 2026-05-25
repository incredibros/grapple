using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
/*
public class TilemapChunkManager : MonoBehaviour
{
    GameObject player;
    TilemapChunker tilemapChunker;

    [SerializeField] Tilemap tilemap;

    [Header("Settings")]
    public int chunkSize = 16;
    public int renderDistance = 2;

    Vector2Int lastPlayerChunk = new Vector2Int(int.MaxValue, int.MaxValue);

    void Awake()
    {
        player = GameObject.FindWithTag("Player");
        tilemapChunker = GetComponent<TilemapChunker>();
    }

    void Start()
    {
        if (tilemapChunker != null)
        {
            tilemapChunker.InitializeChunkCache(tilemap);
        }
        
        if (tilemap != null)
        {
            tilemap.ClearAllTiles();
        }
    }

    void Update()
    {
        if (player == null || tilemapChunker == null || tilemap == null) return;

        Vector2Int currentPlayerChunk = GetChunkCoord.GetChunkCoord(player.transform.position);

        if (currentPlayerChunk != lastPlayerChunk)
        {
            lastPlayerChunk = currentPlayerChunk;
            RenderVisibleChunks(currentPlayerChunk);
        }
    }

    void RenderVisibleChunks(Vector2Int centerChunk)
    {
        tilemap.ClearAllTiles();

        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int y = -renderDistance; y <= renderDistance; y++)
            {
                Vector2Int targetChunkCoord = new Vector2Int(centerChunk.x + x, centerChunk.y + y);
                
                List<TileData> tilesInChunk = GetChunkCoord.GetTilesInChunk(targetChunkCoord);

                if (tilesInChunk != null)
                {
                    foreach (TileData data in tilesInChunk)
                    {
                        tilemap.SetTile((Vector3Int)data.gridPos, data.tile);
                    }
                }
            }
        }
    }
}
*/