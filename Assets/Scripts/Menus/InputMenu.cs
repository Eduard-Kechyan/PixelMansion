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

        // References
        private MenuUI menuUI;
        private I18n LOCALE;

        // UI
        private VisualElement root;
        private VisualElement inputMenu;
        private Label inputLabel;
        private TextField inputTextField;
        private Button inputButton;

        void Start()
        {
            // Cache
            menuUI = GetComponent<MenuUI>();
            LOCALE = I18n.Instance;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            inputMenu = root.Q<VisualElement>("InputMenu");
            inputLabel = inputMenu.Q<Label>("InputLabel");
            inputTextField = inputMenu.Q<TextField>("InputTextField");
            inputButton = inputMenu.Q<Button>("InputButton");

            inputTextField.RegisterValueChangedCallback(evt => HandleInput(evt));

            inputButton.clicked += () => Accept();

            inputButton.SetEnabled(false);

            Init();
        }

        void Init()
        {
            // Make sure the menu is closed
            inputMenu.style.display = DisplayStyle.None;
            inputMenu.style.opacity = 0;
        }

        public void Open(string inputId, Action<string> newCallback)
        {
            callback = newCallback;

            string title = "";

            if (inputId == "PlayerName")
            {
                // Reset text field value
                inputTextField.value = "";

                // Set the title
                title = LOCALE.Get("input_menu_title_player_name");

                // Set the input label
                inputLabel.text = LOCALE.Get("input_menu_label_player_name");
            }

            // Open menu
            menuUI.OpenMenu(inputMenu, title);
        }

        void HandleInput(ChangeEvent<string> newValue)
        {
            if (newValue.newValue.Length > 12)
            {
                inputTextField.value = inputText;
            }
            else
            {
                inputText = newValue.newValue;
            }

            if (inputText == "")
            {
                inputButton.SetEnabled(false);
            }
            else if (inputText.Length < 3)
            {
                // TODO - Notify the player of the length limits
                inputButton.SetEnabled(false);
            }
            else
            {
                inputButton.SetEnabled(true);
            }
        }

        void Accept()
        {
            menuUI.CloseMenu(inputMenu.name, () =>
            {
                callback(inputText);
            });
        }
    }
}
