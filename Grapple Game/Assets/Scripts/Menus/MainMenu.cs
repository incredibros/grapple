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

    [SerializeField] AudioMixer audioMixer;
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] TextMeshProUGUI progressText;
    [SerializeField] Slider loadingSlider;
    Resolution[] resolutions;

    void Start()
    {
        Deactivate();

        SettingsSetUp();
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

    void SettingsSetUp()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
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

    #region Settings
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("volume", volume);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
    #endregion
}
