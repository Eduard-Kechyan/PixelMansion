using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class SettingsMenu : MonoBehaviour
    {
        // Variables
        public Color onColor;
        public Color offColor;

        private bool externalAppOpened = false;

        private MenuUI.Menu menuType = MenuUI.Menu.Settings;

        // Enums
        public enum SocialMediaType
        {
            Instagram,
            Facebook,
            Youtube
        }

        // References
        private MenuUI menuUI;
        private LocaleMenu localeMenu;
        private ConfirmMenu confirmMenu;
        private NoteMenu noteMenu;
        private RateMenu rateMenu;
        private MenuUtilities menuUtilities;
        private I18n LOCALE;
        private Settings settings;
        private Services services;
        private AuthManager authManager;
        private SoundManager soundManager;
        //  private Notifics notifics;
        private ResetHandler resetHandler;
        private FeedbackManager feedbackManager;
        private UIData uiData;

        // UI
        private VisualElement content;

        private Button soundButton;
        private Button musicButton;
        private Button vibrationButton;
        private Button notificationsButton;

        private Button feedbackButton;
        private Button supportButton;
        private Button privacyButton;
        private Button termsButton;
        private Button languageButton;
        private Button resetButton;
        private Button exitButton;
        private Button rateButton;

        private Button signInButton;
        private Button signOutButton;

        private Button instagramFollowButton;
        private Button facebookFollowButton;
        private Button youtubeFollowButton;

        private Label signInLabel;
        private Label followLabel;
        private Label versionLabel;

        private Label idLabel;
        private Button idCopyButton;
        private VisualElement copyCheck;

        void Start()
        {
            // Cache
            menuUI = GetComponent<MenuUI>();
            localeMenu = GetComponent<LocaleMenu>();
            confirmMenu = GetComponent<ConfirmMenu>();
            noteMenu = GetComponent<NoteMenu>();
            rateMenu = GetComponent<RateMenu>();
            menuUtilities = GetComponent<MenuUtilities>();
            LOCALE = I18n.Instance;
            settings = Settings.Instance;
            services = Services.Instance;
            authManager = services.GetComponent<AuthManager>();
            soundManager = SoundManager.Instance;
            //notifics = Services.Instance.GetComponent<Notifics>();
            resetHandler = GetComponent<ResetHandler>();
            feedbackManager = GameRefs.Instance.feedbackManager;
            uiData = GameData.Instance.GetComponent<UIData>();

            DataManager.Instance.CheckLoaded(() =>
            {
                // UI
                content = uiData.GetMenuAsset(menuType);

                soundButton = content.Q<Button>("SoundButton");
                musicButton = content.Q<Button>("MusicButton");
                vibrationButton = content.Q<Button>("VibrationButton");
                notificationsButton = content.Q<Button>("NotificationsButton");

                feedbackButton = content.Q<Button>("FeedbackButton");
                supportButton = content.Q<Button>("SupportButton");
                privacyButton = content.Q<Button>("PrivacyButton");
                termsButton = content.Q<Button>("TermsButton");
                languageButton = content.Q<Button>("LanguageButton");
                resetButton = content.Q<Button>("ResetButton");
                exitButton = content.Q<Button>("ExitButton");
                rateButton = content.Q<Button>("RateButton");

                signInButton = content.Q<Button>("SignInButton");
                signOutButton = content.Q<Button>("SignOutButton");

                instagramFollowButton = content.Q<Button>("InstagramFollowButton");
                facebookFollowButton = content.Q<Button>("FacebookFollowButton");
                youtubeFollowButton = content.Q<Button>("YoutubeFollowButton");

                signInLabel = content.Q<Label>("SignInLabel");
                followLabel = content.Q<Label>("FollowLabel");
                versionLabel = content.Q<Label>("VersionLabel");

                idLabel = content.Q<Label>("IDLabel");
                idCopyButton = content.Q<Button>("IDCopyButton");
                copyCheck = content.Q<VisualElement>("CopyCheck");

                // Button clicks 
                soundButton.clicked += () => settings.ToggleSound();
                musicButton.clicked += () => settings.ToggleMusic();
                vibrationButton.clicked += () => settings.ToggleVibration();
                notificationsButton.clicked += () => settings.ToggleNotifications();

                feedbackButton.clicked += () => soundManager.Tap(feedbackManager.Open);
                supportButton.clicked += () => soundManager.Tap(() => Application.OpenURL(GameData.WEB_ADDRESS + "/support"));
                privacyButton.clicked += () => soundManager.Tap(() => menuUtilities.TryToGetOnlineData(MessageMenu.MessageType.Terms));
                termsButton.clicked += () => soundManager.Tap(() => menuUtilities.TryToGetOnlineData(MessageMenu.MessageType.Privacy));
                languageButton.clicked += () => soundManager.Tap(localeMenu.Open);
                resetButton.clicked += () => soundManager.Tap(() => confirmMenu.Open("reset", resetHandler.ResetAndRestartApp));
                exitButton.clicked += () => soundManager.Tap(() => confirmMenu.Open("exit", Application.Quit));
                rateButton.clicked += () => soundManager.Tap(() => rateMenu.Open(true));

#if UNITY_ANDROID
                signInButton.clicked += () => HandleSignIn(AuthManager.AuthType.Google);
#elif UNITY_IOS
            signInButton.clicked += () => HandleSignIn(AuthManager.AuthType.Apple);
#elif UNITY_EDITOR
            signInButton.clicked += () => {
                Debug.Log("Signed in to dummy!");
            };
#endif

                signOutButton.clicked += () =>
                {
                    authManager.SignOut();

                    SetUISignInText();
                };

                instagramFollowButton.clicked += () => OpenSocialMediaLink(SocialMediaType.Instagram);
                facebookFollowButton.clicked += () => OpenSocialMediaLink(SocialMediaType.Facebook);
                youtubeFollowButton.clicked += () => OpenSocialMediaLink(SocialMediaType.Youtube);

                idCopyButton.clicked += () => CopyIdToClipboard();
            });
        }

        void OnApplicationPause(bool pauseStatus)
        {
            externalAppOpened = true;
        }

        public void Open()
        {
            // Check menu
            if (menuUI.IsMenuOpen(menuType))
            {
                return;
            }

            // Set menu content            
            if (rateMenu == null)
            {
                // Hide the rate menu button if it's disabled
                rateButton.style.display = DisplayStyle.None;
            }

            idLabel.text = LOCALE.Get("settings_menu_user_id") + authManager.playerId;

            versionLabel.text = LOCALE.Get("settings_menu_version") + Application.version;

            SetUIText();

            SetUIOptionsButtons();

            SetUISignInText();

            // Open menu
            menuUI.OpenMenu(content, menuType);
        }

        void SetUIText()
        {
            // Buttons
            feedbackButton.text = LOCALE.Get("settings_menu_feedback_label");
            supportButton.text = LOCALE.Get("settings_menu_support_label");
            privacyButton.text = LOCALE.Get("settings_menu_privacy_label");
            termsButton.text = LOCALE.Get("settings_menu_terms_label");
            languageButton.text = LOCALE.Get("settings_menu_language_label");
            resetButton.text = LOCALE.Get("settings_menu_reset_label");
            exitButton.text = LOCALE.Get("settings_menu_exit_label");
            rateButton.text = LOCALE.Get("settings_menu_rate_label");

            signInButton.text = LOCALE.Get("settings_menu_sign_in_button");
            signOutButton.text = LOCALE.Get("settings_menu_sign_out_button");

            // Labels
            followLabel.text = LOCALE.Get("settings_menu_follow_label");
        }

        public void SetUIOptionsButtons()
        {
            // Sound
            soundButton.style.unityBackgroundImageTintColor = offColor;
            if (settings.soundOn)
            {
                soundButton.style.unityBackgroundImageTintColor = onColor;
            }

            // Music
            musicButton.style.unityBackgroundImageTintColor = offColor;
            if (settings.musicOn)
            {
                musicButton.style.unityBackgroundImageTintColor = onColor;
            }

            // Vibration
            vibrationButton.style.unityBackgroundImageTintColor = offColor;
            if (settings.vibrationOn)
            {
                vibrationButton.style.unityBackgroundImageTintColor = onColor;
            }

            // Notifications
            notificationsButton.style.unityBackgroundImageTintColor = offColor;
            if (settings.notificationsOn)
            {
                notificationsButton.style.unityBackgroundImageTintColor = onColor;
            }
        }

        public void SetUISignInText()
        {
            string socialName = "Dummy";

#if UNITY_ANDROID
            socialName = LOCALE.Get("social_Google");

            if (services.googleSignIn)
            {
                signInLabel.text = LOCALE.Get("settings_menu_signed_in_label_to", socialName);

                signInButton.style.display = DisplayStyle.None;

                signOutButton.style.display = DisplayStyle.Flex;
            }
            else
            {
                signInLabel.text = LOCALE.Get("settings_menu_sign_in_label");

                signInButton.style.display = DisplayStyle.Flex;
                signInButton.text = LOCALE.Get("settings_menu_sing_in_to", socialName);

                signOutButton.style.display = DisplayStyle.None;
            }
#elif UNITY_IOS
            socialName = LOCALE.Get("social_Apple");

            if (services.appleSignIn)
            {
                signInLabel.text = LOCALE.Get("settings_menu_signed_in_label_to", socialName);

                signInButton.style.display = DisplayStyle.None;

                signOutButton.style.display = DisplayStyle.Flex;
            }
            else
            {
                signInLabel.text = LOCALE.Get("settings_menu_sign_in_label");

                signInButton.style.display = DisplayStyle.Flex;
                signInButton.text = LOCALE.Get("settings_menu_sing_in_to", socialName);

                signOutButton.style.display = DisplayStyle.None;
            }
#endif
        }

        public void SetLocale(I18n.Locale newLocale)
        {
            settings.SetLocale(newLocale, false);
        }

        void CopyIdToClipboard()
        {
            GUIUtility.systemCopyBuffer = authManager.playerId;

            copyCheck.style.display = DisplayStyle.Flex;
            copyCheck.style.opacity = 1;

            StartCoroutine(RemoveCopyCheck());
        }

        IEnumerator RemoveCopyCheck()
        {
            yield return new WaitForSeconds(2f);

            copyCheck.style.display = DisplayStyle.None;
            copyCheck.style.opacity = 0;
        }

        void HandleSignIn(AuthManager.AuthType type)
        {
            authManager.SignIn(() =>
            {
                noteMenu.Open("note_menu_log_signed_in_title", new List<string>() { "note_menu_log_signed_in_" + type });

                SetUISignInText();
            }, (bool canceled, string preFix) =>
            {
                if (!canceled)
                {
                    List<string> notes = new List<string>() { "note_menu_log_in_failed_" + type };

                    if (preFix != "")
                    {
                        notes.Add("note_menu_log_in_failed_" + preFix);
                    }

                    noteMenu.Open("note_menu_log_in_failed_title", notes);
                }
            });
        }

        public void OpenSocialMediaLink(SocialMediaType type)
        {
            switch (type)
            {
                case SocialMediaType.Instagram:
                    Application.OpenURL("instragram://user?username=" + GameData.STUDIO_NAME);

                    break;
                case SocialMediaType.Facebook:
                    Application.OpenURL("https://facebook.com/" + GameData.STUDIO_NAME);

                    break;
                case SocialMediaType.Youtube:
                    Application.OpenURL("https://youtube.com/@" + GameData.STUDIO_NAME);

                    break;
            }

            if (!PlayerPrefs.HasKey("followResult"))
            {
                PlayerPrefs.SetInt("followResult", 1);
            }

            // TODO - Send statistic to the server (type)

            externalAppOpened = false;

            StartCoroutine(CheckSocialMediaLink(type));
        }

        IEnumerator CheckSocialMediaLink(SocialMediaType type)
        {
            yield return new WaitForSeconds(1f);

            if (!externalAppOpened)
            {
                switch (type)
                {
                    case SocialMediaType.Instagram:
                        Application.OpenURL("https://instagram.com/" + GameData.STUDIO_NAME);

                        break;
                    case SocialMediaType.Facebook:
                        Application.OpenURL("https://facebook.com/" + GameData.STUDIO_NAME);

                        break;
                    case SocialMediaType.Youtube:
                        Application.OpenURL("https://youtube.com/@" + GameData.STUDIO_NAME);

                        break;
                }
            }
        }
    }
}