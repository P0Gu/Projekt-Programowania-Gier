using UnityEngine;
using System.Collections.Generic;
using KillerPrices.Data;

namespace KillerPrices.Systems
{
    public class PriceManager : MonoBehaviour
    {
        public static PriceManager Instance 
        { 
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<PriceManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("PriceManager");
                        _instance = go.AddComponent<PriceManager>();
                        Debug.Log("PriceManager: Auto-created instance.");
                    }
                }
                return _instance;
            }
        }
        private static PriceManager _instance;

        private Dictionary<GunData, int> itemPrices = new Dictionary<GunData, int>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject); // Optional: keep across scenes
        }

        public int GetPrice(GunData item)
        {
            if (item == null) return 0;

            if (itemPrices.TryGetValue(item, out int price))
            {
                return price;
            }

            // If price not set, return base sell price
            return item.baseSellPrice;
        }

        public void SetPrice(GunData item, int newPrice)
        {
            if (item == null) return;

            if (itemPrices.ContainsKey(item))
            {
                itemPrices[item] = newPrice;
            }
            else
            {
                itemPrices.Add(item, newPrice);
            }
            
            Debug.Log($"PriceManager: Cena dla {item.displayName} ustawiona na ${newPrice}");
        }
    }
}
