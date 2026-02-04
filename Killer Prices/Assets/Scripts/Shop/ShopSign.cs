using UnityEngine;
using KillerPrices.Interaction;

namespace KillerPrices.Shop
{
    public class ShopSign : MonoBehaviour, IInteractable
    {
        [Header("Appearance")]
        [SerializeField] private Material openMaterial; // Green
        [SerializeField] private Material closedMaterial; // Red
        [SerializeField] private Renderer signRenderer; // The object that changes color

        private void Start()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnShopStatusChanged += UpdateVisuals;
                UpdateVisuals(ShopManager.Instance.IsOpen);
            }
        }

        private void OnDestroy()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnShopStatusChanged -= UpdateVisuals;
            }
        }

        public void Interact()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.ToggleShop();
            }
        }

        public string GetInteractionPrompt()
        {
            if (ShopManager.Instance == null) return "Shop Manager Missing";
            return ShopManager.Instance.IsOpen ? "ZAMKNIJ SKLEP" : "OTWÓRZ SKLEP";
        }

        private void UpdateVisuals(bool isOpen)
        {
            if (signRenderer == null) return;

            if (isOpen && openMaterial != null)
            {
                signRenderer.sharedMaterial = openMaterial;
            }
            else if (!isOpen && closedMaterial != null)
            {
                signRenderer.sharedMaterial = closedMaterial;
            }
        }
    }
}
