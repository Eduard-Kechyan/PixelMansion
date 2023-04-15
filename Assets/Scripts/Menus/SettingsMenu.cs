using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class SettingsMenu : MonoBehaviour
{
    private MenuManager menuManager;

    private VisualElement root;
    private VisualElement settingsMenu;

    private I18n LOCALE = I18n.Instance;
    
    void Start()
    {
        // Cache
        menuManager = GetComponent<MenuManager>();

        // Cache UI
        root = menuManager.menuUI.rootVisualElement;

        settingsMenu = root.Q<VisualElement>("SettingsMenu");
        
    }

    public void Open()
    {
        // Set the title
        string title = LOCALE.Get("settings_menu_title");

        // Open menu
        menuManager.OpenMenu(settingsMenu, title);
    }
    
    void LocaleTest()
    {
        if (PlayerPrefs.GetString("locale") == "en-US")
        {
            I18n.SetLocale("hy-HY");
            PlayerPrefs.SetString("locale", "hy-HY");
        }
        else
        {
            I18n.SetLocale("en-US");
            PlayerPrefs.SetString("locale", "en-US");
        }
    }
}
