using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class FollowMenu : MonoBehaviour
    {
        // Variables
        [HideInInspector]
        public bool shouldShow = true;
        private string followDate;

        // References
        private MenuUI menuUI;
        private GameData gameData;
        private SettingsMenu settingsMenu;
        private I18n LOCALE;
        private CloudSave cloudSave;

        // UI
        private VisualElement root;
        private VisualElement followMenu;
        private Label followLabel0;
        private Label followLabel1;
        private Button instagramFollowButton;
        private Button facebookFollowButton;
        private Button youtubeFollowButton;
        private Button noButton;

        void Awake()
        {
            followDate = DateTime.UtcNow.Date.ToString();

            if (PlayerPrefs.HasKey("followResult") || (PlayerPrefs.HasKey("followDate") && PlayerPrefs.GetString("followDate") == followDate))
            {
                shouldShow = false;
            }
        }

        void Start()
        {
            // Cache
            menuUI = GetComponent<MenuUI>();
            gameData = GameData.Instance;
            settingsMenu = GetComponent<SettingsMenu>();
            LOCALE = I18n.Instance;
            cloudSave = Services.Instance.GetComponent<CloudSave>();

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            followMenu = root.Q<VisualElement>("FollowMenu");
            followLabel0 = followMenu.Q<Label>("FollowLabel0");
            followLabel1 = followMenu.Q<Label>("FollowLabel1");

            instagramFollowButton = followMenu.Q<Button>("InstagramFollowButton");
            facebookFollowButton = followMenu.Q<Button>("FacebookFollowButton");
            youtubeFollowButton = followMenu.Q<Button>("YoutubeFollowButton");
            noButton = followMenu.Q<Button>("NoButton");

            // UI Taps
            instagramFollowButton.clicked += () => HandleSocialMediaButton(Types.SocialMediaType.Instagram);
            facebookFollowButton.clicked += () => HandleSocialMediaButton(Types.SocialMediaType.Facebook);
            youtubeFollowButton.clicked += () => HandleSocialMediaButton(Types.SocialMediaType.Youtube);

            noButton.clicked += () => HandleNoButton();

            Init();
        }

        void Init()
        {
            // Make sure the menu is closed
            followMenu.style.display = DisplayStyle.None;
            followMenu.style.opacity = 0;

            noButton.text = LOCALE.Get("follow_menu_no_button");

            followLabel0.text = LOCALE.Get("follow_menu_rate_label_0", GameData.GAME_TITLE);
            followLabel1.text = LOCALE.Get("follow_menu_rate_label_1");
        }

        public void Open(bool ignoreCheck = false)
        {
            if (ignoreCheck || gameData.level >= 4)
            {
                // Set the title
                string title = LOCALE.Get("follow_menu_title");

                // Open menu
                menuUI.OpenMenu(followMenu, title, false, false, true);
            }
        }

        void HandleSocialMediaButton(Types.SocialMediaType type)
        {
            settingsMenu.OpenSocialMediaLink(type);

            PlayerPrefs.SetInt("followResult", 1); // Note the 1

            cloudSave.SaveDataAsync("followResult", 1);

            // FIX - Send statistic to the server (type)

            // FIX - Open the game's social media page
            // FIX - Optionally reward the player

            CloseMenu();
        }

        void HandleNoButton()
        {
            PlayerPrefs.SetInt("followResult", 0); // Note the 0

            cloudSave.SaveDataAsync("followResult", 0);

            // FIX - Send statistic to the server

            CloseMenu();
        }

        void CloseMenu()
        {
            menuUI.CloseMenu(followMenu.name, () =>
            {
                shouldShow = false;
            });
        }
    }
}
