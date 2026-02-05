using UnityEngine;
using UnityEngine.InputSystem;
using KillerPrices.Systems;
using KillerPrices.Placement;

namespace KillerPrices.Interaction
{
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Konfiguracja")]
        [SerializeField] private float interactionRange = 4f;
        [SerializeField] private float interactionRadius = 0.5f; // Promień Spherecasta
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private Transform detectionPoint; // Opcjonalnie: punkt z którego rzucamy promień (np. kamera)

        [Header("UI")]
        [SerializeField] private UI.InteractionPromptUI interactionPromptUI;

        // Referencja do Input System (zakładamy że zostanie zaktualizowany)
        private PlayerControl playerControl;
        private IInteractable currentInteractable;

        private void Awake()
        {
            playerControl = new PlayerControl();
        }

        private void Start()
        {
            // Auto-find InteractionPromptUI if not assigned
            if (interactionPromptUI == null)
            {
                interactionPromptUI = FindFirstObjectByType<UI.InteractionPromptUI>();
            }
        }

        private void OnEnable()
        {
            if (playerControl == null)
                playerControl = new PlayerControl();
                
            playerControl.Enable();
            // Subskrypcja zdarzenia Interact (jeśli istnieje - użytkownik musi zaktualizować Input Actions)
            // playerControl.CharacterControl.Interact.performed += OnInteractPerformed;
            
            // Tymczasowo: Używamy starego inputu do testów, dopóki nie zaktualizujemy InputActions
            // W finalnej wersji: odkomentować linię wyżej i usunąć Update
        }

        private void OnDisable()
        {
            if (playerControl != null)
                playerControl.Disable();
            // playerControl.CharacterControl.Interact.performed -= OnInteractPerformed;
        }

        private void Update()
        {
            CheckForInteractables();

            // Fallback input (Klawisz E)
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryInteract();
            }

            // Obsługa użycia przedmiotu (LPM) - np. stawianie mebli
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Jeśli jesteśmy w trybie stawiania, nie wywołuj UseItem (PlacementManager sam obsłuży kliknięcie)
                if (PlacementManager.Instance != null && PlacementManager.Instance.IsPlacing)
                    return;

                if (PlayerInventory.Instance != null)
                {
                    PlayerInventory.Instance.UseCurrentItem();
                }
            }
        }

        private void CheckForInteractables()
        {
            Vector3 origin = detectionPoint != null ? detectionPoint.position : transform.position;
            Vector3 direction = detectionPoint != null ? detectionPoint.forward : transform.forward;

            // Używamy SphereCast zamiast Raycast dla łatwiejszego trafiania
            if (Physics.SphereCast(origin, interactionRadius, direction, out RaycastHit hit, interactionRange, interactableLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    if (currentInteractable != interactable)
                    {
                        currentInteractable = interactable;
                        
                        // Show interaction prompt
                        if (interactionPromptUI != null)
                        {
                            string promptKey = currentInteractable.GetInteractionPrompt();
                            interactionPromptUI.Show(promptKey);
                        }
                    }
                    return;
                }
            }

            // No interactable found - hide prompt
            if (currentInteractable != null)
            {
                if (interactionPromptUI != null)
                {
                    interactionPromptUI.Hide();
                }
                currentInteractable = null;
            }
        }

        private void TryInteract()
        {
            if (currentInteractable != null)
            {
                currentInteractable.Interact();
            }
        }
        
        // Metoda do podpięcia pod Input System w przyszłości
        public void OnInteractPerformed(InputAction.CallbackContext context)
        {
            TryInteract();
        }
    }
}
