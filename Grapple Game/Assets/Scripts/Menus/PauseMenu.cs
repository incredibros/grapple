using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    Player player;
    UIDocument pauseDocument;
    VisualElement pauseVE;

    Button resumeButton;
    Button restartButton;
    Button optionsButton;
    Button mainMenuButton;

    void Awake()
    {
        pauseDocument = GetComponent<UIDocument>();
        FindElements();

        GameObject playerObj = GameObject.Find("Player");

        player = playerObj.GetComponent<Player>();
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
    }

    #region Event Handlers
    public void OnMenuDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (MainMenu.GameIsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void OnCancelDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (MainMenu.GameIsPaused)
            {
                ResumeGame();
            }
        }
    }

    void PauseGame()
    {
        pauseVE.style.display = DisplayStyle.Flex;
        MainMenu.GameIsPaused = true;
        Time.timeScale = 0f;

        StartCoroutine(FocusFirstButtonNextFrame());

        //hud.DeactivateHUD();
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
        Debug.Log("Options");
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
