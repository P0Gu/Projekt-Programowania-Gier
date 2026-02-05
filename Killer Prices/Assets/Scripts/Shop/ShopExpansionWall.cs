using UnityEngine;
using KillerPrices.Interaction;
using KillerPrices.UI;

namespace KillerPrices.Shop
{
    public class ShopExpansionWall : MonoBehaviour, IInteractable
    {
        [Header("Settings")]
        public string ExpansionId = "UniqueId";
        public int Cost = 1000;
        public int RequiredLevel = 1;

        [Header("References")]
        [SerializeField] private GameObject wallObject; // The wall to hide
        [SerializeField] private GameObject hiddenArea; // The area to show (or trigger active)

        private bool isPurchased = false;

        private void Start()
        {
            // Check if already bought
            if (ShopExpansionManager.Instance != null && ShopExpansionManager.Instance.IsPurchased(ExpansionId))
            {
                ApplyUnlockedState();
            }
        }

        public void Interact()
        {
            if (isPurchased) return;
            
            Debug.Log("Interakcja ze ścianą rozbudowy.");
            if (ExpansionUI.Instance != null)
            {
                ExpansionUI.Instance.Open(this);
            }
            else
            {
                Debug.LogError("Brak ExpansionUI w scenie!");
            }
        }

        public string GetInteractionPrompt()
        {
            return isPurchased ? "" : "INTERACT_EXPAND";
        }

        public void Unlock()
        {
            if (isPurchased) return;

            isPurchased = true;
            
            // Notify Manager
            if (ShopExpansionManager.Instance != null)
            {
                ShopExpansionManager.Instance.SetPurchased(ExpansionId);
            }

            ApplyUnlockedState();
        }

        private void ApplyUnlockedState()
        {
            isPurchased = true;
            if (wallObject != null) wallObject.SetActive(false);
            if (hiddenArea != null) hiddenArea.SetActive(true);
            
            // Disable interaction collider if this script is on it
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
    }
}
