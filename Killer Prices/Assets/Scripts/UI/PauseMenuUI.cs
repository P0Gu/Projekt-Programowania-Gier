using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using KillerPrices.Systems;

namespace KillerPrices.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("UI Toolkit")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private string mainMenuScene = "MainMenu"; // Name of Main Menu scene

        private VisualElement container;
        private Label titleLabel;
        private Button resumeButton;
        private Button saveButton;
        private Button menuButton;
        private Button quitButton;

        public static bool IsPaused { get; private set; } = false;

        private void OnEnable()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();

            if (uiDocument != null)
            {
                var root = uiDocument.rootVisualElement;
                container = root.Q<VisualElement>("PauseContainer");
                
                titleLabel = root.Q<Label>("PauseTitle");
                resumeButton = root.Q<Button>("ResumeButton");
                saveButton = root.Q<Button>("SaveButton");
                menuButton = root.Q<Button>("MenuButton");
                quitButton = root.Q<Button>("QuitButton");

                // AUTO-FIX: Wymuś skalowanie z ekranem
                if (uiDocument.panelSettings != null)
                {
                   uiDocument.panelSettings.scaleMode = UnityEngine.UIElements.PanelScaleMode.ScaleWithScreenSize;
                   uiDocument.panelSettings.referenceResolution = new Vector2Int(1920, 1080);
                   uiDocument.panelSettings.match = 0.5f;
                   uiDocument.panelSettings.screenMatchMode = UnityEngine.UIElements.PanelScreenMatchMode.MatchWidthOrHeight;
                }

                if (resumeButton != null) resumeButton.clicked += Resume;
                if (saveButton != null) saveButton.clicked += SaveGame;
                if (menuButton != null) menuButton.clicked += LoadMainMenu;
                if (quitButton != null) quitButton.clicked += QuitGame;

                // Subscribe Localization
                if (LocalizationManager.Instance != null)
                {
                    LocalizationManager.Instance.OnLanguageChanged += UpdateTexts;
                    UpdateTexts();
                }

                // Initial state
                if (container != null) container.style.display = DisplayStyle.None;
            }
        }

        private void OnDisable()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= UpdateTexts;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Toggle Pause
                if (IsPaused) Resume();
                else Pause();
            }
        }

        public void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            
            if (container != null) container.style.display = DisplayStyle.Flex;
            
            // Show Cursor
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            
            // Block Player Control? handled by Time.timeScale=0 mostly, but can disable script too
            var player = FindFirstObjectByType<Controls>();
            if (player != null) player.IsLocked = true;
        }

        public void Resume()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            
            if (container != null) container.style.display = DisplayStyle.None;
            
            // Hide Cursor
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            
            var player = FindFirstObjectByType<Controls>();
            if (player != null) player.IsLocked = false;
        }

        private void SaveGame()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.Save();
                
                // Optional: Show "Saved" feedback, for now just log
                Debug.Log("UI: Game Saved via Pause Menu.");
                
                // Optional: Resume after save? Or stay in menu? Staying in menu is safer.
            }
        }

        private void LoadMainMenu()
        {
            Time.timeScale = 1f; // Important reset
            IsPaused = false;
            SceneManager.LoadScene(mainMenuScene);
        }

        private void QuitGame()
        {
            Debug.Log("Quitting Game...");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void UpdateTexts()
        {
            if (LocalizationManager.Instance == null) return;
            var loc = LocalizationManager.Instance;

            if (titleLabel != null) titleLabel.text = loc.GetTranslation("PAUSE_TITLE");
            if (resumeButton != null) resumeButton.text = loc.GetTranslation("PAUSE_RESUME");
            if (saveButton != null) saveButton.text = loc.GetTranslation("PAUSE_SAVE");
            if (menuButton != null) menuButton.text = loc.GetTranslation("PAUSE_MENU");
            if (quitButton != null) quitButton.text = loc.GetTranslation("PAUSE_QUIT");
        }
    }
}
