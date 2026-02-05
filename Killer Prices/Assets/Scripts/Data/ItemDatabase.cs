using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace KillerPrices.Data
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Shop/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        public List<GunData> allItems;

        public GunData GetItem(string id)
        {
            return allItems.FirstOrDefault(i => i.id == id);
        }

        [ContextMenu("Auto Populate")]
        public void AutoPopulate()
        {
            allItems = Resources.LoadAll<GunData>("Items").ToList(); // Zakładając, że są w folderze Resources/Items
            // Alternatywnie, jeśli nie używamy Resources, można ręcznie przypisać w edytorze lub użyć AssetDatabase w editor script
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:GunData");
            allItems = new List<GunData>();
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                GunData item = UnityEditor.AssetDatabase.LoadAssetAtPath<GunData>(path);
                if (item != null)
                {
                    allItems.Add(item);
                }
            }
#endif
        }
    }
}
