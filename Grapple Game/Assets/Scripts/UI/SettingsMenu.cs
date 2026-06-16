using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SettingsMenu : MonoBehaviour
{
    // Add camera sensitivity
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

    Resolution[] filteredResolutions;

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

    void PopulateResolutions()
    {
        Resolution[] allResolutions = Screen.resolutions;
        List<string> options = new List<string>();
        List<Resolution> uniqueResolutions = new List<Resolution>();
        HashSet<string> seenResolutions = new HashSet<string>();

        int currentResolutionIndex = 0;

        for (int i = allResolutions.Length - 1; i >= 0; i--)
        {
            string optionText = $"{allResolutions[i].width} x {allResolutions[i].height}";
            
            if (!seenResolutions.Contains(optionText))
            {
                seenResolutions.Add(optionText);
                uniqueResolutions.Add(allResolutions[i]);
                options.Add(optionText);
            }

            if (allResolutions[i].width == Screen.currentResolution.width &&
                allResolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = uniqueResolutions.Count - 1;
            }
        }

        filteredResolutions = uniqueResolutions.ToArray();
        resolutionDropdown.choices = options;

        if (options.Count > 0)
        {
            resolutionDropdown.index = currentResolutionIndex;
        }
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
        saveSystem.SaveSettings(volumeSlider.value, graphicsDropdown.index, fullscreenToggle.value, resolutionDropdown.index);
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
        int selectedIndex = resolutionDropdown.index;

        if (selectedIndex >= 0 && selectedIndex < filteredResolutions.Length)
        {
            Resolution targetResolution = filteredResolutions[selectedIndex];
            
            Screen.SetResolution(targetResolution.width, targetResolution.height, Screen.fullScreenMode);
            Debug.Log($"Resolution changed to: {targetResolution.width}x{targetResolution.height}");
        }

        saveSystem.Data.resolution = selectedIndex;
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

        PopulateResolutions();
        resolutionDropdown.index = settings.resolution;
    }
    #endregion

    #region Events
    void OnEnable()
    {
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