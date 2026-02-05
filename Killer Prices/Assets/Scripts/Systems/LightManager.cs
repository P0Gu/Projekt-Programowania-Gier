using UnityEngine;
using System.Collections.Generic;

namespace KillerPrices.Systems
{
    public class LightManager : MonoBehaviour
    {
        public static LightManager Instance { get; private set; }

        private List<LightController> allLights = new List<LightController>();
        public bool IsLightOn { get; private set; } = true;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void RegisterLight(LightController light)
        {
            if (!allLights.Contains(light))
            {
                allLights.Add(light);
                // Sync new light with current state immediately
                light.SetState(IsLightOn);
            }
        }

        public void UnregisterLight(LightController light)
        {
            if (allLights.Contains(light))
            {
                allLights.Remove(light);
            }
        }

        public void ToggleAllLights()
        {
            IsLightOn = !IsLightOn;
            Debug.Log($"LightManager: Przełączono światła na {(IsLightOn ? "WŁ" : "WYŁ")}. Liczba lamp: {allLights.Count}");

            foreach (var light in allLights)
            {
                if (light != null)
                {
                    light.SetState(IsLightOn);
                }
            }
        }
    }
}
