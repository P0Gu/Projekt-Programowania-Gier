using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using KillerPrices.Data;
using KillerPrices.Systems; // Dla InventoryManager i DeliveryManager
using System;

namespace KillerPrices.UI
{
    public class WholesaleUI : MonoBehaviour
    {
        [Header("UI Toolkit")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private List<GunData> availableGuns;

        private Controls playerControls; // Referencja do skryptu gracza
        private UnityEngine.InputSystem.PlayerInput playerInput; // Referencja do nowego systemu inputu

        private VisualElement root;
        private VisualElement container;
        private ScrollView productList;
        private Button closeButton;
        
        private Button weaponsTab;
        private Button furnitureTab;

        private ItemType currentTab = ItemType.Weapon; // Default Tab

        private void OnEnable()
        {
            InitializeUI();

            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
            }
        }

        private void OnDisable()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        private void Start()
        {
            // Diagnostyka i Konfiguracja PanelSettings (Auto-Scaling)
            if (uiDocument != null)
            {
                if (uiDocument.panelSettings == null)
                {
                    Debug.LogError("⛔ UIDOCUMENT NIE MA PANEL SETTINGS! Przypisz domyślny PanelSettings w Inspektorze.");
                }
                else
                {
                    // AUTO-FIX: Wymuś skalowanie z ekranem
                    uiDocument.panelSettings.scaleMode = UnityEngine.UIElements.PanelScaleMode.ScaleWithScreenSize;
                    uiDocument.panelSettings.referenceResolution = new Vector2Int(1920, 1080);
                    uiDocument.panelSettings.match = 0.5f; // Balance width/height
                    uiDocument.panelSettings.screenMatchMode = UnityEngine.UIElements.PanelScreenMatchMode.MatchWidthOrHeight;
                }
            }
            
            // Diagnostyka EventSystemu
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                Debug.LogError("⛔ BRAK EVENTSYSTEMU W SCENIE! Utwórz go: PPM -> UI -> Event System.");
            }
            else
            {
                var inputModule = UnityEngine.EventSystems.EventSystem.current.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                if (inputModule == null)
                {
                    Debug.LogError("⛔ EVENTSYSTEM MA ZŁY MODUŁ! Kliknij 'Replace with InputSystemUIInputModule' na obiekcie EventSystem.");
                }
            }

            InitializeUI(); // Fallback in case OnEnable ran before UIDocument was ready
            
            playerControls = FindFirstObjectByType<Controls>();
            playerInput = FindFirstObjectByType<UnityEngine.InputSystem.PlayerInput>();
            
            // Domyślnie ukryj po inicjalizacji
            Invoke(nameof(Close), 0.1f); 
        }

        private void InitializeUI()
        {
            if (root != null) return; // Already initialized

            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            if (uiDocument != null)
            {
                root = uiDocument.rootVisualElement;
                if (root == null) return; 

                /* Interaction Debugging
                if (root != null)
                {
                    root.RegisterCallback<PointerDownEvent>(evt => Debug.Log($"UI Received PointerDown on: {evt.target}"));
                }
                */

                container = root.Q<VisualElement>("Container");
                productList = root.Q<ScrollView>("ProductList");
                
                closeButton = root.Q<Button>("CloseButton");
                if (closeButton != null)
                {
                    // FORCE INTERACTIVITY
                    closeButton.pickingMode = PickingMode.Position;
                    closeButton.BringToFront();
                    closeButton.clicked += Close;
                }

                weaponsTab = root.Q<Button>("WeaponsTab");
                furnitureTab = root.Q<Button>("FurnitureTab");

                if (weaponsTab != null) weaponsTab.clicked += () => SwitchTab(ItemType.Weapon);
                if (furnitureTab != null) furnitureTab.clicked += () => SwitchTab(ItemType.Furniture);

                // Initial text update
                UpdateTexts();
            }
        }

        private void OnLanguageChanged()
        {
            UpdateTexts();
            RefreshList();
        }

        private void UpdateTexts()
        {
            if (LocalizationManager.Instance != null)
            {
                 if (weaponsTab != null) weaponsTab.text = LocalizationManager.Instance.GetTranslation("WS_TAB_WEAPONS");
                 if (furnitureTab != null) furnitureTab.text = LocalizationManager.Instance.GetTranslation("WS_TAB_FURNITURE");
            }
        }

        public void SwitchTab(ItemType type)
        {
            currentTab = type;
            RefreshList();
            UpdateTabVisuals();
        }

        private void UpdateTabVisuals()
        {
            if (weaponsTab == null || furnitureTab == null) return;

            weaponsTab.RemoveFromClassList("active");
            furnitureTab.RemoveFromClassList("active");

            if (currentTab == ItemType.Weapon) weaponsTab.AddToClassList("active");
            else furnitureTab.AddToClassList("active");
        }

        private void Update()
        {
            // Awaryjne wyjście Escape
            if (container != null && container.style.display == DisplayStyle.Flex)
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                {
                    Close();
                }
            }
        }

        public void Toggle()
        {
            if (container != null)
            {
                bool isVisible = container.style.display == DisplayStyle.Flex;
                if (isVisible) Close(); else Open();
            }
        }

        public void Open()
        {
            if (container == null) return;

            // Blokada gracza (Legacy / Custom)
            if (playerControls != null) playerControls.IsLocked = true;
            
            // Ukryj Hotbar
            var hotbar = FindFirstObjectByType<HotbarUI>();
            if (hotbar != null) hotbar.SetVisible(false);

            container.style.display = DisplayStyle.Flex;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;

            SwitchTab(currentTab); // Refresh and highlight
        }

        public void Close()
        {
            if (container == null) return;

            container.style.display = DisplayStyle.None;
            
            // Odblokowanie gracza (Legacy / Custom)
            if (playerControls != null) playerControls.IsLocked = false;
            
            // Pokaż Hotbar
            var hotbar = FindFirstObjectByType<HotbarUI>();
            if (hotbar != null) hotbar.SetVisible(true);

            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }

        private void RefreshList()
        {
            if (productList == null) return;

            productList.Clear();
            int playerLevel = PlayerStats.Instance != null ? PlayerStats.Instance.Level : 1;
            
            // Safe access to translation
            var loc = LocalizationManager.Instance;

            foreach (var gun in availableGuns)
            {
                if (gun == null) continue;
                if (gun.itemType != currentTab) continue; // Filter by Tab

                // Create Card
                var card = new VisualElement();
                card.AddToClassList("item-card");

                // Check Level Lock
                bool isLocked = gun.requiredLevel > playerLevel;
                if (isLocked) card.AddToClassList("locked");

                // Icon
                var icon = new VisualElement();
                icon.AddToClassList("item-icon");
                if (gun.icon != null) icon.style.backgroundImage = new StyleBackground(gun.icon);
                card.Add(icon);

                // Name
                var nameLabel = new Label(gun.displayName);
                nameLabel.AddToClassList("item-name");
                card.Add(nameLabel);

                // Price (Cost)
                var priceLabel = new Label($"Koszt: ${gun.baseCost}");
                priceLabel.AddToClassList("item-price");
                card.Add(priceLabel);

                // Current Sell Price (New Feature)
                int currentSellPrice = gun.baseSellPrice;
                if (PriceManager.Instance != null)
                {
                    currentSellPrice = PriceManager.Instance.GetPrice(gun);
                }
                
                var sellPriceLabel = new Label($"Cena: ${currentSellPrice}");
                sellPriceLabel.style.fontSize = 14;
                sellPriceLabel.style.color = new StyleColor(new Color(0.6f, 1f, 0.6f)); // Light Green
                card.Add(sellPriceLabel);

                // Buy Button logic
                var buyBtn = new Button();
                buyBtn.AddToClassList("buy-button");

                if (isLocked)
                {
                    string lockedText = loc != null ? loc.GetTranslation("WS_LOCKED") : "LV.";
                    buyBtn.text = $"{lockedText} {gun.requiredLevel}";
                    buyBtn.SetEnabled(false);
                    
                    var lockLabel = new Label("LOCKED");
                    lockLabel.AddToClassList("locked-label");
                    card.Add(lockLabel);
                }
                else
                {
                    buyBtn.text = loc != null ? loc.GetTranslation("WS_BUY") : "KUP";
                    buyBtn.clicked += () => BuyGun(gun);
                }
                
                card.Add(buyBtn);
                productList.Add(card);
            }
        }

        private void BuyGun(GunData gun)
        {
            if (PlayerStats.Instance != null && DeliveryManager.Instance != null)
            {
                if (PlayerStats.Instance.SpendMoney(gun.baseCost))
                {
                    DeliveryManager.Instance.SpawnOrder(gun);
                    Debug.Log($"Zamówiono: {gun.displayName}");
                }
                else
                {
                    Debug.Log("Za mało pieniędzy!");
                }
            }
        }
    }
}
