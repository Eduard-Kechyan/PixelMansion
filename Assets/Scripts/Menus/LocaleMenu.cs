using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class LocaleMenu : MonoBehaviour
{
    // Variables
    public Settings settings;
    public Sprite localeButtonSprite;
    public Color localeButtonColor;
    public Color localeButtonColorSelected;

    // References
    private MenuUI menuUI;
    private SettingsMenu settingsMenu;
    private I18n LOCALE;

    // UI
    private VisualElement root;
    private VisualElement localeMenu;
    private VisualElement content;

    void Start()
    {
        // Cache
        menuUI = GetComponent<MenuUI>();
        settingsMenu = GetComponent<SettingsMenu>();
        LOCALE = I18n.Instance;

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

        Debug.Log(I18n.GetLocale());

        Types.Locale currentLocale = LOCALE.ConvertToLocale(I18n.GetLocale());

        foreach (Types.Locale locale in System.Enum.GetValues(typeof(Types.Locale)))
        {
            string localeString = locale.ToString();

            Button localeButton = new Button { name = "LocaleButton" + localeString, text = localeString };

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

            localeButton.clicked += () => SetLocale(localeString);

            // TODO - Remove this line after adding all the necesary lanugages
            if (locale != Types.Locale.English && locale != Types.Locale.Հայերեն)
            {
                localeButton.SetEnabled(false);
            }

            content.Add(localeButton);
        }

        // Open menu
        menuUI.OpenMenu(localeMenu, title);
    }

    void SetLocale(string newLocale)
    {
        settingsMenu.SetLocale((Types.Locale)System.Enum.Parse(typeof(Types.Locale), newLocale));

        menuUI.CloseMenu(localeMenu.name);
    }
}
