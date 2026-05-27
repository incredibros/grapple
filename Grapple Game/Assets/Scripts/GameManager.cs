using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject[] persistentObjects;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            MakePersistentObjects();
        }
        else
        {
            Instance.CleanUpSceneDuplicates(this.persistentObjects);
            Destroy(gameObject);
        }
    }

    void MakePersistentObjects()
    {
        foreach (GameObject obj in persistentObjects)
        {
            if (obj != null)
            {
                DontDestroyOnLoad(obj);
            }
        }
    }

    void CleanUpSceneDuplicates(GameObject[] duplicatesInScene)
    {
        foreach (GameObject duplicatePrefabOrObj in duplicatesInScene)
        {
            if (duplicatePrefabOrObj != null)
            {
                GameObject duplicateInScene = GameObject.Find(duplicatePrefabOrObj.name);
                
                if (duplicateInScene != null && duplicateInScene.scene.name != "DontDestroyOnLoad")
                {
                    Destroy(duplicateInScene);
                }
            }
        }
    }
}
