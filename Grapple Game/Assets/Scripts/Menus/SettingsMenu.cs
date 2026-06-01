using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] GameObject menu;
    MainMenu mainMenu;
    PauseMenu pauseMenu;

    UIDocument settingsDocument;
    public VisualElement settingsVE;

    Button backButton;
    Button applyButton;

    void Awake()
    {
        settingsDocument = GetComponent<UIDocument>();
        FindScripts();
        FindElements();
    }

    void Start()
    {
        settingsVE.style.display = DisplayStyle.None;
    }

    void FindScripts()
    {
        if (menu.name == "MainMenuScreen")
        {
            mainMenu = menu.GetComponent<MainMenu>();
        }
        if (menu.name == "PauseMenuScreen")
        {
            pauseMenu = menu.GetComponent<PauseMenu>();
        }
    }

    void FindElements()
    {
        settingsVE = settingsDocument.rootVisualElement;
        backButton = settingsVE.Q<Button>("Back");
        applyButton = settingsVE.Q<Button>("Apply");
    }

    public void LoadScreen()
    {
        settingsVE.style.display = DisplayStyle.Flex;
    }

    #region Event Handlers
    public void OnMenuDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (settingsVE.style.display == DisplayStyle.Flex)
            {
                BackGame();
            }
            else if (menu.name == "MainMenuScreen")
            {
                LoadScreen();
            }
        }
    }

    public void OnCancelDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (settingsVE.style.display == DisplayStyle.Flex)
            {
                if (menu.name == "MainMenuScreen")
                {
                    settingsVE.style.display = DisplayStyle.None;
                    mainMenu.LoadScreen();
                }
                if (menu.name == "PauseMenuScreen")
                {
                    settingsVE.style.display = DisplayStyle.None;
                    pauseMenu.LoadScreen();
                }
            }
        }
    }

    void BackGame()
    {
        settingsVE.style.display = DisplayStyle.None;
        if (menu.name == "MainMenuScreen")
        {
            mainMenu.LoadScreen();
        }
        if (menu.name == "PauseMenuScreen")
        {
            pauseMenu.LoadScreen();
        }
    }
    
    void ApplyGame()
    {
        Debug.Log("Apply settings");
    }
    #endregion

    #region Events
    void OnEnable()
    {
        backButton.clicked += BackGame;
        applyButton.clicked += ApplyGame;
    }

    void OnDisable()
    {
        backButton.clicked -= BackGame;
        applyButton.clicked -= ApplyGame;
    }
    #endregion
}
