using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : PlayerSystem
{
    // This system script listens to all user inputs and calls actions based on the inputs
	
	bool canMove = true;
	
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

	void OnDeath()
	{
		canMove = false;
	}

	void OnRespawn()
	{
		canMove = true;
	}

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
}
