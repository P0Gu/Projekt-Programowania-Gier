using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using KillerPrices.Data;

namespace KillerPrices.Shop
{
    public class CheckoutUI : MonoBehaviour
    {
        [Header("UI Document")]
        [SerializeField] private UIDocument uiDocument;

        private VisualElement mainContainer;
        private VisualElement root;
        private ScrollView itemsList;
        private Label totalLabel;
        private Label paidLabel;
        private TextField changeInput;
        private Button finishButton;

        private void OnEnable()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
            if (uiDocument != null) root = uiDocument.rootVisualElement;
            
            if (root != null)
            {
                mainContainer = root.Q<VisualElement>("MainContainer"); // FIX: Query specific container
                itemsList = root.Q<ScrollView>("ItemsList");
                totalLabel = root.Q<Label>("TotalLabel");
                paidLabel = root.Q<Label>("PaidLabel");
                changeInput = root.Q<TextField>("ChangeInput");
                finishButton = root.Q<Button>("FinishButton");

                if (finishButton != null) finishButton.clicked += OnFinishClicked;
                
                Debug.Log("CheckoutUI: Initialized.");
                Hide(); 
            }
            else
            {
                Debug.LogError("CheckoutUI: Root Null!");
            }
        }

        public void Show()
        {
            if (mainContainer != null) 
            {
                mainContainer.style.display = DisplayStyle.Flex;
                Debug.Log("CheckoutUI: MainContainer shown.");
            }
            else if (root != null)
            {
                 // Fallback if MainContainer not found but root exists
                 root.style.display = DisplayStyle.Flex;
                 Debug.LogWarning("CheckoutUI: MainContainer not found, showing Root.");
            }
            else
            {
                 Debug.LogError("CheckoutUI: Cannot Show - Container missing.");
            }

            if (finishButton != null) finishButton.SetEnabled(false); 
            if (changeInput != null) changeInput.value = "";
        }

        public void Hide()
        {
            if (mainContainer != null) mainContainer.style.display = DisplayStyle.None;
            else if (root != null) root.style.display = DisplayStyle.None;
        }

        public void UpdateList(List<GunData> items)
        {
            if (itemsList == null) return;
            itemsList.Clear();

            foreach (var item in items)
            {
                AddItemToFeed(item.displayName, item.baseSellPrice);
            }
        }

        public void AddItemToFeed(string name, int price)
        {
            if (itemsList == null) return;

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            
            var nameLbl = new Label(name);
            nameLbl.AddToClassList("item-row-label"); // FIX: Add style class

            var priceLbl = new Label($"${price}");
            priceLbl.AddToClassList("item-row-label"); // FIX: Add style class
            
            row.Add(nameLbl);
            row.Add(priceLbl);
            itemsList.Add(row);
        }

        public void UpdateTotals(int scannedTotal, int cashGiven)
        {
            if (totalLabel != null) totalLabel.text = $"Suma: ${scannedTotal}";
            if (paidLabel != null) paidLabel.text = $"Otrzymano: ${cashGiven}";
        }

        public void EnablePayment()
        {
            if (finishButton != null) finishButton.SetEnabled(true);
        }

        private void OnFinishClicked()
        {
            if (changeInput == null) return;
            
            int change = 0;
            string text = changeInput.value;

            // Treat empty as 0, otherwise try parse
            if (string.IsNullOrEmpty(text))
            {
                change = 0;
            }
            else if (!int.TryParse(text, out change))
            {
                Debug.LogWarning("Invalid change input!");
                return; // Invalid format
            }

            CheckoutManager.Instance.FinishTransaction(change);
        }
    }
}
