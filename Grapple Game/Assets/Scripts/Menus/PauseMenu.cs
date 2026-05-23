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

    Player player;

    void Awake()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        Deactivate();
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
        player.events.OnDeath?.Invoke();
        player.transform.position = Vector2.zero;
        player.events.OnRespawn?.Invoke();
        Time.timeScale = 1f;
        MainMenu.GameIsPaused = false;
        pauseMenuUI.SetActive(false);
    }
    #endregion
}
