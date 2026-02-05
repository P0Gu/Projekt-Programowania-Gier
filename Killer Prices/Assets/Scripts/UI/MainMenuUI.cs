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

        private VisualElement popupOverlay;
        private Label popupMessage;
        private Button popupYesBtn;
        private Button popupNoBtn;
        private Button popupOkBtn;

        private void OnEnable()
        {
            // PURGE: Destroy old singletons to force a clean state
            PurgeOldSingletons();

            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            if (uiDocument != null)
            {
                var root = uiDocument.rootVisualElement;

                newGameBtn = root.Q<Button>("NewGameButton");
                loadGameBtn = root.Q<Button>("LoadGameButton");
                settingsBtn = root.Q<Button>("SettingsButton");
                quitBtn = root.Q<Button>("QuitButton");
                
                // Popup Elements
                popupOverlay = root.Q<VisualElement>("PopupOverlay");
                popupMessage = root.Q<Label>("PopupMessage");
                popupYesBtn = root.Q<Button>("PopupYesBtn");
                popupNoBtn = root.Q<Button>("PopupNoBtn");
                popupOkBtn = root.Q<Button>("PopupOkBtn");

                // AUTO-FIX: Wymuś skalowanie
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
                
                // Popup Events
                if (popupNoBtn != null) popupNoBtn.clicked += ClosePopup;
                if (popupOkBtn != null) popupOkBtn.clicked += ClosePopup;
                // Yes button logic is dynamic or assigned in Show method, but simpler to assign static and use flag? 
                // Better: Assign specific callback in ShowPopup, or just handle in specific method.
                // Let's keep it simple: Yes button calls a specific method "ConfirmOverwrite".
                if (popupYesBtn != null) popupYesBtn.clicked += ConfirmOverwrite;

                // Subscribe to Localization
                if (LocalizationManager.Instance != null)
                {
                    LocalizationManager.Instance.OnLanguageChanged += UpdateTexts;
                    UpdateTexts(); 
                }
            }
        }

        private void OnDisable()
        {
            if (LocalizationManager.Instance != null)
                LocalizationManager.Instance.OnLanguageChanged -= UpdateTexts;
        }

        private void UpdateTexts()
        {
            if (LocalizationManager.Instance == null) return;
            var loc = LocalizationManager.Instance;

            if (newGameBtn != null) newGameBtn.text = loc.GetTranslation("MENU_NEW_GAME");
            if (loadGameBtn != null) loadGameBtn.text = loc.GetTranslation("MENU_LOAD_GAME");
            if (settingsBtn != null) settingsBtn.text = loc.GetTranslation("MENU_SETTINGS");
            if (quitBtn != null) quitBtn.text = loc.GetTranslation("MENU_QUIT");
            
            if (popupYesBtn != null) popupYesBtn.text = loc.GetTranslation("MENU_YES");
            if (popupNoBtn != null) popupNoBtn.text = loc.GetTranslation("MENU_NO");
            if (popupOkBtn != null) popupOkBtn.text = loc.GetTranslation("MENU_OK");
        }

        private void StartNewGame()
        {
            if (SaveManager.Instance == null)
            {
                 Debug.LogError("MainMenu: SaveManager Instance is NULL! Cannot check for saves.");
                 LoadGameScene(false); // Fallback: New Game
                 return;
            }

            bool hasSave = SaveManager.Instance.HasSave();
            Debug.Log($"MainMenu: Checking for save... HasSave = {hasSave}");

            if (hasSave)
            {
                // Show Confirmation
                string msg = LocalizationManager.Instance != null ? 
                    LocalizationManager.Instance.GetTranslation("MENU_OVERWRITE_CONFIRM") : "Save exists. Overwrite?";
                ShowPopup(msg, true);
            }
            else
            {
                LoadGameScene(false); // New Game
            }
        }

        private void LoadGame()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
            {
                LoadGameScene(true); // Load existing save
            }
            else
            {
                string msg = LocalizationManager.Instance != null ? 
                    LocalizationManager.Instance.GetTranslation("MENU_NO_SAVE") : "No save found.";
                ShowPopup(msg, false);
            }
        }

        private void ShowPopup(string message, bool isConfirmation)
        {
            if (popupOverlay == null) return;
            
            popupOverlay.style.display = DisplayStyle.Flex;
            if (popupMessage != null) popupMessage.text = message;

            if (isConfirmation)
            {
                // Show Yes/No, Hide OK
                if (popupYesBtn != null) popupYesBtn.style.display = DisplayStyle.Flex;
                if (popupNoBtn != null) popupNoBtn.style.display = DisplayStyle.Flex;
                if (popupOkBtn != null) popupOkBtn.style.display = DisplayStyle.None;
            }
            else
            {
                // Hide Yes/No, Show OK
                if (popupYesBtn != null) popupYesBtn.style.display = DisplayStyle.None;
                if (popupNoBtn != null) popupNoBtn.style.display = DisplayStyle.None;
                if (popupOkBtn != null) popupOkBtn.style.display = DisplayStyle.Flex;
            }
        }

        private void ClosePopup()
        {
            if (popupOverlay != null) popupOverlay.style.display = DisplayStyle.None;
        }

        private void ConfirmOverwrite()
        {
            Debug.Log("MainMenu: Overwriting save...");
            // Delete old save to start fresh
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.DeleteSave();
            }
            ClosePopup();
            LoadGameScene(false); // New Game (fresh start)
        }

        private void LoadGameScene(bool loadFromSave)
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.ShouldLoadOnStart = loadFromSave;
            }

            Debug.Log($"MainMenu: Loading Scene '{gameSceneName}' with LoadFromSave={loadFromSave}");
            SceneManager.LoadScene(gameSceneName);
        }

        private void OpenSettings()
        {
            Debug.Log("MainMenu: Opening Settings...");
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
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
        private void PurgeOldSingletons()
        {
            // DISABLED PURGE: The GameScene might not spawn these if they are expected to persist.
            // If we destroy them here, and GameScene doesn't have the prefabs, they are lost forever.
            // Instead of destroying, we rely on their internal "Awake" logic to handle duplicates
            // OR we explicitly Reset them if needed.
            
            /*
            if (PlayerStats.Instance != null)
            {
                Debug.Log("MainMenu: Purging old PlayerStats...");
                Destroy(PlayerStats.Instance.gameObject);
            }

            if (PlayerInventory.Instance != null)
            {
                Debug.Log("MainMenu: Purging old PlayerInventory...");
                Destroy(PlayerInventory.Instance.gameObject);
            }

            if (KillerPrices.Placement.PlacementManager.Instance != null)
            {
                Debug.Log("MainMenu: Purging old PlacementManager...");
                Destroy(KillerPrices.Placement.PlacementManager.Instance.gameObject);
            }
            */
            Debug.Log("MainMenu: Purge skipped to ensure Singletons persist into GameScene.");
        }
    }
}
