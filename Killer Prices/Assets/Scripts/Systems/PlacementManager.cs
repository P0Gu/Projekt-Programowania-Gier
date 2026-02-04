using UnityEngine;
using KillerPrices.Data;
using KillerPrices.Systems; // For PlayerInventory logic access if needed

namespace KillerPrices.Placement
{
    public class PlacementManager : MonoBehaviour
    {
        public static PlacementManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private LayerMask floorLayer;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private Material validPreviewMaterial;
        [SerializeField] private Material invalidPreviewMaterial;

        public bool IsPlacing { get; private set; }

        private GameObject currentGhost;
        private GunData currentItemData;
        private float currentYRotation;
        private float cachedBottomOffset; // Cache offset
        private Camera mainCamera;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            mainCamera = Camera.main;
        }

        public void StartPlacement(GunData furnitureItem)
        {
            if (IsPlacing) CancelPlacement();

            if (furnitureItem.itemType != ItemType.Furniture || furnitureItem.modelPrefab == null)
            {
                Debug.LogWarning("PlacementManager: Przedmiot nie jest meblem lub brakuje modelu!");
                return;
            }

            IsPlacing = true;
            currentItemData = furnitureItem;
            currentYRotation = 0f;

            // Create Ghost
            currentGhost = Instantiate(furnitureItem.modelPrefab);
            
            // Disable colliders on ghost
            var colliders = currentGhost.GetComponentsInChildren<Collider>();
            foreach (var col in colliders) col.enabled = false;
            
            // Calculate Offset ONCE
            cachedBottomOffset = GetPivotToBottomOffset(currentGhost);
        }

        public void CancelPlacement()
        {
            IsPlacing = false;
            if (currentGhost != null) Destroy(currentGhost);
            currentItemData = null;
        }

        private void Update()
        {
            if (!IsPlacing || currentGhost == null) return;

            HandleMovement();
            HandleRotation();
            HandleInput();
        }

        private void HandleMovement()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            // FPS Ray (Center of Screen)
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            
            // Debug the ray visually in Scene View
            Debug.DrawRay(ray.origin, ray.direction * 20f, Color.red);

            // Safety check for layer mask
            if (floorLayer.value == 0)
            {
                Debug.LogWarning("PlacementManager: Floor Layer is not set! Defaulting to 'Default' layer.");
                floorLayer = LayerMask.GetMask("Default");
            }

            // Increased distance to 20f
            if (Physics.Raycast(ray, out RaycastHit hit, 20f, floorLayer))
            {
                // Surface Angle Check (Anti-Levitation / Wall Placement Prevention)
                float angle = Vector3.Angle(hit.normal, Vector3.up);
                
                // Allow only flat surfaces (roughly < 45 degrees slope)
                if (angle > 45f)
                {
                    currentGhost.SetActive(false);
                    return;
                }

                currentGhost.SetActive(true);
                
                // Use Cached Offset
                Vector3 finalPosition = hit.point + Vector3.up * cachedBottomOffset;

                currentGhost.transform.position = finalPosition;
                currentGhost.transform.rotation = Quaternion.Euler(0, currentYRotation, 0);
            }
            else
            {
                currentGhost.SetActive(false); // Hide if too far or invalid surface
            }
        }

        private float GetPivotToBottomOffset(GameObject go)
        {
            // Calculate bounds of all renderers
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return 0f;

            Bounds combinedBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }

            // Distance from Pivot (transform.position) to Bottom (bounds.min.y)
            // Note: bounds.min.y is world space. We need purely the vertical distance.
            // But since 'go' is the ghost, its position is what we are setting.
            // We can calculate local offset approx. 
            // Better: use local bounds or just relative Y.
            
            float pivotY = go.transform.position.y;
            float bottomY = combinedBounds.min.y;
            
            return Mathf.Max(0, pivotY - bottomY);
        }

        private void HandleRotation()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                currentYRotation += rotationSpeed;
            }
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0)) // Left Click to Place
            {
                if (CheckPlacementValidity())
                {
                    PlaceFurniture();
                }
                else
                {
                    Debug.Log("Cannot place here!");
                }
            }

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) // Right Click/Esc to Cancel
            {
                CancelPlacement();
            }
        }

        private bool CheckPlacementValidity()
        {
            // Simple check: Is ghost active? (on floor)
            if (!currentGhost.activeSelf) return false;

            // TODO: Add complex BoxOverlap check with obstacleLayer
            return true; 
        }

        private void PlaceFurniture()
        {
            // Instantiate real object
            GameObject placedObject = Instantiate(currentItemData.modelPrefab, currentGhost.transform.position, currentGhost.transform.rotation);
            
            // Enable colliders if prefab has them disabled by default (usually prefabs have them enabled)
            
            // Consume Item
            if (PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.RemoveItem(currentItemData, 1);
            }

            // Finish
            CancelPlacement();
        }
    }
}
