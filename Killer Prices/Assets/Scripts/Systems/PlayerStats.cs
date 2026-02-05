using UnityEngine;
using System;

namespace KillerPrices.Systems
{
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        [Header("Konfiguracja Startowa")]
        [SerializeField] private int startMoney = 1000;
        [SerializeField] private int startLevel = 1;

        // Dane publiczne (do odczytu)
        public int Money { get; private set; }
        public int Level { get; private set; }
        public float CurrentXP { get; private set; }
        public float MaxXP { get; private set; } = 100f;

        // Event: Informuje UI o zmianach (Money, Level, currentXP, maxXP)
        public event Action<int, int, float, float> OnStatsChanged;

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
                return;
            }

            // Inicjalizacja
            Money = startMoney;
            Level = startLevel;
            MaxXP = CalculateMaxXP(Level);
            Debug.Log($"PlayerStats: Awake. Initialized with Default Money: {Money}");
        }

        private void Start()
        {
            // Wymuś odświeżenie UI na starcie
            NotifyStatsChanged();
            Debug.Log($"PlayerStats: Start. Current Money: {Money}");
        }

        // ...

        public void LoadFromSaveData(GameSaveData data)
        {
            Debug.Log($"PlayerStats: Loading Data... Old Money: {Money}, New Money: {data.money}");
            Money = data.money;
            Level = data.level;
            CurrentXP = data.currentXP;
            MaxXP = CalculateMaxXP(Level);
            
            NotifyStatsChanged();
            Debug.Log($"PlayerStats: Data Loaded. Money is now: {Money}");
        }

        public void AddMoney(int amount)
        {
            Money += amount;
            NotifyStatsChanged();
        }

        public bool SpendMoney(int amount)
        {
            if (Money >= amount)
            {
                Money -= amount;
                NotifyStatsChanged();
                return true;
            }
            return false;
        }

        public void AddXP(float amount)
        {
            CurrentXP += amount;
            while (CurrentXP >= MaxXP)
            {
                LevelUp();
            }
            NotifyStatsChanged();
        }

        private void LevelUp()
        {
            CurrentXP -= MaxXP;
            Level++;
            MaxXP = CalculateMaxXP(Level);
            // Tu można dodać efekty dźwiękowe / wizualne awansu
            Debug.Log($"Level Up! New Level: {Level}");
        }

        private float CalculateMaxXP(int level)
        {
            // Prosta formuła: lvl 1 = 100, lvl 2 = 120, itd.
            return 100f * Mathf.Pow(1.2f, level - 1);
        }



        // Helper to apply position after scene load is now deprecated or handled elsewhere, 
        // as SaveManager doesn't focus on player position in this iteration yet.

        private void NotifyStatsChanged()
        {
            OnStatsChanged?.Invoke(Money, Level, CurrentXP, MaxXP);
        }
    }
}
