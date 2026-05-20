using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class TileSnapper : MonoBehaviour
{
    Grid grid;

    #region Get Grid
    void OnEnable()
    {
        grid = GetComponentInParent<Grid>();
    }

    void OnTransformParentChanged()
    {
        grid = GetComponentInParent<Grid>();
    }
    #endregion

    #region Snap
    void Update()
    {
        if (!grid)
            return;

       if (transform.hasChanged)
        {
            grid = GetComponentInParent<Grid>();

            if (grid != null)
            {
                Snap();
            }
            transform.hasChanged = false;
        }
    }

    void Snap()
    {
        Vector3Int cell = grid.WorldToCell(transform.position);
        transform.position = grid.GetCellCenterWorld(cell);
    }
    #endregion
}