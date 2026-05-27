using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using TMPro;

public class PauseMenu : MonoBehaviour
{
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
            child.gameObject.SetActive(false);
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
        player.events.OnDeath?.Invoke();
        player.transform.position = (Vector3) PlayerInteractions.lastCheckpoint;
        player.events.OnRespawn?.Invoke();
        Time.timeScale = 1f;
        MainMenu.GameIsPaused = false;
        pauseMenuUI.SetActive(false);
    }
    #endregion
}
