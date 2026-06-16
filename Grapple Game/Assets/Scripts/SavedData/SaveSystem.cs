using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    [Header("Save Settings")]
    [SerializeField] string fileName = "savefile.json";
    [SerializeField] bool useEncryption = true;
    [SerializeField] string encryptionKey = "SecretKey";

    string saveFilePath;
    
    // Keeps a running cache of the game data in memory
    public GameData Data { get; private set; } = new GameData();

    void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, fileName);
        LoadGame();
    }

    // Saves gameplay progression data.
    public void SaveProgression(int crystalAmount, string levelString)
    {
        Data.crystal = crystalAmount;
        Data.currentLevel = levelString;
        WriteToFile();
    }

    // Saves the audio and visual configuration states.
    public void SaveSettings(float volume, int graphicsQuality, bool isFullscreen, int resolution)
    {
        Data.volume = volume;
        Data.graphicsQuality = graphicsQuality;
        Data.isFullscreen = isFullscreen;
        Data.resolution = resolution;
        WriteToFile();
    }

    /// Loads the file from disk or instantiates a fresh default template.
    public GameData LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("No save file found. Creating default template.");
            Data = new GameData();
            return Data;
        }

        try
        {
            string jsonString = File.ReadAllText(saveFilePath);

            if (useEncryption)
                jsonString = EncryptDecrypt(jsonString);

            Data = JsonUtility.FromJson<GameData>(jsonString);
            return Data;
        }
        catch (Exception e)
        {
            Debug.LogError($"Save file corrupted. Resetting data. Error: {e.Message}");
            Data = new GameData();
            return Data;
        }
    }

    private void WriteToFile()
    {
        try
        {
            string jsonString = JsonUtility.ToJson(Data, true);

            if (useEncryption)
                jsonString = EncryptDecrypt(jsonString);

            File.WriteAllText(saveFilePath, jsonString);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write game data to disk: {e.Message}");
        }
    }

    string EncryptDecrypt(string data)
    {
        string result = "";
        for (int i = 0; i < data.Length; i++)
        {
            result += (char)(data[i] ^ encryptionKey[i % encryptionKey.Length]);
        }
        return result;
    }

    [ContextMenu("Open Save Folder")]
    public void OpenSaveFolder()
    {
        System.Diagnostics.Process.Start(Application.persistentDataPath);
    }
}