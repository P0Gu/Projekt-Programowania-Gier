using UnityEngine;
using KillerPrices.Interaction;
using KillerPrices.AI;
using KillerPrices.Systems;

using System.Collections.Generic;

namespace KillerPrices.Shop
{
    public class CounterInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private float checkRadius = 3f;
        [SerializeField] private LayerMask customerLayer;
        
        [Header("Queue Settings")]
        [SerializeField] private float queueSpacing = 1.5f;
        [SerializeField] private Transform queueStartPoint; // Optional: specific start point

        private List<CustomerController> customerQueue = new List<CustomerController>();

        public void Interact()
        {
            // PROCESOWANIE KOLEJKI
            if (customerQueue.Count > 0)
            {
                var currentCustomer = customerQueue[0];
                if (currentCustomer.CurrentState == CustomerState.WaitingAtCounter)
                {
                    TrySell(currentCustomer);
                }
                else
                {
                    Debug.Log("Klient jeszcze nie dotarł do lady.");
                }
                return;
            }

            Debug.Log("Brak klientów w kolejce.");
        }

        public string GetInteractionPrompt()
        {
            return customerQueue.Count > 0 ? $"Obsłuż klienta ({customerQueue.Count} w kolejce)" : "Oczekiwanie na klientów...";
        }

        public Vector3 RegisterCustomer(CustomerController customer)
        {
            if (!customerQueue.Contains(customer))
            {
                customerQueue.Add(customer);
            }
            return GetQueuePosition(customerQueue.Count - 1);
        }

        public void UnregisterCustomer(CustomerController customer)
        {
            if (customerQueue.Contains(customer))
            {
                customerQueue.Remove(customer);
                UpdateQueuePositions();
            }
        }

        private void UpdateQueuePositions()
        {
            for (int i = 0; i < customerQueue.Count; i++)
            {
                if (customerQueue[i] != null)
                {
                    Vector3 newPos = GetQueuePosition(i);
                    customerQueue[i].MoveToQueuePosition(newPos);
                }
            }
        }

        private Vector3 GetQueuePosition(int index)
        {
            Vector3 startPos = queueStartPoint != null ? queueStartPoint.position : transform.position + transform.forward * 2.0f;
            Vector3 direction = queueStartPoint != null ? queueStartPoint.right : transform.right; // Kierunek kolejki
            
            return startPos + (direction * (queueSpacing * index));
        }

        private void TrySell(CustomerController customer)
        {
            // NEW SYSTEM: Checkout Mini-game
            if (CheckoutManager.Instance != null)
            {
                Debug.Log("CounterInteraction: Starting Checkout via Manager.");
                CheckoutManager.Instance.StartCheckout(customer);
                return;
            }
            else
            {
                 Debug.LogError("CounterInteraction: CheckoutManager Instance is NULL!");
            }

            // OLD SYSTEM Fallback (if CheckoutManager is missing)
            if (customer.Basket != null && customer.Basket.Count > 0)
            {
                int price = customer.TotalAgreedPrice;
                PlayerStats.Instance.AddMoney(price);
                PlayerStats.Instance.AddXP(20 * customer.Basket.Count); // XP zależne od liczby przedmiotów
                
                Debug.Log($"Skasowano klienta: {customer.Basket.Count} przedmioty za łączną kwotę {price}$");
                
                // Klient wychodzi zadowolony i znikają z kolejki
                UnregisterCustomer(customer);
                customer.LeaveStore(true);
                return;
            }
            
            Debug.Log("Klient przy ladzie nie ma żadnych zakupów. Dziwne.");
            UnregisterCustomer(customer);
            customer.LeaveStore(false);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, checkRadius);
            
            Gizmos.color = Color.blue;
            Vector3 start = queueStartPoint != null ? queueStartPoint.position : transform.position + transform.forward * 2.0f;
            Vector3 dir = queueStartPoint != null ? queueStartPoint.right : transform.right;
            for(int i=0; i<5; i++)
            {
                Gizmos.DrawWireSphere(start + dir * (queueSpacing * i), 0.3f);
            }
        }
    }
}
