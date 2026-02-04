using UnityEngine;
using System.Collections.Generic;
using KillerPrices.AI;
using KillerPrices.Data;
using KillerPrices.Systems;

namespace KillerPrices.Shop
{
    public class CheckoutManager : MonoBehaviour
    {
        public static CheckoutManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private CheckoutUI checkoutUI;
        [SerializeField] private Transform[] itemSpawnPoints; // Points on counter to spawn items
        [SerializeField] private Camera checkoutCamera; // Optional: separate camera or move main camera

        [Header("Settings")]
        [SerializeField] private float tipPercentage = 0.1f;

        // State
        private CustomerController currentCustomer;
        private List<CheckoutItem> spawnedItems = new List<CheckoutItem>();
        private List<GunData> scannedItems = new List<GunData>();
        
        private int totalCost;
        private int cashGiven;
        private bool isCheckoutActive = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void StartCheckout(CustomerController customer)
        {
            if (isCheckoutActive) return;

            currentCustomer = customer;
            isCheckoutActive = true;
            
            // 1. Setup Data
            totalCost = customer.TotalAgreedPrice;
            scannedItems.Clear();
            
            // 2. Generate "Cash Given" (Total + Random change, usually rounded like 50, 100)
            int roundUp = Mathf.CeilToInt(totalCost / 10.0f) * 10;
            cashGiven = roundUp + Random.Range(0, 5) * 10; // e.g. Cost 125, Given 130, 140, etc.

            // 3. Spawn Visuals
            SpawnItems(customer.Basket);

            // 4. Open UI
            if (checkoutUI != null)
            {
                Debug.Log("CheckoutManager: Showing UI.");
                checkoutUI.Show();
                checkoutUI.UpdateList(scannedItems); // Empty at start
                checkoutUI.UpdateTotals(0, cashGiven);
            }
            else
            {
                Debug.LogError("CheckoutManager: CheckoutUI reference is missing!");
            }
            
            // 5. Block Player & Show Cursor
            if (checkoutCamera) checkoutCamera.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0.1f; // Slow down game but not pause completely? Or just Pause? Let's keep normal time for now, or Pause.
            // Actually, keep normal time so queue moves behind? But player is stuck.
            // Let's just block input.
        }

        private void SpawnItems(List<GunData> items)
        {
            // Clear old
            foreach (var item in spawnedItems) if (item) Destroy(item.gameObject);
            spawnedItems.Clear();

            // Spawn new
            for (int i = 0; i < items.Count; i++)
            {
                if (i >= itemSpawnPoints.Length) break; // Limit visually

                Transform point = itemSpawnPoints[i];
                var data = items[i];
                
                // Use model prefab or a placeholder/box if null
                GameObject prefab = data.modelPrefab; 
                if(prefab == null) continue; // Or spawn default box

                var go = Instantiate(prefab, point.position, point.rotation);
                // Randomize slightly
                go.transform.Rotate(0, Random.Range(-45, 45), 0);
                
                var script = go.AddComponent<CheckoutItem>();
                script.Initialize(data);
                
                // Add Collider if missing (needed for click)
                if (go.GetComponent<Collider>() == null) go.AddComponent<BoxCollider>();

                spawnedItems.Add(script);
            }
        }

        public void OnItemClicked(CheckoutItem item)
        {
            if (!isCheckoutActive) return;

            item.MarkAsScanned();
            scannedItems.Add(item.Data);
            
            // Get Dynamic Price
            int currentPrice = item.Data.baseSellPrice;
            if (PriceManager.Instance != null)
            {
                currentPrice = PriceManager.Instance.GetPrice(item.Data);
            }

            if (checkoutUI)
            {
                checkoutUI.AddItemToFeed(item.Data.displayName, currentPrice); 
                checkoutUI.UpdateTotals(CalculateScannedTotal(), cashGiven);
            }

            CheckAllScanned();
        }
        
        private int CalculateScannedTotal()
        {
            int sum = 0;
            if (PriceManager.Instance != null)
            {
                foreach(var s in scannedItems) sum += PriceManager.Instance.GetPrice(s);
            }
            else
            {
                foreach(var s in scannedItems) sum += s.baseSellPrice;
            }
            return sum;
        }

        private void CheckAllScanned()
        {
            if (scannedItems.Count >= currentCustomer.Basket.Count)
            {
                // All scanned! Enable payment button in UI
                if (checkoutUI) checkoutUI.EnablePayment();
            }
        }

        public void FinishTransaction(int playerInputChange)
        {
            int correctChange = cashGiven - totalCost;
            
            // Validate
            if (playerInputChange == correctChange)
            {
                // Success
                PlayerStats.Instance.AddMoney(totalCost);
                PlayerStats.Instance.AddXP(50 + (scannedItems.Count * 10));
                currentCustomer.LeaveStore(true);
                CloseCheckout();
            }
            else
            {
                // Block incorrect change
                Debug.Log($"Błędna reszta! Wpisano: {playerInputChange}, Oczekiwano: {correctChange}");
                
                // TODO: Add visual feedback in UI (e.g. Flash Red)
                // For now, just don't close the checkout
            }
        }

        public void CancelCheckout()
        {
            // Abort
            if(currentCustomer) currentCustomer.LeaveStore(false);
            CloseCheckout();
        }

        private void CloseCheckout()
        {
            isCheckoutActive = false;
            if (checkoutUI) checkoutUI.Hide();
            if (checkoutCamera) checkoutCamera.gameObject.SetActive(false);
            
            // Clear items
             foreach (var item in spawnedItems) if (item) Destroy(item.gameObject);
            spawnedItems.Clear();

            // Resume
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1.0f;
        }
    }
}
