using UnityEngine;
using KillerPrices.Data;

namespace KillerPrices.Shop
{
    public class CheckoutItem : MonoBehaviour
    {
        public GunData Data { get; private set; }
        private bool isScanned = false;

        public void Initialize(GunData data)
        {
            Data = data;
            isScanned = false;
        }

        private void OnMouseDown() // Simple interaction for now, can be replaced with Raycast system later
        {
            if (isScanned) return;
            
            // Notify Manager
            if (CheckoutManager.Instance != null)
            {
                CheckoutManager.Instance.OnItemClicked(this);
            }
        }

        public void MarkAsScanned()
        {
            isScanned = true;
            // Visual feedback
            GetComponent<Renderer>().material.color = Color.green; // Simple feedback
            
            // Optional: Bounce animation or particle
            transform.localScale *= 1.1f;
        }
    }
}
