using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct PlayerEvents
{
    // This script contains all the ways the player system scripts can talk to each other, using predetermined actions with
    // specific results to let each of the systems know what the player is doing
    // So far, all inputs preformed by the user are listened to by the player input system script, and that script sends out
    // signal using these actions to each of the other system scripts that connected a function to that specific action

    // A struct is different from a class where structs cannot be attached to objects. This means there can only ever be one
    // of these struct scripts where classes can have mulitple copies by being placed on multiple objects.
    // Because of this, all scripts will automatically have access to structs, where with classes they need the specific
    // reference to that copy of the script

    // An Action is a type of variable that stores functions. Because these are public, the player system scripts will add
    // specific functions of their own to these different actions
    // The player input system script can call an action and all of the functions will run on the other scripts
    #region Movement
    public Action<Vector2> OnXYInput;
    public Action OnJumpButtonDown;
    public Action OnJumpButtonUp;
    public Action<Vector2, bool> OnPointerMove;
    #endregion

    #region Grapple
    public Action OnGrappleButtonDown;
    public Action<Vector2> OnGrapple;
    public Action OnGrappleButtonUp;
    public Action OnPullButtonDown;
    public Action OnPull;
    public Action<Vector2, bool> OnChangeAnchorPoint;
    #endregion

    #region Death and Respawn
    public Action OnDeath;
    public Action OnRespawn;
    #endregion

    #region Extras
    public Action<GameObject> OnOrbPickUp;
    #endregion
}
