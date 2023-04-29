using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class SettingsMenu : MonoBehaviour
{
    // Variables
    public Settings settings;
    public Color onColor;
    public Color offColor;

    // References
    private MenuUI menuUI;
    private LocaleMenu localeMenu;
    private I18n LOCALE;

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
    private Button youtubeFollowButton;
    private Button tikTokFollowButton;

    private Label signInLabel;
    private Label followLabel;
    private Label idLabel;
    private Label versionLabel;

    void Start()
    {
        // Cache
        menuUI = GetComponent<MenuUI>();
        localeMenu = GetComponent<LocaleMenu>();
        LOCALE = I18n.Instance;

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
        youtubeFollowButton = settingsMenu.Q<Button>("YoutubeFollowButton");
        tikTokFollowButton = settingsMenu.Q<Button>("TikTokFollowButton");

        signInLabel = settingsMenu.Q<Label>("SignInLabel");
        followLabel = settingsMenu.Q<Label>("FollowLabel");
        idLabel = settingsMenu.Q<Label>("IDLabel");
        versionLabel = settingsMenu.Q<Label>("VersionLabel");

        soundButton.clicked += () => settings.ToggleSound();
        musicButton.clicked += () => settings.ToggleMusic();
        vibrationButton.clicked += () => settings.ToggleVibration();
        notificationsButton.clicked += () => settings.ToggleNotifications();

        // TODO - Add all button clicks to ////

        supportButton.clicked += () => Debug.Log("Support Button Clicked!"); ////
        privacyButton.clicked += () => Debug.Log("Privacy Button Clicked!"); ////
        termsButton.clicked += () => Debug.Log("Terms Button Clicked!"); ////
        languageButton.clicked += () => localeMenu.Open();
        resetButton.clicked += () => Debug.Log("Reset Button Clicked!"); ////
        exitButton.clicked += () => Debug.Log("Exit Button Clicked!"); ////

        googleSignInButton.clicked += () => Debug.Log("Google Sing In Button Clicked!"); ////
        facebookSignInButton.clicked += () => Debug.Log("Facebook Sing In Button Clicked!"); ////
        appleSignInButton.clicked += () => Debug.Log("Apple Sing In Button Clicked!"); ////

        instagramFollowButton.clicked += () => Debug.Log("Instagram Follow Button Clicked!"); ////
        facebookFollowButton.clicked += () => Debug.Log("Facebook Follow Button Clicked!"); ////
        youtubeFollowButton.clicked += () => Debug.Log("Youtube Follow Button Clicked!"); ////
        tikTokFollowButton.clicked += () => Debug.Log("TikTok Follow Button Clicked!"); ////

        Init();
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
        idLabel.text = "UUID: " + "00010001000";

        versionLabel.text = "v." + Application.version;

#if UNITY_ANDROID
        appleSignInButton.style.display = DisplayStyle.None;
#endif

#if UNITY_IOS
        googleSignInButton.style.display = DisplayStyle.None;
#endif

        SetUiText();

        SetUIOptionsButtons();

        SetUISignInButtons();

        // Open menu
        menuUI.OpenMenu(settingsMenu, title);
    }

    void SetUiText(bool update = false)
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

        if (update)
        {
            // Title
            string title = LOCALE.Get("settings_menu_title");

            menuUI.UpdateTitle(title);
        }
    }

    public void SetUIOptionsButtons(){
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
        settings.SetLocale(newLocale);

        SetUiText(true);
    }
}
