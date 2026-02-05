using UnityEngine;
using KillerPrices.Data;
using KillerPrices.Systems; // Dla InventoryManager i PlayerInventory

namespace KillerPrices.Interaction
{
    // Zakładamy, że paczka jest interaktywna (IInteractable).
    // Jeśli nie masz interfejsu w osobny pliku, musimy sprawdzić gdzie on jest.
    // Zakładam, że IInteractable jest zdefiniowany w PlayerInteractor.cs lub podobnym.
    
    [RequireComponent(typeof(Rigidbody))]
    public class DeliveryBox : MonoBehaviour, IInteractable
    {
        [SerializeField] private GunData content;
        
        // Wizualizacja (opcjonalne)
        [SerializeField] private SpriteRenderer iconRenderer;

        public string GetInteractionPrompt()
        {
            return "INTERACT_PICKUP";
        }

        public void Initialize(GunData gun)
        {
            content = gun;
            if (iconRenderer != null && gun != null)
            {
                iconRenderer.sprite = gun.icon;
            }
        }

        public void Interact()
        {
            if (content == null) return;

            if (PlayerInventory.Instance != null)
            {
                if (PlayerInventory.Instance.AddItem(content))
                {
                    Debug.Log($"Podniesiono paczkę: {content.displayName}");
                    Destroy(gameObject);
                    return;
                }
                else
                {
                    Debug.Log("Brak miejsca w ekwipunku!");
                    return;
                }
            }
            else
            {
                // Fallback dla kompatybilności wstecznej (jeśli PlayerInventory nie istnieje)
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.AddStock(content, 1);
                    Debug.Log($"Podniesiono paczkę (Legacy): {content.displayName}");
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }
}
