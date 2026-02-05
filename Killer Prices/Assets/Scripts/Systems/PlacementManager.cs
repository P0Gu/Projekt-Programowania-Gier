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
        [SerializeField] private LayerMask ceilingLayer; // Layer for ceilings (e.g., Default or specialized)
        [SerializeField] private LayerMask obstacleLayer;

        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float maxPlacementDistance = 5f;
        [SerializeField] private float maxCeilingDistance = 5f; // Add dedicated distance for ceilings if needed
        [SerializeField] private Material validPreviewMaterial;
        [SerializeField] private Material invalidPreviewMaterial;

        public bool IsPlacing { get; private set; }

        private GameObject currentGhost;
        private GunData currentItemData;
        private float currentYRotation;
        private Quaternion initialRotation; // Store the original prefab rotation
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

            if ((furnitureItem.itemType != ItemType.Furniture && furnitureItem.itemType != ItemType.CeilingItem) || furnitureItem.modelPrefab == null)
            {
                Debug.LogWarning("PlacementManager: Przedmiot nie jest meblem/lampą lub brakuje modelu!");
                return;
            }

            IsPlacing = true;
            currentItemData = furnitureItem;
            currentYRotation = 0f;

            // Create Ghost
            currentGhost = Instantiate(furnitureItem.modelPrefab);
            initialRotation = currentGhost.transform.rotation; // Capture initial rotation from prefab
            
            // Disable colliders on ghost
            var colliders = currentGhost.GetComponentsInChildren<Collider>();
            foreach (var col in colliders) col.enabled = false;

            // Automation: Disable NavMeshObstacle on ghost if present to avoid carving while moving
            var ghostObstacle = currentGhost.GetComponent<UnityEngine.AI.NavMeshObstacle>();
            if (ghostObstacle != null) ghostObstacle.enabled = false;
            
            var renderers = currentGhost.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) Debug.LogError($"Placement: GHOST MA 0 RENDERERÓW! Prefab: {furnitureItem.modelPrefab.name}");
            else Debug.Log($"Placement: Ghost utworzony. Renderery: {renderers.Length}. Pozycja: {currentGhost.transform.position}");
            
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

            bool isCeiling = currentItemData.itemType == ItemType.CeilingItem;
            LayerMask targetLayer = isCeiling ? ceilingLayer : floorLayer;
            float distance = isCeiling ? maxCeilingDistance : maxPlacementDistance;

            // Ensure reasonable layers only if absolutely missing
            if (floorLayer.value == 0) floorLayer = 1;
            
            // Fix: Force include Layer 8 ("celling") if mask seems to be just Default (1), 
            // because user clearly has ceiling on Layer 8 but Inspector might be set to Default.
            if (ceilingLayer.value == 0 || ceilingLayer.value == 1) ceilingLayer = 1 | (1 << 8); 

            if (Physics.Raycast(ray, out RaycastHit hit, distance, targetLayer))
            {
                // Surface Angle Check
                Vector3 expectedNormal = isCeiling ? Vector3.down : Vector3.up;
                float angle = Vector3.Angle(hit.normal, expectedNormal);
                
                // Special case for Ceilings: Allow "Up" normal (180 deg) if it's likely a thin plane/inverted mesh
                if (isCeiling && angle > 135f) 
                {
                    // Safe to ignore angle check for inverted ceilings
                }
                else if (angle > 45f)
                {
                    currentGhost.SetActive(false);
                    return;
                }

                if (!currentGhost.activeSelf) currentGhost.SetActive(true);
                
                // Use Cached Offset
                Vector3 finalPosition = hit.point;
                
                if (!isCeiling) finalPosition.y += cachedBottomOffset; 

                currentGhost.transform.position = finalPosition;
                // Apply user rotation (Y-axis) ON TOP OF the initial prefab rotation
                currentGhost.transform.rotation = Quaternion.Euler(0, currentYRotation, 0) * initialRotation;

                UpdateGhostVisuals(CheckPlacementValidity());
            }
            else
            {
                currentGhost.SetActive(false); 
            }
        }

        private void UpdateGhostVisuals(bool isValid)
        {
            Material targetMat = isValid ? validPreviewMaterial : invalidPreviewMaterial;
            if (targetMat == null)
            {
                // Keep this warning as it's a configuration error
                Debug.LogWarning("PlacementManager: Brak przypisanych materiałów (Valid/Invalid) w Inspektorze!");
                return; 
            }

            var renderers = currentGhost.GetComponentsInChildren<Renderer>();
            // if (renderers.Length == 0) Debug.LogWarning("PlacementManager: Duch nie ma Rendererów!");

            foreach(var r in renderers)
            {
                // Assign to ALL material slots to fully override visuals
                Material[] newMats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < newMats.Length; i++) newMats[i] = targetMat;
                r.sharedMaterials = newMats;
            }
        }

        private float GetPivotToBottomOffset(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return 0f;

            Bounds combinedBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }

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
                    string msg = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetTranslation("PLACE_INVALID") : "Cannot place here!";
                    Debug.Log(msg);
                }
            }

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) // Right Click/Esc to Cancel
            {
                CancelPlacement();
            }
        }

        // Insert GetGhostBounds helper if needed or reuse logic

        private Bounds GetGhostBounds()
        {
             var renderers = currentGhost.GetComponentsInChildren<Renderer>();
             if (renderers.Length == 0) return new Bounds(currentGhost.transform.position, Vector3.one);

             Bounds b = renderers[0].bounds;
             for(int i=1; i<renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
             return b;
        }

        private bool CheckPlacementValidity()
        {
            if (!currentGhost.activeSelf) return false;

            // Calculate bounds for OverlapBox
            Bounds b = GetGhostBounds();
            
            // Shrink slightly to avoid touching floor or being too strict
            Vector3 center = b.center;
            Vector3 halfExtents = b.extents * 0.95f; 

            // Check for collisions
            if (obstacleLayer.value == 0) obstacleLayer = LayerMask.GetMask("Default", "Furniture", "Player");
            
            Collider[] hits = Physics.OverlapBox(center, halfExtents, currentGhost.transform.rotation, obstacleLayer);
            
            if (hits.Length > 0)
            {
                // Debug.Log($"Kolizja z: {hits[0].name}");
                return false;
            }

            return true; 
        }

        private void PlaceFurniture()
        {
            // Instantiate real object
            GameObject placedObject = Instantiate(currentItemData.modelPrefab, currentGhost.transform.position, currentGhost.transform.rotation);
            
            // Automation: Add NavMeshObstacle with Carve on placed objects
            var obstacle = placedObject.GetComponent<UnityEngine.AI.NavMeshObstacle>();
            if (obstacle == null) obstacle = placedObject.AddComponent<UnityEngine.AI.NavMeshObstacle>();
            
            obstacle.carving = true;
            // Opcjonalnie: dostosuj kształt do collidera jeśli to Box
            var boxCol = placedObject.GetComponent<BoxCollider>();
            if (boxCol != null)
            {
                obstacle.shape = UnityEngine.AI.NavMeshObstacleShape.Box;
                obstacle.center = boxCol.center;
                obstacle.size = boxCol.size;
            }
            
            // Track Placed Object
            if (currentItemData != null)
            {
                // VALIDATION: Check if ID is empty and auto-generate if needed
                string itemId = currentItemData.id;
                if (string.IsNullOrEmpty(itemId))
                {
                    itemId = currentItemData.name; // Use ScriptableObject asset name as fallback
                    Debug.LogWarning($"PlacementManager: Item '{currentItemData.displayName}' has empty ID! Using asset name '{itemId}' instead. Please set the ID field in the Inspector!");
                }
                
                placedObjects.Add(new PlacedObjectInfo
                {
                   itemID = itemId,
                   instance = placedObject
                });
                Debug.Log($"PlacementManager: Placed '{currentItemData.displayName}' with ID '{itemId}' at {placedObject.transform.position}");
            }

            // Consume Item
            if (PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.RemoveItem(currentItemData, 1);
            }

            // Finish
            CancelPlacement();
        }

        // --- Save System Integration ---

        private class PlacedObjectInfo
        {
            public string itemID;
            public GameObject instance;
        }

        private System.Collections.Generic.List<PlacedObjectInfo> placedObjects = new System.Collections.Generic.List<PlacedObjectInfo>();

        public System.Collections.Generic.List<PlacedObjectSaveData> GetPlacedObjectsData()
        {
            var data = new System.Collections.Generic.List<PlacedObjectSaveData>();
            
            // Cleanup nulls (destroyed objects)
            placedObjects.RemoveAll(x => x.instance == null);

            foreach (var info in placedObjects)
            {
                data.Add(new PlacedObjectSaveData
                {
                    itemID = info.itemID,
                    position = info.instance.transform.position,
                    rotation = info.instance.transform.rotation
                });
            }
            return data;
        }

        public void RestorePlacedObjects(System.Collections.Generic.List<PlacedObjectSaveData> data, ItemDatabase db)
        {
             // Clear existing tracked objects (optional: destroy them if this is a full reload)
             // For now assuming scene reload clears actual objects, so we just clear list
             placedObjects.Clear();
             
             if (data == null) return;

             foreach (var objData in data)
             {
                 var item = db.GetItem(objData.itemID);
                 if (item != null && item.modelPrefab != null)
                 {
                     GameObject newObj = Instantiate(item.modelPrefab, objData.position, objData.rotation);
                     Debug.Log($"PlacementManager: Restored '{item.displayName}' (ID: '{objData.itemID}') at {objData.position}");
                     
                     // Restore NavMeshObstacle logic if needed
                     var obstacle = newObj.GetComponent<UnityEngine.AI.NavMeshObstacle>();
                     if (obstacle == null) obstacle = newObj.AddComponent<UnityEngine.AI.NavMeshObstacle>();
                     obstacle.carving = true;
                     var boxCol = newObj.GetComponent<BoxCollider>();
                     if (boxCol != null)
                     {
                         obstacle.shape = UnityEngine.AI.NavMeshObstacleShape.Box;
                         obstacle.center = boxCol.center;
                         obstacle.size = boxCol.size;
                     }

                     placedObjects.Add(new PlacedObjectInfo
                     {
                         itemID = item.id,
                         instance = newObj
                     });
                 }
                 else
                 {
                     Debug.LogError($"PlacementManager: Failed to restore object with ID '{objData.itemID}' - Item not found in database or missing prefab!");
                 }
             }
        }
    }
}
