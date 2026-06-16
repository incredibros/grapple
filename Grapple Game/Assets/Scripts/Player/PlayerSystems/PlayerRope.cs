using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerRope : PlayerSystem
{
    // This system script creates, moves, and detaches ropes from the player, but does not handle rope physics or rendering
    
    Rope rope;
    bool detached = true;
    
    void FixedUpdate()
    {
        if (detached) return;

        rope.points[^1].pastPos = rope.points[^1].currentPos;
		rope.points[^1].currentPos = transform.position;
        rope.wrapPoints[^1].pos = transform.position;
    }

    #region Rope
    void CreateNewRope(Vector2 point)
    {
        if (!detached)
        {
            DetachRope();
        }
        
        if (rope != null)
        {
            rope.stopCollisions = true;
        }
        
        
        GameObject currentPrefab = Instantiate(player.data.grapplePrefab);
		rope = currentPrefab.GetComponent<Rope>();
        rope.player = player;

        float radius = Vector2.Distance(point, transform.position);
        rope.maxLength = radius;
        int totalLines = Mathf.FloorToInt(radius / player.data.minLineLength);
        float lineLength = radius / totalLines;

        for (int i = 0; i < totalLines + 1; i++)
        {
            rope.points.Add(new Point(Vector2.Lerp(point, transform.position, (float) i / totalLines), i == 0 || i == totalLines));
            
            if (i == 0) continue;
            
            rope.lines.Add(new Line(new int2(i, i - 1), lineLength));
        }

        rope.wrapPoints.Add(new WrapPoint(point, 0, 0f));
        rope.wrapPoints.Add(new WrapPoint(transform.position, rope.points.Count - 1, 0f));
        detached = false;
    }

    void DetachRope()
    {
        if (rope == null)
            return;

        rope.points[^1].isLocked = false;
		for (int i = 0; i < rope.lines.Count; i++)
		{
			rope.lines[i].length = Vector2.Distance(
                rope.points[rope.lines[i].pointIndexes[0]].currentPos,
                rope.points[rope.lines[i].pointIndexes[1]].currentPos);
		}

        rope.DetachRope();
        detached = true;
    }
    #endregion
    
    #region Events
    void OnEnable()
    {
        player.events.OnGrapple += CreateNewRope;
        player.events.OnGrappleButtonUp += DetachRope;
        player.events.OnPull += DetachRope;
        player.events.OnDeath += DetachRope;
    }

    void OnDisable()
    {
        player.events.OnGrapple -= CreateNewRope;
        player.events.OnGrappleButtonUp -= DetachRope;
        player.events.OnPull -= DetachRope;
        player.events.OnDeath -= DetachRope;
    }
    #endregion
}
