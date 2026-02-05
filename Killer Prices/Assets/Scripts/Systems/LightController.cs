using UnityEngine;

namespace KillerPrices.Systems
{
    public class LightController : MonoBehaviour
    {
        private Light myLight;
        private Renderer[] renderers;
        
        [SerializeField] private int materialIndex = 0; // Który materiał ma emisję (zazwyczaj 0 lub 1 dla żarówki)
        [SerializeField] private Color emissionColor = Color.white;
        [SerializeField] private float emissionIntensity = 2f;

        private void Awake()
        {
            myLight = GetComponentInChildren<Light>();
            renderers = GetComponentsInChildren<Renderer>();
        }

        private void Start()
        {
            if (LightManager.Instance != null)
            {
                LightManager.Instance.RegisterLight(this);
            }
            else
            {
                // Fallback if no manager initially
                SetState(true); 
            }
        }

        private void OnDestroy()
        {
            if (LightManager.Instance != null)
            {
                LightManager.Instance.UnregisterLight(this);
            }
        }

        public void SetState(bool isOn)
        {
            if (myLight != null) myLight.enabled = isOn;

            // Optional: Toggle Emissive material
            if (renderers != null)
            {
                foreach (var rend in renderers)
                {
                    if (rend == null) continue;
                    
                    Material[] mats = rend.materials; // Copy
                    if (materialIndex < mats.Length)
                    {
                        Material targetMat = mats[materialIndex];
                        if (isOn)
                        {
                             targetMat.EnableKeyword("_EMISSION");
                             targetMat.SetColor("_EmissionColor", emissionColor * emissionIntensity);
                        }
                        else
                        {
                            targetMat.DisableKeyword("_EMISSION");
                            targetMat.SetColor("_EmissionColor", Color.black);
                        }
                    }
                    rend.materials = mats; // Apply copy back (Unity quirk)
                }
            }
        }
    }
}
