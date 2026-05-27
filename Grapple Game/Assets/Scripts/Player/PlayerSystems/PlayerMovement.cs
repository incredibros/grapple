using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(DistanceJoint2D))]
public class PlayerMovement : PlayerSystem
{
    // This system script controls all the physics of the player, including swinging, but doesn't render the rope here
	
	Rigidbody2D rb;
	DistanceJoint2D joint;

    PlayerStates state = new PlayerStates();

	Vector2 moveInput;
	Vector2 mouseDirection;

	float coyote;
	float2 wallCoyote;
	float jumpBuffer;
	float2 lateralBuffer;
	float jumpDelay;
	float grappleReleaseDelay;
	float accelTime;

	[HideInInspector] public Vector2 grapplePoint;
	[HideInInspector] public float grappleRadius;
	[HideInInspector] public static bool CursorMode = true;

	int grapples;

	Piton activePiton;
	Vector2 pitonStartPos;
	float pitonZipTimer;
	float pitonZipDuration;

	// Override calls this awake function instead of the awake function from the parent
	// base.Awake still makes sure to call the parent's awake function
	protected override void Awake()
	{
		base.Awake();

        rb = GetComponent<Rigidbody2D>();
		joint = GetComponent<DistanceJoint2D>();
	}

	void Update()
	{
		if (state.isDead || state.isFrozen || MainMenu.GameIsPaused) return;
		
		Timers();
		Checks();

		if (jumpBuffer > 0f && jumpDelay <= 0f && !state.isJumping)
		{
			OnJumpInput();
		}

		if (state.isGrappled)
		{
			OnGrapple();
		}
	}

	void FixedUpdate()
	{
		if (state.isDead || state.isFrozen || MainMenu.GameIsPaused) return;
		
		if (state.isPullingToPiton)
		{
			PitonZipMovement();
		}
		else
		{
			LateralMovement();
			Gravity();
		}
	}

	#region Timers
	void Timers()
	{
		if (moveInput.x <= -1)
		{
			lateralBuffer[0] = player.data.lateralBufferTime;
		}
		if (moveInput.x >= 1)
		{
			lateralBuffer[1] = player.data.lateralBufferTime;
		}

		coyote -= Time.deltaTime;
		wallCoyote[0] -= Time.deltaTime;
		wallCoyote[1] -= Time.deltaTime;
		jumpBuffer -= Time.deltaTime;
		lateralBuffer[0] -= Time.deltaTime;
		lateralBuffer[1] -= Time.deltaTime;
		jumpDelay -= Time.deltaTime;
		grappleReleaseDelay -= Time.deltaTime;
		accelTime += Time.deltaTime;
	}
	#endregion

	#region Checks
	void Checks()
	{
		if (Physics2D.OverlapBox(player.data.groundCheckPoint + (Vector2) transform.position, player.data.groundCheckSize, 0f, player.data.groundLayer) && Mathf.Abs(rb.velocity.y) <= 0.001f)
		{
			coyote = player.data.coyoteTime;
			state.onGround = true;
			if (grapples == 0 && !state.isGrappled)
			{
				grapples++;
			}
		}
		else
		{
			state.onGround = false;
		}
		
		if (rb.velocity.y < -0.001f && !state.isHanging)
		{
			state.isFalling = true;
			if (moveInput.y == -1)
			{
				state.isFastFalling = true;
			}
		}
		else
		{
			state.isFalling = false;
			state.isFastFalling = false;
		}

		state.onWall = new bool2(CheckForWalls(-1), CheckForWalls(1));
		state.isClinging = (state.onWall[0] && lateralBuffer[0] > 0f) || (state.onWall[1] && lateralBuffer[1] > 0f);

		if (state.onWall[0])
		{
			wallCoyote[0] = player.data.wallCoyoteTime;
		}
		if (state.onWall[1])
		{
			wallCoyote[1] = player.data.wallCoyoteTime;
		}
		
		if (state.isClinging && state.isFalling && !state.isHanging)
		{
			if (!state.isSliding)
			{
				rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -player.data.maxWallSlideSpeed));
			}
			state.isSliding = true;
		}
		else
		{
			state.isSliding = false;
		}
	}

	bool CheckForWalls(int dir)
	{
		if (Mathf.Abs(rb.velocity.x) > 0.001f) return false;
		
		int wallsDetected = 0;
		foreach (Vector2 pos in dir == 1 ? player.data.rightCheckPoints : player.data.leftCheckPoints)
		{
			wallsDetected += Physics2D.OverlapBox(pos + (Vector2) transform.position, player.data.wallCheckSize, 0f, player.data.wallLayer) ? 1 : 0;
		}
		return wallsDetected >= 3;
	}
	#endregion

	#region Lateral Movement
	void OnXYInput(Vector2 input)
    {
		if (!CursorMode)
		{
			mouseDirection = input;
		}

        moveInput = Vector2Int.RoundToInt(input);
    }

	void LateralMovement()
	{
		float targetSpeed = moveInput.x * player.data.moveSpeed;
		float speedDif = targetSpeed - rb.velocity.x;
		float accelRate = FindAcceleration(Mathf.Abs(targetSpeed) > 0.01f);
		float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, player.data.velPower) * Mathf.Sign(speedDif);
		rb.AddForce(movement * Vector2.right);
	}

	float FindAcceleration(bool accel)
	{
		// Swing(2) -> WallJump(3) -> Pull(4) -> Ground(0) -> Air(1)

		if (!state.isWallJumping.Equals(false) && accelTime >= player.data.accels[3].time)
		{
			state.isWallJumping = false;
		}
		if (state.isPulling && accelTime >= player.data.accels[4].time)
		{
			state.isPulling = false;
		}
		
		int type = state.isHanging ? 2 : !state.isWallJumping.Equals(false) ? 3 : state.isPulling ? 4 : state.onGround ? 0 : 1;
		float value = accel ? player.data.accels[type].accel : player.data.accels[type].decel;
		if (!player.data.accels[type].constant)
		{
			value *= accel ? player.data.accels[type].accelCurve.Evaluate(accelTime / player.data.accels[type].time)
			: player.data.accels[type].decelCurve.Evaluate(accelTime / player.data.accels[type].time);
		}
		return value;
	}
	#endregion

	#region Gravity
	void Gravity()
	{
		if (state.isFalling && !state.isHanging)
		{
			rb.gravityScale = player.data.gravityScale * (!state.isSliding ? player.data.fallGravityMultiplier : player.data.wallSlideGravityMultiplier);
			rb.gravityScale *= state.isFastFalling ? player.data.fastFallMultiplier : 1;
			rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, !state.isSliding ? -player.data.maxFallSpeed : -player.data.maxWallSlideSpeed));
		}
		else
		{
			rb.gravityScale = player.data.gravityScale;
		}
	}
	#endregion

    #region Jump
	void OnJumpButtonDown()
    {
        jumpBuffer = player.data.jumpBufferTime;
    }

	void OnJumpInput()
	{
		if (coyote > 0f)
		{
			OnJump(0);
		}
		else if (wallCoyote[0] > 0f || wallCoyote[1] > 0f)
		{
			OnJump(wallCoyote[0] > wallCoyote[1] ? -1 : 1);
		}
	}

	void OnJump(int dir)
	{
		if (dir == 0)
		{
			rb.velocity = new Vector2(rb.velocity.x, player.data.jumpForce);
		}
		else
		{
			rb.velocity = new Vector2(player.data.wallJumpForce.x * -dir, player.data.wallJumpForce.y);
		}

		coyote = 0f;
		wallCoyote[0] = 0f;
		wallCoyote[1] = 0f;
		jumpBuffer = 0f;
		lateralBuffer[0] = 0f;
		lateralBuffer[1] = 0f;
		jumpDelay = player.data.jumpDelayTime;
		
		state.isJumping = true;
		state.isWallJumping = new bool2(dir == -1, dir == 1);
		accelTime = dir != 0 ? 0f : accelTime;
	}

	void OnJumpButtonUp()
	{
		if (state.isJumping && !state.isFalling)
		{
			rb.AddForce(rb.velocity.y * (1 - player.data.jumpCutMultiplier) * Vector2.down, ForceMode2D.Impulse);
		}
		state.isJumping = false;
		jumpBuffer = 0f;
	}
	#endregion

	#region Grapple
	void OnPointerMove(Vector2 pos, bool directional)
	{
		if (pos != null)
		{
			if (directional)
			{
				mouseDirection = pos.normalized;
			}
			else if (CursorMode)
			{
				Vector2 mousePos = Camera.main.ScreenToWorldPoint(pos);
				mouseDirection = (mousePos - (Vector2) transform.position).normalized;
			}
		}
		
	}

	void OnGrappleButtonDown()
	{
		if (grapples == 0) return;
		grapples--;
		
		Vector2 nudge = mouseDirection * -0.01f;
		RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position, mouseDirection, player.data.grappleRange, player.data.grappleLayers);
		if (hit.Length == 0) return;
		
		int hitIndex = -1;
		for (int i = 0; i < hit.Length; i++)
		{
			int layer = hit[i].collider.transform.gameObject.layer;

			if ((player.data.nonGrappleLayer.value & (1 << layer)) != 0)
			{
				Debug.Log("Ungrappleable");
				return;
			}
			if ((player.data.semiSolidLayer.value & (1 << layer)) != 0 && mouseDirection.y >= 0)
				continue;

			hitIndex = i;
			break;
		}
		if (hitIndex == -1) return;
		
		TargetPosition hitCollider = hit[hitIndex].collider.GetComponentInParent<TargetPosition>();
		Vector2 hitPoint = hit[hitIndex].point;
		if (hitCollider != null)
		{
			hitPoint = new Vector2(
				Mathf.Clamp(hitPoint.x, hitCollider.transform.position.x + hitCollider.minGrappleBounds.x, hitCollider.transform.position.x + hitCollider.maxGrappleBounds.x),
				Mathf.Clamp(hitPoint.y, hitCollider.transform.position.y + hitCollider.minGrappleBounds.y, hitCollider.transform.position.y + hitCollider.maxGrappleBounds.y)
			);
		}

		state.isGrappled = true;
		joint.enabled = true;
		grappleReleaseDelay = player.data.releaseDelayTime;

		joint.connectedAnchor = grapplePoint = nudge + hitPoint;
		joint.distance = grappleRadius = Vector2.Distance(transform.position, grapplePoint);

		Piton piton = hit[hitIndex].collider.GetComponentInParent<Piton>();
		if (piton != null)
		{
			activePiton = piton;
		}
		else
		{
			activePiton = null;
		}

		player.events.OnGrapple?.Invoke(grapplePoint);
	}

	void OnGrapple()
	{
		if (Vector2.Distance(transform.position, grapplePoint) >= grappleRadius - 0.1f && !state.onGround)
		{
			state.isHanging = true;
		}
		else
		{
			state.isHanging = false;
		}
	}

	void OnGrappleButtonUp()
	{
		if (grappleReleaseDelay > 0) return;
			
		state.isGrappled = false;
		state.isHanging = false;
		joint.enabled = false;
	}

	void OnChangeAnchorPoint(Vector2 point, bool shorten)
	{
		grappleRadius += Vector2.Distance(grapplePoint, point) * (shorten ? -1f : 1f);
		joint.connectedAnchor = grapplePoint = point;
		joint.distance = grappleRadius;
	}
	#endregion

	#region Pull
	void OnPullButtonDown()
	{
		if (!state.isGrappled)
		{
			player.events.OnGrappleButtonDown?.Invoke();
			if (!state.isGrappled) return;
		}
		
		state.isHanging = false;
		state.isJumping = false;
		state.isWallJumping = false;
		state.isPulling = true;
		accelTime = 0f;
		grappleReleaseDelay = 0f;

		if (activePiton != null)
		{
			state.isPullingToPiton = true;
			pitonStartPos = transform.position;
        	pitonZipTimer = 0f;
			float distance = Vector2.Distance(pitonStartPos, grapplePoint);
			pitonZipDuration = distance > 0.01f ? distance / activePiton.zipSpeed : 0.1f;
		}
		else
		{
			rb.velocity = (grapplePoint - (Vector2) transform.position).normalized * player.data.minPullSpeed;
		}

		OnGrappleButtonUp();
		StartCoroutine(FreezeMovement());
		player.events.OnPull?.Invoke();
	}

	IEnumerator FreezeMovement()
	{
		state.isFrozen = true;

		coyote = 0f;
		wallCoyote[0] = 0f;
		wallCoyote[1] = 0f;
		jumpBuffer = 0f;
		lateralBuffer[0] = 0f;
		lateralBuffer[1] = 0f;
		jumpDelay = 0f;

		rb.gravityScale = 0;

		Vector2 startingVelocity = rb.velocity;
		float timer = 0f;
		
		while (timer < player.data.freezeDuration)
		{
			rb.velocity = startingVelocity * player.data.freezeVelocity.Evaluate(timer / player.data.freezeDuration);
			timer += Time.deltaTime;
			yield return new WaitForSeconds(0);
		}

		rb.velocity = startingVelocity;
		state.isFrozen = false;
	}
	#endregion

	#region Death
	void OnDeath()
	{
		state.isDead = true;

		coyote = 0f;
		wallCoyote[0] = 0f;
		wallCoyote[1] = 0f;
		jumpBuffer = 0f;
		lateralBuffer[0] = 0f;
		lateralBuffer[1] = 0f;
		jumpDelay = 0f;
		rb.velocity = Vector2.zero;
		rb.gravityScale = 0;

		StopAllCoroutines();
		CancelPitonZip();

		if (state.isGrappled)
		{
			OnGrappleButtonUp();
		}
	}

	void OnRespawn()
	{
		state.isDead = false;
	}
	#endregion

	#region Orbs
	void OnOrbPickUp(GameObject orb)
	{
		if (grapples != 0) return;
		grapples = 1;
		orb.GetComponent<Orb>().OnPickUp(this.transform);
	}
	#endregion

	#region Spring
	void OnSpringActivated()
	{
		state.isHanging = false;
		state.isJumping = false;
		state.isWallJumping = false;
		state.isPulling = true;
		accelTime = 0f;
		rb.velocity = new Vector2(rb.velocity.x, player.data.springForce);
	}
	#endregion

	#region Piton
	void PitonZipMovement()
	{
		rb.gravityScale = 0f;

		pitonZipTimer += Time.fixedDeltaTime;
    	float t = Mathf.Clamp01(pitonZipTimer / pitonZipDuration);

		float smoothT = Mathf.SmoothStep(0f, 1f, t);

		Vector2 nextPosition = Vector2.Lerp(pitonStartPos, grapplePoint, smoothT);
    	rb.MovePosition(nextPosition);

		float distance = Vector2.Distance(transform.position, grapplePoint);

		if (t >= 1f || distance <= 0.6f) 
		{
			OnPitonActivated();
		}
	}

	void OnPitonActivated()
	{
		if (activePiton == null) return;

		state.isHanging = false;
		state.isJumping = false;
		state.isWallJumping = false;
		state.isPulling = true;
		accelTime = 0f;

		if (activePiton.boostForce > 0)
		{
			Vector2 boostDirection = (grapplePoint - pitonStartPos).normalized;
			rb.velocity = boostDirection * activePiton.boostForce;
		}
		else
		{
			rb.velocity = Vector2.zero;
		}
		
		CancelPitonZip();
	}

	void CancelPitonZip()
	{
		state.isPullingToPiton = false;
		activePiton = null;
		rb.gravityScale = player.data.gravityScale;
	}
	#endregion

    #region Events
	void OnEnable()
    {
        player.events.OnXYInput += OnXYInput;
        player.events.OnJumpButtonDown += OnJumpButtonDown;
        player.events.OnJumpButtonUp += OnJumpButtonUp;
		player.events.OnPointerMove += OnPointerMove;
        player.events.OnGrappleButtonDown += OnGrappleButtonDown;
        player.events.OnGrappleButtonUp += OnGrappleButtonUp;
		player.events.OnPullButtonDown += OnPullButtonDown;
		player.events.OnChangeAnchorPoint += OnChangeAnchorPoint;
		player.events.OnDeath += OnDeath;
		player.events.OnRespawn += OnRespawn;
		player.events.OnOrbPickUp += OnOrbPickUp;
		player.events.OnSpringActivated += OnSpringActivated;
    }

    void OnDisable()
    {
        player.events.OnXYInput -= OnXYInput;
        player.events.OnJumpButtonDown -= OnJumpButtonDown;
        player.events.OnJumpButtonUp -= OnJumpButtonUp;
		player.events.OnPointerMove -= OnPointerMove;
        player.events.OnGrappleButtonDown -= OnGrappleButtonDown;
        player.events.OnGrappleButtonUp -= OnGrappleButtonUp;
		player.events.OnPullButtonDown -= OnPullButtonDown;
		player.events.OnChangeAnchorPoint -= OnChangeAnchorPoint;
		player.events.OnDeath -= OnDeath;
		player.events.OnRespawn -= OnRespawn;
		player.events.OnOrbPickUp -= OnOrbPickUp;
		player.events.OnSpringActivated -= OnSpringActivated;
    }
	#endregion
}

#region Player States
public class PlayerStates
{
	public bool onGround;
	public bool2 onWall;
	public bool isFalling;
	public bool isFastFalling;
	public bool isClinging;
	public bool isSliding;
	public bool isJumping;
	public bool2 isWallJumping;
	public bool isGrappled;
	public bool isHanging;
	public bool isPulling;
	public bool isPullingToPiton;
	public bool isFrozen;
	public bool isDead;
}
#endregion