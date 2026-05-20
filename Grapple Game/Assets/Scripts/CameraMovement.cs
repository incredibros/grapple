using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    GameObject target;
    Vector3 targetPosition;
    Vector3 currentPosition;
    Vector3 currentOffset;
    [SerializeField] float smoothTime;
    Vector3 velocity;
    Vector3 velocityOffset;

    bool foundPlayer;

    [SerializeField] AnimationCurve offsetCurve;
    [SerializeField] int offsetMult;
    
    [SerializeField] List<Area> allAreas = new List<Area>();
    int currentArea = -1;
    [SerializeField] bool showAreas = true;

    void Awake()
    {
        target = GameObject.FindWithTag("Player");
        foundPlayer = target != null;
    }
    
    void Start()
    {
        velocity = Vector3.zero;
        velocityOffset = Vector3.zero;
        currentPosition = new Vector3(transform.position.x, transform.position.y, -10);
        currentOffset = Vector3.zero;
    }

    #region Move Camera
    void LateUpdate()
    {
        if (!foundPlayer)
            { return; }
        
        targetPosition = new Vector3(target.transform.position.x, target.transform.position.y, -10);
        
        if (currentArea == -1 || !(targetPosition.x >= allAreas[currentArea].playerBounds[0].x && targetPosition.x < allAreas[currentArea].playerBounds[1].x
            && targetPosition.y >= allAreas[currentArea].playerBounds[0].y && targetPosition.y < allAreas[currentArea].playerBounds[1].y))
        {
            currentArea = FindCurrentArea(targetPosition);
        }

        if (currentArea != -1)
        {
            targetPosition = new Vector3(Mathf.Clamp(targetPosition.x, allAreas[currentArea].cameraBounds[0].x, allAreas[currentArea].cameraBounds[1].x),
                Mathf.Clamp(targetPosition.y, allAreas[currentArea].cameraBounds[0].y, allAreas[currentArea].cameraBounds[1].y), -10);
        }
        
        currentPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref velocity, smoothTime);
        
        Vector2 mousePosition = Input.mousePosition;
        Vector2 center = new Vector2(Screen.width / 2, Screen.height / 2);
        Vector2 offset = new Vector2((mousePosition.x - center.x) / (Screen.width / 2), (mousePosition.y - center.y) / (Screen.height / 2));
        Vector3 shiftedOffset = new Vector3(offsetCurve.Evaluate(Mathf.Abs(offset.x)) * Mathf.Sign(offset.x) * offsetMult, offsetCurve.Evaluate(Mathf.Abs(offset.y)) * Mathf.Sign(offset.y) * offsetMult, 0);

        currentOffset = Vector3.SmoothDamp(currentOffset, shiftedOffset, ref velocityOffset, smoothTime);

        transform.position = currentPosition + currentOffset;
    }

    int FindCurrentArea(Vector2 targetPosition)
    {
        for (int i = 0; i < allAreas.Count; i++)
        {
            if (targetPosition.x >= allAreas[i].playerBounds[0].x && targetPosition.x < allAreas[i].playerBounds[1].x
                && targetPosition.y >= allAreas[i].playerBounds[0].y && targetPosition.y < allAreas[i].playerBounds[1].y)
                { return i; }
        }
        return -1;
    }
    #endregion

    #region Draw Gizmos
    void OnDrawGizmos()
    {
        if (!showAreas) { return; }
        
        for (int i = 0; i < allAreas.Count; i++)
        {
            Gizmos.color = Color.green;
            DrawBox(allAreas[i].cameraBounds);

            Gizmos.color = Color.white;
            DrawBox(allAreas[i].playerBounds);
        }
    }

    void DrawBox(Vector2[] points)
    {
        if (points[0] == points[1])
        {
            Gizmos.DrawLine(points[0], points[0] + Vector2.right / 4);
            Gizmos.DrawLine(points[0], points[0] + Vector2.down / 4);
            Gizmos.DrawLine(points[0], points[0] + Vector2.left / 4);
            Gizmos.DrawLine(points[0], points[0] + Vector2.up / 4);
        } else
        {
            Gizmos.DrawLine(points[0], new Vector2(points[0].x, points[1].y));
            Gizmos.DrawLine(new Vector2(points[0].x, points[1].y), points[1]);
            Gizmos.DrawLine(points[1], new Vector2(points[1].x, points[0].y));
            Gizmos.DrawLine(new Vector2(points[1].x, points[0].y), points[0]);
        }
    }
    #endregion
}

[System.Serializable]
public class Area
{
    public Vector2[] playerBounds = new Vector2[2];
    public Vector2[] cameraBounds = new Vector2[2];
}
