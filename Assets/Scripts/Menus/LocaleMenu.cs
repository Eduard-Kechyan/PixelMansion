using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class LocaleMenu : MonoBehaviour
    {
        // Variables
        public Sprite localeButtonSprite;
        public Color localeButtonColor;
        public Color localeButtonColorSelected;
        public Types.Locale[] localeToInclude;

        private Types.Menu menuType = Types.Menu.Locale;

        // References
        private MenuUI menuUI;
        private ConfirmMenu confirmMenu;
        private SettingsMenu settingsMenu;
        private I18n LOCALE;
        private UIData uiData;

        // UI
        private VisualElement content;

        void Start()
        {
            // Cache
            menuUI = GetComponent<MenuUI>();
            settingsMenu = GetComponent<SettingsMenu>();
            confirmMenu = GetComponent<ConfirmMenu>();
            LOCALE = I18n.Instance;
            uiData = GameData.Instance.GetComponent<UIData>();

            DataManager.Instance.CheckLoaded(() =>
            {
                // UI
                content = uiData.GetMenuAsset(menuType);
            });
        }

        public void Open()
        {
            // Check menu
            if (menuUI.IsMenuOpen(menuType))
            {
                return;
            }

            // Set menu content
            content.Clear();

            Types.Locale currentLocale = LOCALE.GetLocale();

            foreach (Types.Locale locale in System.Enum.GetValues(typeof(Types.Locale)))
            {
                Button localeButton = new() { name = "LocaleButton" + locale.ToString() };

                localeButton.style.backgroundImage = new StyleBackground(localeButtonSprite);

                if (locale == currentLocale)
                {
                    localeButton.style.unityBackgroundImageTintColor = localeButtonColorSelected;
                }
                else
                {
                    localeButton.style.unityBackgroundImageTintColor = localeButtonColor;
                }

                // Handle style classes
                localeButton.AddToClassList("local_button");
                localeButton.AddToClassList("button_active");
                localeButton.RemoveFromClassList("local_button_disabled");

                switch (locale)
                {
                    case Types.Locale.English:
                        localeButton.text = "English";
                        break;
                    case Types.Locale.Armenian:
                        localeButton.text = "Հայերեն";
                        break;
                    case Types.Locale.Japanese:
                        localeButton.text = "日本語";
                        break;
                    case Types.Locale.Korean:
                        localeButton.text = "한국어";
                        break;
                    case Types.Locale.Chinese:
                        localeButton.text = "中文";
                        break;
                    case Types.Locale.Russian:
                        localeButton.text = "Русский";
                        break;
                    case Types.Locale.French:
                        localeButton.text = "Français";
                        break;
                    case Types.Locale.German:
                        localeButton.text = "Deutsch";
                        break;
                    case Types.Locale.Spanish:
                        localeButton.text = "Español";
                        break;
                    case Types.Locale.Italian:
                        localeButton.text = "Italiano";
                        break;
                }

                // TODO - Remove this line after adding all the necessary languages
                bool include = false;

                foreach (var loc in localeToInclude)
                {
                    if (locale == loc)
                    {
                        include = true;

                        break;
                    }
                }

                if (!include)
                {
                    localeButton.SetEnabled(false);
                }

                // Indicating the currently selected locale
                if (locale == currentLocale)
                {
                    localeButton.SetEnabled(false);
                    localeButton.AddToClassList("local_button_selected");
                }

                // Tap
                localeButton.clicked += () => SetLocale(locale);

                content.Add(localeButton);
            }

            // Open menu
            menuUI.OpenMenu(content, menuType);
        }

        void SetLocale(Types.Locale newLocale)
        {
            settingsMenu.SetLocale(newLocale);

            confirmMenu.Open("locale", Application.Quit, menuUI.CloseAllMenus, true, true);
        }
    }
}