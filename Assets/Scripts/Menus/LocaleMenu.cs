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
        public I18n.Locale[] localeToInclude;

        private MenuUI.Menu menuType = MenuUI.Menu.Locale;

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
            SetUI();

            // Open menu
            menuUI.OpenMenu(content, menuType);
        }

        void SetUI()
        {
            content.Clear();

            I18n.Locale currentLocale = LOCALE.GetLocale();

            foreach (I18n.Locale locale in System.Enum.GetValues(typeof(I18n.Locale)))
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

                /*switch (locale)
                {
                    case I18n.Locale.English:
                        localeButton.text = "English";
                        break;
                    case I18n.Locale.Armenian:
                        localeButton.text = "Հայերեն";
                        break;
                    case I18n.Locale.Japanese:
                        localeButton.text = "日本語";
                        break;
                    case I18n.Locale.Korean:
                        localeButton.text = "한국어";
                        break;
                    case I18n.Locale.Chinese:
                        localeButton.text = "中文";
                        break;
                    case I18n.Locale.Russian:
                        localeButton.text = "Русский";
                        break;
                    case I18n.Locale.French:
                        localeButton.text = "Français";
                        break;
                    case I18n.Locale.German:
                        localeButton.text = "Deutsch";
                        break;
                    case I18n.Locale.Spanish:
                        localeButton.text = "Español";
                        break;
                    case I18n.Locale.Italian:
                        localeButton.text = "Italiano";
                        break;
                }*/

                localeButton.text = ((I18n.LocaleAlt)(int)locale).ToString();

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

        }

        void SetLocale(I18n.Locale newLocale)
        {
            settingsMenu.SetLocale(newLocale);

            confirmMenu.Open("locale", Application.Quit, menuUI.CloseAllMenus, true, true);
        }
    }
}