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

        // References
        private MenuUI menuUI;
        private LocaleMenu localeMenu;
        private ConfirmMenu confirmMenu;
        private NoteMenu noteMenu;
        private RateMenu rateMenu;
        private I18n LOCALE;
        private Settings settings;
        private Services services;
        private SignIn signIn;
        //  private Notifics notifics;
        private ResetHandler resetHandler;

        // UI
        private VisualElement root;
        private VisualElement settingsMenu;

        private Button soundButton;
        private Button musicButton;
        private Button vibrationButton;
        private Button notificationsButton;

        private Button supportButton;
        private Button privacyButton;
        private Button termsButton;
        private Button languageButton;
        private Button resetButton;
        private Button exitButton;
        private Button rateButton;

        private Button googleSignInButton;
        private VisualElement googleSignInCheck;
        private Button facebookSignInButton;
        private VisualElement facebookSignInCheck;
        private Button appleSignInButton;
        private VisualElement appleSignInCheck;

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
            LOCALE = I18n.Instance;
            settings = Settings.Instance;
            services = Services.Instance;
            signIn = services.GetComponent<SignIn>();
            //  notifics = Services.Instance.GetComponent<Notifics>();
            resetHandler = GetComponent<ResetHandler>();

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            settingsMenu = root.Q<VisualElement>("SettingsMenu");

            soundButton = settingsMenu.Q<Button>("SoundButton");
            musicButton = settingsMenu.Q<Button>("MusicButton");
            vibrationButton = settingsMenu.Q<Button>("VibrationButton");
            notificationsButton = settingsMenu.Q<Button>("NotificationsButton");

            supportButton = settingsMenu.Q<Button>("SupportButton");
            privacyButton = settingsMenu.Q<Button>("PrivacyButton");
            termsButton = settingsMenu.Q<Button>("TermsButton");
            languageButton = settingsMenu.Q<Button>("LanguageButton");
            resetButton = settingsMenu.Q<Button>("ResetButton");
            exitButton = settingsMenu.Q<Button>("ExitButton");
            rateButton = settingsMenu.Q<Button>("RateButton");

            googleSignInButton = settingsMenu.Q<Button>("GoogleSignInButton");
            facebookSignInButton = settingsMenu.Q<Button>("FacebookSignInButton");
            appleSignInButton = settingsMenu.Q<Button>("AppleSignInButton");

            googleSignInCheck = googleSignInButton.Q<VisualElement>("SignedInCheck");
            facebookSignInCheck = facebookSignInButton.Q<VisualElement>("SignedInCheck");
            appleSignInCheck = appleSignInButton.Q<VisualElement>("SignedInCheck");

            instagramFollowButton = settingsMenu.Q<Button>("InstagramFollowButton");
            facebookFollowButton = settingsMenu.Q<Button>("FacebookFollowButton");
            youtubeFollowButton = settingsMenu.Q<Button>("YoutubeFollowButton");

            signInLabel = settingsMenu.Q<Label>("SignInLabel");
            followLabel = settingsMenu.Q<Label>("FollowLabel");
            versionLabel = settingsMenu.Q<Label>("VersionLabel");

            idLabel = settingsMenu.Q<Label>("IDLabel");
            idCopyButton = settingsMenu.Q<Button>("IDCopyButton");
            copyCheck = settingsMenu.Q<VisualElement>("CopyCheck");

            // Button clicks 
            soundButton.clicked += () => settings.ToggleSound();
            musicButton.clicked += () => settings.ToggleMusic();
            vibrationButton.clicked += () => settings.ToggleVibration();
            notificationsButton.clicked += () => settings.ToggleNotifications();

            supportButton.clicked += () => Application.OpenURL(GameData.WEB_ADDRESS + "/support");
            privacyButton.clicked += () => Application.OpenURL(GameData.WEB_ADDRESS + "/privacy");
            termsButton.clicked += () => Application.OpenURL(GameData.WEB_ADDRESS + "/terms");
            languageButton.clicked += () => localeMenu.Open();
            resetButton.clicked += () => confirmMenu.Open("reset", resetHandler.ResetAndRestartApp);
            exitButton.clicked += () => confirmMenu.Open("exit", Application.Quit);
            rateButton.clicked += () => rateMenu.Open(true);

            googleSignInButton.clicked += () => HandleSignIn(SignIn.SignInType.Google);
            facebookSignInButton.clicked += () => HandleSignIn(SignIn.SignInType.Facebook);
            appleSignInButton.clicked += () => HandleSignIn(SignIn.SignInType.Apple);

            instagramFollowButton.clicked += () => OpenSocialMediaLink(Types.SocialMediaType.Instagram);
            facebookFollowButton.clicked += () => OpenSocialMediaLink(Types.SocialMediaType.Facebook);
            youtubeFollowButton.clicked += () => OpenSocialMediaLink(Types.SocialMediaType.Youtube);

            idCopyButton.clicked += () => CopyIdToClipboard();

            Init();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            externalAppOpened = true;
        }

        void Init()
        {
            // Make sure the menu is closed
            settingsMenu.style.display = DisplayStyle.None;
            settingsMenu.style.opacity = 0;

            if (!Debug.isDebugBuild || !Application.isEditor) // TODO - Remove this  after adding at least one other language
            {
                languageButton.style.display = DisplayStyle.None;
            }
        }

        public void Open()
        {
            // Set the title
            string title = LOCALE.Get("settings_menu_title");

            // Hide the rate menu button if it's disabled
            if (rateMenu == null)
            {
                rateButton.style.display = DisplayStyle.None;
            }

            idLabel.text = LOCALE.Get("settings_menu_user_id") + GameData.Instance.userId;

            versionLabel.text = LOCALE.Get("settings_menu_version") + Application.version;

#if UNITY_ANDROID
            appleSignInButton.style.display = DisplayStyle.None;
#elif UNITY_IOS
            googleSignInButton.style.display = DisplayStyle.None;
#endif

            SetUiText();

            SetUIOptionsButtons();

            SetUISignInButtons();

            // Open menu
            menuUI.OpenMenu(settingsMenu, title);
        }

        void SetUiText()
        {
            // Buttons
            supportButton.text = LOCALE.Get("settings_menu_support_label");
            privacyButton.text = LOCALE.Get("settings_menu_privacy_label");
            termsButton.text = LOCALE.Get("settings_menu_terms_label");
            languageButton.text = LOCALE.Get("settings_menu_language_label");
            resetButton.text = LOCALE.Get("settings_menu_reset_label");
            exitButton.text = LOCALE.Get("settings_menu_exit_label");

            // Labels
            signInLabel.text = LOCALE.Get("settings_menu_sign_in_label");
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

        public void SetUISignInButtons()
        {
            // Google sign in
            googleSignInCheck.style.display = DisplayStyle.None;
            if (services.googleSignIn)
            {
                googleSignInCheck.style.display = DisplayStyle.Flex;
            }

            // Facebook sign in
            facebookSignInCheck.style.display = DisplayStyle.None;
            if (services.facebookSignIn)
            {
                facebookSignInCheck.style.display = DisplayStyle.Flex;
            }

            // Apple sign in
            appleSignInCheck.style.display = DisplayStyle.None;
            if (services.appleSignIn)
            {
                appleSignInCheck.style.display = DisplayStyle.Flex;
            }
        }

        public void SetLocale(Types.Locale newLocale)
        {
            settings.SetLocale(newLocale, false);
        }

        void CopyIdToClipboard()
        {
            GUIUtility.systemCopyBuffer = GameData.Instance.userId;

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

        void HandleSignIn(SignIn.SignInType type)
        {
            signIn.LogIn(type, (bool firstSingIn) =>
            {
                if (firstSingIn)
                {
                    googleSignInCheck.style.display = DisplayStyle.Flex;
                }
                else
                {
                    string[] notes = new string[] { "note_menu_log_in_already_" + type };

                    noteMenu.Open("note_menu_log_in_already_title", notes);
                }
            }, (string preFix) =>
            {
                string[] notes = new string[] { "note_menu_log_in_failed_" + type, "note_menu_log_in_failed_" + preFix };

                noteMenu.Open("note_menu_log_in_failed_title", notes);
            });
        }

        public void OpenSocialMediaLink(Types.SocialMediaType type)
        {
            switch (type)
            {
                case Types.SocialMediaType.Instagram:
                    Application.OpenURL("instragram://user?username=" + GameData.STUDIO_NAME);

                    break;
                case Types.SocialMediaType.Facebook:
                    Application.OpenURL("https://facebook.com/" + GameData.STUDIO_NAME);

                    break;
                case Types.SocialMediaType.Youtube:
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

        IEnumerator CheckSocialMediaLink(Types.SocialMediaType type)
        {
            yield return new WaitForSeconds(1f);

            if (!externalAppOpened)
            {
                switch (type)
                {
                    case Types.SocialMediaType.Instagram:
                        Application.OpenURL("https://instagram.com/" + GameData.STUDIO_NAME);

                        break;
                    case Types.SocialMediaType.Facebook:
                        Application.OpenURL("https://facebook.com/" + GameData.STUDIO_NAME);

                        break;
                    case Types.SocialMediaType.Youtube:
                        Application.OpenURL("https://youtube.com/@" + GameData.STUDIO_NAME);

                        break;
                }
            }
        }
    }
}