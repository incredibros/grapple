using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    GameObject pauseMenuUI;

    void Awake()
    {
        pauseMenuUI = transform.GetChild(0).gameObject;
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
        MainMenu.GameIsPaused = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        MainMenu.GameIsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    #endregion
}
