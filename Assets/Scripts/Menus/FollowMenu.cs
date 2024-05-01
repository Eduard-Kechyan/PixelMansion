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

        private Types.Menu menuType = Types.Menu.Follow;

        // References
        private MenuUI menuUI;
        private GameData gameData;
        private SettingsMenu settingsMenu;
        private I18n LOCALE;
        private CloudSave cloudSave;
        private UIData uiData;

        // UI
        private VisualElement content;
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
            uiData = GameData.Instance.GetComponent<UIData>();

            DataManager.Instance.CheckLoaded(() =>
            {
                // UI
                content = uiData.GetMenuAsset(menuType);

                followLabel0 = content.Q<Label>("FollowLabel0");
                followLabel1 = content.Q<Label>("FollowLabel1");

                instagramFollowButton = content.Q<Button>("InstagramFollowButton");
                facebookFollowButton = content.Q<Button>("FacebookFollowButton");
                youtubeFollowButton = content.Q<Button>("YoutubeFollowButton");
                noButton = content.Q<Button>("NoButton");

                // UI Taps
                instagramFollowButton.clicked += () => HandleSocialMediaButton(Types.SocialMediaType.Instagram);
                facebookFollowButton.clicked += () => HandleSocialMediaButton(Types.SocialMediaType.Facebook);
                youtubeFollowButton.clicked += () => HandleSocialMediaButton(Types.SocialMediaType.Youtube);

                noButton.clicked += () => HandleNoButton();

                Init();
            });
        }

        void Init()
        {
            noButton.text = LOCALE.Get("follow_menu_no_button");

            followLabel0.text = LOCALE.Get("follow_menu_rate_label_0", LOCALE.Get("game_title"));
            followLabel1.text = LOCALE.Get("follow_menu_rate_label_1");
        }

        public void Open(bool ignoreCheck = false)
        {
            // Check menu
            if (menuUI.IsMenuOpen(menuType))
            {
                return;
            }

            if (ignoreCheck || gameData.level >= 4)
            {
                // Open menu
                menuUI.OpenMenu(content, menuType, "", false, false, true, false, true);
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
            menuUI.CloseMenu(menuType, () =>
            {
                shouldShow = false;
            });
        }
    }
}
