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
    public List<WrapPoint> wrapPoints = new List<WrapPoint>();

    static readonly Collider2D[] collisionResults = new Collider2D[8];
    Vector3[] renderPositions;

    [HideInInspector] public float maxLength;
    float wrappedLength;
    bool isDetached;
    float timeAfterDetached;
    public bool stopCollisions;
    
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
    [SerializeField] int subSteps;
    [SerializeField] int iterationsPerSubStep;
    [SerializeField] int collisionIntervals;

    [Header("Wrap")]
    [SerializeField] float wrapResolution;
    [SerializeField] int maxWrapChecks;
    
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
        wrappedLength = maxLength;

        renderPositions = new Vector3[points.Count];
    }

    void Update()
    {
        #region Update Renderer
        for (int i = 0; i < renderPositions.Length; i++)
        {
            renderPositions[i] = points[i].currentPos;
        }

        lineRenderer.SetPositions(renderPositions);
        #endregion
        
        if (!isDetached)
            return;
        
        timeAfterDetached += Time.deltaTime;
        if (timeAfterDetached > fullTime)
        {
            Destroy(this.gameObject);
        }
    }

    void FixedUpdate()
    {
        CheckForWrapping();
        FindValuesForRope(out float distance, out float tension, out float adjustment);
        for (int i = 0; i < subSteps; i++)
        {
            ChangeVelocities(adjustment);
            SolveConstraints(tension);
        }
        AdjustRope(distance, adjustment);
    }

    void CheckForWrapping()
    {
        #region Check Wrapping
        if (isDetached)
            return;
        
        RaycastHit2D hit;
        float distance = Vector2.Distance(points[^1].pastPos, points[^1].currentPos);
        int steps = Mathf.Min(Mathf.CeilToInt(distance / wrapResolution), maxWrapChecks);
        Vector2 end = wrapPoints[^1].pos;
        
        for (int i = 0; i < steps + 1; i++)
        {
            Vector2 start = i != steps ? Vector2.Lerp(points[^1].pastPos, points[^1].currentPos, (wrapResolution * i) / distance) : points[^1].currentPos;
            List<RaycastHit2D> cornerHits = new List<RaycastHit2D>();
            
            hit = Physics2D.Linecast(start, end, collisionLayer);
            if (hit.collider == null || Vector2.Distance(hit.point, end) < 0.1f)
                continue;
            cornerHits.Add(hit);
            
            hit = Physics2D.Linecast(end, start, collisionLayer);
            if (hit.collider == null || Vector2.Distance(cornerHits[0].point, hit.point) > 0.45f)
            {
                WrapPointFallback(end, cornerHits[0].point, (start - end).normalized);
                Debug.Log("Fallback: no collision or too far collision");
                break;
            }
            cornerHits.Add(hit);

            Vector2 normal1 = cornerHits[0].normal;
            Vector2 normal2 = cornerHits[1].normal;
            Vector2 cornerNormal = normal1 + normal2;
            Vector2 offset = cornerNormal * collisionRadius;

            float distance1 = Vector2.Dot(normal1, cornerHits[0].point);
            float distance2 = Vector2.Dot(normal2, cornerHits[1].point);
            float determinate = normal1.x * normal2.y - normal1.y * normal2.x;
            if (Mathf.Abs(determinate) < 1e-6f)
            {
                WrapPointFallback(end, cornerHits[0].point, (start - end).normalized);
                Debug.Log("Fallback: raycast within collider");
                break;
            }
            
            float x = (distance1 * normal2.y - distance2 * normal1.y) / determinate;
            float y = (normal1.x * distance2 - normal2.x * distance1) / determinate;
            Vector2 corner = new Vector2(x, y);

            Vector2 newPos = corner + offset;
            FindValuesForWrapPoint(end, newPos, out int index, out float remainder);
            wrapPoints.Add(new WrapPoint(newPos, index, remainder));
            player.events.OnChangeAnchorPoint?.Invoke(newPos, true);
            wrappedLength -= Vector2.Distance(end, newPos);
            Debug.Log(corner + ", " + offset);
            break;
        }

        if (wrapPoints.Count == 1)
            return;
        
        hit = Physics2D.Linecast(points[^1].currentPos, wrapPoints[^2].pos, collisionLayer);
        if (hit.collider == null || Vector2.Distance(hit.point, wrapPoints[^2].pos) <= 0.1f)
        {
            wrappedLength += Vector2.Distance(wrapPoints[^1].pos, wrapPoints[^2].pos);
            player.events.OnChangeAnchorPoint?.Invoke(wrapPoints[^2].pos, false);
            wrapPoints.RemoveAt(wrapPoints.Count - 1);
        }
        #endregion
    }

    void WrapPointFallback(Vector2 wrapPos, Vector2 hitPos, Vector2 direction)
    {
        Vector2 partialOffset = direction * collisionRadius;
        Vector2 newPos = hitPos + partialOffset;
        FindValuesForWrapPoint(wrapPos, newPos, out int index, out float remainder);
        wrapPoints.Add(new WrapPoint(newPos, index, remainder));
        player.events.OnChangeAnchorPoint?.Invoke(newPos, true);
        wrappedLength -= Vector2.Distance(wrapPos, newPos);
    }

    void FindValuesForWrapPoint(Vector2 wrapPos, Vector2 newPos, out int index, out float remainder)
    {
        float lineLength = maxLength / lines.Count;
        float accumulatedLength = wrapPoints[^1].index * lineLength + wrapPoints[^1].remainder;
        accumulatedLength += Vector2.Distance(wrapPos, newPos);
        
        index = Mathf.FloorToInt(accumulatedLength / lineLength);
        remainder = accumulatedLength % lineLength;

        Debug.Log("Max length: " + maxLength + ", Line Length: " + lineLength + ", Lines: " + lines.Count + ", Index: " + index + ", Remainder: " + remainder);
    }

    void FindValuesForRope(out float distance, out float tension, out float adjustment)
    {
        distance = Vector2.Distance(wrapPoints[^1].pos, points[^1].currentPos);
        tension = !isDetached ? tensionCurve.Evaluate(Mathf.Clamp01(distance / wrappedLength)) : 0.97f;
        adjustment = !isDetached ? adjustmentCurve.Evaluate(Mathf.Clamp01((wrappedLength - distance) / adjustmentLength)) : 0;
    }

    void ChangeVelocities(float adjustment)
    {
        #region Change Velocity
        foreach (Point point in points)
        {
            if (point.isLocked)
                continue;
            
            Vector2 velocity = (point.currentPos - point.pastPos) / subSteps;
            Vector2 gravity = gravityScale * Mathf.Pow(Time.fixedDeltaTime / subSteps, 2) * (1 - adjustment) * Physics.gravity;
            
            point.pastPos += velocity;
            
            velocity += gravity;
            
            point.currentPos += velocity;
        }
        #endregion
    }

    void SolveConstraints(float tension)
    {
        for (int i = 0; i < iterationsPerSubStep; i++)
        {
            #region Distance Constraints
            foreach (Line line in lines)
            {
                int2 index = line.pointIndexes;
                if (points[index[0]].isLocked && points[index[1]].isLocked)
                    continue;
                
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
                    {
                        points[index[0]].currentPos -= difference * direction;
                    }
                    
                    if (!points[index[1]].isLocked)
                    {
                        points[index[1]].currentPos += difference * direction;
                    }
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
                    continue;
                
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
                {
                    points[j- 1].currentPos += correction1;
                }
                if (!points[j + 1].isLocked)
                {
                    points[j + 1].currentPos += correction2;
                }
                if (!points[j].isLocked)
                {
                    points[j].currentPos -= correction1 + correction2;
                }
            }
            */
            #endregion

            #region Collisions
            if (i % collisionIntervals != 0 || stopCollisions)
                continue;

            foreach (Point point in points)
            {
                if (point.isLocked)
                    continue;
                
                Vector2 velocity = point.currentPos - point.pastPos;
                int count = Physics2D.OverlapCircleNonAlloc(point.currentPos, collisionRadius, collisionResults, collisionLayer);

                for (int c = 0; c < count; c++)
                {
                    Collider2D collider = collisionResults[c];

                    Physics2D.SyncTransforms();
                    Vector2 closestPoint = collider.ClosestPoint(point.currentPos);
                    float distance = Vector2.Distance(point.currentPos, closestPoint);
                    if (distance < collisionRadius)
                    {
                        Vector2 normal = (point.currentPos - closestPoint).normalized;
                        if (normal == Vector2.zero)
                        {
                            normal = (point.currentPos - (Vector2) collider.transform.position).normalized;
                        }
                        
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
        if (isDetached || wrappedLength - distance > adjustmentLength)
            return;

        int totalPoints = points.Count - 1;
        float lineLength = maxLength / totalPoints;
        for (int i = 0; i < wrapPoints.Count; i++)
        {
            int totalBoundedPoints = (i + 1 != wrapPoints.Count ? wrapPoints[i + 1].index + 1 : points.Count) - (wrapPoints[i].index + 1);
            int startIndex = wrapPoints[i].index + 1;
            Vector2 startPos = wrapPoints[i].pos;
            Vector2 endPos = i + 1 != wrapPoints.Count ? wrapPoints[i + 1].pos : points[^1].currentPos;
            float boundedDistance = Vector2.Distance(startPos, endPos);
            float offset = lineLength - wrapPoints[i].remainder;
            
            for (int j = 0; j < totalBoundedPoints; j++)
            {
                int index = startIndex + j;
                Vector2 setPos = Vector2.Lerp(startPos, endPos, ((float) (j * lineLength) + offset) / boundedDistance);
                Vector2 displacement = setPos - points[index].currentPos;

                points[index].currentPos += displacement * adjustment;
                points[index].pastPos += displacement * adjustment;
            }
        }
        
        /* for (int i = 1; i < points.Count - 1; i++)
        {
            Vector2 setPos = Vector2.Lerp(points[0].currentPos, points[^1].currentPos, (float) i / totalPoints);
            Vector2 displacement = setPos - points[i].currentPos;
            
            points[i].currentPos += displacement * adjustment;
            points[i].pastPos += displacement * adjustment;
        } */
        #endregion
    }

    public void DetachRope()
    {
        FindValuesForRope(out _, out _, out float adjustment);
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

[System.Serializable]
public class WrapPoint
{
    public Vector2 pos;
    public int index;
    public float remainder;

    public WrapPoint(Vector2 p, int i, float r)
    {
        pos = p;
        index = i;
        remainder = r;
    }
}
