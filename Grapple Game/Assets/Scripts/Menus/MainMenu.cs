using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] SettingsMenu settingsMenu;
    [SerializeField] CreditsMenu creditsMenu;
    [HideInInspector] public static bool GameIsPaused;

    UIDocument mainMenuDocument;
    VisualElement mainMenuVE;

    Button playButton;
    Button optionsButton;
    Button creditsButton;
    Button quitButton;

    void Awake()
    {
        GameIsPaused = true;

        mainMenuDocument = GetComponent<UIDocument>();
    }

    void Start()
    {
        LoadScreen();
    }

    void FindElements()
    {
        mainMenuVE = mainMenuDocument.rootVisualElement;
        playButton = mainMenuVE.Q<Button>("Play");
        optionsButton = mainMenuVE.Q<Button>("Options");
        creditsButton = mainMenuVE.Q<Button>("Credits");
        quitButton = mainMenuVE.Q<Button>("Quit");
    }

    public void LoadScreen()
    {
        mainMenuVE.style.display = DisplayStyle.Flex;
        playButton.Focus();
    }

    void LoadAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("Game");
        /*operation.allowSceneActivation = false;

        loadingSlider.minValue = 0f;
        loadingSlider.maxValue = 1f;
        loadingSlider.value = 0f;
        progressText.text = "0%";

        yield return null;

        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);

            loadingSlider.value = progressValue;

            progressText.text = Mathf.RoundToInt(progressValue * 100f) + "%";

            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
            }
            
            yield return null;
        }*/
    }

    #region Event Handlers
    void PlayGame()
    {
        GameIsPaused = false;
        LoadAsync();
    }

    void OptionsGame()
    {
        mainMenuVE.style.display = DisplayStyle.None;
        settingsMenu.LoadScreen();
    }

    void CreditsGame()
    {
        mainMenuVE.style.display = DisplayStyle.None;
        creditsMenu.LoadScreen();
    }

    void QuitGame()
    {
        GameIsPaused = false;
        Debug.Log("Quit");
        Application.Quit();
    }
    #endregion

    #region Events
    void OnEnable()
    {
        FindElements();
        
        playButton.clicked += PlayGame;
        optionsButton.clicked += OptionsGame;
        creditsButton.clicked += CreditsGame;
        quitButton.clicked += QuitGame;
    }

    void OnDisable()
    {
        playButton.clicked -= PlayGame;
        optionsButton.clicked -= OptionsGame;
        creditsButton.clicked -= CreditsGame;
        quitButton.clicked -= QuitGame;
    }
    #endregion
}
