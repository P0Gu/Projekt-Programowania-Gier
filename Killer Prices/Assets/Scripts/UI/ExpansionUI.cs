using UnityEngine;
using UnityEngine.UIElements;
using KillerPrices.Shop;
using KillerPrices.Systems; // For PlayerStats

namespace KillerPrices.UI
{
    public class ExpansionUI : MonoBehaviour
    {
        public static ExpansionUI Instance { get; private set; }

        [Header("UI Document")]
        [SerializeField] private UIDocument uiDocument;

        private VisualElement root;
        private VisualElement container;
        private Label costLabel;
        private Label levelLabel;
        private Button confirmButton;
        private Button cancelButton;

        private ShopExpansionWall currentWall;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
            if (uiDocument != null) 
            {
                root = uiDocument.rootVisualElement;
                container = root.Q<VisualElement>("ExpansionContainer");
                costLabel = root.Q<Label>("CostLabel");
                levelLabel = root.Q<Label>("LevelLabel");
                confirmButton = root.Q<Button>("ConfirmButton");
                cancelButton = root.Q<Button>("CancelButton");

                if (confirmButton != null) confirmButton.clicked += ConfirmPurchase;
                if (cancelButton != null) cancelButton.clicked += Close;
                
                Close();
            }
        }

        public void Open(ShopExpansionWall wall)
        {
            if (wall == null) return;
            currentWall = wall;
            
            if (container != null)
            {
                // Update Labels
                if (costLabel != null) costLabel.text = $"Koszt: ${wall.Cost}";
                if (levelLabel != null)
                {
                    int currentLevel = PlayerStats.Instance != null ? PlayerStats.Instance.Level : 1;
                    levelLabel.text = $"Wymagany Poziom: {wall.RequiredLevel} (Twój: {currentLevel})";
                    
                    if (currentLevel < wall.RequiredLevel)
                    {
                        levelLabel.style.color = Color.red;
                        confirmButton.SetEnabled(false);
                    }
                    else
                    {
                        levelLabel.style.color = Color.white;
                        confirmButton.SetEnabled(true);
                    }
                }

                container.style.display = DisplayStyle.Flex;
                
                // Show Cursor
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
            }
        }

        public void Close()
        {
            if (container != null) container.style.display = DisplayStyle.None;
            currentWall = null;
            
            // Hide Cursor
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }

        private void ConfirmPurchase()
        {
            if (currentWall == null) return;

            if (PlayerStats.Instance != null)
            {
                if (PlayerStats.Instance.SpendMoney(currentWall.Cost))
                {
                    currentWall.Unlock();
                    Close();
                }
                else
                {
                     Debug.Log("ExpansionUI: Za mało pieniędzy!");
                     // Optional: Feedback logic
                }
            }
        }
    }
}
