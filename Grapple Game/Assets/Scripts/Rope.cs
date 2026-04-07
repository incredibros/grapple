using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Rope : MonoBehaviour
{
    LineRenderer lineRenderer;

    public Player player;
    
    public List<Point> points = new List<Point>();
    public List<Line> lines = new List<Line>();
    public List<Vector2> wrapPoints = new List<Vector2>();

    [HideInInspector] public float maxLength;
    bool isDetached;
    float timeAfterDetached;
    
    [Header("Swing")]
    [SerializeField] float gravityScale;
    [SerializeField] AnimationCurve tensionCurve;
    [SerializeField] AnimationCurve adjustmentCurve;
    [SerializeField] float adjustmentLength;
    [SerializeField] float bendingStiffness;
    
    [Header("Collision")]
    [SerializeField] float collisionRadius;
    [SerializeField] LayerMask collisionLayer;
    [SerializeField] float bounceFactor;
    
    [Header("Optimizations")]
    [SerializeField] int iterationsPerSubStep;
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
        lineRenderer.startWidth = collisionRadius * 2;
        lineRenderer.endWidth = collisionRadius * 2;
        timeAfterDetached = 0.0f;

        lineRenderer.positionCount = points.Count;
        wrapPoints.Add(points[0].currentPos);
    }

    void Update()
    {
        #region Update Renderer
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
        CheckForWrapping();
        FindValues(out float distance, out float tension, out float adjustment);
        for (int i = 0; i < subSteps; i++)
        {
            ChangeVelocities(adjustment);
            SolveConstraints(tension, adjustment);
        }
        AdjustRope(distance, adjustment);
    }

    void CheckForWrapping()
    {
        if (isDetached)
            { return; }
        
        RaycastHit2D hit;
        float distance = Vector2.Distance(points[^1].pastPos, points[^1].currentPos);
        int steps = Mathf.FloorToInt(distance / 0.01f);
        for (int i = 0; i < steps + 1; i++)
        {
            Vector2 point = i != steps ? Vector2.Lerp(points[^1].pastPos, points[^1].currentPos, 0.01f * i / distance) : points[^1].currentPos;
            hit = Physics2D.Linecast(point, wrapPoints[^1], collisionLayer);
            if (hit.collider != null && Vector2.Distance(hit.point, wrapPoints[^1]) > 0.1f)
            {
                wrapPoints.Add(hit.point);
                player.events.OnChangeAnchorPoint?.Invoke(wrapPoints[^1], true);
                Debug.Log(hit.point + ", " + steps + ", " + i);
                break;
            }
        }

        if (wrapPoints.Count == 1)
            { return; }
        
        hit = Physics2D.Linecast(points[^1].currentPos, wrapPoints[^2], collisionLayer);
        if (hit.collider == null || Vector2.Distance(hit.point, wrapPoints[^2]) <= 0.1f)
        {
            wrapPoints.RemoveAt(wrapPoints.Count - 1);
            player.events.OnChangeAnchorPoint?.Invoke(wrapPoints[^1], false);
            Debug.Log("Removed wrap point");
        }
    }

    void FindValues(out float distance, out float tension, out float adjustment)
    {
        distance = Vector2.Distance(points[0].currentPos, points[^1].currentPos);
        tension = !isDetached ? tensionCurve.Evaluate(Mathf.Clamp01(distance / maxLength)) : 0.97f;
        adjustment = !isDetached ? adjustmentCurve.Evaluate(Mathf.Clamp01((maxLength - distance) / adjustmentLength)) : 0;
    }

    void ChangeVelocities(float adjustment)
    {
        #region Change Velocity
        foreach (Point point in points)
        {
            if (point.isLocked)
                { continue; }
            
            Vector2 velocity = (point.currentPos - point.pastPos) / subSteps;
            Vector2 gravity = gravityScale * Mathf.Pow(Time.fixedDeltaTime / subSteps, 2) * (1 - adjustment) * Physics.gravity;
            
            point.pastPos += velocity;
            
            velocity += gravity;
            RaycastHit2D hit;
            if (hit = Physics2D.CircleCast(point.currentPos, collisionRadius, velocity.normalized, velocity.magnitude, collisionLayer))
                { point.currentPos = hit.point + (hit.normal * collisionRadius); }
            else
                { point.currentPos += velocity; }
        }
        #endregion
    }

    void SolveConstraints(float tension, float adjustment)
    {
        for (int i = 0; i < iterationsPerSubStep; i++)
        {
            #region Distance Constraints
            foreach (Line line in lines)
            {
                int2 index = line.pointIndexes;
                if (points[index[0]].isLocked && points[index[1]].isLocked)
                    { continue; }
                
                Vector2 distance = points[index[0]].currentPos - points[index[1]].currentPos;
                float difference = distance.magnitude - (line.length * tension);
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
            #endregion

            #region Bending Constraints
            /*
            for (int j = 1; j < points.Count - 1; j++)
            {
                Vector2 v1 = points[j - 1].currentPos - points[j].currentPos;
                Vector2 v2 = points[j + 1].currentPos - points[j].currentPos;

                float distance1 = v1.magnitude;
                float distance2 = v2.magnitude;
                if (distance1 == 0 || distance2 == 0)
                    { continue; }
                
                Vector2 direction1 = v1.normalized;
                Vector2 direction2 = v2.normalized;
                
                float cosTheta = Vector2.Dot(direction1, direction2);
                cosTheta = Mathf.Clamp(cosTheta, -1f, 1f); // -1 <= cosTheta <= 1
                float error = (cosTheta + 1f) / 2; // 0 <= error <= 1
                float stiffness = bendingStiffness * (1 - adjustment);
                
                Vector2 gradient1 = direction1 - (cosTheta * direction2);
                Vector2 gradient2 = direction2 - (cosTheta * direction1);
                Vector2 correction1 = error * stiffness * gradient1;
                Vector2 correction2 = error * stiffness * gradient2;

                if (!points[j - 1].isLocked)
                    { points[j- 1].currentPos += correction1; }
                if (!points[j + 1].isLocked)
                    { points[j + 1].currentPos += correction2; }
                if (!points[j].isLocked)
                    { points[j].currentPos -= correction1 + correction2; }
            }
            */
            #endregion

            #region Collisions
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
            #endregion
        }
    }
    
    void AdjustRope(float distance, float adjustment)
    {
        #region Adjustements
        if (isDetached || maxLength - distance > adjustmentLength)
            { return; }

        int totalPoints = points.Count - 1;
        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector2 setPos = Vector2.Lerp(points[0].currentPos, points[^1].currentPos, (float) i / totalPoints);
            Vector2 displacement = setPos - points[i].currentPos;
            
            points[i].currentPos += displacement * adjustment;
            points[i].pastPos += displacement * adjustment;
        }
        #endregion
    }

    public void DetachRope()
    {
        FindValues(out _, out _, out float adjustment);
        if (adjustment != 0)
        {
            Vector2 direction = Vector2.Perpendicular((points[0].currentPos - points[^1].currentPos).normalized);
            foreach (Point point in points)
            {
                Vector2 velocity = point.currentPos - point.pastPos;
                Vector2 projected = Vector2.Dot(direction, velocity) * direction;
                Vector2 finalVelocity = Vector2.Lerp(velocity, projected, adjustment);
                point.pastPos = point.currentPos - finalVelocity;
            }
        }

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
