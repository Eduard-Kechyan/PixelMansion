using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MenuUtilities : MonoBehaviour
    {
        // Variables
        public TextAsset termsHtml;
        public TextAsset privacyHtml;

        // References
        private MenuUI menuUI;
        private MessageMenu messageMenu;
        private GameData gameData;

        void Start()
        {
            // Cache
            menuUI = GetComponent<MenuUI>();
            messageMenu = GetComponent<MessageMenu>();
            gameData = GameData.Instance;
        }

        public void TryToGetOnlineData(Types.MessageType messageType)
        {
            menuUI.ShowMenuOverlay(() =>
            {
                StartCoroutine(WaitForLegalData(() =>
                {
                    menuUI.HideMenuOverlay(() =>
                    {
                        string foundMessage = messageType == Types.MessageType.Terms ? gameData.termsHtml : gameData.privacyHtml;

                        if (foundMessage == "")
                        {
                            // Open in browser
                            // TODO - Use the next line 
                            // Application.OpenURL(GameData.WEB_ADDRESS + "/" + messageType.ToString().ToLower());
                            messageMenu.Open(messageType, "", messageType == Types.MessageType.Terms ? termsHtml.text : privacyHtml.text, "", true);
                        }
                        else
                        {
                            // Open in game
                            messageMenu.Open(messageType, "", foundMessage, "", true);
                        }
                    }, true);
                }));
            }, true);
        }

        IEnumerator WaitForLegalData(Action callback)
        {
            while (gameData.gettingLegalData)
            {
                yield return null;
            }

            callback();
        }
    }
}
