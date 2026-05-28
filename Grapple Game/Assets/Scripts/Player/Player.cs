using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // This is the main script that all other player scripts access to talk to the other player scripts
    // Every player system script will have access to this
    // On here are access to player data, containing all the info to control the player to change and tweak in the editor,
    // and player events, containing all the ways the player system scripts can talk to each other

    // Static Configuration Assets (Scriptable Object)
    public PlayerData data;
    // Dynamic, Temporary Game State (Lives in RAM)
    public PlayerSaveData saveData;
    //Communications (Actions/Events)
    public PlayerEvents events;
}
