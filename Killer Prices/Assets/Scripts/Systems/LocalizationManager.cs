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
                
                // IMPORTANT: Save immediately to ensure persistence even if game crashes
                PlayerPrefs.Save(); 
                
                OnLanguageChanged?.Invoke();
                Debug.Log($"Localization: Language set to {langCode}");
            }
        }

        public void CycleLanguage()
        {
            // Simple toggle for PL/EN. Can be expanded to List if more langs added.
            if (CurrentLanguage == "pl") SetLanguage("en");
            else SetLanguage("pl");
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

            // Quality Names (Defaults)
            AddTrans(pl, en, "QUALITY_VERY LOW", "Bardzo Niska", "Very Low");
            AddTrans(pl, en, "QUALITY_LOW", "Niska", "Low");
            AddTrans(pl, en, "QUALITY_MEDIUM", "Średnia", "Medium");
            AddTrans(pl, en, "QUALITY_HIGH", "Wysoka", "High");
            AddTrans(pl, en, "QUALITY_VERY HIGH", "Bardzo Wysoka", "Very High");
            AddTrans(pl, en, "QUALITY_ULTRA", "Ultra", "Ultra");

            // Misc
            AddTrans(pl, en, "WS_LOCKED_LABEL", "ZABLOKOWANE", "LOCKED");

            // Pause Menu
            AddTrans(pl, en, "PAUSE_TITLE", "PAUZA", "PAUSE");
            AddTrans(pl, en, "PAUSE_RESUME", "WZNÓW", "RESUME");
            AddTrans(pl, en, "PAUSE_SAVE", "ZAPISZ GRĘ", "SAVE GAME");
            AddTrans(pl, en, "PAUSE_MENU", "MENU GŁÓWNE", "MAIN MENU");
            AddTrans(pl, en, "PAUSE_QUIT", "WYJŚCIE", "QUIT GAME");

            // Main Menu Popups
            AddTrans(pl, en, "MENU_OVERWRITE_CONFIRM", "Istnieje zapis gry. Czy chcesz rozpocząć nową grę i nadpisać stary zapis?", "Save file exists. Do you want to overwrite it?");
            AddTrans(pl, en, "MENU_NO_SAVE", "Brak zapisu gry. Kliknij NOWA GRA, aby rozpocząć.", "No save file found. Click NEW GAME to start.");
            AddTrans(pl, en, "MENU_YES", "TAK", "YES");
            AddTrans(pl, en, "MENU_NO", "NIE", "NO");
            AddTrans(pl, en, "MENU_OK", "OK", "OK");

            // Wholesale
            AddTrans(pl, en, "WS_TAB_WEAPONS", "BRONIE", "WEAPONS");
            AddTrans(pl, en, "WS_TAB_FURNITURE", "MEBLE", "FURNITURE");
            AddTrans(pl, en, "WS_BUY", "KUP", "BUY");
            AddTrans(pl, en, "WS_LOCKED", "WYMAGANY LV.", "REQ. LV.");
            AddTrans(pl, en, "WS_TITLE", "HURTOWNIA", "WHOLESALE");
            
            // HUD
            AddTrans(pl, en, "HUD_MONEY", "Gotówka: $", "Cash: $");
            AddTrans(pl, en, "HUD_LEVEL", "Poziom: ", "Level: ");

            // Wholesale & Shop
            AddTrans(pl, en, "WS_COST", "Koszt: $", "Cost: $");
            AddTrans(pl, en, "WS_PRICE", "Cena: $", "Price: $");
            AddTrans(pl, en, "WS_LOCKED_MSG", "Za mało pieniędzy!", "Not enough money!");
            AddTrans(pl, en, "WS_ORDERED", "Zamówiono: ", "Ordered: ");

            // Shelf & Placement
            AddTrans(pl, en, "SHELF_FULL", "Ta półka jest pełna!", "This shelf is full!");
            AddTrans(pl, en, "SHELF_EMPTY", "Ta półka jest pusta.", "This shelf is empty.");
            AddTrans(pl, en, "PLACE_PLACED", "Położono na półce.", "Placed on shelf.");
            AddTrans(pl, en, "PLACE_INVALID", "Nie można tu postawić!", "Cannot place here!");

            // Checkout
            AddTrans(pl, en, "CHECKOUT_TOTAL", "Do zapłaty: $", "Total: $");
            AddTrans(pl, en, "CHECKOUT_PAID", "Zapłacono: $", "Paid: $");
            AddTrans(pl, en, "CHECKOUT_CHANGE", "Reszta: $", "Change: $");
            AddTrans(pl, en, "CHECKOUT_HEADER", "KASA", "CHECKOUT");

            // Interaction Prompts
            AddTrans(pl, en, "INTERACT_PROMPT", "Naciśnij [E] aby {0}", "Press [E] to {0}");
            AddTrans(pl, en, "INTERACT_OPEN", "otworzyć", "open");
            AddTrans(pl, en, "INTERACT_CLOSE", "zamknąć", "close");
            AddTrans(pl, en, "INTERACT_TOGGLE_LIGHT", "przełączyć światło", "toggle light");
            AddTrans(pl, en, "INTERACT_CHECKOUT", "użyć kasy", "use checkout");
            AddTrans(pl, en, "INTERACT_SHELF", "zarządzać półką", "manage shelf");
            AddTrans(pl, en, "INTERACT_WHOLESALE", "otworzyć hurtownię", "open wholesale");
            AddTrans(pl, en, "INTERACT_PICKUP", "podnieść", "pick up");
            AddTrans(pl, en, "INTERACT_OPEN_SHOP", "otworzyć sklep", "open shop");
            AddTrans(pl, en, "INTERACT_CLOSE_SHOP", "zamknąć sklep", "close shop");
            AddTrans(pl, en, "INTERACT_EXPAND", "rozbudować sklep", "expand shop");

            // Pricing UI
            AddTrans(pl, en, "PRICING_PROFIT", "Zysk: ${0}", "Profit: ${0}");
            AddTrans(pl, en, "PRICING_LOSS", "Strata: ${0}", "Loss: ${0}");
            AddTrans(pl, en, "PRICING_CONFIRM", "Zatwierdź", "Confirm");
            AddTrans(pl, en, "PRICING_TAKE", "Zabierz", "Take");

            // Checkout UI
            AddTrans(pl, en, "CHECKOUT_HEADER", "KASA FISKALNA", "CASH REGISTER");
            AddTrans(pl, en, "CHECKOUT_SCANNED", "Zeskanowane Produkty:", "Scanned Products:");
            AddTrans(pl, en, "CHECKOUT_TOTAL", "Suma: ${0}", "Total: ${0}");
            AddTrans(pl, en, "CHECKOUT_RECEIVED", "Otrzymano: ${0}", "Received: ${0}");
            AddTrans(pl, en, "CHECKOUT_CHANGE", "Reszta:", "Change:");
            AddTrans(pl, en, "CHECKOUT_FINISH", "ZATWIERDŹ", "CONFIRM");

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
