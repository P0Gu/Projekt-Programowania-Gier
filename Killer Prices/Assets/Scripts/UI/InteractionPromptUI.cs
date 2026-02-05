using UnityEngine;
using UnityEngine.UIElements;
using KillerPrices.Systems;

namespace KillerPrices.UI
{
    public class InteractionPromptUI : MonoBehaviour
    {
        private Label promptLabel;
        private VisualElement promptContainer;
        
        private void Start()
        {
            // Find PlayerHUD's UIDocument
            var playerHUD = FindFirstObjectByType<PlayerHUD>();
            if (playerHUD != null)
            {
                var uiDoc = playerHUD.GetComponent<UIDocument>();
                if (uiDoc != null)
                {
                    var root = uiDoc.rootVisualElement;
                    promptContainer = root.Q<VisualElement>("InteractionPrompt");
                    promptLabel = root.Q<Label>("InteractionPromptText");
                }
            }
            
            // Subscribe to language changes
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += RefreshCurrentPrompt;
            }
            
            Hide(); // Start hidden
        }
        
        private void OnDestroy()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= RefreshCurrentPrompt;
            }
        }
        
        private string currentActionKey = "";
        
        public void Show(string actionKey)
        {
            if (promptContainer == null || promptLabel == null)
                return;
            
            currentActionKey = actionKey;
            RefreshCurrentPrompt();
            
            promptContainer.style.display = DisplayStyle.Flex;
        }
        
        public void Hide()
        {
            if (promptContainer == null)
                return;
            
            currentActionKey = "";
            promptContainer.style.display = DisplayStyle.None;
        }
        
        private void RefreshCurrentPrompt()
        {
            if (string.IsNullOrEmpty(currentActionKey) || promptLabel == null)
                return;
            
            if (LocalizationManager.Instance != null)
            {
                string action = LocalizationManager.Instance.GetTranslation(currentActionKey);
                string prompt = LocalizationManager.Instance.GetTranslation("INTERACT_PROMPT");
                promptLabel.text = string.Format(prompt, action);
            }
        }
    }
}
