using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlayerData", order = 1)]
public class PlayerData : ScriptableObject
{
    // This script contains all the info to control the player
	// This script is a child of scriptable object, which is a premade unity script that basically allows this script to keep all of its info
	// in its own file location outside of the player
	
	[Header("Run")]
	public float moveSpeed;
	public float velPower;

	[Header("Jump")]
	public float jumpForce;
	public Vector2 wallJumpForce;
	[Range(0f, 1)] public float jumpCutMultiplier;
	[Space(5)]
	public float coyoteTime;
	public float wallCoyoteTime;
	public float jumpBufferTime;
	public float lateralBufferTime;
	public float jumpDelayTime;
	[Space(5)]
	public float gravityScale;
	public float fallGravityMultiplier;
	public float maxFallSpeed;
	public float fastFallMultiplier;
	[Space(5)]
	public float wallSlideGravityMultiplier;
	public float startWallSlideSpeed;
	public float maxWallSlideSpeed;
	
	[Header("Checks")]
	public Vector2 groundCheckPoint;
	public Vector2 groundCheckSize;
	public LayerMask groundLayer;
	[Space(5)]
	public List<Vector2> leftCheckPoints;
	public List<Vector2> rightCheckPoints;
	public Vector2 wallCheckSize;
	public LayerMask wallLayer;
	[Space(5)]
	public Vector2 ceilingCheckPoint;
	public Vector2 ceilingCheckSize;
	public LayerMask ceilingLayer;

	[Header("Grapple")]
	public GameObject grapplePrefab;
	public float grappleRange;
	public LayerMask grappleLayers;
	public LayerMask semiSolidLayer;
	[Space(5)]
	public float minPullSpeed;
	public float freezeDuration;
	public AnimationCurve freezeVelocity;
	[Space(5)]
	public float releaseDelayTime;

	[Header("Checkpoint")]
	public Vector2 checkpointCheckSize;
	public LayerMask checkpointLayer;

	[Header("Hazards")]
	public Vector2 hazardCheckSize;
	public LayerMask hazardLayer;

	[Header("Orbs")]
	public Vector2 orbCheckSize;
	public LayerMask orbLayer;

	[Header("Accelerations and Decelerations")]
	public List<AccelInfo> accels;
}

[System.Serializable]
public class AccelInfo
{
	public string type;
	public bool constant;
	public float accel;
	[HideIf("constant")] [AllowNesting] public AnimationCurve accelCurve;
	public float decel;
	[HideIf("constant")] [AllowNesting] public AnimationCurve decelCurve;
	[HideIf("constant")] [AllowNesting] public float time;
}