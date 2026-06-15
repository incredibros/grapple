using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SettingsMenu : MonoBehaviour
{
    // Add camera sesnitivity
    // Add color blind mode
    // Add languages
    // Add keybinds
    // Add Music Sound Effects (SFX)

    [SerializeField] GameObject menu;
    [SerializeField] AudioMixer audioMixer;
    MainMenu mainMenu;
    PauseMenu pauseMenu;
    SaveSystem saveSystem;

    UIDocument settingsDocument;
    public VisualElement settingsVE;

    Button backButton;
    Button applyButton;

    Toggle fullscreenToggle;
    DropdownField resolutionDropdown;
    DropdownField graphicsDropdown;
    Slider volumeSlider;

    void Awake()
    {
        settingsDocument = GetComponent<UIDocument>();
        FindScripts();
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

        saveSystem = GameObject.FindWithTag("SaveManager").GetComponent<SaveSystem>();
    }

    void FindElements()
    {
        settingsVE = settingsDocument.rootVisualElement;
        
        backButton = settingsVE.Q<Button>("Back");
        applyButton = settingsVE.Q<Button>("Apply");
        fullscreenToggle = settingsVE.Q<Toggle>("Fullscreen");
        resolutionDropdown = settingsVE.Q<DropdownField>("Resolution");
        graphicsDropdown = settingsVE.Q<DropdownField>("Quality");
        volumeSlider = settingsVE.Q<Slider>("Volume");
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
                HideScreenAndReturn();
            }
        }
    }

    void BackGame()
    {
        HideScreenAndReturn();
    }
    
    void HideScreenAndReturn()
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
        saveSystem.SaveSettings(volumeSlider.value, graphicsDropdown.index, fullscreenToggle.value);
    }

    void OnGraphicsChanged(ChangeEvent<string> evt)
    {
        string selectedQuality = evt.newValue;
        int qualityIndex = graphicsDropdown.index; 

        QualitySettings.SetQualityLevel(qualityIndex, true);
        saveSystem.Data.graphicsQuality = qualityIndex;
    }

    void OnVolumeChanged(ChangeEvent<float> evt)
    {
        float newVolume = evt.newValue;
        
        audioMixer.SetFloat("volume", Mathf.Log10(Mathf.Max(newVolume, 0.0001f)) * 20);
        saveSystem.Data.volume = newVolume;
    }

    void OnResolutionChanged(ChangeEvent<string> evt)
    {
        Debug.Log($"Resolution changed to: {evt.newValue}");
    }

    void OnFullscreenChanged(ChangeEvent<bool> evt)
    {
        bool isFullscreen = evt.newValue;

        Screen.fullScreen = isFullscreen;
        saveSystem.Data.isFullscreen = isFullscreen;
    }

    void LoadSettings()
    {
        GameData settings = saveSystem.Data;

        // Load Volume
        volumeSlider.value = settings.volume;
        audioMixer.SetFloat("volume", Mathf.Log10(Mathf.Max(settings.volume, 0.0001f)) * 20);

        // Load Graphics
        graphicsDropdown.index = settings.graphicsQuality;
        QualitySettings.SetQualityLevel(settings.graphicsQuality, true);

        // Load Fullscreen state
        fullscreenToggle.value = settings.isFullscreen;
        Screen.fullScreen = settings.isFullscreen;
    }
    #endregion

    #region Events
    void OnEnable()
    {
        FindElements();
        LoadSettings();

        backButton.clicked += BackGame;
        applyButton.clicked += ApplyGame;

        resolutionDropdown.RegisterValueChangedCallback(OnResolutionChanged);
        graphicsDropdown.RegisterValueChangedCallback(OnGraphicsChanged);
        volumeSlider.RegisterValueChangedCallback(OnVolumeChanged);
        fullscreenToggle.RegisterValueChangedCallback(OnFullscreenChanged);
    }

    void OnDisable()
    {
        backButton.clicked -= BackGame;
        applyButton.clicked -= ApplyGame;

        resolutionDropdown.UnregisterValueChangedCallback(OnResolutionChanged);
        graphicsDropdown.UnregisterValueChangedCallback(OnGraphicsChanged);
        volumeSlider.UnregisterValueChangedCallback(OnVolumeChanged);
        fullscreenToggle.UnregisterValueChangedCallback(OnFullscreenChanged);
    }
    #endregion
}