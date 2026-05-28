using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject targetToKeepActive;
    [SerializeField] GameObject pauseMenuUI;
    [SerializeField] GameObject optionsMenuUI;

    Player player;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        Deactivate();
        Time.timeScale = 1f;
        MainMenu.GameIsPaused = false;
    }

    void Deactivate()
    {
        foreach (Transform child in transform)
        {
            if (targetToKeepActive != null && child.gameObject == targetToKeepActive)
            {
                child.gameObject.SetActive(true);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (MainMenu.GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    #region Event Handlers
    public void Resume()
    {
        Time.timeScale = 1f;
        MainMenu.GameIsPaused = false;
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
    }
    void Pause()
    {
        pauseMenuUI.SetActive(true);
        MainMenu.GameIsPaused = true;
        Time.timeScale = 0f;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        MainMenu.GameIsPaused = true;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        MainMenu.GameIsPaused = false;
        player.events.OnDeath?.Invoke();
        pauseMenuUI.SetActive(false);
    }
    #endregion
}
