using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CreditsMenu : MonoBehaviour
{
    [SerializeField] MainMenu mainMenu;

    UIDocument creditsDocument;
    VisualElement creditsVE;

    Button backButton;

    void Awake()
    {
        creditsDocument = GetComponent<UIDocument>();
    }

    void Start()
    {
        creditsVE.style.display = DisplayStyle.None;
    }
    
    void FindElements()
    {
        creditsVE = creditsDocument.rootVisualElement;
        backButton = creditsVE.Q<Button>("Back");
    }

    public void LoadScreen()
    {
        creditsVE.style.display = DisplayStyle.Flex;
    }

    #region Event Handlers
    public void OnCancelDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (creditsVE.style.display == DisplayStyle.Flex)
            {
                BackGame();
            }
        }
    }

    void BackGame()
    {
        creditsVE.style.display = DisplayStyle.None;
        mainMenu.LoadScreen();
    }
    #endregion

    #region Events
    void OnEnable()
    {
        FindElements();
        
        backButton.clicked += BackGame;
    }

    void OnDisable()
    {
        backButton.clicked -= BackGame;
    }
    #endregion
}
