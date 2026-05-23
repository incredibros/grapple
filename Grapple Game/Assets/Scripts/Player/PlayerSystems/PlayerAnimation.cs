using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : PlayerSystem
{
    // This system script controls all animation of the player, including gun movement and flipping sprites

    [SerializeField] Transform body;
    [SerializeField] Transform gun;

    Vector2 moveInput;
    Vector2 mouseDirection;

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
            PlayerMovement.CursorMode = !PlayerMovement.CursorMode;
        }
    }

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
        if (!PlayerMovement.CursorMode)
		{
			mouseDirection = input;
		}

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
		else if (PlayerMovement.CursorMode)
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