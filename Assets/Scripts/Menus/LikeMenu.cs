using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class LikeMenu : MonoBehaviour
    {
        // Variables
        public Sprite starEmptySprite;
        public Sprite starFullSprite;

        [HideInInspector]
        public bool shouldShow = true;
        private string rateData;

        private int starCount = 0;

        // References
        private MenuUI menuUI;
        private I18n LOCALE;

        // UI
        private VisualElement root;
        private VisualElement rateMenu;
        private VisualElement starsBox;
        private Label starsLabel;
        private Label likeLabel;
        private Label rateLabel;
        private Button yesButton;
        private Button noButton;
        private Button neverButton;

        void Awake()
        {
            rateData = DateTime.UtcNow.Date.ToString();

            if (PlayerPrefs.HasKey("rateResult") || (PlayerPrefs.HasKey("rateDate") && PlayerPrefs.GetString("rateDate") == rateData))
            {
                shouldShow = false;
            }
        }

        void Start()
        {
            // Cache
            menuUI = GetComponent<MenuUI>();
            LOCALE = I18n.Instance;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            rateMenu = root.Q<VisualElement>("RateMenu");

            starsBox = rateMenu.Q<VisualElement>("StarsBox");

            starsLabel = rateMenu.Q<Label>("StarsLabel");
            likeLabel = rateMenu.Q<Label>("LikeLabel");
            rateLabel = rateMenu.Q<Label>("RateLabel");

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
        }

        public void Open()
        {
            // Set the title
            string title = LOCALE.Get("rate_menu_title");

            // Open menu
            menuUI.OpenMenu(rateMenu, title);

            starsLabel.text = starCount + "/" + 5;

            likeLabel.text = LOCALE.Get("rate_menu_like", GameData.GAME_TITLE);
            rateLabel.text = LOCALE.Get("rate_menu_rate");
        }

        void SetStar(string stringOrder)
        {
            int order = int.Parse(stringOrder);

            starCount = order + 1;

            starsLabel.text = starCount + "/" + 5;

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

            // TODO - Send statistic to the server

            // TODO - Open the feedback menu

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
