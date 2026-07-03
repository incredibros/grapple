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
	Timers timer = new Timers();

	Vector2 moveInput;
	Vector2 mouseDirection;

	[HideInInspector] public Vector2 grapplePoint;
	[HideInInspector] public float grappleRadius;

	int grapples;
	Grappleable grappledObject;

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

	void FixedUpdate()
	{
		if (state.isDead || state.isFrozen || MainMenu.GameIsPaused) return;

		Timers();
		LateralMovement();
		Gravity();
	}
	
	void Update()
	{
		if (state.isDead || state.isFrozen || MainMenu.GameIsPaused) return;

		Checks();

		if (timer.jumpBuffer > 0 && timer.jumpDelay <= 0 && !state.isJumping)
		 	OnJumpInput();

		if (state.isGrappled)
			OnGrapple();
	}

	#region Timers
	void Timers()
	{
		// coyote keeps state active for a time after leaving it
		// buffer keeps input active for a time after pressing it
		// delay prevents action for a time after activating it
		// time is just a normal timer for animation purposes
		
		timer.coyote--;
		timer.wallCoyote[0]--;
		timer.wallCoyote[1]--;
		timer.jumpBuffer--;
		timer.lateralBuffer[0]--;
		timer.lateralBuffer[1]--;
		timer.jumpDelay--;
		timer.grappleReleaseDelay--;
		timer.accelTime += Time.fixedDeltaTime;
	}

	void ResetTimers(bool coyotes = true, bool buffers = true, bool delays = true, bool times = true)
	{
		if (coyotes)
		{
			timer.coyote = 0;
			timer.wallCoyote[0] = 0;
			timer.wallCoyote[1] = 0;
		}
		if (buffers)
		{
			timer.jumpBuffer = 0;
			timer.lateralBuffer[0] = 0;
			timer.lateralBuffer[1] = 0;
		}
		if (delays)
		{
			timer.jumpDelay = 0;
			timer.grappleReleaseDelay = 0;
		}
		if (times)
		{
			timer.accelTime = 0f;
		}
	}
	#endregion

	#region Checks
	void Checks()
	{
		if (Physics2D.OverlapBox(player.data.groundCheckPoint + (Vector2) transform.position, player.data.groundCheckSize, 0f, player.data.groundLayer) && Mathf.Abs(rb.velocity.y) <= 0.001f)
		{
			timer.coyote = player.data.coyoteTime;
			state.onGround = player.tempData.OnGround = true;
			if (grapples == 0 && !state.isGrappled)
				grapples++;
			
			RaycastHit2D hit;
			if(hit = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, player.data.crumblingPlatformLayer))
			{
				CrumblingPlatform crumblingPlatform = hit.collider.GetComponent<CrumblingPlatform>();
				crumblingPlatform.player = player;
				crumblingPlatform.ActivateCrumbling();
			}
		}
		else
			state.onGround = player.tempData.OnGround = false;
		
		if (rb.velocity.y < -0.001f && !state.isHanging)
		{
			state.isFalling = true;
			if (moveInput.y == -1)
			 	state.isFastFalling = true;
		}
		else
		{
			state.isFalling = false;
			state.isFastFalling = false;
		}

		if (moveInput.x <= -1)
			timer.lateralBuffer[0] = player.data.lateralBufferTime;
		if (moveInput.x >= 1)
			timer.lateralBuffer[1] = player.data.lateralBufferTime;
		
		state.onWall = new bool2(CheckForWalls(-1), CheckForWalls(1));
		state.isClinging = (state.onWall[0] && timer.lateralBuffer[0] > 0f) || (state.onWall[1] && timer.lateralBuffer[1] > 0f);

		if (state.onWall[0])
			timer.wallCoyote[0] = player.data.wallCoyoteTime;
		if (state.onWall[1])
			timer.wallCoyote[1] = player.data.wallCoyoteTime;
		
		if (state.isClinging && state.isFalling && !state.isHanging)
		{
			if (!state.isSliding)
				rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -player.data.maxWallSlideSpeed));
			state.isSliding = true;
		}
		else
			state.isSliding = false;
	}

	bool CheckForWalls(int dir)
	{
		//if (Mathf.Abs(rb.velocity.x) > 0.001f) return false;
		
		int wallsDetected = 0;
		bool2 outerWalls = false;
		int index = 0;
		foreach (Vector2 pos in dir == 1 ? player.data.rightCheckPoints : player.data.leftCheckPoints)
		{
			bool detected = Physics2D.OverlapBox(pos + (Vector2) transform.position, player.data.wallCheckSize, 0f, player.data.wallLayer);
			wallsDetected += detected ? 1 : 0;
			outerWalls = new bool2(index == 0 && detected || outerWalls.x, index == 3 && detected || outerWalls.y);
			index++;
		}
		return wallsDetected >= 3 || outerWalls.Equals(true);
	}
	#endregion

	#region Lateral Movement
	void OnXYInput(Vector2 input)
    {
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

		if ((state.isWallJumping.x || state.isWallJumping.y) && timer.accelTime >= player.data.accels[3].time)
        	state.isWallJumping = false;
		if (state.isPulling && timer.accelTime >= player.data.accels[4].time)
			state.isPulling = false;
		
		int type = state.isHanging ? 2 : !state.isWallJumping.Equals(false) ? 3 : state.isPulling ? 4 : state.onGround ? 0 : 1;
		float value = accel ? player.data.accels[type].accel : player.data.accels[type].decel;
		if (!player.data.accels[type].constant)
			value *= accel ? player.data.accels[type].accelCurve.Evaluate(timer.accelTime / player.data.accels[type].time)
				: player.data.accels[type].decelCurve.Evaluate(timer.accelTime / player.data.accels[type].time);
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
			state.isJumping = false;
		}
		else
			rb.gravityScale = player.data.gravityScale;
	}
	#endregion

    #region Jump
	void OnJumpButtonDown()
    {
        timer.jumpBuffer = player.data.jumpBufferTime;
    }

	void OnJumpInput()
	{
		if (timer.coyote > 0)
			OnJump(0);
		else if (timer.wallCoyote[0] > 0 || timer.wallCoyote[1] > 0)
			OnJump(timer.wallCoyote[0] >= timer.wallCoyote[1] ? -1 : 1);
	}

	void OnJump(int dir)
	{
		if (dir == 0)
			rb.velocity = new Vector2(rb.velocity.x, player.data.jumpForce);
		else
			rb.velocity = new Vector2(player.data.wallJumpForce.x * -dir, player.data.wallJumpForce.y);

		ResetTimers(true, true, false, false);
		timer.jumpDelay = player.data.jumpDelayTime;
		timer.jumpBuffer = 0;
		
		state.isJumping = true;
		state.isWallJumping = new bool2(dir == -1, dir == 1);
		
		timer.accelTime = dir != 0 ? 0f : timer.accelTime;
		state.isPulling = dir != 0 ? false : state.isPulling;
	}

	void OnJumpButtonUp()
	{
		if (state.isJumping && !state.isFalling)
		 	rb.AddForce(rb.velocity.y * (1 - player.data.jumpCutMultiplier) * Vector2.down, ForceMode2D.Impulse); 
		state.isJumping = false;
	}
	#endregion

	#region Grapple
	void OnPointerMove(Vector2 pos, bool directional)
	{
		if (pos != null)
		{
			if (directional)
			{
				if (pos.sqrMagnitude < 0.01f) return;
				mouseDirection = pos.normalized;
			}
			else
			{
				Vector2 mousePos = Camera.main.ScreenToWorldPoint(pos);
				mouseDirection = (mousePos - (Vector2) transform.position).normalized;
			}
		}
	}

	void OnGrappleButtonDown()
	{
		if (grapples == 0)
			return;
		grapples--;
		
		Vector2 nudge = mouseDirection * -0.01f;
		RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, mouseDirection, player.data.grappleRange, player.data.grappleLayers);
		
		int hitIndex = -1;
		Vector2 hitPoint = Vector2.zero;
		grappledObject = null;
		for (int i = 0; i < hits.Length; i++)
		{
			grappledObject = hits[i].collider.transform.GetComponent<Grappleable>();
			
			if (grappledObject.type == GrappleableTypes.SemiSolid && mouseDirection.y >= 0)
				continue;
			
			hitIndex = i;
			hitPoint = hits[i].point;
			break;

			/* int layer = hits[i].collider.transform.gameObject.layer;

			if (OnLayer(layer, player.data.nonGrappleLayer))
			{
				grappledObject = GrappleableObjects.NonGrappleable;
				hitIndex = i;
				break;
			}
			else if (OnLayer(layer, player.data.platformLayer))
			{
				grappledObject = GrappleableObjects.Platform;
				hitIndex = i;
				break;
			}
			else if (OnLayer(layer, player.data.semiSolidLayer))
			{
				if (mouseDirection.y >= 0)
					continue;
				else
				{
					grappledObject = GrappleableObjects.SemiSolid;
					hitIndex = i;
					break;
				}
			}
			else
			{
				int parentLayer = hits[i].collider.transform.parent.gameObject.layer;
				
				if (OnLayer(parentLayer, player.data.crumblingPlatformLayer))
					grappledObject = GrappleableObjects.CrumblingPlatform;
				else if (OnLayer(parentLayer, player.data.pitonLayer))
					grappledObject = GrappleableObjects.Piton;
				else if (OnLayer(parentLayer, player.data.flingerLayer))
					grappledObject = GrappleableObjects.Flinger;
				else
					grappledObject = GrappleableObjects.Peg;
				
				hitIndex = i;
				break;
			} */
		}
		
		if (grappledObject == null)
		{
			// Shoot grapple and retract
			return;
		}

		if (grappledObject.type == GrappleableTypes.CrumblingPlatform)
		{
			CrumblingPlatform crumblingPlatform = hits[hitIndex].collider.GetComponentInParent<CrumblingPlatform>();
			crumblingPlatform.player = player;
			crumblingPlatform.ActivateCrumbling();
		}
		
		if (grappledObject.clampGrapple)
		{
			Vector2 center = grappledObject.transform.position;
			Vector2 min = grappledObject.minGrappleBounds;
			Vector2 max = grappledObject.maxGrappleBounds;
			hitPoint = new Vector2(
				Mathf.Clamp(hitPoint.x, center.x + min.x, center.x + max.x),
				Mathf.Clamp(hitPoint.y, center.y + min.y, center.y + min.y)
			);
		}

		state.isGrappled = true;
		joint.enabled = true;
		timer.grappleReleaseDelay = player.data.releaseDelayTime;

		joint.connectedAnchor = grapplePoint = nudge + hitPoint;
		joint.distance = grappleRadius = Vector2.Distance(transform.position, grapplePoint);

		player.events.OnGrapple?.Invoke(grapplePoint);
	}

	/* bool OnLayer(int layer, LayerMask comparedLayer)
	{
		return (comparedLayer.value & (1 << layer)) != 0;
	} */

	void OnGrapple()
	{
		if (Vector2.Distance(transform.position, grapplePoint) >= grappleRadius - 0.1f && !state.onGround)
			state.isHanging = true;
		else
			state.isHanging = false;
	}

	void OnGrappleButtonUp()
	{
		if (timer.grappleReleaseDelay > 0) return;
			
		grappledObject = null;
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

	#region Pull/Reel
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
		timer.accelTime = 0f;
		timer.grappleReleaseDelay = 0;

		
		
		if (grappledObject.pullType == PullTypes.Boost)
		{
			state.isPulling = true;
		}
		else if (grappledObject.pullType == PullTypes.Reel)
		{
			state.isReeling = true;
		}

		StartCoroutine(MovementForPulling());
		player.events.OnPull?.Invoke();
		
		/*if (activePiton != null)
		{
			state.isReeling = true;
			pitonStartPos = transform.position;
        	pitonZipTimer = 0f;
			float distance = Vector2.Distance(pitonStartPos, grapplePoint);
			pitonZipDuration = distance > 0.01f ? distance / activePiton.zipSpeed : 0.1f;
		}
		else
		{
			
		}*/
	}

	IEnumerator MovementForPulling()
	{
		state.isFrozen = true;
		
		state.isGrappled = false;
		state.isHanging = false;
		joint.enabled = false;
		
		ResetTimers(true, true, true, false);

		rb.gravityScale = 0;

		float freezeTimer = 0f;
		
		if (grappledObject.pullType == PullTypes.Boost)
		{
			rb.velocity = (grapplePoint - (Vector2) transform.position).normalized * player.data.pullSpeed;
			Vector2 startingVelocity = rb.velocity;
			
			while (freezeTimer < player.data.pullFreezeDuration)
			{
				rb.velocity = startingVelocity * player.data.pullFreezeVelocity.Evaluate(freezeTimer / player.data.pullFreezeDuration);
				freezeTimer += Time.deltaTime;
				yield return null;
			}

			rb.velocity = startingVelocity;
		}
		else if (grappledObject.pullType == PullTypes.Reel)
		{
			Vector2 startingVelocity = rb.velocity;
			float distance = Vector2.Distance(transform.position, grapplePoint);
			while (distance > rb.velocity.magnitude * Time.deltaTime)
			{
				Vector2 reelVelocity = (grapplePoint - (Vector2) transform.position).normalized * player.data.reelSpeed;
				if (freezeTimer <= player.data.reelVelocityChangeDuration)
					rb.velocity = Vector2.Lerp(startingVelocity, reelVelocity, player.data.reelVelocityChangeLerp.Evaluate(freezeTimer / player.data.reelVelocityChangeDuration));
				else
					rb.velocity = reelVelocity;
				distance = Vector2.Distance(transform.position, grapplePoint);
				freezeTimer += Time.deltaTime;
				yield return null;
			}

			transform.position = grapplePoint;

			if (grappledObject.boostDirection == Vector2.zero)
			{
				rb.velocity = Vector2.zero;
				freezeTimer = 0f;

				while (freezeTimer < player.data.hangTime)
				{
					freezeTimer += Time.deltaTime;
					yield return null;
				}

				grapples++;
				state.isReeling = false;
			}
			else
				rb.velocity = grappledObject.boostDirection * player.data.flingerLaunchForce;

			grapples++;
			state.isReeling = false;
		}

		grappledObject = null;
		state.isFrozen = false;
	}

	void OnPullButtonUp()
	{
		StopAllCoroutines();
		state.isReeling = false;
		state.isFrozen = false;
		grappledObject = null;
	}
	#endregion

	#region Death
	void OnDeath()
	{
		state.isDead = player.tempData.IsDead = true;

		ResetTimers();
		rb.gravityScale = 0;

		StopAllCoroutines();

		if (state.isGrappled)
		{
			OnGrappleButtonUp();
		}

		player.tempData.TotalDeaths++;

		rb.bodyType = RigidbodyType2D.Static;
	}

	void OnRespawn()
	{
		state.isDead = player.tempData.IsDead = false;
		state.isFrozen = false;

		rb.bodyType = RigidbodyType2D.Dynamic;
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

	#region Crystal
	void OnCrystalPickUp(GameObject crystal)
	{
		crystal.GetComponent<Crystal>().OnCrystalPickUp(this.gameObject);
	}
	#endregion

	#region Spring
	void OnSpringActivated()
	{
		state.isHanging = false;
		state.isJumping = false;
		state.isWallJumping = false;
		state.isPulling = false;
		timer.accelTime = 0f;
		rb.velocity = new Vector2(rb.velocity.x, player.data.springForce);
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
		player.events.OnPullButtonUp += OnPullButtonUp;
		player.events.OnChangeAnchorPoint += OnChangeAnchorPoint;
		player.events.OnDeath += OnDeath;
		player.events.OnRespawn += OnRespawn;
		player.events.OnOrbPickUp += OnOrbPickUp;
		player.events.OnCrystalPickUp += OnCrystalPickUp;
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
		player.events.OnPullButtonUp -= OnPullButtonUp;
		player.events.OnChangeAnchorPoint -= OnChangeAnchorPoint;
		player.events.OnDeath -= OnDeath;
		player.events.OnRespawn -= OnRespawn;
		player.events.OnOrbPickUp -= OnOrbPickUp;
		player.events.OnCrystalPickUp -= OnCrystalPickUp;
		player.events.OnSpringActivated -= OnSpringActivated;
    }
	#endregion
}

[System.Serializable]
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
	public bool isReeling;
	public bool isFrozen;
	public bool isDead;
}

public class Timers
{
	public int coyote;
	public int2 wallCoyote;
	public int jumpBuffer;
	public int2 lateralBuffer;
	public int jumpDelay;
	public int grappleReleaseDelay;
	public float accelTime;
}