using UnityEngine;

namespace KillerPrices.Interaction
{
    public interface IInteractable
    {
        void Interact();
        string GetInteractionPrompt();
    }
}
