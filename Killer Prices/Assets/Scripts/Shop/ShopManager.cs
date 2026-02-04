using UnityEngine;
using System;

namespace KillerPrices.Shop
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        public bool IsOpen { get; private set; } = false; // Closed by default

        public event Action<bool> OnShopStatusChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void ToggleShop()
        {
            SetShopStatus(!IsOpen);
        }

        public void SetShopStatus(bool isOpen)
        {
            if (IsOpen != isOpen)
            {
                IsOpen = isOpen;
                OnShopStatusChanged?.Invoke(IsOpen);
                
                string status = IsOpen ? "OTWARTE" : "ZAMKNIĘTE";
                Debug.Log($"Sklep został zmieniony na: {status}");
                
                // Optional: Show prompt or notification on screen
            }
        }
    }
}
