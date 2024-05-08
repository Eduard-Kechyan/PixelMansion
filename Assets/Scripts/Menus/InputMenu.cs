using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class InputMenu : MonoBehaviour
    {
        // Variables
        private Action<string> callback;
        private string inputText;

        private MenuUI.Menu menuType = MenuUI.Menu.Input;

        // References
        private MenuUI menuUI;
        private I18n LOCALE;
        private UIData uiData;

        // UI
        private VisualElement content;
        private Label inputLabel;
        private TextField inputTextField;
        private Button inputButton;
        private Label inputLimitLabel;
        private Button inputRandomButton;

        void Start()
        {
            // Cache
            menuUI = GetComponent<MenuUI>();
            LOCALE = I18n.Instance;
            uiData = GameData.Instance.GetComponent<UIData>();

            DataManager.Instance.CheckLoaded(() =>
            {
                // UI
                content = uiData.GetMenuAsset(menuType);

                inputLabel = content.Q<Label>("InputLabel");
                inputTextField = content.Q<TextField>("InputTextField");
                inputButton = content.Q<Button>("InputButton");
                inputLimitLabel = content.Q<Label>("InputLimitLabel");

                inputTextField.RegisterValueChangedCallback(evt => HandleInput(evt));

                inputButton.clicked += () => SoundManager.Tap(Accept);

                inputButton.SetEnabled(false);

                if (Debug.isDebugBuild && !PlayerPrefs.HasKey("tutorialFinished"))
                {
                    inputRandomButton = content.Q<Button>("RandomButton");

                    inputRandomButton.style.display = DisplayStyle.Flex;

                    inputRandomButton.clicked += () => SoundManager.Tap(AcceptRandomName);
                }
            });
        }

        public void Open(string inputId, Action<string> newCallback)
        {
            // Check menu
            if (menuUI.IsMenuOpen(menuType))
            {
                return;
            }

            // Set menu content
            callback = newCallback;

            if (inputId == "PlayerName")
            {
                // Reset text field value
                inputTextField.value = "";

                inputTextField.Focus();

                inputTextField.SelectAll();

                // Set the input label
                inputLabel.text = LOCALE.Get("input_menu_label_player_name");
            }

            // Open menu
            menuUI.OpenMenu(content, menuType, LOCALE.Get("input_menu_title_player_name"));
        }

        void HandleInput(ChangeEvent<string> changeEvent)
        {
            bool isMax = false;

            if (changeEvent.newValue.Length > 12)
            {
                // Note - use this  to disable typing in more characters
                // inputTextField.value = inputText;

                inputLimitLabel.text = LOCALE.Get("input_menu_error_char_max");
                inputLimitLabel.style.opacity = 1;

                isMax = true;
            }
            else
            {
                inputText = changeEvent.newValue;

                inputLimitLabel.style.opacity = 0;
            }

            if (inputText == "")
            {
                inputButton.SetEnabled(false);
            }
            else if (inputText.Length < 3)
            {
                inputButton.SetEnabled(false);

                inputLimitLabel.text = LOCALE.Get("input_menu_error_char_min");
                inputLimitLabel.style.opacity = 1;
            }
            else
            {
                inputButton.SetEnabled(true);

                if (!isMax)
                {
                    inputLimitLabel.style.opacity = 0;
                }
            }
        }

        void Accept()
        {
            menuUI.CloseMenu(menuType, () =>
            {
                callback(inputText);
            });
        }

        void AcceptRandomName()
        {
            string randomName = Glob.GetRandomWord(3, 12, true);

            menuUI.CloseMenu(menuType, () =>
            {
                callback(randomName);
            });
        }
    }
}
