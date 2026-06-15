using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayerPrefsUtility
{
    [MenuItem("Tools/PlayerPrefs/Print Saved Values")]
    public static void PrintPlayerPrefs()
    {
        Debug.Log("--- CURRENT PLAYERPREFS ---");
        
        float volume = PlayerPrefs.GetFloat("Volume", -1f);
        int graphics = PlayerPrefs.GetInt("GraphicsQuality", -1);
        int fullscreen = PlayerPrefs.GetInt("Fullscreen", -1);

        Debug.Log($"[Volume]: {(volume == -1f ? "Not Set Yet" : volume.ToString())}");
        Debug.Log($"[GraphicsQuality Index]: {(graphics == -1 ? "Not Set Yet" : graphics.ToString())}");
        Debug.Log($"[Fullscreen]: {(fullscreen == -1 ? "Not Set Yet" : (fullscreen == 1 ? "True" : "False"))}");
        
        Debug.Log("---------------------------");
    }

    [MenuItem("Tools/PlayerPrefs/Clear All (Reset)")]
    public static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("⚠️ All PlayerPrefs have been completely deleted and reset!");
    }
}