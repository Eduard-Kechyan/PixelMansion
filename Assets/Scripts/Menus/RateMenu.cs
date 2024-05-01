using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class RateMenu : MonoBehaviour
    {
        // Variables
        public Sprite starEmptySprite;
        public Sprite starFullSprite;
        public int[] characterHights;

        [HideInInspector]
        public bool shouldShow = true;
        private string rateDate;

        private int starCount = 0;

        private Types.Menu menuType = Types.Menu.Rate;

        // References
        private MenuUI menuUI;
        private GameData gameData;
        private I18n LOCALE;
        private CloudSave cloudSave;
        private UIData uiData;

        // UI
        private VisualElement content;
        private VisualElement character;
        private VisualElement starsBox;
        private Label starsLabel;
        private Label rateLabel0;
        private Label rateLabel1;
        private Button yesButton;
        private Button noButton;
        private Button neverButton;

        void Awake()
        {
            rateDate = DateTime.UtcNow.Date.ToString();

            if (PlayerPrefs.HasKey("rateResult") || (PlayerPrefs.HasKey("rateDate") && PlayerPrefs.GetString("rateDate") == rateDate))
            {
                shouldShow = false;
            }
        }

        void Start()
        {
            // Cache
            menuUI = GetComponent<MenuUI>();
            gameData = GameData.Instance;
            LOCALE = I18n.Instance;
            cloudSave = Services.Instance.GetComponent<CloudSave>();
            uiData = GameData.Instance.GetComponent<UIData>();

            DataManager.Instance.CheckLoaded(() =>
            {
                // UI
                content = uiData.GetMenuAsset(menuType);

                character = content.Q<VisualElement>("Character");

                starsBox = content.Q<VisualElement>("StarsBox");

                starsLabel = content.Q<Label>("StarsLabel");
                rateLabel0 = content.Q<Label>("RateLabel0");
                rateLabel1 = content.Q<Label>("RateLabel1");

                yesButton = content.Q<Button>("YesButton");
                noButton = content.Q<Button>("NoButton");
                neverButton = content.Q<Button>("NeverButton");

                // UI Taps
                for (int i = 0; i < starsBox.childCount; i++)
                {
                    string order = i.ToString();

                    starsBox.Q<Button>("StarItem" + order).clicked += () => SetStar(order);
                }

                yesButton.clicked += () => HandleYesButton();
                noButton.clicked += () => HandleNoButton();
                neverButton.clicked += () => HandleNeverButton();

                Init();
            });
        }

        void OnValidate()
        {
            if (characterHights == null || characterHights.Length < 5)
            {
                Debug.LogWarning("Character Hights Length needs to be 5!");
            }
        }

        void Init()
        {
            yesButton.text = LOCALE.Get("rate_menu_yes_button");
            noButton.text = LOCALE.Get("rate_menu_no_button");
            neverButton.text = LOCALE.Get("rate_menu_never_button");

            rateLabel0.text = LOCALE.Get("rate_menu_rate_label_0", LOCALE.Get("game_title"));
            rateLabel1.text = LOCALE.Get("rate_menu_rate_label_1");

            character.style.top = -15;
        }

        public void Open(bool ignoreCheck = false)
        {
            // Check menu
            if (menuUI.IsMenuOpen(menuType))
            {
                return;
            }

            if (ignoreCheck || gameData.level >= 3)
            {
                // Open menu
                menuUI.OpenMenu(content, menuType, "", false, false, true, false, true);

                starsLabel.text = starCount + "/" + 5;
            }
        }

        void SetStar(string stringOrder)
        {
            int order = int.Parse(stringOrder);

            starCount = order + 1;

            starsLabel.text = starCount + "/" + 5;

            character.style.top = characterHights[order];

            for (int i = 0; i < starsBox.childCount; i++)
            {
                starsBox.Q<Button>("StarItem" + i).style.backgroundImage = new StyleBackground(starEmptySprite);
            }

            for (int i = 0; i < order + 1; i++)
            {
                starsBox.Q<Button>("StarItem" + i).style.backgroundImage = new StyleBackground(starFullSprite);
            }
        }

        void HandleYesButton()
        {
            PlayerPrefs.SetInt("rateResult", 1); // Note the 1

            cloudSave.SaveDataAsync("rateResult", 1);

            // TODO - Send statistic to the server (starCount)

            // TODO - Open the game's app store page
            // TODO - Optionally reward the player

            CloseMenu();
        }

        void HandleNoButton()
        {
            // TODO - Send statistic to the server

            CloseMenu();
        }

        void HandleNeverButton()
        {
            PlayerPrefs.SetInt("rateResult", 0); // Note the 0

            cloudSave.SaveDataAsync("rateResult", 0);

            // TODO - Send statistic to the server

            // TODO - Open the feedback menu

            CloseMenu();
        }

        void CloseMenu()
        {
            menuUI.CloseMenu(menuType, () =>
            {
                shouldShow = false;
            });
        }
    }
}
