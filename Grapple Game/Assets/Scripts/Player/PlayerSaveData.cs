using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerSaveData
{
    // This script contains all the info for variables that everyone uses
    // Basically the PlayerEvents but varaibles instead of actions

    public Vector2? LastCheckpoint;
    public float GameTimer;
    public int TotalDeaths;

    public int Gems;

    public bool IsDead;
}
