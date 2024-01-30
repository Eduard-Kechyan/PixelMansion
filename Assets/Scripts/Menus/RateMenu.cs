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

        [HideInInspector]
        public bool shouldShow = true;
        private string rateDate;

        private int starCount = 0;

        // References
        private MenuUI menuUI;
        private GameData gameData;
        private I18n LOCALE;

        // UI
        private VisualElement root;
        private VisualElement rateMenu;
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

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            rateMenu = root.Q<VisualElement>("RateMenu");

            character = rateMenu.Q<VisualElement>("Character");

            starsBox = rateMenu.Q<VisualElement>("StarsBox");

            starsLabel = rateMenu.Q<Label>("StarsLabel");
            rateLabel0 = rateMenu.Q<Label>("RateLabel0");
            rateLabel1 = rateMenu.Q<Label>("RateLabel1");

            yesButton = rateMenu.Q<Button>("YesButton");
            noButton = rateMenu.Q<Button>("NoButton");
            neverButton = rateMenu.Q<Button>("NeverButton");

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
        }

        void Init()
        {
            // Make sure the menu is closed
            rateMenu.style.display = DisplayStyle.None;
            rateMenu.style.opacity = 0;

            yesButton.text = LOCALE.Get("rate_menu_yes_button");
            noButton.text = LOCALE.Get("rate_menu_no_button");
            neverButton.text = LOCALE.Get("rate_menu_never_button");

            rateLabel0.text = LOCALE.Get("rate_menu_rate_label_0", GameData.GAME_TITLE);
            rateLabel1.text = LOCALE.Get("rate_menu_rate_label_1");

            character.style.top = -15;
        }

        public void Open(bool ignoreCheck = false)
        {
            if (ignoreCheck || gameData.level >= 3)
            {
                // Set the title
                string title = LOCALE.Get("rate_menu_title");

                // Open menu
                menuUI.OpenMenu(rateMenu, title, false, false, true);

                starsLabel.text = starCount + "/" + 5;
            }
        }

        void SetStar(string stringOrder)
        {
            int order = int.Parse(stringOrder);

            starCount = order + 1;

            starsLabel.text = starCount + "/" + 5;

            switch (order)
            {
                case 0:
                    character.style.top = -26;
                    break;
                case 1:
                    character.style.top = -30;
                    break;
                case 2:
                    character.style.top = -34;
                    break;
                case 3:
                    character.style.top = -37;
                    break;
                case 4:
                    character.style.top = -41;
                    break;
            }

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

            // FIX - Send statistic to the server (starCount)

            // FIX - Open the game's app store page
            // FIX - Optionally reward the player

            CloseMenu();
        }

        void HandleNoButton()
        {
            // FIX - Send statistic to the server

            CloseMenu();
        }

        void HandleNeverButton()
        {
            PlayerPrefs.SetInt("rateResult", 0); // Note the 0

            // FIX - Send statistic to the server

            // FIX - Open the feedback menu

            CloseMenu();
        }

        void CloseMenu()
        {
            menuUI.CloseMenu(rateMenu.name, () =>
            {
                shouldShow = false;
            });
        }
    }
}
