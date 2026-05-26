using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapChunkManager : MonoBehaviour
{
    GameObject player;
    TilemapChunker tilemapChunker;

    [SerializeField] List<Tilemap> tilemaps;

    [Header("Settings")]
    public int chunkSize = 16;
    public Vector2Int renderDistance = new Vector2Int(2, 1);

    [SerializeField] Vector2Int lastPlayerChunk = new Vector2Int(int.MaxValue, int.MaxValue);

    void Awake()
    {
        player = GameObject.FindWithTag("Player");
        tilemapChunker = GetComponent<TilemapChunker>();
    }

    void Start()
    {
        tilemapChunker.InitializeChunkCache(tilemaps);
        
        Vector2Int currentPlayerChunk = tilemapChunker.GetChunkCoord(player.transform.position);
        RenderVisibleChunks(currentPlayerChunk);
    }

    void Update()
    {
        Vector2Int currentPlayerChunk = tilemapChunker.GetChunkCoord(player.transform.position);

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
                
                List<TileData> tilesInChunk = tilemapChunker.GetTilesInChunk(targetChunkCoord);
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