using UnityEngine;
using KillerPrices.Data;

namespace KillerPrices.Systems
{
    public class DeliveryManager : MonoBehaviour
    {
        public static DeliveryManager Instance { get; private set; }

        [Header("Konfiguracja")]
        [SerializeField] private Transform pickupZone;
        [SerializeField] private GameObject deliveryBoxPrefab;

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
            if (pickupZone == null || deliveryBoxPrefab == null)
            {
                Debug.LogError("DeliveryManager: Brak przypisanej strefy zrzutu (Pickup Zone) lub prefabu paczki!");
                return;
            }

            // Losowe przesunięcie, żeby paczki nie spadały idealnie na siebie
            Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0.5f, Random.Range(-0.5f, 0.5f));
            Vector3 spawnPos = pickupZone.position + randomOffset;

            GameObject boxObj = Instantiate(deliveryBoxPrefab, spawnPos, Quaternion.identity);
            
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

            Debug.Log($"Dostawa: Zrzucono paczkę z {gun.displayName}");
        }
    }
}
