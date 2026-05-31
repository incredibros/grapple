using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct PlayerSaveData
{
    // This script contains all the info for variables that everyone uses
    // Basically the PlayerEvents but varaibles instead of actions

    public Vector2? LastCheckpoint;
    public float GameTimer;
    public int TotalDeaths;

    public int Crystals;

    public bool IsDead;
    public bool OnGround;
    public bool IsJumping;
    public bool IsFalling;
    public bool IsHanging;
    public bool IsPulling;
    public bool2 OnWall;
}
