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
	public int coyoteTime;
	public int wallCoyoteTime;
	public int jumpBufferTime;
	public int lateralBufferTime;
	public int jumpDelayTime;
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
	public float minLineLength;
	[Space(5)]
	public float pullSpeed;
	public float pullFreezeDuration;
	public AnimationCurve pullFreezeVelocity;
	public float reelSpeed;
	public float reelVelocityChangeDuration;
	public AnimationCurve reelVelocityChangeLerp;
	[Space(5)]
	public int releaseDelayTime;
	[Space(5)]
	public LayerMask grappleLayers;
	public LayerMask nonGrappleLayer;
	public LayerMask platformLayer;
	public LayerMask semiSolidLayer;
	public LayerMask pitonLayer;
	public LayerMask flingerLayer;
	public LayerMask spinnerLayer;
	public LayerMask bouncerLayer;
	
	[Header("Checkpoint")]
	public Vector2 checkpointCheckSize;
	public LayerMask checkpointLayer;

	[Header("Hazards")]
	public Vector2 hazardCheckSize;
	public LayerMask hazardLayer;

	[Header("Orbs")]
	public Vector2 orbCheckSize;
	public LayerMask orbLayer;

	[Header("Springs")]
	public Vector2 crystalCheckSize;
	public LayerMask crystalLayer;

	[Header("Springs")]
	public Vector2 springCheckSize;
	public LayerMask springLayer;
	public int springForce;

	[Header("Piton")]
	public float hangTime;
	
	[Header("Flinger")]
	public float flingerLaunchForce;

	[Header("Effects")]
	public float ghostDuplicateDelay;
	public GameObject ghostPrefab;
	public GameObject groundEffectPrefab;
	public GameObject gunShotEffectPrefab;

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