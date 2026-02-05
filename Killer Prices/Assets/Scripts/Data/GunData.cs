using UnityEngine;

namespace KillerPrices.Data
{
    public enum ItemType
    {
        Weapon,
        Furniture,
        CeilingItem // Nowy typ: Lampy sufitowe
    }

    public enum ItemSize
    {
        Small, // Pistolety, amunicja, małe akcesoria
        Large  // Karabiny, strzelby, duże meble
    }

    [CreateAssetMenu(fileName = "New Item", menuName = "Shop/Item Data")]
    public class GunData : ScriptableObject
    {
        [Header("Info")]
        public string id;
        public ItemType itemType; // Typ przedmiotu
        public ItemSize itemSize; // Rozmiar paczki
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
        public float shelfDisplayScale = 1.0f; // Absolutna skala (1.0 = normalna wielkość)
        public Vector3 shelfDisplayRotation; // Rotacja na półce (w stopniach)

        [Header("Checkout Display")]
        public Vector3 checkoutDisplayOffset; // Offset względem punktu na ladzie
        public Vector3 checkoutDisplayRotation; // Rotacja na ladzie
        public float checkoutDisplayScale = 1.0f; // Skala na ladzie
    }
}
