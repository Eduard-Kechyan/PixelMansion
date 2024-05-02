using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class TermsMenu : MonoBehaviour
    {
        // Variables
        private Action termsCallback;

        [HideInInspector]
        public bool hasLoadingEvent = true;

        [HideInInspector]
        public MenuUI.Menu menuType = MenuUI.Menu.Terms;

        // References
        private MenuUI menuUI;
        private MenuUtilities menuUtilities;
        private I18n LOCALE;
        private DataManager dataManager;
        private GameData gameData;
        private Services services;
        private UIData uiData;

        // UI
        private VisualElement content;
        private Label termsLabel;
        private Button termsAcceptButton;
        private Button termsTermsButton;
        private Button termsPrivacyButton;

        void Start()
        {
            // Cache
            menuUI = GetComponent<MenuUI>();
            menuUtilities = GetComponent<MenuUtilities>();
            LOCALE = I18n.Instance;
            dataManager = DataManager.Instance;
            gameData = GameData.Instance;
            services = Services.Instance;
            uiData = gameData.GetComponent<UIData>();

            DataManager.Instance.CheckLoaded(() =>
            {
                // UI
                content = uiData.GetMenuAsset(menuType);

                termsLabel = content.Q<Label>("TermsLabel");
                termsAcceptButton = content.Q<Button>("AcceptButton");
                termsTermsButton = content.Q<Button>("TermsButton");
                termsPrivacyButton = content.Q<Button>("PrivacyButton");

                // UI taps
                termsAcceptButton.clicked += () => AcceptTerms();
                termsTermsButton.clicked += () =>
                {
                    menuUtilities.TryToGetOnlineData(MessageMenu.MessageType.Terms);
                };
                termsPrivacyButton.clicked += () =>
                {
                    menuUtilities.TryToGetOnlineData(MessageMenu.MessageType.Privacy);
                };

                Init();
            });
        }

        void Init()
        {
            termsLabel.text = LOCALE.Get("terms_menu_label");

            termsAcceptButton.text = LOCALE.Get("terms_accept");
            termsTermsButton.text = LOCALE.Get("terms_terms_button");
            termsPrivacyButton.text = LOCALE.Get("terms_privacy_button");

            menuUI.ThrowMenuLoadedEvent();
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
            termsCallback = callback;

            // Open menu
            menuUI.OpenMenu(content, menuType);
        }

        // Handle the player accepting the terms
        public void AcceptTerms(Action callback = null)
        {
            PlayerPrefs.SetInt("termsAccepted", 1);
            PlayerPrefs.Save();

            services.termsAccepted = true;

            dataManager.SaveValue("termsAccepted", true, false);

            if (callback != null)
            {
                callback?.Invoke();
            }
            else
            {
                menuUI.CloseMenu(menuType, () =>
                {
                    termsCallback?.Invoke();
                });
            }
        }
    }
}
