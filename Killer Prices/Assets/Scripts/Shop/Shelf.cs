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
            if (gun.modelPrefab != null && point != null)
            {
                visualModelInstance = Object.Instantiate(gun.modelPrefab, point.position, point.rotation, point);
            }
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
                    Debug.Log($"Położono {handItem.displayName} na półce.");
                }
                else
                {
                    Debug.Log("Ta półka jest pełna!");
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
                    Debug.Log("Ta półka jest pusta.");
                }
            }
        }

        public string GetInteractionPrompt()
        {
            // Prosta logika podpowiedzi
            if (PlayerInventory.Instance != null && PlayerInventory.Instance.GetSelectedItem() != null)
            {
                return "Połóż przedmiot";
            }
            else
            {
                return "Zdejmij przedmiot";
            }
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
    }
}
