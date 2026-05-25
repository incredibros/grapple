using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapChunker : MonoBehaviour
{
    /*
    [SerializeField] ChunkManager chunkManager;
    //[SerializeField] Tilemap tilemap;

    Dictionary<Vector2Int, List<TileData>> cachedChunks;

    public void InitializeChunkCache(Tilemap tilemap)
    {
        cachedChunks = new Dictionary<Vector2Int, List<TileData>>();

        tilemap.CompressBounds();
        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int internalPos = new Vector3Int(x, y, 0); 
                TileBase tile = tilemap.GetTile(internalPos);
                
                if (tile == null) continue;

                Vector3 cellCenter = tilemap.GetCellCenterWorld(internalPos);
                Vector3 worldPos = new Vector3(cellCenter.x, cellCenter.y, 0);

                Vector2Int chunkCoord = GetChunkCoord(worldPos);

                TileData data = new TileData(new Vector2Int(x, y), worldPos, tile);

                if (!cachedChunks.ContainsKey(chunkCoord))
                {
                    cachedChunks[chunkCoord] = new List<TileData>();
                }
                cachedChunks[chunkCoord].Add(data);
            }
        }
    }

    public List<TileData> GetTilesInChunk(Vector2Int chunkCoord)
    {
        if (cachedChunks.TryGetValue(chunkCoord, out List<TileData> tiles))
        {
            return tiles;
        }
        return null; 
    }

    public Dictionary<Vector2Int, List<TileData>> GetAllCachedChunks()
    {
        return cachedChunks;
    }

    public Vector2Int GetChunkCoord(Vector3 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x / chunkManager.chunkSize), Mathf.FloorToInt(pos.y / chunkManager.chunkSize));
    }
    */
}

/*
[Serializable]
public class TileData
{
    public Vector2Int gridPos;
    public Vector2 worldPos;
    public TileBase tile;

    public TileData(Vector2Int g, Vector2 w, TileBase t)
    {
        gridPos = g;
        worldPos = w;
        tile = t;
    }
}
*/