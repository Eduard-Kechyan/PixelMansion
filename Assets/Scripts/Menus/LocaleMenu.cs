using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class LocaleMenu : MonoBehaviour
{
    // Variables
    public Sprite localeButtonSprite;
    public Color localeButtonColor;
    public Color localeButtonColorSelected;

    // References
    private MenuUI menuUI;
    private ConfirmMenu confirmMenu;
    private SettingsMenu settingsMenu;
    private I18n LOCALE;
    private Settings settings;
    private LocaleManager localeManager;

    // UI
    private VisualElement root;
    private VisualElement localeMenu;
    private VisualElement content;

    void Start()
    {
        // Cache
        menuUI = GetComponent<MenuUI>();
        settingsMenu = GetComponent<SettingsMenu>();
        confirmMenu = GetComponent<ConfirmMenu>();
        LOCALE = I18n.Instance;
        settings = Settings.Instance;
        localeManager = settings.GetComponent<LocaleManager>();

        // Cache UI
        root = GetComponent<UIDocument>().rootVisualElement;

        localeMenu = root.Q<VisualElement>("LocaleMenu");
        content = localeMenu.Q<VisualElement>("Content");

        Init();
    }

    void Init()
    {
        // Make sure the menu is closed
        localeMenu.style.display = DisplayStyle.None;
        localeMenu.style.opacity = 0;
    }

    public void Open()
    {
        // Set the title
        string title = LOCALE.Get("locale_menu_title");

        content.Clear();

        Types.Locale currentLocale = LOCALE.ConvertToLocale(I18n.GetLocale());

        foreach (Types.Locale locale in System.Enum.GetValues(typeof(Types.Locale)))
        {
            string localeString = locale.ToString();

            Button localeButton = new Button { name = "LocaleButton" + localeString };

            localeButton.style.backgroundImage = new StyleBackground(localeButtonSprite);

            if (locale == currentLocale)
            {
                localeButton.style.unityBackgroundImageTintColor = localeButtonColorSelected;
            }
            else
            {
                localeButton.style.unityBackgroundImageTintColor = localeButtonColor;
            }

            localeButton.AddToClassList("local_button");
            localeButton.AddToClassList("button_active");
            localeButton.RemoveFromClassList("local_button_disabled");


            localeButton.clicked += () => SetLocale(localeString);

            switch (locale)
            {
                case Types.Locale.Armenian:
                    localeButton.AddToClassList("locale_hy");
                    localeButton.text = "Հայերեն";
                    break;
                case Types.Locale.Japanese:
                    localeButton.AddToClassList("locale_jp");
                    localeButton.text = "日本語";
                    break;
                case Types.Locale.Korean:
                    localeButton.AddToClassList("locale_kr");
                    localeButton.text = "한국어";
                    break;
                case Types.Locale.Chinese:
                    localeButton.AddToClassList("locale_cn");
                    localeButton.text = "中文";
                    break;
                case Types.Locale.Russian:
                    localeButton.AddToClassList("locale_en");
                    localeButton.text = "Русский";
                    break;
                case Types.Locale.French:
                    localeButton.AddToClassList("locale_en");
                    localeButton.text = "Français";
                    break;
                case Types.Locale.German:
                    localeButton.AddToClassList("locale_en");
                    localeButton.text = "Deutsch";
                    break;
                case Types.Locale.Spanish:
                    localeButton.AddToClassList("locale_en");
                    localeButton.text = "Español";
                    break;
                case Types.Locale.Italian:
                    localeButton.AddToClassList("locale_en");
                    localeButton.text = "Italiano";
                    break;
                default: // English
                    localeButton.AddToClassList("locale_en");
                    localeButton.text = localeString;
                    break;
            }

            // TODO - Remove this line after adding all the necessary lanugages
            if (locale != Types.Locale.English && locale != Types.Locale.Armenian)
            {
                localeButton.SetEnabled(false);
            }

            // Indicating the currently selected locale
            if (locale == currentLocale)
            {
                localeButton.SetEnabled(false);
                localeButton.AddToClassList("local_button_selected");
            }

            content.Add(localeButton);
        }

        // Open menu
        menuUI.OpenMenu(localeMenu, title);
    }

    void SetLocale(string newLocale)
    {
        Types.Locale locale = (Types.Locale)System.Enum.Parse(typeof(Types.Locale), newLocale);

        settingsMenu.SetLocale(locale);

        confirmMenu.Open("locale", Application.Quit, menuUI.CloseAllMenus, true, true);
    }
}
