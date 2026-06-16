using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] SettingsMenu settingsMenu;

    Player player;
    UIDocument pauseDocument;
    VisualElement pauseVE;

    Button resumeButton;
    Button restartButton;
    Button optionsButton;
    Button mainMenuButton;

    Label crystalLabel;

    void Awake()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();

        pauseDocument = GetComponent<UIDocument>();

        FindElements();
    }

    void Start()
    {
        pauseVE.style.display = DisplayStyle.None;
    }

    void FindElements()
    {
        pauseVE = pauseDocument.rootVisualElement;
        resumeButton = pauseVE.Q<Button>("Resume");
        restartButton = pauseVE.Q<Button>("Restart");
        optionsButton = pauseVE.Q<Button>("Options");
        mainMenuButton = pauseVE.Q<Button>("MainMenu");
        crystalLabel = pauseVE.Q<Label>("Crystal");
    }

    void Update()
    {
        crystalLabel.text = player.tempData.Crystals.ToString();
    }

    public void LoadScreen()
    {
        pauseVE.style.display = DisplayStyle.Flex;
        MainMenu.GameIsPaused = true;
        Time.timeScale = 0f;

        StartCoroutine(FocusFirstButtonNextFrame());
    }

    #region Event Handlers
    public void OnMenuDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (pauseVE.style.display == DisplayStyle.Flex)
            {
                ResumeGame();
            }
            else
            {
                LoadScreen();
            }
        }
    }

    public void OnCancelDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (pauseVE.style.display == DisplayStyle.Flex)
            {
                ResumeGame();
            }
        }
    }

    IEnumerator FocusFirstButtonNextFrame()
    {
        yield return new WaitForEndOfFrame();
        resumeButton.Focus();
    }

    void ResumeGame()
    {
        pauseVE.style.display = DisplayStyle.None;
        MainMenu.GameIsPaused = false;
        Time.timeScale = 1f;
    }

    void RestartGame()
    {
        pauseVE.style.display = DisplayStyle.None;
        player.events.OnDeath?.Invoke();
        MainMenu.GameIsPaused = false;
        Time.timeScale = 1f;
    }

    void OptionsGame()
    {
        pauseVE.style.display = DisplayStyle.None;
        settingsMenu.LoadScreen();
    }

    void MainMenuGame()
    {
        pauseVE.style.display = DisplayStyle.None;
        Time.timeScale = 1f;
        MainMenu.GameIsPaused = false;
        SceneManager.LoadScene("MainMenu");
    }
    #endregion

    #region Events
    void OnEnable()
    {
        resumeButton.clicked += ResumeGame;
        restartButton.clicked += RestartGame;
        optionsButton.clicked += OptionsGame;
        mainMenuButton.clicked += MainMenuGame;
    }

    void OnDisable()
    {
        resumeButton.clicked -= ResumeGame;
        restartButton.clicked -= RestartGame;
        optionsButton.clicked -= OptionsGame;
        mainMenuButton.clicked -= MainMenuGame;
    }
    #endregion
}
