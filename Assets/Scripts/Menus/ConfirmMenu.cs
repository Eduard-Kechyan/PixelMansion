using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class ConfirmMenu : MonoBehaviour
    {
        // Variables
        private bool denyTapped = false;
        private Action callback;
        private Action callbackAlt;

        private MenuUI.Menu menuType = MenuUI.Menu.Confirm;

        // References
        private MenuUI menuUI;
        private I18n LOCALE;
        private UIData uiData;

        // UI
        private VisualElement content;

        private Label confirmLabel;
        private Button confirmButton;
        private Button denyButton;

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

                confirmLabel = content.Q<Label>("Label");
                confirmButton = content.Q<Button>("ConfirmButton");
                denyButton = content.Q<Button>("DenyButton");

                confirmButton.clicked += () => ConfirmButtonClicked();
                denyButton.clicked += () => DenyButtonClicked();
            });
        }

        public void Open(string preFix, Action newCallback, Action newCallbackAlt = null, bool alt = false, bool closeAll = false)
        {
            // Check menu
            if (menuUI.IsMenuOpen(menuType))
            {
                return;
            }

            callback = newCallback;

            if (!denyTapped)
            {
                if (newCallbackAlt != null)
                {
                    callbackAlt = newCallbackAlt;
                }

                denyTapped = true;

                Glob.SetTimeout(() =>
                {
                    denyTapped = false;
                }, 0.35f);
            }

            // Set the title
            string title = LOCALE.Get("Confirm_title_" + preFix);

            confirmLabel.text = LOCALE.Get("confirm_label_" + preFix);

            if (alt)
            {
                confirmButton.text = LOCALE.Get("confirm_button_" + preFix);
                denyButton.text = LOCALE.Get("confirm_deny_button_" + preFix);
            }
            else
            {
                confirmButton.text = LOCALE.Get("confirm_button");
                denyButton.text = LOCALE.Get("confirm_deny_button");
            }

            // Open menu
            menuUI.OpenMenu(content, menuType, title, closeAll);
        }

        void ConfirmButtonClicked()
        {
            callback?.Invoke();
        }

        void DenyButtonClicked()
        {
            if (callbackAlt != null)
            {
                callbackAlt();
            }
            else
            {
                Close();
            }
        }

        public void Close()
        {
            menuUI.CloseMenu(menuType);
        }
    }
}