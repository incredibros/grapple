using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : PlayerSystem
{
    // This system script controls all animation of the player, including gun movement and flipping sprites
    
    BoxCollider2D boxCollider;
    Animator animator;

    [SerializeField] Transform body;
    [SerializeField] Transform gun;

    [SerializeField] bool showHitboxes;

    Vector2 moveInput;
    Vector2 mouseDirection;

    Vector2 grapplePoint;
    bool isGrappled;

    [SerializeField] float ghostDuplicateDelayTotal;
    float ghostDuplicateDelay;
    float ghostTotalDelay;
    [SerializeField] GameObject ghost;
    bool makeGhost;

    protected override void Awake()
	{
		base.Awake();
        
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (MainMenu.GameIsPaused)
            return;

        SetAnimator();
        FlipPlayer();
        FlipGun();
        SpawnPullGhost();
    }

    void SetAnimator()
    {
        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
    }

    #region Flipping
    void FlipPlayer()
    {
        if (moveInput.x > 0)
        {
            Flip(body, true);
        }
        else if (moveInput.x < 0)
        {
            Flip(body, false);
        }
    }

    void FlipGun()
    {
        float angle;
        Vector2 aimDirection;

        if (isGrappled)
        {
            aimDirection = (grapplePoint - (Vector2)gun.position).normalized;
            angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        }
        else
        {
            aimDirection = mouseDirection;
            angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        }
        
        if (mouseDirection.x > 0)
        {
            gun.rotation = Quaternion.Euler(0f, 0f, angle);
            Flip(gun, true);
        }
        else if (mouseDirection.x < 0)
        {
            gun.rotation = Quaternion.Euler(0f, 0f, angle + 180f);
            Flip(gun, false);
        }
    }

    void Flip(Transform sprite, bool isRight)
    {
        Vector3 scale = sprite.localScale;

        scale.x = Mathf.Abs(scale.x) * (isRight ? 1f : -1f);

        sprite.localScale = scale;
    }
    #endregion

    void SpawnPullGhost()
    {
        if (makeGhost)
        {
            if (ghostTotalDelay > 0)
            {
                ghostTotalDelay -= Time.deltaTime;
            }
            else
            {
                makeGhost = false;
            }

            if (ghostDuplicateDelay > 0)
            {
                ghostDuplicateDelay -= Time.deltaTime;
            }
            else
            {
                GameObject currentGhost = Instantiate(ghost, transform.position, transform.rotation);
                Sprite currentSprite = body.GetComponent<SpriteRenderer>().sprite;
                currentGhost.transform.localScale = transform.localScale;
                currentGhost.GetComponent<SpriteRenderer>().sprite = currentSprite;
                //currentGhost.GetComponent<Ghost>().startingAlpha = 1 - player.data.accels[4].accelTime.Evaluate(ghostTotalDelay);
                ghostDuplicateDelay = ghostDuplicateDelayTotal;
                Destroy(currentGhost, 1f);
            }
        }
    }

    #region Draw Gizmos
    void OnDrawGizmos()
    {
        if (player == null || player.data == null || !showHitboxes) return;

        // Box Collider
        Gizmos.color = Color.green;
        Vector3 bodyCenter = player.transform.TransformPoint(boxCollider.offset);
        Gizmos.DrawWireCube(bodyCenter, boxCollider.size);

        // Ground Check
        Gizmos.color = Color.cyan;
        Vector3 groundPos = player.transform.TransformPoint(player.data.groundCheckPoint);
        Gizmos.DrawWireCube(groundPos, player.data.groundCheckSize);

        // Ceiling Check
        Gizmos.color = Color.yellow;
        Vector3 ceilingPos = player.transform.TransformPoint(player.data.ceilingCheckPoint);
        Gizmos.DrawWireCube(ceilingPos, player.data.ceilingCheckSize);
        
        // Wall Check
        Gizmos.color = Color.red;
        foreach(Vector2 localOffset in player.data.leftCheckPoints)
        {
            Vector3 worldPos = transform.position + new Vector3(localOffset.x, localOffset.y, 0);
            Gizmos.DrawWireCube(worldPos, player.data.wallCheckSize);
        }
        foreach(Vector2 localOffset in player.data.rightCheckPoints)
        {
            Vector3 worldPos = transform.position + new Vector3(localOffset.x, localOffset.y, 0);
            Gizmos.DrawWireCube(worldPos, player.data.wallCheckSize);
        }

        // Gun Barrel Tip
        Gizmos.color = Color.magenta;
        Vector3 gunTipPosition = gun.position + (gun.right * gun.localScale.x);
        Gizmos.DrawWireSphere(gunTipPosition, 0.1f);

        // Gun Aiming
        if (mouseDirection != Vector2.zero)
        {
            Gizmos.color = Color.magenta;
            Vector3 targetPosition = transform.position + (Vector3)(mouseDirection * player.data.grappleRange);
            Gizmos.DrawLine(transform.position, targetPosition);

            Gizmos.DrawWireSphere(targetPosition, 0.1f);
        }
    }
    #endregion

    #region Movement
    void OnXYInput(Vector2 input)
    {
        moveInput = input;
    }
    #endregion

    #region Mouse Movement
    void OnPointerMove(Vector2 pos, bool directional)
	{
		if (directional)
		{
			mouseDirection = pos.normalized;
		}
		else
		{
			Vector2 mousePos = Camera.main.ScreenToWorldPoint(pos);
			mouseDirection = (mousePos - (Vector2) transform.position).normalized;
		}
	}
    #endregion

    #region Grapple
    void OnGrapple(Vector2 hookPoint)
    {
        grapplePoint = hookPoint;
        isGrappled = true;
    }

    void OnGrappleButtonUp()
    {
        isGrappled = false;

    }

    void OnPull()
    {
        isGrappled = false;
        makeGhost = true;
        ghostTotalDelay = player.data.accels[4].time;
    }

    void OnChangeAnchorPoint(Vector2 point, bool shorten)
    {
        grapplePoint = point;
    }
    #endregion

    #region Events
    void OnEnable()
    {
        player.events.OnXYInput += OnXYInput;
        player.events.OnPointerMove += OnPointerMove;
        player.events.OnChangeAnchorPoint += OnChangeAnchorPoint;
        player.events.OnGrapple += OnGrapple;
        player.events.OnGrappleButtonUp += OnGrappleButtonUp;
        player.events.OnPull += OnPull;
    }

    void OnDisable()
    {
        player.events.OnXYInput -= OnXYInput;
        player.events.OnPointerMove -= OnPointerMove;
        player.events.OnChangeAnchorPoint -= OnChangeAnchorPoint;
        player.events.OnGrapple -= OnGrapple;
        player.events.OnGrappleButtonUp -= OnGrappleButtonUp;
        player.events.OnPull -= OnPull;
    }
    #endregion
}