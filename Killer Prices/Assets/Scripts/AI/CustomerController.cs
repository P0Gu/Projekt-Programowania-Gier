using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using KillerPrices.Data;
using KillerPrices.Shop;
using KillerPrices.Systems;
using TMPro; // Dodano namespace TMPro

namespace KillerPrices.AI
{
    public enum CustomerState 
    { 
        SearchingForShelf, 
        WalkingToShelf, 
        Browsing, 
        Evaluating,
        WalkingToCounter, 
        WaitingAtCounter, 
        Leaving,
        Wandering
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class CustomerController : MonoBehaviour
    {
        [Header("AI")]
        [SerializeField] private float stopDistance = 1.0f;
        [SerializeField] private float browsingTime = 2.0f;
        [SerializeField] private float wanderTimeIfNotFound = 6.0f;

        [Header("Request & Feedback")]

        [SerializeField] private GameObject bubbleObject;
        [SerializeField] private TMP_Text feedbackText; // Komponent tekstowy (TextMeshPro)

        [Header("Visuals")]
        [SerializeField] private Transform handPoint; 

        // Nowe struktury danych
        public Queue<GunData> ShoppingQueue { get; private set; } = new Queue<GunData>();
        public List<GunData> Basket { get; private set; } = new List<GunData>();
        public int TotalAgreedPrice { get; private set; }

        public CustomerState CurrentState { get; private set; }
        public GunData CurrentDesiredItem { get; private set; }

        private NavMeshAgent agent;
        private Transform counterTarget;
        private Transform exitTarget;
        private System.Action<CustomerController> onCustomerLeft;
        
        private ShelfSlot targetSlot;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public void Initialize(GunData initialRequest, Transform counter, Transform exit, System.Action<CustomerController> onLeftCallback)
        {
            if(initialRequest != null) 
                ShoppingQueue.Enqueue(initialRequest);
            
            counterTarget = counter;
            exitTarget = exit;
            onCustomerLeft = onLeftCallback;

            if (feedbackText) feedbackText.gameObject.SetActive(false);
            if (bubbleObject) bubbleObject.SetActive(false); // Ukryj dymek na start (brak ikony)

            StartCoroutine(ShoppingLoop());
        }

        private IEnumerator ShoppingLoop()
        {
            // Dopóki klient chce coś kupić
            while (ShoppingQueue.Count > 0)
            {
                CurrentDesiredItem = ShoppingQueue.Dequeue();
                
                // Ikona usunięta - nie aktualizujemy dymka w pętli


                yield return StartCoroutine(ProcessItemRequest(CurrentDesiredItem));
                
                // Mała przerwa między zakupami
                yield return new WaitForSeconds(0.5f);
            }

            // Koniec zakupów
            if (Basket.Count > 0)
            {
                GoToCounter();
            }
            else
            {
                Debug.Log("Klient wychodzi z niczym.");
                LeaveStore(false);
            }
        }

        private IEnumerator ProcessItemRequest(GunData itemToFind)
        {
            CurrentState = CustomerState.SearchingForShelf;
            targetSlot = Shelf.FindAnyShelfSlotWithItem(itemToFind);

            if (targetSlot == null)
            {
                Debug.Log($"Brak {itemToFind.displayName} na półkach.");
                
                // Klient błąka się chwilę, udając że szuka
                ShowFeedback("Gdzie to jest?", Color.yellow);
                yield return StartCoroutine(WanderAroundStore());
                
                yield break;
            }

            CurrentState = CustomerState.WalkingToShelf;
            agent.SetDestination(targetSlot.point.position);
            
            while (agent.pathPending || agent.remainingDistance > stopDistance) yield return null;

            agent.isStopped = true;
            CurrentState = CustomerState.Browsing;
            yield return new WaitForSeconds(browsingTime);

            CurrentState = CustomerState.Evaluating;
            EvaluatePriceAndDecide();
        }

        private void EvaluatePriceAndDecide()
        {
            if (targetSlot == null || !targetSlot.IsOccupied || targetSlot.currentItem != CurrentDesiredItem)
            {
                ShowFeedback("Zniknęło!?", Color.red);
                return;
            }

            int price = targetSlot.currentPrice;
            int basePrice = CurrentDesiredItem.baseSellPrice;
            
            bool isPriceAcceptable = price <= basePrice * 1.2f;
            if (!isPriceAcceptable)
            {
                float ratio = (float)price / basePrice;
                if (Random.value < (1.0f / (ratio * ratio))) isPriceAcceptable = true;
            }

            if (isPriceAcceptable)
            {
                ShowFeedback("Biorę!", Color.green);
                TakeItemFromShelf();
            }
            else
            {
                ShowFeedback("Za drogo!", Color.red);
                if (PlayerStats.Instance != null) PlayerStats.Instance.AddXP(-10);
            }
        }

        private bool isShowingFeedback = false;

        private void ShowFeedback(string text, Color color)
        {
            if (feedbackText != null)
            {
                isShowingFeedback = true;
                
                // Pokaż tekst
                
                feedbackText.text = text;
                feedbackText.color = color;
                
                if (bubbleObject) bubbleObject.SetActive(true); // Upewnij się że tło jest
                feedbackText.gameObject.SetActive(true);
                
                // Resetowanie po czasie
                StopCoroutine(nameof(HideFeedback));
                StartCoroutine(nameof(HideFeedback));
            }
        }

        private IEnumerator HideFeedback()
        {
            yield return new WaitForSeconds(2.0f);
            
            isShowingFeedback = false;
            if (feedbackText) feedbackText.gameObject.SetActive(false);
            
            // Logika po ukryciu tekstu
            // Po ukryciu tekstu zawsze ukrywamy dymek (bo nie ma już ikon)
            if (bubbleObject) bubbleObject.SetActive(false);
        }

        private void TakeItemFromShelf()
        {
            TotalAgreedPrice += targetSlot.currentPrice;
            Basket.Add(targetSlot.currentItem);
            
            // Wizualizacja (tylko ostatni przedmiot w ręku) - zabezpieczona przed brakiem handPoint
            if (handPoint != null)
            {
                if (handPoint.childCount > 0) Destroy(handPoint.GetChild(0).gameObject);
                
                if (targetSlot.currentItem.modelPrefab != null)
                {
                     Instantiate(targetSlot.currentItem.modelPrefab, handPoint.position, handPoint.rotation, handPoint);
                }
            }

            targetSlot.Clear();
        }

        public void MoveToQueuePosition(Vector3 pos)
        {
             if (agent != null && agent.enabled && agent.gameObject.activeSelf) 
             {
                 agent.SetDestination(pos);
             }
        }


        private void GoToCounter()
        {
            CurrentState = CustomerState.WalkingToCounter;
            agent.isStopped = false;
            
            // Queue Logic: Try to find the script on the target, or its parent
            var counterScript = counterTarget.GetComponent<CounterInteraction>();
            if (counterScript == null) counterScript = counterTarget.GetComponentInParent<CounterInteraction>();
            if (counterScript == null) counterScript = FindFirstObjectByType<CounterInteraction>(); // Last resort

            if (counterScript != null)
            {
                Vector3 queuePos = counterScript.RegisterCustomer(this);
                agent.SetDestination(queuePos);
            }
            else
            {
                // Fallback
                agent.SetDestination(counterTarget.position);
            }
            
            // Ukrywamy dymek TYLKO jeśli NIE pokazujemy feedbacku ("Biorę!")
            if(bubbleObject && !isShowingFeedback) bubbleObject.SetActive(false);

            StartCoroutine(WaitForCounterArrival());
        }


        private IEnumerator WaitForCounterArrival()
        {
            while (agent.pathPending || agent.remainingDistance > stopDistance) yield return null;
            
            CurrentState = CustomerState.WaitingAtCounter;
            agent.isStopped = true;
            
            // Look at counter
            if (counterTarget != null)
            {
                Vector3 lookPos = counterTarget.position;
                lookPos.y = transform.position.y;
                transform.LookAt(lookPos);
            }
        }

        public void LeaveStore(bool happy)
        {
            StopAllCoroutines();
            
            // Unregister from queue if necessary
            if (CurrentState == CustomerState.WalkingToCounter || CurrentState == CustomerState.WaitingAtCounter)
            {
                var counterScript = counterTarget.GetComponent<CounterInteraction>();
                if (counterScript != null) counterScript.UnregisterCustomer(this);
            }
            
            // UWAGA: StopAllCoroutines zatrzyma też HideFeedback! Musimy go wznowić ręcznie jeśli trwa.
            
            CurrentState = CustomerState.Leaving;
            agent.isStopped = false;
            if (exitTarget != null) agent.SetDestination(exitTarget.position);
            
            if (isShowingFeedback)
            {
                // Jeśli trwa feedback, uruchom korutynę ponownie (bo StopAll ją zabiło)
                StartCoroutine(nameof(HideFeedback));
            }
            else
            {
                if (bubbleObject) bubbleObject.SetActive(false);
            }
        }

        private IEnumerator WanderAroundStore()
        {
            CurrentState = CustomerState.Wandering;
            float endTime = Time.time + wanderTimeIfNotFound;

            while (Time.time < endTime)
            {
                // Find random point 
                Vector3 randomPoint = GetRandomNavMeshPoint(transform.position, 8.0f);
                agent.SetDestination(randomPoint);
                agent.isStopped = false;

                // Wait until reached
                while (agent.pathPending || agent.remainingDistance > stopDistance)
                {
                    if (Time.time > endTime) break;
                    yield return null;
                }

                if (Time.time > endTime) break;

                // Wait a bit ("looking around")
                yield return new WaitForSeconds(Random.Range(1.0f, 2.5f));
            }
        }

        private Vector3 GetRandomNavMeshPoint(Vector3 center, float range)
        {
            for (int i = 0; i < 30; i++)
            {
                Vector3 randomPoint = center + Random.insideUnitSphere * range;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
                {
                    return hit.position;
                }
            }
            return center;
        }

        private void Update()
        {
             if (CurrentState == CustomerState.Leaving)
            {
                if (!agent.pathPending && agent.remainingDistance <= 1.0f)
                {
                    onCustomerLeft?.Invoke(this);
                    Destroy(gameObject);
                }
            }
        }

        private void LateUpdate()
        {
            if (bubbleObject != null && bubbleObject.activeInHierarchy)
            {
                // Znajdź główną kamerę (jeśli nie jest zcache'owana, Camera.main robi to wewnętrznie, ale warto uważać w update)
                Camera cam = Camera.main;
                if (cam != null)
                {
                    // Billboard: Ustaw rotację taką samą jak kamera, żeby tekst był zawsze przodem do gracza
                    bubbleObject.transform.rotation = cam.transform.rotation;
                }
            }
        }
    }
}
