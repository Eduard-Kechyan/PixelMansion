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
    private I18n LOCALE;
    private Settings settings;
    private Notifics notifics;
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

    private Button googleSignInButton;
    private VisualElement googleSignInCheck;
    private Button facebookSignInButton;
    private VisualElement facebookSignInCheck;
    private Button appleSignInButton;
    private VisualElement appleSignInCheck;

    private Button instagramFollowButton;
    private Button facebookFollowButton;
    //private Button youtubeFollowButton;

    private Label signInLabel;
    private Label followLabel;
    private Label versionLabel;

    private Label idLabel;
    private Button idCopyButton;
    private VisualElement copyCheck;

    // Enums
    enum SocialMediaType
    {
        Instagram,
        Facebook,
        Youtube
    }

    void Start()
    {
        // Cache
        menuUI = GetComponent<MenuUI>();
        localeMenu = GetComponent<LocaleMenu>();
        confirmMenu = GetComponent<ConfirmMenu>();
        LOCALE = I18n.Instance;
        settings = Settings.Instance;
        notifics = Services.Instance.GetComponent<Notifics>();
        resetHandler = GetComponent<ResetHandler>();

        // Cache UI
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

        googleSignInButton = settingsMenu.Q<Button>("GoogleSignInButton");
        facebookSignInButton = settingsMenu.Q<Button>("FacebookSignInButton");
        appleSignInButton = settingsMenu.Q<Button>("AppleSignInButton");

        googleSignInCheck = googleSignInButton.Q<VisualElement>("SignedInCheck");
        facebookSignInCheck = facebookSignInButton.Q<VisualElement>("SignedInCheck");
        appleSignInCheck = appleSignInButton.Q<VisualElement>("SignedInCheck");

        instagramFollowButton = settingsMenu.Q<Button>("InstagramFollowButton");
        facebookFollowButton = settingsMenu.Q<Button>("FacebookFollowButton");
        //youtubeFollowButton = settingsMenu.Q<Button>("YoutubeFollowButton");

        signInLabel = settingsMenu.Q<Label>("SignInLabel");
        followLabel = settingsMenu.Q<Label>("FollowLabel");
        versionLabel = settingsMenu.Q<Label>("VersionLabel");

        idLabel = settingsMenu.Q<Label>("IDLabel");
        idCopyButton = settingsMenu.Q<Button>("IDCopyButton");
        copyCheck = settingsMenu.Q<VisualElement>("CopyCheck");

        // Button clicks // TODO - Add all button clicks to ////
        soundButton.clicked += () => settings.ToggleSound();
        musicButton.clicked += () => settings.ToggleMusic();
        vibrationButton.clicked += () => settings.ToggleVibration();
        notificationsButton.clicked += () => settings.ToggleNotifications();

        supportButton.clicked += () => Application.OpenURL(GameData.WEB_ADDRESS + "/support");
        privacyButton.clicked += () => Application.OpenURL(GameData.WEB_ADDRESS + "/privacy");
        termsButton.clicked += () => Application.OpenURL(GameData.WEB_ADDRESS + "/terms");
        languageButton.clicked += () => localeMenu.Open();
        resetButton.clicked += () => confirmMenu.Open("reset", resetHandler.RestartAndResetApp);
        exitButton.clicked += () => confirmMenu.Open("exit", Application.Quit);

        googleSignInButton.clicked += () => Debug.Log("Google Sing In Button Clicked!"); ////
        facebookSignInButton.clicked += () => Debug.Log("Facebook Sing In Button Clicked!"); ////
        appleSignInButton.clicked += () => Debug.Log("Apple Sing In Button Clicked!"); ////

        instagramFollowButton.clicked += () => OpenSocialMediaLink(SocialMediaType.Instagram);
        facebookFollowButton.clicked += () => OpenSocialMediaLink(SocialMediaType.Facebook);
       // youtubeFollowButton.clicked += () => OpenSocialMediaLink(SocialMediaType.Youtube);

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
    }

    public void Open()
    {
        // Set the title
        string title = LOCALE.Get("settings_menu_title");

        // TODO - Get the user ID
        idLabel.text = "User ID: " + GameData.Instance.userId;

        versionLabel.text = "v." + Application.version;

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
        if (settings.googleSignedIn)
        {
            googleSignInCheck.style.display = DisplayStyle.Flex;
        }

        // Facebook sign in
        facebookSignInCheck.style.display = DisplayStyle.None;
        if (settings.facebookSignedIn)
        {
            facebookSignInCheck.style.display = DisplayStyle.Flex;
        }

        // Facebook sign in
        appleSignInCheck.style.display = DisplayStyle.None;
        if (settings.appleSignedIn)
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

    void OpenSocialMediaLink(SocialMediaType type)
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
                    Application.OpenURL("https://instagram.com/" + "nasa");

                    break;
                case SocialMediaType.Facebook:
                    Application.OpenURL("https://facebook.com/" + "nasa");

                    break;
                case SocialMediaType.Youtube:
                    Application.OpenURL("https://youtube.com/@" + "nasa");

                    break;
            }
        }
    }
}
}