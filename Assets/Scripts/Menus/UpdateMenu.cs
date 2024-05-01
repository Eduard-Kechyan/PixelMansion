using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class UpdateMenu : MonoBehaviour
    {
        // Variables
        private Action updateCallback;

        [HideInInspector]
        public bool hasLoadingEvent = true;

        [HideInInspector]
        public Types.Menu menuType = Types.Menu.Update;

        // References
        private MenuUI menuUI;
        private I18n LOCALE;
        private UIData uiData;

        // UI
        private VisualElement content;
        private Label updateLabel;
        private Button updateButton;
        private Button updateExitButton;

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

                updateLabel = content.Q<Label>("UpdateLabel");
                updateButton = content.Q<Button>("UpdateButton");
                updateExitButton = content.Q<Button>("ExitButton");

                // UI Taps
                updateButton.clicked += () => UpdateGame();
                updateExitButton.clicked += () => Application.Quit();

                menuUI.ThrowMenuLoadedEvent();
            });
        }

        void OnDestroy()
        {
            uiData.ClearMenuElement(menuType);
        }

        public void Open(Action callback = null)
        {
            // Check menu
            if (menuUI.IsMenuOpen(menuType))
            {
                return;
            }

            // Set menu content
            updateCallback = callback;

            // FIX - Check here if the game version matches the latest available version on the app store
            if (true)
            {
                updateCallback?.Invoke();
            }
            else
            {
                // Show the overlay
                //overlayBackground.style.display = DisplayStyle.Flex;
                // overlayBackground.style.opacity = 1;

                // Show the menu
                /* updateMenu.style.display = DisplayStyle.Flex;
                 updateMenu.style.opacity = 1f;
    
                 updateLabel.text = LOCALE.Get("menu_Update_label");
    
                 updateButton.text = LOCALE.Get("menu_Update_button");
                 updateExitButton.text = LOCALE.Get("menu_Update_exit_button");*/
            }

            // Open menu
            menuUI.OpenMenu(content, menuType);
        }

        void UpdateGame()
        {
            // FIX - Open the app store here, or update in game
            Debug.Log("Updating!");
        }

    }
}
