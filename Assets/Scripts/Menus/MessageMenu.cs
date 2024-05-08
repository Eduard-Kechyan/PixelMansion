using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class MessageMenu : MonoBehaviour
    {
        // Variables
        private Action messageCallback;

        private string newTitle = "";

        private MenuUI.Menu menuType = MenuUI.Menu.Message;

        // Enums
        public enum MessageType
        {
            None,
            Custom,
            Terms,
            Privacy,
            Note,
            DigitalServiceAct
        }

        // References
        private MenuUI menuUI;
        private I18n LOCALE;
        private UIData uiData;
        private HtmlHandler htmlHandler;

        // UI
        private VisualElement content;
        private ScrollView messageScrollView;
        private Button acceptButton;
        private ErrorManager errorManager;

        void Start()
        {
            // Cache
            menuUI = GetComponent<MenuUI>();
            LOCALE = I18n.Instance;
            uiData = GameData.Instance.GetComponent<UIData>();
            errorManager = ErrorManager.Instance;
            htmlHandler = Glob.Instance.GetComponent<HtmlHandler>();

            DataManager.Instance.CheckLoaded(() =>
            {
                // UI
                content = uiData.GetMenuAsset(menuType);
                messageScrollView = content.Q<ScrollView>("MessageScrollView");
                acceptButton = content.Q<Button>("AcceptButton");

                // UI taps
                acceptButton.clicked += () => SoundManager.Tap(AcceptMessage);
            });
        }

        public void Open(MessageType messageType, string title = "", string message = "", string buttonPrefix = "", bool isHtml = false, Action callback = null)
        {
            // Check menu
            if (menuUI.IsMenuOpen(menuType) || messageType == MessageType.None)
            {
                return;
            }

            // Set menu content
            messageCallback = callback;

            messageScrollView.Clear();

            messageScrollView.scrollOffset = Vector2.zero;

            if (messageType == MessageType.Custom)
            {
                newTitle = title;

                NewMessageLabel(message);
            }
            else if (messageType == MessageType.DigitalServiceAct)
            {
                newTitle = LOCALE.Get("Message_menu_title_" + messageType);

                NewMessageLabel(message);
            }
            else
            {
                newTitle = LOCALE.Get("Message_menu_title_" + messageType);

                if (isHtml)
                {
                    GetDataOnline(message);
                }
            }

            if (buttonPrefix != "")
            {
                acceptButton.text = LOCALE.Get("message_type_" + buttonPrefix);
            }
            else
            {
                acceptButton.text = LOCALE.Get("message_type_" + messageType);
            }

            // Open menu
            menuUI.OpenMenu(content, menuType, newTitle);
        }

        void NewMessageLabel(string message)
        {
            Label newMessageLabel = new() { name = "MessageLabel0", text = message };

            newMessageLabel.AddToClassList("message_label");

            messageScrollView.Add(newMessageLabel);
        }

        void GetDataOnline(string html)
        {
            List<Label> newMessageElements = htmlHandler.ConvertHtmlToUI(html);

            if (newMessageElements != null && newMessageElements.Count > 0)
            {
                for (int i = 0; i < newMessageElements.Count; i++)
                {
                    messageScrollView.Add(newMessageElements[i]);
                }
            }
            else
            {
                NewMessageLabel(LOCALE.Get("message_error"));

                // ERROR
                errorManager.Throw(ErrorManager.ErrorType.Code, GetType().Name, "newMessageLabels count is 0!");
            }
        }

        void AcceptMessage()
        {
            menuUI.CloseMenu(menuType, () =>
            {
                Glob.SetTimeout(() =>
                {
                    messageCallback?.Invoke();

                    messageCallback = null;
                }, 0.3f);
            });
        }
    }
}