using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Tilemaps;

[InitializeOnLoad]
public static class EditorScript
{
    public static List<GameObject> tilemaps = new List<GameObject>();
    public static List<GridBrushBase> gridBrushBases = new List<GridBrushBase>();
    public static GameObject player;

    static EditorScript()
    {
        SceneView.duringSceneGui += OnSceneGUI;

        EditorApplication.delayCall += Initialize;
    }

    private static void Initialize()
    {
        tilemaps.Add(GameObject.Find("PlatformTilemap"));
        tilemaps.Add(GameObject.Find("SemiSolidTilemap"));
        tilemaps.Add(GameObject.Find("HazardTilemap"));
        tilemaps.Add(GameObject.Find("SpriteTilemap"));

        tilemaps.Add(GameObject.Find("HalfPlatformTilemap"));
        tilemaps.Add(GameObject.Find("HalfSemiSolidTilemap"));
        tilemaps.Add(GameObject.Find("HalfHazardTilemap"));
        tilemaps.Add(GameObject.Find("HalfSpriteTilemap"));

        gridBrushBases.Add(AssetDatabase.LoadAssetAtPath<GridBrushBase>("Assets/Graphics/Tilemap/Brushes/GroundBrush.asset"));
        gridBrushBases.Add(AssetDatabase.LoadAssetAtPath<GridBrushBase>("Assets/Graphics/Tilemap/Brushes/SemiSolidBrush.asset"));
        gridBrushBases.Add(AssetDatabase.LoadAssetAtPath<GridBrushBase>("Assets/Graphics/Tilemap/Brushes/HalfGroundBrush.asset"));

        player = GameObject.Find("Player");
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        if (e.type != EventType.KeyDown)
            { return; }

        if (e.keyCode == KeyCode.Alpha0)
        {
            Vector2 mousePos = Event.current.mousePosition;
            Ray worldRay = HandleUtility.GUIPointToWorldRay(mousePos);
            player.transform.position = worldRay.GetPoint(10f);
            player.transform.position = new Vector3(Mathf.Round(player.transform.position.x - 0.5f) + 0.5f, Mathf.Round(player.transform.position.y - 0.5f) + 0.5f, 0);
            Debug.Log("Player Selected");
        }
            
        if (e.keyCode == KeyCode.Alpha1)
        {
            GridPaintingState.scenePaintTarget = tilemaps[0];
            GridPaintingState.gridBrush = gridBrushBases[0];
            TilemapEditorTool.SetActiveEditorTool(typeof(PaintTool));
            Debug.Log("Ground Brush Selected");
        }
        if (e.keyCode == KeyCode.Alpha2)
        {
            GridPaintingState.scenePaintTarget = tilemaps[1];
            GridPaintingState.gridBrush = gridBrushBases[1];
            TilemapEditorTool.SetActiveEditorTool(typeof(PaintTool));
            Debug.Log("Semi Solid Brush Selected");
        }
        if (e.keyCode == KeyCode.Alpha3)
        {
            GridPaintingState.scenePaintTarget = tilemaps[2];
            GridPaintingState.gridBrush = GridPaintingState.brushes[0];
            TilemapEditorTool.SetActiveEditorTool(typeof(PaintTool));
            Debug.Log("Hazard Brush Selected");
        }
        if (e.keyCode == KeyCode.Alpha4)
        {
            GridPaintingState.scenePaintTarget = tilemaps[3];
            GridPaintingState.gridBrush = GridPaintingState.brushes[0];
            TilemapEditorTool.SetActiveEditorTool(typeof(PaintTool));
            Debug.Log("Sprite Brush Selected");
        }

        if (e.keyCode == KeyCode.Alpha5)
        {
            GridPaintingState.scenePaintTarget = tilemaps[4];
            GridPaintingState.gridBrush = gridBrushBases[2];
            TilemapEditorTool.SetActiveEditorTool(typeof(PaintTool));
            Debug.Log("Half Ground Brush Selected");
        }
        if (e.keyCode == KeyCode.Alpha6)
        {
            GridPaintingState.scenePaintTarget = tilemaps[5];
            GridPaintingState.gridBrush = GridPaintingState.brushes[0];
            TilemapEditorTool.SetActiveEditorTool(typeof(PaintTool));
            Debug.Log("Half Semi Solid Brush Selected");
        }
        if (e.keyCode == KeyCode.Alpha7)
        {
            GridPaintingState.scenePaintTarget = tilemaps[6];
            GridPaintingState.gridBrush = GridPaintingState.brushes[0];
            TilemapEditorTool.SetActiveEditorTool(typeof(PaintTool));
            Debug.Log("Half Hazard Brush Selected");
        }
        if (e.keyCode == KeyCode.Alpha8)
        {
            GridPaintingState.scenePaintTarget = tilemaps[7];
            GridPaintingState.gridBrush = GridPaintingState.brushes[0];
            TilemapEditorTool.SetActiveEditorTool(typeof(PaintTool));
            Debug.Log("Half Sprite Brush Selected");
        }
    }
}
