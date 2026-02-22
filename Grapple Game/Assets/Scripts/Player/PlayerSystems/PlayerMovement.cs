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
	Rope ropeScript;

	PlayerStates state = new PlayerStates();
	Vector2 moveInput;

	float coyote;
	float2 wallCoyote;
	float buffer;
	float jumpDelay;

	[HideInInspector] public Vector2 grapplePoint;
	[HideInInspector] public float grappleRadius;
	[HideInInspector] public bool inGrappleAccel;

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
		if (state.isDead || state.isPulling)
			{ return; }
		
		#region Timers
		coyote -= Time.deltaTime;
		wallCoyote[0] -= Time.deltaTime;
		wallCoyote[1] -= Time.deltaTime;
		buffer -= Time.deltaTime;
		jumpDelay -= Time.deltaTime;
		#endregion

		#region Checks
		if (Physics2D.OverlapBox(player.data.groundCheckPoint + (Vector2) transform.position, player.data.groundCheckSize, 0f, player.data.groundLayer) && Mathf.Abs(rb.velocity.y) <= 0.001f)
		{
			coyote = player.data.coyoteTime;
			state.onGround = true;
			inGrappleAccel = false;
		}
		else
			{ state.onGround = false; }
		
		if (rb.velocity.y < -0.001f && !state.isHanging)
		{
			state.isFalling = true;
			if (moveInput.y == -1)
				{ state.isFastFalling = true; }
			// Keep here for now
			state.isJumping = false;
			state.isWallJumping = false;
		}
		else
		{
			state.isFalling = false;
			state.isFastFalling = false;
		}
		
		if (moveInput.x == -Mathf.Sign(rb.velocity.x))
			{ inGrappleAccel = false; }

		state.onWall = new bool2(Physics2D.OverlapBox(player.data.leftCheckPoint + (Vector2) transform.position, player.data.wallCheckSize, 0f, player.data.wallLayer) && Mathf.Abs(rb.velocity.x) <= 0.001f,
			Physics2D.OverlapBox(player.data.rightCheckPoint + (Vector2) transform.position, player.data.wallCheckSize, 0f, player.data.wallLayer) && Mathf.Abs(rb.velocity.x) <= 0.001f);
		state.isClinging = (state.onWall[0] && moveInput.x == -1) || (state.onWall[1] && moveInput.x == 1);

		if (state.onWall[0])
			{ wallCoyote[0] = player.data.wallCoyoteTime; }
		if (state.onWall[1])
			{ wallCoyote[1] = player.data.wallCoyoteTime; }
		
		if (state.isClinging && state.isFalling && !state.isHanging)
		{
			if (!state.isSliding)
				{ rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -player.data.maxWallSlideSpeed)); }
			state.isSliding = true;
		}
		else
			{ state.isSliding = false; }
		#endregion

		#region Jump
		if (buffer > 0f && jumpDelay <= 0f && !state.isJumping)
		{
			if (coyote > 0f)
				{ OnJump(0); }
			else if (wallCoyote[0] > 0f || wallCoyote[1] > 0f)
				{ OnJump(wallCoyote[0] > wallCoyote[1] ? -1 : 1); }
		}
		#endregion
		
		#region Grapple
		if (state.isGrappled)
			{ OnGrapple(); }
		#endregion
	}

	void FixedUpdate()
	{
		if (state.isDead || state.isPulling)
			{ return; }
		
		#region Run
		if ((state.isWallJumping[0] && moveInput.x == -1) || (state.isWallJumping[1] && moveInput.x == 1))
			{ moveInput.x = 0; }

		float targetSpeed = moveInput.x * player.data.moveSpeed;
		float speedDif = targetSpeed - rb.velocity.x;
		float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? (!inGrappleAccel ? player.data.acceleration : player.data.swingAcceleration) : (state.isWallJumping.Equals(false) ? (!inGrappleAccel ? player.data.decceleration : player.data.swingDecceleration) : player.data.wallJumpDecceleration);
		float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, player.data.velPower) * Mathf.Sign(speedDif);
		rb.AddForce(movement * Vector2.right);
		#endregion

		#region Friction
		if (state.onGround && Mathf.Abs(moveInput.x) == 0f)
		{
			float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), player.data.frictionAmount) * Mathf.Sign(rb.velocity.x);
			rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
		}
		#endregion

		#region Gravity
		if (state.isFalling && !state.isHanging)
		{
			rb.gravityScale = player.data.gravityScale * (!state.isSliding ? player.data.fallGravityMultiplier : player.data.wallSlideGravityMultiplier);
			rb.gravityScale *= state.isFastFalling ? player.data.fastFallMultiplier : 1;
			rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, !state.isSliding ? -player.data.maxFallSpeed : -player.data.maxWallSlideSpeed));
		}
		else
			{ rb.gravityScale = player.data.gravityScale; }
		#endregion
	}

	void OnXYInput(Vector2 input)
    {
        moveInput = input;
    }

    void OnJumpButtonDown()
    {
        buffer = player.data.bufferTime;
    }

	void OnJump(int dir)
	{
		if (dir == 0)
			{ rb.velocity = new Vector2(rb.velocity.x, player.data.jumpForce); }
		else
			{ rb.velocity = new Vector2(player.data.wallJumpForce.x * -dir, player.data.wallJumpForce.y); }

		coyote = 0f;
		wallCoyote[0] = 0f;
		wallCoyote[1] = 0f;
		buffer = 0f;
		jumpDelay = player.data.jumpDelayTime;
		inGrappleAccel = false;
		
		state.isJumping = true;
		state.isWallJumping = new bool2(dir == -1, dir == 1);
	}

	void OnJumpButtonUp()
	{
		if (state.isJumping)
			{ rb.AddForce(rb.velocity.y * (1 - player.data.jumpCutMultiplier) * Vector2.down, ForceMode2D.Impulse); }
		state.isJumping = false;
		//state.isWallJumping = false;
		buffer = 0f;
	}

	void OnGrappleButtonDown()
	{
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 direction = (mousePos - (Vector2) transform.position).normalized;
		RaycastHit2D raycast = Physics2D.Raycast(transform.position, direction, player.data.grappleRange, player.data.grappleLayers);
		
		if (raycast.collider == null)
			{ return; }
			
		state.isGrappled = true;
		joint.enabled = true;
		TargetPosition hitCollider = raycast.collider.gameObject.GetComponentInParent<TargetPosition>();
		grapplePoint = new Vector2(
			Mathf.Clamp(raycast.point.x, hitCollider.gameObject.transform.position.x + hitCollider.minGrappleBounds.x, hitCollider.gameObject.transform.position.x + hitCollider.maxGrappleBounds.x),
			Mathf.Clamp(raycast.point.y, hitCollider.gameObject.transform.position.y + hitCollider.minGrappleBounds.y, hitCollider.gameObject.transform.position.y + hitCollider.maxGrappleBounds.y));
		joint.connectedAnchor = grapplePoint;
		grappleRadius = Vector2.Distance(transform.position, grapplePoint);
		joint.distance = grappleRadius;

		player.events.OnGrapple?.Invoke(grapplePoint);
	}

	void OnGrapple()
	{
		if (Vector2.Distance(transform.position, grapplePoint) >= grappleRadius - 0.1f && !state.onGround)
		{
			inGrappleAccel = true;
			state.isHanging = true;
		}
		else
			{ state.isHanging = false; }
	}

	void OnGrappleButtonUp()
	{
		state.isGrappled = false;
		state.isHanging = false;
		joint.enabled = false;
	}

	void OnPullButtonDown()
	{
		if (!state.isGrappled)
			{ return; }
		
		state.isHanging = false;
		state.isJumping = false;
		state.isWallJumping = false;
		inGrappleAccel = true;
		rb.velocity = (grapplePoint - (Vector2) transform.position).normalized * Mathf.Max(player.data.minPullSpeed, rb.velocity.magnitude);
		OnGrappleButtonUp();
		StartCoroutine(PullStopMovement());
	}

	IEnumerator PullStopMovement()
	{
		state.isPulling = true;
		
		buffer = 0.0f;
		coyote = 0.0f;
		wallCoyote[0] = 0.0f;
		wallCoyote[1] = 0.0f;
		jumpDelay = 0.0f;

		rb.gravityScale = 0;

		yield return new WaitForSeconds(player.data.pullDuration);

		state.isPulling = false;
	}

	void OnDeath()
	{
		state.isDead = true;

		buffer = 0.0f;
		coyote = 0.0f;
		wallCoyote[0] = 0.0f;
		wallCoyote[1] = 0.0f;
		jumpDelay = 0.0f;

		rb.velocity = Vector2.zero;
		rb.gravityScale = 0;

		StopAllCoroutines();

		if (state.isGrappled)
			{ OnGrappleButtonUp(); }
	}

	void OnRespawn()
	{
		state.isDead = false;
	}

    void OnEnable()
    {
        player.events.OnXYInput += OnXYInput;
        player.events.OnJumpButtonDown += OnJumpButtonDown;
        player.events.OnJumpButtonUp += OnJumpButtonUp;
        player.events.OnGrappleButtonDown += OnGrappleButtonDown;
        player.events.OnGrappleButtonUp += OnGrappleButtonUp;
        player.events.OnPullButtonDown += OnPullButtonDown;
		player.events.OnDeath += OnDeath;
		player.events.OnRespawn += OnRespawn;
    }

    void OnDisable()
    {
        player.events.OnXYInput -= OnXYInput;
        player.events.OnJumpButtonDown -= OnJumpButtonDown;
        player.events.OnJumpButtonUp -= OnJumpButtonUp;
        player.events.OnGrappleButtonDown -= OnGrappleButtonDown;
        player.events.OnGrappleButtonUp -= OnGrappleButtonUp;
        player.events.OnPullButtonDown -= OnPullButtonDown;
		player.events.OnDeath -= OnDeath;
		player.events.OnRespawn -= OnRespawn;
    }
}

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
	public bool isDead;
}
