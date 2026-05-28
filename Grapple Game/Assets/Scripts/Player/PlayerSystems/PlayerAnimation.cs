using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : PlayerSystem
{
    // This system script controls all animation of the player, including gun movement and flipping sprites
    
    BoxCollider2D boxCollider;

    [SerializeField] Transform body;
    [SerializeField] Transform gun;

    [SerializeField] bool showHitboxes = true;

    Vector2 moveInput;
    Vector2 mouseDirection;

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
        float angle = Mathf.Atan2(mouseDirection.y, mouseDirection.x) * Mathf.Rad2Deg;
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

        if (Input.GetKeyDown(KeyCode.H) && Input.GetKeyDown(KeyCode.O) && Input.GetKeyDown(KeyCode.G))
        {
            player.saveData.DirectionalMode = !player.saveData.DirectionalMode;
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

        if (player.saveData.DirectionalMode)
		{
			mouseDirection = input;
		}
    }
    #endregion

    #region Mouse Movement
    void OnPointerMove(Vector2 pos, bool directional)
	{
		if (directional)
		{
			mouseDirection = pos.normalized;
		}
		else if (!player.saveData.DirectionalMode)
		{
			Vector2 mousePos = Camera.main.ScreenToWorldPoint(pos);
			mouseDirection = (mousePos - (Vector2) transform.position).normalized;
		}
	}
    #endregion

    #region Grapple
    void OnChangeAnchorPoint(Vector2 point, bool shorten)
    {
        //Maybe point to last point that was called
    }
    #endregion

    #region Events
    void OnEnable()
    {
        player.events.OnXYInput += OnXYInput;
        player.events.OnPointerMove += OnPointerMove;
        player.events.OnChangeAnchorPoint += OnChangeAnchorPoint;
    }

    void OnDisable()
    {
        player.events.OnXYInput -= OnXYInput;
        player.events.OnPointerMove -= OnPointerMove;
        player.events.OnChangeAnchorPoint -= OnChangeAnchorPoint;
    }
    #endregion
}