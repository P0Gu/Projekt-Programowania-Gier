using UnityEngine;
using System;
using System.Collections.Generic;

namespace KillerPrices.Systems
{
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        public event Action OnLanguageChanged;
        public string CurrentLanguage { get; private set; } = "pl"; // Default

        private Dictionary<string, Dictionary<string, string>> translations;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeTranslations();
            
            // Load saved language
            string savedLang = PlayerPrefs.GetString("Language", "pl");
            SetLanguage(savedLang);
        }

        public void SetLanguage(string langCode)
        {
            if (translations.ContainsKey(langCode) || langCode == "pl" || langCode == "en")
            {
                CurrentLanguage = langCode;
                PlayerPrefs.SetString("Language", langCode);
                OnLanguageChanged?.Invoke();
                Debug.Log($"Localization: Language set to {langCode}");
            }
        }

        public string GetTranslation(string key)
        {
            if (translations.ContainsKey(CurrentLanguage) && translations[CurrentLanguage].ContainsKey(key))
            {
                return translations[CurrentLanguage][key];
            }
            
            // Fallback to English if key missing in current lang
            if (translations.ContainsKey("en") && translations["en"].ContainsKey(key))
            {
                return translations["en"][key];
            }

            return key; // Return key if translation missing
        }

        private void InitializeTranslations()
        {
            translations = new Dictionary<string, Dictionary<string, string>>();

            var pl = new Dictionary<string, string>();
            var en = new Dictionary<string, string>();

            // Main Menu
            AddTrans(pl, en, "MENU_NEW_GAME", "NOWA GRA", "NEW GAME");
            AddTrans(pl, en, "MENU_LOAD_GAME", "WCZYTAJ GRĘ", "LOAD GAME");
            AddTrans(pl, en, "MENU_SETTINGS", "USTAWIENIA", "SETTINGS");
            AddTrans(pl, en, "MENU_QUIT", "WYJŚCIE", "QUIT");

            // Settings
            AddTrans(pl, en, "SETTINGS_TITLE", "USTAWIENIA", "SETTINGS");
            AddTrans(pl, en, "SETTINGS_AUDIO", "DŹWIĘK", "AUDIO");
            AddTrans(pl, en, "SETTINGS_VOLUME", "Głośność", "Volume");
            AddTrans(pl, en, "SETTINGS_GRAPHICS", "GRAFIKA", "GRAPHICS");
            AddTrans(pl, en, "SETTINGS_RESOLUTION", "Rozdzielczość", "Resolution");
            AddTrans(pl, en, "SETTINGS_QUALITY", "Jakość", "Quality");
            AddTrans(pl, en, "SETTINGS_FULLSCREEN", "Pełny Ekran", "Fullscreen");
            AddTrans(pl, en, "SETTINGS_SAVE", "ZAPISZ", "SAVE");
            AddTrans(pl, en, "SETTINGS_BACK", "ANULUJ", "CANCEL");
            AddTrans(pl, en, "SETTINGS_LANG", "Język", "Language");

            // Pause Menu
            AddTrans(pl, en, "PAUSE_TITLE", "PAUZA", "PAUSE");
            AddTrans(pl, en, "PAUSE_RESUME", "WZNÓW", "RESUME");
            AddTrans(pl, en, "PAUSE_SAVE", "ZAPISZ GRĘ", "SAVE GAME");
            AddTrans(pl, en, "PAUSE_MENU", "MENU GŁÓWNE", "MAIN MENU");
            AddTrans(pl, en, "PAUSE_QUIT", "WYJŚCIE", "QUIT GAME");

            // Wholesale
            AddTrans(pl, en, "WS_TAB_WEAPONS", "BRONIE", "WEAPONS");
            AddTrans(pl, en, "WS_TAB_FURNITURE", "MEBLE", "FURNITURE");
            AddTrans(pl, en, "WS_BUY", "KUP", "BUY");
            AddTrans(pl, en, "WS_LOCKED", "WYMAGANY LV.", "REQ. LV.");
            AddTrans(pl, en, "WS_TITLE", "HURTOWNIA", "WHOLESALE");
            
            // HUD
            AddTrans(pl, en, "HUD_MONEY", "Gotówka: $", "Cash: $");
            AddTrans(pl, en, "HUD_LEVEL", "Poziom: ", "Level: ");

            translations["pl"] = pl;
            translations["en"] = en;
        }

        private void AddTrans(Dictionary<string, string> plDict, Dictionary<string, string> enDict, string key, string pl, string en)
        {
            plDict[key] = pl;
            enDict[key] = en;
        }
    }
}
