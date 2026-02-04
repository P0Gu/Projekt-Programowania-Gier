using UnityEngine;
using UnityEngine.InputSystem;
using System;
using KillerPrices.Data;

namespace KillerPrices.Systems
{
    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory Instance { get; private set; }

        [Header("Konfiguracja")]
        [SerializeField] private int slotsCount = 5;
        public GunData[] slots;
        
        public int SelectedSlotIndex { get; private set; } = 0;

        // Event: (slotIndex, GunData) - wywoływany przy zmianie zawartości lub wyborze
        public event Action<int, GunData> OnSlotChanged;
        public event Action<int> OnSelectionChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                slots = new GunData[slotsCount];
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.digit1Key.wasPressedThisFrame) SelectSlot(0);
            if (keyboard.digit2Key.wasPressedThisFrame) SelectSlot(1);
            if (keyboard.digit3Key.wasPressedThisFrame) SelectSlot(2);
            if (keyboard.digit4Key.wasPressedThisFrame) SelectSlot(3);
            if (keyboard.digit5Key.wasPressedThisFrame) SelectSlot(4);
        }

        public GunData GetSelectedItem()
        {
            if (SelectedSlotIndex >= 0 && SelectedSlotIndex < slots.Length)
            {
                return slots[SelectedSlotIndex];
            }
            return null;
        }

        public void SelectSlot(int index)
        {
            if (index < 0 || index >= slotsCount) return;
            SelectedSlotIndex = index;
            OnSelectionChanged?.Invoke(SelectedSlotIndex);
            Debug.Log($"Ekwipunek: Wybrano slot {index + 1}");

            CheckForFurniture();
        }

        private void CheckForFurniture()
        {
            var item = GetSelectedItem();
            if (item != null && item.itemType == ItemType.Furniture)
            {
                if (KillerPrices.Placement.PlacementManager.Instance != null)
                {
                    Debug.Log("Furniture selected: Starting Placement Mode.");
                    KillerPrices.Placement.PlacementManager.Instance.StartPlacement(item);
                }
            }
            else
            {
                // If we switched to a gun or empty hand, cancel placement
                if (KillerPrices.Placement.PlacementManager.Instance != null)
                {
                    KillerPrices.Placement.PlacementManager.Instance.CancelPlacement();
                }
            }
        }

        public bool AddItem(GunData gun)
        {
            // Najpierw sprawdź, czy wybrany slot jest pusty - jeśli tak, to priorytet
            if (slots[SelectedSlotIndex] == null)
            {
                slots[SelectedSlotIndex] = gun;
                OnSlotChanged?.Invoke(SelectedSlotIndex, gun);
                CheckForFurniture(); // Update state if we picked up something into hand
                return true;
            }

            // Jeśli zajęty, szukaj pierwszego wolnego
            for (int i = 0; i < slotsCount; i++)
            {
                if (slots[i] == null)
                {
                    slots[i] = gun;
                    OnSlotChanged?.Invoke(i, gun);
                    return true;
                }
            }

            Debug.Log("Ekwipunek pełny!");
            return false;
        }

        public GunData GetSelectedGun()
        {
            return slots[SelectedSlotIndex];
        }

        public void RemoveSelectedItem()
        {
            if (slots[SelectedSlotIndex] != null)
            {
                var removedItem = slots[SelectedSlotIndex];
                slots[SelectedSlotIndex] = null;
                OnSlotChanged?.Invoke(SelectedSlotIndex, null);
                Debug.Log($"Usunięto z ekwipunku: {removedItem.displayName}");
            }
        }

        public bool HasItem(GunData gun)
        {
            foreach (var slot in slots)
            {
                if (slot == gun) return true;
            }
            return false;
        }

        public bool RemoveItem(GunData gun, int amount = 1)
        {
            // Na razie system nie wspiera stackowania (amount jest zawsze 1 w logice slotów), 
            // ale dodajemy ten parametr dla zgodności z PlacementManager.
            
            for (int i = 0; i < slotsCount; i++)
            {
                if (slots[i] == gun)
                {
                    slots[i] = null;
                    OnSlotChanged?.Invoke(i, null);
                    if (i == SelectedSlotIndex) CheckForFurniture(); // Update if current item removed
                    return true;
                }
            }
            return false;
        }

        // Metoda wywoływana np. przez PlayerController po kliknięciu LPM
        public void UseCurrentItem()
        {
            var item = GetSelectedItem();
            // Logic for Guns?
            if (item != null && item.itemType == ItemType.Weapon)
            {
                Debug.Log($"Using Weapon: {item.displayName}");
                // Weapon logic here
            }
            
            // Furniture logic is now handled by PlacementManager's internal update loop
            // So we don't start it here.
        }
    }
}
