using UnityEngine;
using KillerPrices.Interaction;
using KillerPrices.UI;

namespace KillerPrices.Shop
{
    public class WholesaleTerminal : MonoBehaviour, IInteractable
    {
        [Header("Konfiguracja")]
        [SerializeField] private string interactionText = "Otwórz Hurtownię";
        
        // Referencja do UI Hurtowni (będzie stworzone później)
        [SerializeField] private WholesaleUI wholesaleUI;

        public void Interact()
        {
            Debug.Log("Terminal: Interakcja wykryta!");
            if (wholesaleUI != null)
            {
                wholesaleUI.Toggle();
            }
            else
            {
                Debug.LogError("Terminal: Brak przypisanego WholesaleUI! Przypisz go w Inspektorze.");
            }
        }

        public string GetInteractionPrompt()
        {
            return interactionText;
        }
    }
}
