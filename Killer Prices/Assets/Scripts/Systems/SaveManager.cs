using UnityEngine;
using System.IO;
using System.Collections.Generic;
using KillerPrices.Data;
using System;
using UnityEngine.SceneManagement;

namespace KillerPrices.Systems
{
    [Serializable]
    public class GameSaveData
    {
        public int money;
        public int level;
        public float currentXP;
        
        public List<InventoryItemSaveData> inventory = new List<InventoryItemSaveData>();
        public List<PlacedObjectSaveData> placedObjects = new List<PlacedObjectSaveData>();
        public List<ShelfSaveData> shelves = new List<ShelfSaveData>();

        // Player Position & Camera Rotation
        public Vector3 playerPosition;
        public Quaternion playerRotation; // Body rotation (usually not used in FPS)
        public Quaternion cameraRotation; // Camera look direction
    }

    [Serializable]
    public class InventoryItemSaveData
    {
        public int slotIndex;
        public string itemID;
    }

    [Serializable]
    public class PlacedObjectSaveData
    {
        public string itemID;
        public Vector3 position;
        public Quaternion rotation;
    }

    [Serializable]
    public class ShelfSaveData
    {
        public int shelfIndex; 
        public List<ShelfSlotSaveData> slots = new List<ShelfSlotSaveData>();
    }

    [Serializable]
    public class ShelfSlotSaveData
    {
        public int slotIndex;
        public string itemID;
    }

        public class SaveManager : MonoBehaviour
        {
            public static SaveManager Instance { get; private set; }

            [SerializeField] private ItemDatabase itemDatabase;
            
            private string saveFilePath;
            
            // Flag to trigger load after scene transition
            public bool ShouldLoadOnStart = false;

            private void Awake()
            {
                if (Instance != null)
                {
                    Destroy(gameObject);
                    return;
                }
                Instance = this;
                DontDestroyOnLoad(gameObject);

                saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
            }
            
            private void OnEnable()
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
            }

            private void OnDisable()
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }

            private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
            {
                // Verify we are in the Game Scene (not MainMenu)
                // Assuming MainMenu scene name is "MainMenu"
                if (scene.name != "MainMenu" && ShouldLoadOnStart)
                {
                    Debug.Log("SaveManager: Scene loaded, triggering delayed LoadGame...");
                    LoadGame();
                    ShouldLoadOnStart = false; // Reset flag
                }
            }

        public void SaveGame()
        {
            GameSaveData data = new GameSaveData();

            // 1. Stats
            if (PlayerStats.Instance != null)
            {
                data.money = PlayerStats.Instance.Money;
                data.level = PlayerStats.Instance.Level;
                data.currentXP = PlayerStats.Instance.CurrentXP;
            }

            // 2. Inventory
            if (PlayerInventory.Instance != null)
            {
                data.inventory = PlayerInventory.Instance.GetInventorySaveData();
            }

            // 3. Placed Objects
            if (KillerPrices.Placement.PlacementManager.Instance != null)
            {
                data.placedObjects = KillerPrices.Placement.PlacementManager.Instance.GetPlacedObjectsData();
            }

            // 4. Shelves
            // Sort keys by position to ensure deterministic index order
            KillerPrices.Shop.Shelf.AllShelves.Sort(SortShelvesDeterministically);
            
            var allShelves = KillerPrices.Shop.Shelf.AllShelves;
            Debug.Log($"SaveManager: Saving {allShelves.Count} shelves.");
            for (int i = 0; i < allShelves.Count; i++)
            {
                data.shelves.Add(allShelves[i].GetShelfData(i));
            }

            // 5. Player Position & Camera Rotation
            var player = FindFirstObjectByType<Controls>();
            if (player != null)
            {
                data.playerPosition = player.transform.position;
                
                // In FPS: Player body handles Y rotation (horizontal), Camera handles X rotation (vertical)
                // Save camera's full rotation - we'll split it on load
                if (Camera.main != null)
                {
                    data.cameraRotation = Camera.main.transform.rotation;
                    Vector3 camEuler = Camera.main.transform.eulerAngles;
                    Debug.Log($"SaveManager: Saved camera rotation: X={camEuler.x:F1}, Y={camEuler.y:F1}, Z={camEuler.z:F1}");
                }
                else
                {
                    // Fallback to player rotation if no camera
                    data.playerRotation = player.transform.rotation;
                }
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"Game Saved to: {saveFilePath}");
        }

        public bool HasSave()
        {
            return File.Exists(saveFilePath);
        }

        public void LoadGame()
        {
            if (!File.Exists(saveFilePath))
            {
                Debug.LogWarning("No save file found!");
                return;
            }

            string json = File.ReadAllText(saveFilePath);
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            Debug.Log($"SaveManager: File Read! JSON money value: {data.money}");

            // 1. Stats
            try
            {
                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.LoadFromSaveData(data);
                }
                else
                {
                    Debug.LogError("SaveManager: PlayerStats Instance is NULL during LoadGame!");
                }
            }
            catch (Exception e) { Debug.LogError($"SaveManager: Error loading Stats: {e.Message}"); }

            // 2. Inventory
            try
            {
                if (PlayerInventory.Instance != null)
                {
                    PlayerInventory.Instance.RestoreInventory(data.inventory, itemDatabase);
                }
            }
            catch (Exception e) { Debug.LogError($"SaveManager: Error loading Inventory: {e.Message}"); }

            // 3. Placed Objects
            try
            {
                if (KillerPrices.Placement.PlacementManager.Instance != null)
                {
                    Debug.Log($"SaveManager: Restoring {data.placedObjects.Count} placed objects...");
                    KillerPrices.Placement.PlacementManager.Instance.RestorePlacedObjects(data.placedObjects, itemDatabase);
                }
            }
            catch (Exception e) { Debug.LogError($"SaveManager: Error loading Placed Objects: {e.Message}"); }

            // 4. Shelves
            try
            {
                // Sort to match Save order
                KillerPrices.Shop.Shelf.AllShelves.Sort(SortShelvesDeterministically);
                
                var allShelves = KillerPrices.Shop.Shelf.AllShelves;
                Debug.Log($"SaveManager: Restoring data for {Mathf.Min(data.shelves.Count, allShelves.Count)} shelves (Total in scene: {allShelves.Count}).");
                
                foreach (var shelfData in data.shelves)
                {
                    if (shelfData.shelfIndex < allShelves.Count)
                    {
                        allShelves[shelfData.shelfIndex].RestoreShelfData(shelfData, itemDatabase);
                    }
                }
            }
            catch (Exception e) { Debug.LogError($"SaveManager: Error loading Shelves: {e.Message}"); }

            // 5. Player Position & Camera Rotation
            try
            {
                var player = FindFirstObjectByType<Controls>();
                if (player != null)
                {
                    Debug.Log($"SaveManager: Teleporting player to {data.playerPosition}");
                    // Must disable CharacterController to teleport if present
                    var cc = player.GetComponent<CharacterController>();
                    bool wasEnabled = cc != null && cc.enabled;
                    if (wasEnabled) cc.enabled = false;

                    // Check if position is non-zero
                    if (data.playerPosition != Vector3.zero)
                    {
                        player.transform.position = data.playerPosition;
                        
                        // In FPS: Apply Y rotation (horizontal) to player body
                        if (Camera.main != null && data.cameraRotation != Quaternion.identity)
                        {
                            Vector3 camEuler = data.cameraRotation.eulerAngles;
                            
                            // Player body gets Y rotation only (horizontal look)
                            player.transform.rotation = Quaternion.Euler(0, camEuler.y, 0);
                            
                            // Camera gets full rotation (including X for vertical look)
                            Camera.main.transform.rotation = data.cameraRotation;
                            
                            Debug.Log($"SaveManager: Restored rotation - Player Y={camEuler.y:F1}, Camera X={camEuler.x:F1} Y={camEuler.y:F1}");
                        }
                        else if (data.playerRotation != Quaternion.identity)
                        {
                            // Fallback to old save format
                            player.transform.rotation = data.playerRotation;
                        }
                        
                        Physics.SyncTransforms(); // Ensure physics engine updates immediately
                    }

                    if (wasEnabled) cc.enabled = true;
                }
                else
                {
                    Debug.LogError("SaveManager: Player (Controls) not found!");
                }
            }
            catch (Exception e) { Debug.LogError($"SaveManager: Error loading Player Position: {e.Message}"); }

            Debug.Log("Game Loaded!");
        }
        
        private int SortShelvesDeterministically(KillerPrices.Shop.Shelf a, KillerPrices.Shop.Shelf b)
        {
            // Robust check against nulls
            if (a == null) return 1;
            if (b == null) return -1;
            
            // Sort by Position (X, then Z, then Y)
            Vector3 posA = a.transform.position;
            Vector3 posB = b.transform.position;
            
            if (Mathf.Abs(posA.x - posB.x) > 0.01f) return posA.x.CompareTo(posB.x);
            if (Mathf.Abs(posA.z - posB.z) > 0.01f) return posA.z.CompareTo(posB.z);
            return posA.y.CompareTo(posB.y);
        }

        [ContextMenu("Delete Save")]
        public void DeleteSave()
        {
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
                Debug.Log("Save file deleted.");
            }
        }
    }
}
