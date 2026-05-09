using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public void PlayGame()
    {
        GameIsPaused = false;
        SceneManager.LoadScene("Prologue");
    }

    public void QuitGame()
    {
        GameIsPaused = false;
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
