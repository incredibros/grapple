using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject targetToKeepActive;
    [HideInInspector] public static bool GameIsPaused = false;

    [SerializeField] TextMeshProUGUI progressText;
    [SerializeField] Slider loadingSlider;

    void Start()
    {
        Deactivate();
        GameIsPaused = true;
    }

    void Deactivate()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject == targetToKeepActive)
            {
                child.gameObject.SetActive(true);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    IEnumerator LoadAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("Game");
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);

            //loadingSlider.value = progressValue;
            loadingSlider.value = 0;

            //progressText.text = Mathf.RoundToInt(progressValue * 100f) + "%";
            progressText.text = "0%";

            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
            }
            
            yield return null;
        }
    }

    #region Main Menu
    public void PlayGame()
    {
        GameIsPaused = false;
        StartCoroutine(LoadAsync());
    }

    public void QuitGame()
    {
        GameIsPaused = false;
        Debug.Log("Quitting game...");
        Application.Quit();
    }
    #endregion
}
