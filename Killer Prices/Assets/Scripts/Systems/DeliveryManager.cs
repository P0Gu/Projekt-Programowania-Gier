using UnityEngine;
using KillerPrices.Data;

namespace KillerPrices.Systems
{
    public class DeliveryManager : MonoBehaviour
    {
        public static DeliveryManager Instance { get; private set; }

        [Header("Konfiguracja")]
        [SerializeField] private Transform pickupZone;
        [SerializeField] private GameObject smallBoxPrefab;
        [SerializeField] private GameObject largeBoxPrefab;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SpawnOrder(GunData gun)
        {
            if (pickupZone == null)
            {
                Debug.LogError("DeliveryManager: Brak przypisanej strefy zrzutu (Pickup Zone)!");
                return;
            }

            // Losowe przesunięcie, żeby paczki nie spadały idealnie na siebie
            Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0.5f, Random.Range(-0.5f, 0.5f));
            Vector3 spawnPos = pickupZone.position + randomOffset;

            SpawnBox(gun, spawnPos);
        }

        public void SpawnBox(GunData gun, Vector3 position)
        {
             if (smallBoxPrefab == null || largeBoxPrefab == null)
            {
                Debug.LogError("DeliveryManager: Brak prefabów paczek!");
                return;
            }

            GameObject prefabToSpawn = (gun.itemSize == ItemSize.Large) ? largeBoxPrefab : smallBoxPrefab;
            GameObject boxObj = Instantiate(prefabToSpawn, position, Quaternion.identity);
            
            // Inicjalizacja paczki danymi broni
            var deliveryBox = boxObj.GetComponent<KillerPrices.Interaction.DeliveryBox>();
            if (deliveryBox != null)
            {
                deliveryBox.Initialize(gun);
            }
            else
            {
                Debug.LogError("DeliveryManager: Prefab paczki nie ma komponentu DeliveryBox!");
            }

            Debug.Log($"Dostawa/Drop: Paczka z {gun.displayName} na {position}");
        }
    }
}
