using UnityEngine;
using UnityEngine.UIElements;
using KillerPrices.Systems; // Dla PlayerInventory
using KillerPrices.Data;
using System.Collections.Generic;

namespace KillerPrices.UI
{
    public class HotbarUI : MonoBehaviour
    {
        [Header("UI Toolkit")]
        [SerializeField] private UIDocument uiDocument;

        private List<VisualElement> slotsUI = new List<VisualElement>();

        private void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            if (uiDocument != null)
            {
                var root = uiDocument.rootVisualElement;
                
                // Pobierz sloty
                for (int i = 1; i <= 5; i++)
                {
                    var slot = root.Q<VisualElement>($"Slot{i}");
                    if (slot != null) slotsUI.Add(slot);
                }
            }

        }
        
        private void Start()
        {
             if (PlayerInventory.Instance != null)
             {
                 Debug.Log("HotbarUI: Podłączanie do PlayerInventory...");
                 PlayerInventory.Instance.OnSlotChanged += UpdateSlot;
                 PlayerInventory.Instance.OnSelectionChanged += UpdateSelection;
                 
                 // Inicjalizacja stanu początkowego
                 UpdateSelection(PlayerInventory.Instance.SelectedSlotIndex);
                 for (int i = 0; i < PlayerInventory.Instance.slots.Length; i++)
                 {
                     UpdateSlot(i, PlayerInventory.Instance.slots[i]);
                 }
             }
             else
             {
                 Debug.LogError("HotbarUI: PlayerInventory.Instance jest NULL w Start!");
             }
        }

        private void UpdateSlot(int index, GunData gun)
        {
            Debug.Log($"HotbarUI: Aktualizacja slotu {index} -> {(gun != null ? gun.displayName : "PUSTY")}");
            if (index < 0 || index >= slotsUI.Count) return;

            var slot = slotsUI[index];
            var icon = slot.Q<VisualElement>("Icon");
            
            if (icon != null)
            {
                if (gun != null && gun.icon != null)
                {
                    icon.style.backgroundImage = new StyleBackground(gun.icon);
                    icon.style.display = DisplayStyle.Flex;
                }
                else
                {
                    icon.style.backgroundImage = null; // Clear
                    icon.style.display = DisplayStyle.None;
                }
            }
        }

        private void UpdateSelection(int selectedIndex)
        {
            Debug.Log($"HotbarUI: Zmiana zaznaczenia na {selectedIndex}");
            for (int i = 0; i < slotsUI.Count; i++)
            {
                if (i == selectedIndex)
                {
                    slotsUI[i].AddToClassList("selected");
                    var yellow = new StyleColor(Color.yellow);
                    slotsUI[i].style.borderTopColor = yellow;
                    slotsUI[i].style.borderRightColor = yellow;
                    slotsUI[i].style.borderBottomColor = yellow;
                    slotsUI[i].style.borderLeftColor = yellow;
                }
                else
                {
                    slotsUI[i].RemoveFromClassList("selected");
                    var defaultColor = new StyleColor(new Color(0.33f, 0.33f, 0.33f));
                    slotsUI[i].style.borderTopColor = defaultColor;
                    slotsUI[i].style.borderRightColor = defaultColor;
                    slotsUI[i].style.borderBottomColor = defaultColor;
                    slotsUI[i].style.borderLeftColor = defaultColor;
                }
            }
        }
        public void SetVisible(bool isVisible)
        {
            if (uiDocument != null && uiDocument.rootVisualElement != null)
            {
                uiDocument.rootVisualElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
