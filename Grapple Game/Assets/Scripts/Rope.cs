using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Rope : MonoBehaviour
{
    LineRenderer lineRenderer;

    public List<Point> points = new List<Point>();
    public List<Line> lines = new List<Line>();

    [HideInInspector] public float maxLength;
    bool isDetached;
    float timeAfterDetached;
    
    float currentTension;
    
    [Header("Swing")]
    [SerializeField] float gravityScale;
    [SerializeField] AnimationCurve tensionCurve;
    
    [Header("Collision")]
    [SerializeField] float collisionRadius;
    [SerializeField] LayerMask collisionLayer;
    [SerializeField] float bounceFactor;
    
    [Header("Optimizations")]
    [SerializeField] int iterations;
    [SerializeField] int subSteps;
    [SerializeField] int collisionIntervals;
    
    [Header("Timers")]
    [SerializeField] float fullTime;
    [SerializeField] float invisTime;
    
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.15f;
        timeAfterDetached = 0.0f;

        lineRenderer.positionCount = points.Count;
    }

    void Update()
    {
        #region Update Renderer Position
        Vector3[] pointPositions = new Vector3[points.Count];
        for (int i = 0; i < pointPositions.Length; i++)
        {
            pointPositions[i] = points[i].currentPos;
        }
        lineRenderer.SetPositions(pointPositions);
        #endregion
        
        if (!isDetached)
            { return; }
        
        timeAfterDetached += Time.deltaTime;
        if (timeAfterDetached > fullTime)
            { Destroy(this.gameObject); }
    }

    void FixedUpdate()
    {
        SetTension();
        for (int i = 0; i < subSteps; i++)
        {
            ChangeVelocities();
            SolveConstraints();
        }
    }

    void SetTension()
    {
        float distance = Vector2.Distance(points[0].currentPos, points[^1].currentPos);
        currentTension = tensionCurve.Evaluate(Mathf.Clamp01(distance / maxLength));
    }

    void ChangeVelocities()
    {
        #region Change Velocity
        foreach (Point point in points)
        {
            if (point.isLocked)
                { continue; }
            
            Vector2 velocity = (point.currentPos - point.pastPos) / subSteps;
            Vector2 gravity =  gravityScale * Time.fixedDeltaTime / subSteps * Physics.gravity;

            point.pastPos += velocity;
            point.currentPos += velocity + gravity;
        }
        #endregion
    }

    void SolveConstraints()
    {
        #region Find Positions
        for (int i = 0; i < iterations / subSteps; i++)
        {
            foreach (Line line in lines)
            {
                int2 index = line.pointIndexes;
                if (points[index[0]].isLocked && points[index[1]].isLocked)
                    { continue; }
                
                Vector2 distance = points[index[0]].currentPos - points[index[1]].currentPos;
                float difference = distance.magnitude - (line.length * (!isDetached ? currentTension : 1));
                Vector2 direction = distance.normalized;

                if (!points[index[0]].isLocked && !points[index[1]].isLocked)
                {
                    points[index[0]].currentPos -= 0.5f * difference * direction;
                    points[index[1]].currentPos += 0.5f * difference * direction;
                }
                else
                {
                    if (!points[index[0]].isLocked)
                        { points[index[0]].currentPos -= difference * direction; }
                    
                    if (!points[index[1]].isLocked)
                        { points[index[1]].currentPos += difference * direction; }
                }
            }

            if (i % collisionIntervals != 0)
                { continue; }

            foreach (Point point in points)
            {
                if (point.isLocked)
                    { continue; }
                
                Vector2 velocity = point.currentPos - point.pastPos;
                Collider2D[] colliders = Physics2D.OverlapCircleAll(point.currentPos, collisionRadius, collisionLayer);
                foreach (Collider2D collider in colliders)
                {
                    Vector2 closestPoint = collider.ClosestPoint(point.currentPos);
                    float distance = Vector2.Distance(point.currentPos, closestPoint);
                    if (distance < collisionRadius)
                    {
                        Vector2 normal = (point.currentPos - closestPoint).normalized;
                        if (normal == Vector2.zero)
                            { normal = (point.currentPos - (Vector2) collider.transform.position).normalized; }
                        
                        float depth = collisionRadius - distance;
                        point.currentPos += depth * normal;

                        velocity = Vector2.Reflect(velocity, normal) * bounceFactor;
                    }
                }

                point.pastPos = point.currentPos - velocity;
            }
        }
        #endregion
    }

    public void DetachRope()
    {
        isDetached = true;
        timeAfterDetached = 0.0f;
    }
}

[System.Serializable]
public class Point
{
    public Vector2 currentPos;
    public Vector2 pastPos;
    public bool isLocked;

    public Point(Vector2 c, bool i)
    {
        currentPos = c;
        pastPos = c;
        isLocked = i;
    }
}

[System.Serializable]
public class Line
{
    public int2 pointIndexes;
    public float length;

    public Line(int2 p, float l)
    {
        pointIndexes = p;
        length = l;
    }
}
