using UnityEngine;
using System.Collections.Generic;
using KillerPrices.Interaction;
using KillerPrices.Data;
using KillerPrices.Systems;
using KillerPrices.UI;
using System.Linq;

namespace KillerPrices.Shop
{
    [System.Serializable]
    public class ShelfSlot
    {
        public Transform point; // Punkt w przestrzeni (ItemSpot)
        public GunData currentItem;
        
        // Refactored to use Global PriceManager
        public int currentPrice => (currentItem != null && PriceManager.Instance != null) 
                                    ? PriceManager.Instance.GetPrice(currentItem) 
                                    : 0;

        public GameObject visualModelInstance;

        public bool IsOccupied => currentItem != null;

        public void Place(GunData gun)
        {
            currentItem = gun;
            // currentPrice is now dynamic, no need to set
            
            if (visualModelInstance != null) Object.Destroy(visualModelInstance);
                visualModelInstance = Object.Instantiate(gun.modelPrefab, point.position, point.rotation, point);
                
                // "taką skale jak wpisze w gun data taka ma być ustawiana" (Absolute Scale)
                visualModelInstance.transform.localScale = Vector3.one * gun.shelfDisplayScale;
                
                // "dodaj też ustawianie rotacji w wszystkich osiach"
                // Ustawiamy rotację lokalną względem punktu zaczepienia (ItemSpot)
                visualModelInstance.transform.localRotation = Quaternion.Euler(gun.shelfDisplayRotation);
        }

        public void Clear()
        {
            if (visualModelInstance != null) Object.Destroy(visualModelInstance);
            currentItem = null;
            // currentPrice becomes 0 automatically via property
        }
    }

    public class Shelf : MonoBehaviour, IInteractable
    {
        public static List<Shelf> AllShelves = new List<Shelf>();

        [Header("Konfiguracja")]
        [SerializeField] private List<ShelfSlot> slots = new List<ShelfSlot>();

        private void OnEnable()
        {
            AllShelves.Add(this);
        }

        private void OnDisable()
        {
            AllShelves.Remove(this);
        }

        // Znajdź slot z konkretnym przedmiotem na tej półce
        public ShelfSlot FindSlotWithItem(GunData item)
        {
            return slots.FirstOrDefault(s => s.IsOccupied && s.currentItem == item);
        }

        // Statyczna metoda pomocnicza dla AI
        public static ShelfSlot FindAnyShelfSlotWithItem(GunData item)
        {
            foreach (var shelf in AllShelves)
            {
                var slot = shelf.FindSlotWithItem(item);
                if (slot != null) return slot;
            }
            return null;
        }
        
        public void Interact()
        {
            if (PlayerInventory.Instance == null) return;

            GunData handItem = PlayerInventory.Instance.GetSelectedItem();

            // Scenariusz 1: Gracz kładzie przedmiot (ma broń w ręku)
            if (handItem != null && handItem.itemType == ItemType.Weapon)
            {
                ShelfSlot emptySlot = GetBestEmptySlot();
                if (emptySlot != null)
                {
                    emptySlot.Place(handItem);
                    PlayerInventory.Instance.RemoveSelectedItem();
                    string msg = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetTranslation("PLACE_PLACED") : "Położono na półce.";
                    Debug.Log($"{msg} ({handItem.displayName})");
                }
                else
                {
                    string msg = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetTranslation("SHELF_FULL") : "Ta półka jest pełna!";
                    Debug.Log(msg);
                }
            }
            // Scenariusz 2: Gracz wchodzi w interakcję z przedmiotem (ma pustą rękę) -> Otwórz Menu Cen
            else if (handItem == null)
            {
                ShelfSlot occupiedSlot = GetBestOccupiedSlot();
                if (occupiedSlot != null)
                {
                    if (PricingUI.Instance != null)
                    {
                        PricingUI.Instance.Open(occupiedSlot);
                    }
                    else
                    {
                        // Fallback: Jeśli brak UI, po prostu zabierz (jak dawniej)
                        if (PlayerInventory.Instance.AddItem(occupiedSlot.currentItem))
                        {
                            occupiedSlot.Clear();
                        }
                    }
                }
                else
                {
                    string msg = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetTranslation("SHELF_EMPTY") : "Ta półka jest pusta.";
                    Debug.Log(msg);
                }
            }
        }

        public string GetInteractionPrompt()
        {
            return "INTERACT_SHELF";
        }

        private ShelfSlot GetBestEmptySlot()
        {
            // Na razie bierzemy pierwszy wolny.
            // W przyszłości można sortować po Vector3.Distance od gracza
            return slots.FirstOrDefault(s => !s.IsOccupied);
        }

        private ShelfSlot GetBestOccupiedSlot()
        {
            // Bierzemy pierwszy zajęty.
             // W przyszłości można sortować po Vector3.Distance od gracza
            return slots.FirstOrDefault(s => s.IsOccupied);
        }

        // Pomocnicze dla Editora (automatyczne znajdowanie Spotów)
        [ContextMenu("Auto-Find Spots")]
        private void AutoFindSpots()
        {
            slots.Clear();
            foreach (Transform child in transform)
            {
                if (child.name.StartsWith("ItemSpot"))
                {
                    slots.Add(new ShelfSlot { point = child });
                }
            }
        }
        // --- Save System Integration ---

        public ShelfSaveData GetShelfData(int index)
        {
            var data = new ShelfSaveData
            {
                shelfIndex = index
            };

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsOccupied)
                {
                    // VALIDATION: Check if ID is empty
                    string itemId = slots[i].currentItem.id;
                    if (string.IsNullOrEmpty(itemId))
                    {
                        itemId = slots[i].currentItem.name; // Use asset name as fallback
                        Debug.LogWarning($"Shelf: Item '{slots[i].currentItem.displayName}' in slot {i} has empty ID! Using asset name '{itemId}'. Please set the ID field in the Inspector!");
                    }
                    
                    data.slots.Add(new ShelfSlotSaveData
                    {
                        slotIndex = i,
                        itemID = itemId
                    });
                }
            }
            return data;
        }

        public void RestoreShelfData(ShelfSaveData data, ItemDatabase db)
        {
            if (data == null) return;

            // Clear first
            foreach (var slot in slots) slot.Clear();

            foreach (var slotData in data.slots)
            {
                if (slotData.slotIndex >= 0 && slotData.slotIndex < slots.Count)
                {
                    var item = db.GetItem(slotData.itemID);
                    if (item != null)
                    {
                        slots[slotData.slotIndex].Place(item);
                    }
                }
            }
        }
    }
}
