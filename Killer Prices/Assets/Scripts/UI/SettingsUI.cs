using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using KillerPrices.Systems;

namespace KillerPrices.UI
{
    public class SettingsUI : MonoBehaviour
    {
        [Header("UI Toolkit")]
        [SerializeField] private UIDocument uiDocument;
        
        [SerializeField] private GameObject mainMenuContainer; 

        private Slider volumeSlider;
        private DropdownField resolutionDropdown;
        private DropdownField qualityDropdown;
        private DropdownField languageDropdown;
        private Toggle fullscreenToggle;
        private Button backButton;
        private Button saveButton;

        // Labels to translate
        private Label audioLabel;
        private Label graphicsLabel;
        private Label langLabel;

        private const string PREF_VOLUME = "MasterVolume";
        private const string PREF_QUALITY = "GraphicsQuality";
        private const string PREF_FULLSCREEN = "Fullscreen";
        private const string PREF_RESOLUTION_INDEX = "ResolutionIndex";

        private readonly List<Vector2Int> supportedResolutions = new List<Vector2Int>()
        {
            new Vector2Int(1280, 720),
            new Vector2Int(1366, 768),
            new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440),
            new Vector2Int(3840, 2160)
        };

        private void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            // Subscribe to Localization
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += UpdateTexts;
            }

            if (uiDocument != null)
            {
                var root = uiDocument.rootVisualElement;

                volumeSlider = root.Q<Slider>("VolumeSlider");
                resolutionDropdown = root.Q<DropdownField>("ResolutionDropdown");
                qualityDropdown = root.Q<DropdownField>("QualityDropdown");
                languageDropdown = root.Q<DropdownField>("LanguageDropdown");
                fullscreenToggle = root.Q<Toggle>("FullscreenToggle");
                backButton = root.Q<Button>("BackButton");
                saveButton = root.Q<Button>("SaveButton");

                // Headers
                audioLabel = root.Q<Label>("SettingsAudioLabel");
                graphicsLabel = root.Q<Label>("SettingsGraphicsLabel"); 
                langLabel = root.Q<Label>("SettingsLangLabel");

                // AUTO-FIX: Wymuś skalowanie z ekranem (1920x1080)
                if (uiDocument.panelSettings != null)
                {
                   uiDocument.panelSettings.scaleMode = UnityEngine.UIElements.PanelScaleMode.ScaleWithScreenSize;
                   uiDocument.panelSettings.referenceResolution = new Vector2Int(1920, 1080);
                   uiDocument.panelSettings.match = 0.5f;
                   uiDocument.panelSettings.screenMatchMode = UnityEngine.UIElements.PanelScreenMatchMode.MatchWidthOrHeight;
                }

                // Initialize UI Values
                LoadSettings();
                UpdateTexts();

                // Subscribe Events
                if (volumeSlider != null) volumeSlider.RegisterValueChangedCallback(evt => SetVolume(evt.newValue));
                if (resolutionDropdown != null) resolutionDropdown.RegisterValueChangedCallback(evt => SetResolution(evt.newValue));
                if (qualityDropdown != null) qualityDropdown.RegisterValueChangedCallback(evt => SetQuality(evt.newValue));
                if (languageDropdown != null) languageDropdown.RegisterValueChangedCallback(evt => SetLanguage(evt.newValue));
                if (fullscreenToggle != null) fullscreenToggle.RegisterValueChangedCallback(evt => SetFullscreen(evt.newValue));
                
                if (backButton != null) backButton.clicked += CloseWithoutSaving;
                if (saveButton != null) saveButton.clicked += SaveAndClose;
            }
        }

        private void OnDisable()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= UpdateTexts;
            }
        }

        private void UpdateTexts()
        {
            if (LocalizationManager.Instance == null) return;
            var loc = LocalizationManager.Instance;

            if (audioLabel != null) audioLabel.text = loc.GetTranslation("SETTINGS_AUDIO");
            
            if (langLabel != null) langLabel.text = loc.GetTranslation("SETTINGS_LANG");
            if (backButton != null) backButton.text = loc.GetTranslation("SETTINGS_BACK"); // "ANULUJ"
            if (saveButton != null) saveButton.text = loc.GetTranslation("SETTINGS_SAVE"); // "ZAPISZ"

            if (volumeSlider != null) volumeSlider.label = loc.GetTranslation("SETTINGS_VOLUME");
            if (resolutionDropdown != null) resolutionDropdown.label = loc.GetTranslation("SETTINGS_RESOLUTION");
            if (qualityDropdown != null) qualityDropdown.label = loc.GetTranslation("SETTINGS_QUALITY");
            if (languageDropdown != null) languageDropdown.label = loc.GetTranslation("SETTINGS_LANG");
            if (fullscreenToggle != null) fullscreenToggle.label = loc.GetTranslation("SETTINGS_FULLSCREEN");
        }

        private void LoadSettings()
        {
            // Language
            if (languageDropdown != null && LocalizationManager.Instance != null)
            {
                languageDropdown.choices = new List<string> { "PL", "EN" };
                string currentLang = LocalizationManager.Instance.CurrentLanguage;
                languageDropdown.value = currentLang == "pl" ? "PL" : "EN";
            }

            // Volume
            float volume = PlayerPrefs.GetFloat(PREF_VOLUME, 1f);
            AudioListener.volume = volume;
            if (volumeSlider != null) volumeSlider.value = volume;

            // Fullscreen
            bool isFullscreen = PlayerPrefs.GetInt(PREF_FULLSCREEN, 1) == 1;
            Screen.fullScreen = isFullscreen;
            if (fullscreenToggle != null) fullscreenToggle.value = isFullscreen;

            // Resolution
            if (resolutionDropdown != null)
            {
                var choices = supportedResolutions.Select(res => $"{res.x}x{res.y}").ToList();
                resolutionDropdown.choices = choices;

                // Load saved index or find current screen resolution
                int savedIndex = PlayerPrefs.GetInt(PREF_RESOLUTION_INDEX, -1);
                
                if (savedIndex >= 0 && savedIndex < choices.Count)
                {
                    resolutionDropdown.index = savedIndex;
                    resolutionDropdown.value = choices[savedIndex];
                }
                else
                {
                    // Try to match current screen resolution
                    Vector2Int currentRes = new Vector2Int(Screen.width, Screen.height);
                    int index = supportedResolutions.FindIndex(r => r.x == currentRes.x && r.y == currentRes.y);
                    
                    if (index == -1) index = 2; // Default to 1920x1080 if not found
                    
                    resolutionDropdown.index = index;
                    resolutionDropdown.value = choices[index];
                }
            }

            // Quality
            string[] names = QualitySettings.names;
            if (qualityDropdown != null)
            {
                qualityDropdown.choices = names.ToList();
                
                int qualityIndex = PlayerPrefs.GetInt(PREF_QUALITY, QualitySettings.GetQualityLevel());
                qualityIndex = Mathf.Clamp(qualityIndex, 0, names.Length - 1);
                
                QualitySettings.SetQualityLevel(qualityIndex);
                qualityDropdown.value = names[qualityIndex]; 
                qualityDropdown.index = qualityIndex;
            }
        }

        private void SetLanguage(string val)
        {
            string code = val == "PL" ? "pl" : "en";
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.SetLanguage(code);
            }
        }

        private void SetVolume(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(PREF_VOLUME, value);
        }

        private void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt(PREF_FULLSCREEN, isFullscreen ? 1 : 0);
            
            // Re-apply resolution just in case fullscreen switch changes it
            if (resolutionDropdown != null) SetResolution(resolutionDropdown.value);
        }
        
        private void SetResolution(string resString)
        {
            int index = resolutionDropdown.choices.IndexOf(resString);
            if (index >= 0)
            {
                Vector2Int res = supportedResolutions[index];
                Screen.SetResolution(res.x, res.y, Screen.fullScreen);
                PlayerPrefs.SetInt(PREF_RESOLUTION_INDEX, index);
            }
        }

        private void SetQuality(string qualityName)
        {
            int index = qualityDropdown.choices.IndexOf(qualityName);
            if (index >= 0)
            {
                QualitySettings.SetQualityLevel(index);
                PlayerPrefs.SetInt(PREF_QUALITY, index);
            }
        }

        private void SaveAndClose()
        {
            PlayerPrefs.Save();
            Debug.Log("Settings: Saved.");
            CloseMenu();
        }

        private void CloseWithoutSaving()
        {
            Debug.Log("Settings: Cancelled.");
            CloseMenu();
        }

        private void CloseMenu()
        {
             gameObject.SetActive(false);
            if (mainMenuContainer != null)
                mainMenuContainer.SetActive(true);
        }
    }
}
