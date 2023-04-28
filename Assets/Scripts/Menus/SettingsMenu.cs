using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class SettingsMenu : MonoBehaviour
{
    // Variables
    public Settings settings;

    // References
    private MenuUI menuUI;
    private LocaleMenu localeMenu;
    private I18n LOCALE;

    // UI
    private VisualElement root;
    private VisualElement settingsMenu;

    private Slider soundSlider;
    private VisualElement soundDragger;
    private Slider musicSlider;
    private VisualElement musicDragger;
    private VisualElement languageContainer;
    private Label languageLabel;
    private Button languageButton;
    private Toggle saveToggle;

    void Start()
    {
        // Cache
        menuUI = GetComponent<MenuUI>();
        localeMenu = GetComponent<LocaleMenu>();
        LOCALE = I18n.Instance;

        // Cache UI
        root = GetComponent<UIDocument>().rootVisualElement;

        settingsMenu = root.Q<VisualElement>("SettingsMenu");

        soundSlider = settingsMenu.Q<Slider>("SoundSlider");
        soundDragger = soundSlider.Q<Slider>("unity-dragger");

        musicSlider = settingsMenu.Q<Slider>("MusicSlider");
        musicDragger = musicSlider.Q<Slider>("unity-dragger");

        languageContainer = settingsMenu.Q<VisualElement>("LanguageContainer");
        languageLabel = languageContainer.Q<Label>("Label");
        languageButton = languageContainer.Q<Button>("Button");

        saveToggle = settingsMenu.Q<Toggle>("SaveToggle");
        // Callbacks
        soundSlider.RegisterValueChangedCallback(SetSound);
        musicSlider.RegisterValueChangedCallback(SetMusic);
        languageButton.clicked += () => localeMenu.Open();
        saveToggle.RegisterValueChangedCallback(SetSave);

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

        SetUi();

        soundSlider.value = settings.GetSound();
        musicSlider.value = settings.GetMusic();
        languageButton.text = settings.GetLocale().ToString();
        saveToggle.value = settings.GetSave();

        // Open menu
        menuUI.OpenMenu(settingsMenu, title);
    }

    void SetUi(bool update = false, Types.Locale newLocale = Types.Locale.English)
    {
        soundSlider.label = LOCALE.Get("settings_menu_sound_label");
        musicSlider.label = LOCALE.Get("settings_menu_music_label");
        languageLabel.text = LOCALE.Get("settings_menu_language_label");
        saveToggle.label = LOCALE.Get("settings_menu_save_label");

        if (update)
        {
            string title = LOCALE.Get("settings_menu_title");

            languageButton.text = newLocale.ToString();

            menuUI.UpdateTitle(title);
        }
    }

    //// SET ////

    void SetSound(ChangeEvent<float> evt)
    {
        settings.SetSound(soundSlider.value);
    }

    void SetMusic(ChangeEvent<float> evt)
    {
        settings.SetMusic(musicSlider.value);
    }

    public void SetLocale(Types.Locale newLocale)
    {
        settings.SetLocale(newLocale);

        SetUi(true, newLocale);
    }

    void SetSave(ChangeEvent<bool> evt)
    {
        if (saveToggle.resolvedStyle.width != 0)
        {
            settings.SetSave(evt.newValue);
        }
    }
}
