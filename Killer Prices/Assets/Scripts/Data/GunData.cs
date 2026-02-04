using UnityEngine;

namespace KillerPrices.Data
{
    public enum ItemType
    {
        Weapon,
        Furniture
    }

    [CreateAssetMenu(fileName = "New Item", menuName = "Shop/Item Data")]
    public class GunData : ScriptableObject
    {
        [Header("Info")]
        public string id;
        public ItemType itemType; // Typ przedmiotu
        public string displayName;
        [TextArea] public string description;

        [Header("Economy")]
        [Tooltip("Koszt zakupu w hurtowni")]
        public int baseCost; 
        [Tooltip("Sugerowana cena sprzedaży")]
        public int baseSellPrice;
        [Tooltip("Wymagany poziom gracza do odblokowania")]
        public int requiredLevel = 0; 

        [Header("Visuals")]
        public Sprite icon;
        public GameObject modelPrefab;
    }
}
