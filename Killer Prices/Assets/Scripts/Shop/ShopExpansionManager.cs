using UnityEngine;
using KillerPrices.AI;

namespace KillerPrices.Shop
{
    public class ShopExpansionManager : MonoBehaviour
    {
        public static ShopExpansionManager Instance { get; private set; }

        private const string EXPANSION_SAVE_KEY = "ShopExpansionsCount";
        private int expansionsPurchased = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadState();
        }

        public void RegisterPurchase(string expansionId)
        {
            // Note: expansionId can be used for specific save flags if needed in future.
            // For now we just count total.
            
            expansionsPurchased++;
            SaveState();

            Debug.Log($"ShopExpansionManager: Rozbudowa kupiona! Razem: {expansionsPurchased}");

            // Check for bonuses
            // Every 3 expansions -> +1 Customer Capacity
            if (expansionsPurchased % 3 == 0)
            {
                IncreaseShopCapacity();
            }
        }

        public bool IsPurchased(string expansionId)
        {
            // Simple check using PlayerPrefs for specific ID if we wanted per-wall persistence
            return PlayerPrefs.GetInt($"Expansion_{expansionId}", 0) == 1;
        }

        public void SetPurchased(string expansionId)
        {
            PlayerPrefs.SetInt($"Expansion_{expansionId}", 1);
            RegisterPurchase(expansionId);
        }

        private void IncreaseShopCapacity()
        {
            var spawner = FindFirstObjectByType<CustomerSpawner>();
            if (spawner != null)
            {
                spawner.IncreaseCapacity(1);
                Debug.Log("ShopExpansionManager: Zwiększono limit klientów sklepu!");
            }
        }

        private void SaveState()
        {
            PlayerPrefs.SetInt(EXPANSION_SAVE_KEY, expansionsPurchased);
            PlayerPrefs.Save();
        }

        private void LoadState()
        {
            expansionsPurchased = PlayerPrefs.GetInt(EXPANSION_SAVE_KEY, 0);
            
            // Re-apply capacity bonuses based on loaded count
            // Initial capacity is e.g. 5. If we have 6 expansions, we need +2 capacity.
            int bonusCapacity = expansionsPurchased / 3;
            if (bonusCapacity > 0)
            {
                var spawner = FindFirstObjectByType<CustomerSpawner>();
                if (spawner != null)
                {
                    // We might need a method to set BASE + BONUS, but IncreaseCapacity works if called incrementally
                    // Actually, safer to have SetBonusCapacity in Spawner.
                    // For now, let's just log it. Spawner needs to know this on Start too.
                    spawner.IncreaseCapacity(bonusCapacity); 
                }
            }
        }
    }
}
