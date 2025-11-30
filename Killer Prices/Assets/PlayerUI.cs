using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("Referencje do elementów UI - Pieniądze")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [Tooltip("Opcjonalnie: ikona waluty")]
    [SerializeField] private Image moneyIcon;
    
    [Header("Referencje do elementów UI - Poziom")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI xpText;
    
    [Header("Pasek XP")]
    [SerializeField] private Slider xpSlider;
    [Tooltip("Czy pokazywać procenty na pasku XP?")]
    [SerializeField] private bool showPercentage = true;
    [SerializeField] private TextMeshProUGUI xpPercentageText;
    
    [Header("Ustawienia Formatowania")]
    [SerializeField] private string moneyPrefix = "";
    [SerializeField] private string moneySuffix = "$";
    [SerializeField] private string levelPrefix = "Poziom ";
    [SerializeField] private string xpFormat = "{0} / {1} XP";
    
    [Header("Animacje (opcjonalne)")]
    [SerializeField] private bool animateChanges = true;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Color flashColor = Color.yellow;
    
    private Color originalMoneyColor;
    private Color originalXPColor;
    private int displayedMoney = 0;
    
    private void Start()
    {
        if (moneyText != null)
            originalMoneyColor = moneyText.color;
        if (xpText != null)
            originalXPColor = xpText.color;
        
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnMoneyChanged += UpdateMoneyDisplay;
            PlayerStats.Instance.OnLevelChanged += UpdateLevelDisplay;
            PlayerStats.Instance.OnXPChanged += UpdateXPDisplay;
            
            UpdateMoneyDisplay(PlayerStats.Instance.Money);
            UpdateLevelDisplay(PlayerStats.Instance.Level);
            UpdateXPDisplay(PlayerStats.Instance.XP, PlayerStats.Instance.XPToNextLevel);
        }
        else
        {
            Debug.LogError("PlayerStats.Instance nie znalezione! Upewnij się, że PlayerStats jest w scenie.");
        }
    }
    
    private void OnDestroy()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
            PlayerStats.Instance.OnLevelChanged -= UpdateLevelDisplay;
            PlayerStats.Instance.OnXPChanged -= UpdateXPDisplay;
        }
    }

    private void UpdateMoneyDisplay(int newAmount)
    {
        if (moneyText == null) return;
        
        displayedMoney = newAmount;
        moneyText.text = $"{moneyPrefix}{newAmount:N0}{moneySuffix}";
        
        if (animateChanges)
        {
            FlashText(moneyText, originalMoneyColor);
        }
    }
    
    private void UpdateLevelDisplay(int newLevel)
    {
        if (levelText == null) return;
        
        levelText.text = $"{levelPrefix}{newLevel}";
        
        if (animateChanges)
        {
            FlashText(levelText, levelText.color);
        }
    }

    private void UpdateXPDisplay(int currentXP, int maxXP)
    {
        if (xpText != null)
        {
            xpText.text = string.Format(xpFormat, currentXP, maxXP);
            
            if (animateChanges)
            {
                FlashText(xpText, originalXPColor);
            }
        }
        
        if (xpSlider != null)
        {
            xpSlider.maxValue = maxXP;
            
            if (animateChanges)
            {
                StartCoroutine(AnimateSlider(xpSlider, currentXP));
            }
            else
            {
                xpSlider.value = currentXP;
            }
        }
        
        if (showPercentage && xpPercentageText != null)
        {
            float percentage = maxXP > 0 ? (float)currentXP / maxXP * 100f : 0f;
            xpPercentageText.text = $"{percentage:F0}%";
        }
    }

    private void FlashText(TextMeshProUGUI text, Color originalColor)
    {
        if (text == null) return;
        
        StopAllCoroutines();
        StartCoroutine(FlashCoroutine(text, originalColor));
    }
    
    private System.Collections.IEnumerator FlashCoroutine(TextMeshProUGUI text, Color originalColor)
    {
        text.color = flashColor;
        
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            text.color = Color.Lerp(flashColor, originalColor, elapsed / animationDuration);
            yield return null;
        }
        
        text.color = originalColor;
    }
    

    private System.Collections.IEnumerator AnimateSlider(Slider slider, float targetValue)
    {
        float startValue = slider.value;
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(startValue, targetValue, elapsed / animationDuration);
            yield return null;
        }
        
        slider.value = targetValue;
    }
    
    [ContextMenu("Test: Dodaj 100$")]
    private void TestAddMoney()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.AddMoney(100);
    }
    
    [ContextMenu("Test: Dodaj 50 XP")]
    private void TestAddXP()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.AddXP(50);
    }
    
    [ContextMenu("Test: Wydaj 50$")]
    private void TestRemoveMoney()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.RemoveMoney(50);
    }
    

}
