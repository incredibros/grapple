using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : PlayerSystem
{
    // This system script controls all animation of the player, including gun movement and flipping sprites
    
    BoxCollider2D boxCollider;

    [SerializeField] Transform body;
    [SerializeField] Transform gun;

    [SerializeField] bool showHitboxes;

    Vector2 moveInput;
    Vector2 mouseDirection;

    [SerializeField] Vector2 grapplePoint;
    [SerializeField] bool isGrappled;

    protected override void Awake()
	{
		base.Awake();
        
        boxCollider = player.GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (MainMenu.GameIsPaused)
            return;

        // Flip player
        if (moveInput.x > 0)
        {
            Flip(body, true);
        }
        else if (moveInput.x < 0)
        {
            Flip(body, false);
        }

        // Flip gun
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

    #region Flip Sprite
    void Flip(Transform sprite, bool isRight)
    {
        Vector3 scale = sprite.localScale;

        scale.x = Mathf.Abs(scale.x) * (isRight ? 1f : -1f);

        sprite.localScale = scale;
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