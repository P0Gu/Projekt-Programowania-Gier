using UnityEngine;
using KillerPrices.Systems;

namespace KillerPrices.Interaction
{
    public class LightSwitch : MonoBehaviour, IInteractable
    {
        public void Interact()
        {
            if (LightManager.Instance != null)
            {
                LightManager.Instance.ToggleAllLights();
            }
            else
            {
                Debug.LogError("LightSwitch: Brak LightManager na scenie!");
            }
        }

        public string GetInteractionPrompt()
        {
            return "INTERACT_TOGGLE_LIGHT";
        }
    }
}
