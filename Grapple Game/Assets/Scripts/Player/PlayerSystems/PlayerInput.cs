using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : PlayerSystem
{
    // This system script listens to all user inputs and calls actions based on the inputs
	
	bool canMove = true;
	
	/*
	#region Input Manager (Old)
	void Update()
	{
		if (!canMove || MainMenu.GameIsPaused)
			{ return; }
		
		// The question mark after each action makes sure there is as least one function connected to the action so that
		// it doesn't try to call an action without functions in it
		Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        player.events.OnXYInput?.Invoke(moveInput);

		if (Input.GetButtonDown("Jump"))
			{ player.events.OnJumpButtonDown?.Invoke(); }
		
		if (Input.GetButtonUp("Jump"))
			{ player.events.OnJumpButtonUp?.Invoke(); }
		
		if (Input.GetMouseButtonDown(0))
			{ player.events.OnGrappleButtonDown?.Invoke(); }

        if (Input.GetMouseButtonUp(0))
			{ player.events.OnGrappleButtonUp?.Invoke(); }

        if (Input.GetMouseButtonDown(1))
			{ player.events.OnPullButtonDown?.Invoke(); }
	}
	#endregion
	*/

	#region Input System (New)
	public void OnMove(InputAction.CallbackContext context)
	{
		if (!canMove || MainMenu.GameIsPaused)
			return;
		
		Vector2 moveInput = context.ReadValue<Vector2>().normalized;
		player.events.OnXYInput?.Invoke(moveInput);
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		if (!canMove || MainMenu.GameIsPaused)
			return;
		
		if (context.performed)
		{
			player.events.OnJumpButtonDown?.Invoke();
		}
		
		if (context.canceled)
		{
			player.events.OnJumpButtonUp?.Invoke();
		}
	}

	public void OnGrapple(InputAction.CallbackContext context)
	{
		if (!canMove || MainMenu.GameIsPaused)
			return;
		
		if (context.performed)
		{
			player.events.OnGrappleButtonDown?.Invoke();
		}
		
		if (context.canceled)
		{
			player.events.OnGrappleButtonUp?.Invoke();
		}
	}

	public void OnPull(InputAction.CallbackContext context)
	{
		if (!canMove || MainMenu.GameIsPaused)
			return;

		if (context.performed)
		{
			player.events.OnPullButtonDown?.Invoke();
		}
	}

	public void OnPointerMove(InputAction.CallbackContext context)
	{
		Vector2 currentPos = context.ReadValue<Vector2>();

		bool isGamepad = context.control.device is Gamepad || context.control.device is Joystick;

        UpdateCursorState(isGamepad);
        player.events.OnPointerMove?.Invoke(currentPos, isGamepad);
	}

	public void OnLookMove(InputAction.CallbackContext context)
	{
		Vector2 currentPos = context.ReadValue<Vector2>();

		bool isGamepad = context.control.device is Gamepad;

		UpdateCursorState(isGamepad);
        player.events.OnLookMove?.Invoke(currentPos, isGamepad);
	}

	void UpdateCursorState(bool isGamepad)
    {
        if (isGamepad)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
	#endregion

	#region Event Handlers
	void OnDeath()
	{
		canMove = false;
		player.events.OnXYInput?.Invoke(Vector2.zero);
		player.events.OnJumpButtonUp?.Invoke();
		player.events.OnGrappleButtonUp?.Invoke();
	}

	void OnRespawn()
	{
		canMove = true;
	}
	#endregion

	#region Events
	void OnEnable()
	{
		player.events.OnDeath += OnDeath;
		player.events.OnRespawn += OnRespawn;
	}

	void OnDisable()
	{
		player.events.OnDeath -= OnDeath;
		player.events.OnRespawn -= OnRespawn;
	}
	#endregion
}
