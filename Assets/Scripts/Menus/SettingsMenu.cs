using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class SettingsMenu : MonoBehaviour
{
    // References
    private MenuUI menuUI;

    // Instances
    private I18n LOCALE;

    // UI
    private VisualElement root;
    private VisualElement settingsMenu;

    private Slider soundSlider;
    private Slider musicSlider;
    private EnumField languageEnum;
    private Toggle saveToggle;

    void Start()
    {
        // Cache
        menuUI = GetComponent<MenuUI>();

        // Cache instances
        LOCALE = I18n.Instance;

        // Cache UI
        root = GetComponent<UIDocument>().rootVisualElement;

        settingsMenu = root.Q<VisualElement>("SettingsMenu");

        soundSlider=settingsMenu.Q<Slider>("SoundSlider");
        musicSlider=settingsMenu.Q<Slider>("MusicSlider");
        languageEnum=settingsMenu.Q<EnumField>("LanguageEnum");
        saveToggle=settingsMenu.Q<Toggle>("SaveToggle");
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

        soundSlider.label = LOCALE.Get("settings_menu_sound_label");
        musicSlider.label = LOCALE.Get("settings_menu_music_label");
        languageEnum.label = LOCALE.Get("settings_menu_language_label");
        saveToggle.label = LOCALE.Get("settings_menu_save_label");

        // Open menu
        menuUI.OpenMenu(settingsMenu, title);
    }

    void SetLocale(string locale)
    {
        I18n.SetLocale(locale);
        PlayerPrefs.SetString("locale", locale);

        //I18n.SetLocale("hy-HY");
        // PlayerPrefs.SetString("locale", "hy-HY");
    }
}
