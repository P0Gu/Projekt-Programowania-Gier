using UnityEngine;
using System.Collections.Generic;
using KillerPrices.Data;
using KillerPrices.Systems;
using KillerPrices.Shop; // FIX: Import Shop namespace

namespace KillerPrices.AI
{
    public class CustomerSpawner : MonoBehaviour
    {
        // ... (pola bez zmian)
        [SerializeField] private CustomerController customerPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform counterPoint;
        [SerializeField] private Transform exitPoint;

        [SerializeField] private float spawnInterval = 10f;
        [SerializeField] private List<GunData> potentialRequests;
        [SerializeField] private int maxCustomers = 5;
        private int currentCustomers = 0;
        private float timer;

        private void Update()
        {
            if (currentCustomers >= maxCustomers) return;

            // NEW: Check if shop is open
            if (ShopManager.Instance != null && !ShopManager.Instance.IsOpen) return;

            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                SpawnCustomer();
                timer = 0;
            }
        }

        private void SpawnCustomer()
        {
            if (potentialRequests == null || potentialRequests.Count == 0) return;
            if (counterPoint == null || exitPoint == null) return;

            // Filtrowanie listy dostępnych przedmiotów dla klienta (wg poziomu gracza)
            int playerLevel = (PlayerStats.Instance != null) ? PlayerStats.Instance.Level : 1;
            
            var unlockedItems = potentialRequests.FindAll(item => item.requiredLevel <= playerLevel);

            GunData selectedItem;
            if (unlockedItems.Count > 0)
            {
                selectedItem = unlockedItems[Random.Range(0, unlockedItems.Count)];
            }
            else
            {
                // Fallback: Jeśli nic nie pasuje (np. błąd danych), weź cokolwiek z pełnej listy
                Debug.LogWarning("CustomerSpawner: Brak odblokowanych przedmiotów dla poziomu " + playerLevel + ". Używam losowego przedmiotu.");
                selectedItem = potentialRequests[Random.Range(0, potentialRequests.Count)];
            }

            var customer = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
            
            customer.Initialize(selectedItem, counterPoint, exitPoint, OnCustomerLeft);
            currentCustomers++;
            Debug.Log($"Nowy klient (chce: {selectedItem.displayName}). W sklepie: {currentCustomers}/{maxCustomers}");
        }

        private void OnCustomerLeft(CustomerController c)
        {
            currentCustomers--;
            if (currentCustomers < 0) currentCustomers = 0;
            Debug.Log($"Klient wyszedł. W sklepie: {currentCustomers}/{maxCustomers}");
        }

        public void IncreaseCapacity(int amount)
        {
            maxCustomers += amount;
            Debug.Log($"CustomerSpawner: Limit klientów zwiększony o {amount}. Nowy limit: {maxCustomers}");
        }
    }
}
