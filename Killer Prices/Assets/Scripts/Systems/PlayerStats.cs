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
        }

        private void Start()
        {
            // Wymuś odświeżenie UI na starcie
            NotifyStatsChanged();
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

        public void Save()
        {
            // Stats
            PlayerPrefs.SetInt("Stats_Money", Money);
            PlayerPrefs.SetInt("Stats_Level", Level);
            PlayerPrefs.SetFloat("Stats_XP", CurrentXP);

            // Player Position & Rotation
            var player = FindFirstObjectByType<Controls>();
            if (player != null)
            {
                Vector3 pos = player.transform.position;
                PlayerPrefs.SetFloat("Player_PosX", pos.x);
                PlayerPrefs.SetFloat("Player_PosY", pos.y);
                PlayerPrefs.SetFloat("Player_PosZ", pos.z);

                // Save Camera Rotation (Head) if possible, or Player Rotation (Body)
                // Assuming Camera.main is the head
                if (Camera.main != null)
                {
                    Quaternion rot = Camera.main.transform.rotation;
                    PlayerPrefs.SetFloat("Player_RotX", rot.x);
                    PlayerPrefs.SetFloat("Player_RotY", rot.y);
                    PlayerPrefs.SetFloat("Player_RotZ", rot.z);
                    PlayerPrefs.SetFloat("Player_RotW", rot.w);
                }
            }

            PlayerPrefs.Save();
            Debug.Log("Game Saved!");
        }

        public void Load()
        {
            Money = PlayerPrefs.GetInt("Stats_Money", startMoney);
            Level = PlayerPrefs.GetInt("Stats_Level", startLevel);
            CurrentXP = PlayerPrefs.GetFloat("Stats_XP", 0f);
            MaxXP = CalculateMaxXP(Level);
            
            NotifyStatsChanged();
            Debug.Log("Game Loaded!");
        }

        // Helper to apply position after scene load
        public void ApplySavedState()
        {
            if (!PlayerPrefs.HasKey("Player_PosX")) return;

            var player = FindFirstObjectByType<Controls>();
            if (player != null)
            {
                float x = PlayerPrefs.GetFloat("Player_PosX");
                float y = PlayerPrefs.GetFloat("Player_PosY");
                float z = PlayerPrefs.GetFloat("Player_PosZ");
                
                // Disable CharacterController to teleport
                var cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                
                player.transform.position = new Vector3(x, y, z);
                
                if (cc != null) cc.enabled = true;

                // Rotation
                if (Camera.main != null && PlayerPrefs.HasKey("Player_RotX"))
                {
                   float rx = PlayerPrefs.GetFloat("Player_RotX");
                   float ry = PlayerPrefs.GetFloat("Player_RotY");
                   float rz = PlayerPrefs.GetFloat("Player_RotZ");
                   float rw = PlayerPrefs.GetFloat("Player_RotW");
                   
                   Camera.main.transform.rotation = new Quaternion(rx, ry, rz, rw);
                   
                   // Also sync body Y rotation if needed
                   // Vector3 euler = Camera.main.transform.rotation.eulerAngles;
                   // player.transform.rotation = Quaternion.Euler(0, euler.y, 0); 
                }
            }
        }

        private void NotifyStatsChanged()
        {
            OnStatsChanged?.Invoke(Money, Level, CurrentXP, MaxXP);
        }
    }
}
