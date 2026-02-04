using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using KillerPrices.Systems;

namespace KillerPrices.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI Toolkit")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private string gameSceneName = "GameScene"; // Name of the scene to load

        [SerializeField] private GameObject settingsPanel; // Reference to the Settings UI GameObject

        private Button newGameBtn;
        private Button loadGameBtn;
        private Button settingsBtn;
        private Button quitBtn;

        private void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            if (uiDocument != null)
            {
                var root = uiDocument.rootVisualElement;

                newGameBtn = root.Q<Button>("NewGameButton");
                loadGameBtn = root.Q<Button>("LoadGameButton");
                settingsBtn = root.Q<Button>("SettingsButton");
                quitBtn = root.Q<Button>("QuitButton");
                
                // AUTO-FIX: Wymuś skalowanie z ekranem (1920x1080)
                if (uiDocument.panelSettings != null)
                {
                   uiDocument.panelSettings.scaleMode = UnityEngine.UIElements.PanelScaleMode.ScaleWithScreenSize;
                   uiDocument.panelSettings.referenceResolution = new Vector2Int(1920, 1080);
                   uiDocument.panelSettings.match = 0.5f;
                   uiDocument.panelSettings.screenMatchMode = UnityEngine.UIElements.PanelScreenMatchMode.MatchWidthOrHeight;
                }

                if (newGameBtn != null) newGameBtn.clicked += StartNewGame;
                if (loadGameBtn != null) loadGameBtn.clicked += LoadGame;
                if (settingsBtn != null) settingsBtn.clicked += OpenSettings;
                if (quitBtn != null) quitBtn.clicked += QuitGame;

                // Subscribe to Localization
                if (LocalizationManager.Instance != null)
                {
                    LocalizationManager.Instance.OnLanguageChanged += UpdateTexts;
                    UpdateTexts(); // Initial update
                }
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

            if (newGameBtn != null) newGameBtn.text = loc.GetTranslation("MENU_NEW_GAME");
            if (loadGameBtn != null) loadGameBtn.text = loc.GetTranslation("MENU_LOAD_GAME");
            if (settingsBtn != null) settingsBtn.text = loc.GetTranslation("MENU_SETTINGS");
            if (quitBtn != null) quitBtn.text = loc.GetTranslation("MENU_QUIT");
        }

        private void StartNewGame()
        {
            Debug.Log($"MainMenu: Starting New Game... Loading Scene '{gameSceneName}'");
            SceneManager.LoadScene(gameSceneName);
        }

        private void LoadGame()
        {
            Debug.Log("MainMenu: Load Game not implemented yet.");
        }

        private void OpenSettings()
        {
            Debug.Log("MainMenu: Opening Settings...");
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                // Hide main menu
                gameObject.SetActive(false); 
            }
            else
            {
                Debug.LogError("MainMenu: Settings Panel reference is missing!");
            }
        }

        private void QuitGame()
        {
            Debug.Log("MainMenu: Quitting Application...");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
