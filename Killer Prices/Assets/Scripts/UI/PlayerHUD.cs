using UnityEngine;
using UnityEngine.UIElements;
using KillerPrices.Systems;

namespace KillerPrices.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        [Header("UI Toolkit")]
        [SerializeField] private UIDocument uiDocument;

        private Label moneyLabel;
        private Label levelLabel;
        private VisualElement xpFill;

        private bool isSubscribed = false;

        private void OnEnable()
        {
            refreshRetryCount = 0; // Reset retry counter on enable
            
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
            
            if (uiDocument != null)
            {
                var root = uiDocument.rootVisualElement;
                moneyLabel = root.Q<Label>("MoneyText");
                levelLabel = root.Q<Label>("LevelText");
                xpFill = root.Q<VisualElement>("XPFill");
            }

            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += RefreshUI;
            }

            TrySubscribe();
        }

        private void Start()
        {
            TrySubscribe();
        }

        private void OnDisable()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnStatsChanged -= UpdateUI;
                isSubscribed = false;
            }
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= RefreshUI;
            }
        }
        
        private int refreshRetryCount = 0;
        private const int MAX_REFRESH_RETRIES = 5;
        
        private void RefreshUI()
        {
            if (PlayerStats.Instance != null)
            {
                // Force update on the Main Thread safely
                UpdateUI(PlayerStats.Instance.Money, PlayerStats.Instance.Level, PlayerStats.Instance.CurrentXP, PlayerStats.Instance.MaxXP);
                refreshRetryCount = 0; // Reset counter on success
            }
            else
            {
                // Retry if Stats not ready yet (e.g. during scene load race)
                // But limit retries to avoid infinite loop in scenes without PlayerStats (like Main Menu)
                if (refreshRetryCount < MAX_REFRESH_RETRIES)
                {
                    refreshRetryCount++;
                    Invoke(nameof(RefreshUI), 0.1f);
                }
                else
                {
                    // Give up after max retries - likely in a scene without PlayerStats
                    Debug.LogWarning("PlayerHUD: PlayerStats not found after multiple retries. Likely in Main Menu or scene without player.");
                }
            }
        }

        private void UpdateUI(int money, int level, float currentXP, float maxXP)
        {
            if (moneyLabel != null)
            {
                string prefix = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetTranslation("HUD_MONEY") : "Money: $";
                moneyLabel.text = prefix + money;
            }
            
            if (levelLabel != null)
            {
                string prefix = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetTranslation("HUD_LEVEL") : "Level: ";
                levelLabel.text = prefix + level;
            }

            if (xpFill != null && maxXP > 0)
            {
                float percentage = (currentXP / maxXP) * 100f;
                xpFill.style.width = Length.Percent(percentage);
            }
        }
        
        private void TrySubscribe()
        {
            if (!isSubscribed)
            {
                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.OnStatsChanged += UpdateUI;
                    isSubscribed = true;
                    UpdateUI(PlayerStats.Instance.Money, PlayerStats.Instance.Level, PlayerStats.Instance.CurrentXP, PlayerStats.Instance.MaxXP);
                }
            }
        }

        public void SetVisible(bool isVisible)
        {
            if (uiDocument != null && uiDocument.rootVisualElement != null)
            {
                uiDocument.rootVisualElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
