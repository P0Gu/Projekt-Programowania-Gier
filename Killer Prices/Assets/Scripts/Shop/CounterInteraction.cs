using UnityEngine;
using KillerPrices.Interaction;
using KillerPrices.AI;
using KillerPrices.Systems;

namespace KillerPrices.Shop
{
    public class CounterInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private float checkRadius = 3f;
        [SerializeField] private LayerMask customerLayer;

        public void Interact()
        {
            // Znajdź najbliższego klienta w stanie WaitingAtCounter
             Collider[] hits = Physics.OverlapSphere(transform.position, checkRadius, customerLayer);
             foreach(var hit in hits)
             {
                 var customer = hit.GetComponent<CustomerController>();
                 if(customer && customer.CurrentState == CustomerState.WaitingAtCounter)
                 {
                     TrySell(customer);
                     return;
                 }
             }
             
             Debug.Log("Brak klientów przy ladzie.");
        }

        public string GetInteractionPrompt()
        {
            return "Obsłuż klienta";
        }

        private void TrySell(CustomerController customer)
        {
            // SCENARIUSZ 1: Klient ma towary w koszyku
            if (customer.Basket != null && customer.Basket.Count > 0)
            {
                int price = customer.TotalAgreedPrice;
                PlayerStats.Instance.AddMoney(price);
                PlayerStats.Instance.AddXP(20 * customer.Basket.Count); // XP zależne od liczby przedmiotów
                
                Debug.Log($"Skasowano klienta: {customer.Basket.Count} przedmioty za łączną kwotę {price}$");
                
                // Klient wychodzi zadowolony
                customer.LeaveStore(true);
                return;
            }
            
            // Jeśli klient jakimś cudem znalazł się tu bez towarów:
            Debug.Log("Klient przy ladzie nie ma żadnych zakupów. Dziwne.");
            customer.LeaveStore(false);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, checkRadius);
        }
    }
}
