using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    #region Event Handlers
    public void PlayGame()
    {
        GameIsPaused = false;
        int nextScene = (SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings;
        SceneManager.LoadScene(nextScene);
    }

    public void QuitGame()
    {
        GameIsPaused = false;
        Debug.Log("Quitting game...");
        Application.Quit();
    }
    #endregion
}
