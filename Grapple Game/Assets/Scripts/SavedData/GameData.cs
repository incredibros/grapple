using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int crystal;
    public string currentLevel;

    public float volume = 1.0f;
    public int graphicsQuality = 0;
    public int resolution = 0;
    public bool isFullscreen = false;
}