using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class TileSnapper : MonoBehaviour
{
    Grid grid;

    void OnEnable()
    {
        GetGrid();
    }

    void OnTransformParentChanged()
    {
        GetGrid();
    }

    void GetGrid()
    {
        grid = GetComponentInParent<Grid>();
    }

    void Update()
    {
        if (!grid)
            return;

       if (transform.hasChanged)
        {
            GetGrid();

            if (grid != null)
                Snap();

            transform.hasChanged = false;
        }
    }

    void Snap()
    {
        Vector3Int cell = grid.WorldToCell(transform.position);
        transform.position = grid.GetCellCenterWorld(cell);
    }
}