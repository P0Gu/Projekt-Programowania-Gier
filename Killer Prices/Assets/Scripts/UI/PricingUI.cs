using UnityEngine;
using UnityEngine.UIElements;
using KillerPrices.Shop; // Potrzebne do ShelfSlot
using KillerPrices.Systems; // PlayerInventory

namespace KillerPrices.UI
{
    public class PricingUI : MonoBehaviour
    {
        public static PricingUI Instance { get; private set; }

        [SerializeField] private UIDocument uiDocument;
        
        private VisualElement root;
        private VisualElement container;
        private Label itemNameLabel;
        private Label priceLabel;
        private Label profitLabel;
        
        // Stan
        private ShelfSlot currentSlot;
        private int currentEditPrice;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;

            container = root.Q<VisualElement>("PricingContainer");
            itemNameLabel = root.Q<Label>("ItemNameLabel");
            priceLabel = root.Q<Label>("PriceLabel");
            profitLabel = root.Q<Label>("ProfitLabel");

            root.Q<Button>("IncreasePriceButton").clicked += () => ChangePrice(10); // +$10
            root.Q<Button>("DecreasePriceButton").clicked += () => ChangePrice(-10); // -$10
            root.Q<Button>("ConfirmButton").clicked += ConfirmPrice;
            root.Q<Button>("TakeItemButton").clicked += TakeItem;

            Close(); // Domyślnie zamknięte
        }

        public void Open(ShelfSlot slot)
        {
            if (slot == null || slot.currentItem == null) return;

            currentSlot = slot;
            currentEditPrice = slot.currentPrice;

            itemNameLabel.text = slot.currentItem.displayName;
            UpdateDisplay();

            container.RemoveFromClassList("hidden");
            
            // Odblokuj kursor
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }

        public void Close()
        {
            container.AddToClassList("hidden");
            currentSlot = null;

            // Zablokuj kursor (powrót do gry)
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }

        private void ChangePrice(int amount)
        {
            currentEditPrice += amount;
            if (currentEditPrice < 0) currentEditPrice = 0;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            priceLabel.text = $"${currentEditPrice}";
            
            int baseCost = currentSlot.currentItem.baseCost;
            int profit = currentEditPrice - baseCost;
            
            if (profit >= 0)
            {
                profitLabel.text = $"Zysk: ${profit}";
                profitLabel.style.color = new StyleColor(new Color(0.5f, 1f, 0.5f)); // Green
            }
            else
            {
                profitLabel.text = $"Strata: ${profit}";
                profitLabel.style.color = new StyleColor(new Color(1f, 0.5f, 0.5f)); // Red
            }
        }

        private void ConfirmPrice()
        {
            if (currentSlot != null)
            {
                currentSlot.currentPrice = currentEditPrice;
                Debug.Log($"Ustawiono nową cenę dla {currentSlot.currentItem.displayName}: {currentEditPrice}");
            }
            Close();
        }

        private void TakeItem()
        {
            if (currentSlot != null && PlayerInventory.Instance != null)
            {
                if (PlayerInventory.Instance.AddItem(currentSlot.currentItem))
                {
                    Debug.Log($"Zabrano {currentSlot.currentItem.displayName}");
                    currentSlot.Clear();
                    Close();
                }
                else
                {
                    Debug.Log("Brak miejsca w ekwipunku!");
                }
            }
        }
    }
}
