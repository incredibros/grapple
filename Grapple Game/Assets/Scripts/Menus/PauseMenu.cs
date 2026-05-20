using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;

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
}
