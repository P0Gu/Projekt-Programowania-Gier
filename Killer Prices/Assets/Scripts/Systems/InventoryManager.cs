using UnityEngine;
using System.Collections.Generic;
using System;
using KillerPrices.Data;

namespace KillerPrices.Systems
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        // Słownik: Broń -> Ilość sztuk
        private Dictionary<GunData, int> gunStock = new Dictionary<GunData, int>();

        public event Action OnInventoryChanged;

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

        public void AddStock(GunData gun, int amount)
        {
            if (gun == null) return;
            if (amount <= 0) return;

            if (gunStock.ContainsKey(gun))
            {
                gunStock[gun] += amount;
            }
            else
            {
                gunStock.Add(gun, amount);
            }
            
            Debug.Log($"Dodano {amount}szt. {gun.displayName}. Obecnie: {gunStock[gun]}");
            OnInventoryChanged?.Invoke();
        }

        public bool RemoveStock(GunData gun, int amount)
        {
            if (gun == null) return false;
            
            if (gunStock.ContainsKey(gun) && gunStock[gun] >= amount)
            {
                gunStock[gun] -= amount;
                if (gunStock[gun] < 0) gunStock[gun] = 0; // Zabezpieczenie
                
                OnInventoryChanged?.Invoke();
                return true;
            }
            return false;
        }

        public int GetStockCount(GunData gun)
        {
            if (gun != null && gunStock.ContainsKey(gun))
            {
                return gunStock[gun];
            }
            return 0;
        }
    }
}
