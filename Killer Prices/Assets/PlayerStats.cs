using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Pieniądze")]
    [SerializeField] private int currentMoney = 0;
    
    [Header("System Poziomów")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int xpToNextLevel = 100;
    [SerializeField] private float xpMultiplier = 1.5f;
    
    public event Action<int> OnMoneyChanged;
    public event Action<int> OnLevelChanged;
    public event Action<int, int> OnXPChanged;
    
    public int Money => currentMoney;
    public int Level => currentLevel;
    public int XP => currentXP;
    public int XPToNextLevel => xpToNextLevel;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        OnMoneyChanged?.Invoke(currentMoney);
        OnLevelChanged?.Invoke(currentLevel);
        OnXPChanged?.Invoke(currentXP, xpToNextLevel);
    }
    
    public void AddMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Próba dodania ujemnej kwoty pieniędzy. Użyj RemoveMoney()");
            return;
        }
        
        currentMoney += amount;
        OnMoneyChanged?.Invoke(currentMoney);
        Debug.Log($"Dodano {amount}$. Aktualna kwota: {currentMoney}$");
    }
    
    public bool RemoveMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Próba usunięcia ujemnej kwoty pieniędzy. Użyj AddMoney()");
            return false;
        }
        
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            OnMoneyChanged?.Invoke(currentMoney);
            Debug.Log($"Wydano {amount}$. Pozostało: {currentMoney}$");
            return true;
        }
        
        Debug.LogWarning($"Za mało pieniędzy! Potrzeba: {amount}$, Posiadasz: {currentMoney}$");
        return false;
    }
    
    public bool HasEnoughMoney(int amount)
    {
        return currentMoney >= amount;
    }
    
    
    public void AddXP(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Próba dodania ujemnego XP");
            return;
        }
        
        currentXP += amount;
        Debug.Log($"Zdobyto {amount} XP. Aktualnie: {currentXP}/{xpToNextLevel} XP");
        
        CheckLevelUp();
        
        OnXPChanged?.Invoke(currentXP, xpToNextLevel);
    }
    
    private void CheckLevelUp()
    {
        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            currentLevel++;
            
            xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpMultiplier);
            
            Debug.Log($"<color=yellow>POZIOM WZRÓSŁ! Nowy poziom: {currentLevel}</color>");
            OnLevelChanged?.Invoke(currentLevel);
            OnXPChanged?.Invoke(currentXP, xpToNextLevel);
        }
    }
    
    public void SetLevel(int newLevel)
    {
        if (newLevel < 1)
        {
            Debug.LogWarning("Poziom nie może być mniejszy niż 1");
            return;
        }
        
        currentLevel = newLevel;
        currentXP = 0;
        xpToNextLevel = 100;
        for (int i = 1; i < currentLevel; i++)
        {
            xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpMultiplier);
        }
        
        OnLevelChanged?.Invoke(currentLevel);
        OnXPChanged?.Invoke(currentXP, xpToNextLevel);
    }

    public void ResetStats()
    {
        currentMoney = 0;
        currentLevel = 1;
        currentXP = 0;
        xpToNextLevel = 100;
        
        OnMoneyChanged?.Invoke(currentMoney);
        OnLevelChanged?.Invoke(currentLevel);
        OnXPChanged?.Invoke(currentXP, xpToNextLevel);
        
        Debug.Log("Statystyki gracza zresetowane");
    }
    
}
