using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class NoteMenu : MonoBehaviour
    {
        // Variables
        public Action callback;

        private bool menuOpen = false;

        private Types.Menu menuType = Types.Menu.Note;

        // References
        private MenuUI menuUI;
        private I18n LOCALE;
        private UIData uiData;

        // UI
        private VisualElement root;
        private VisualElement content;

        void Start()
        {
            // Cache
            menuUI = GetComponent<MenuUI>();
            LOCALE = I18n.Instance;
            uiData = GameData.Instance.GetComponent<UIData>();

            DataManager.Instance.CheckLoaded(() =>
            {
                // UI
                root = GetComponent<UIDocument>().rootVisualElement;

                root.RegisterCallback<GeometryChangedEvent>(HandleCallback);

                content = uiData.GetMenuAsset(menuType);
            });
        }

        public void Open(string newTitle, List<string> notes, Action newCallback = null)
        {
            // Check menu
            if (menuUI.IsMenuOpen(menuType))
            {
                return;
            }

            // Set menu content
            callback = newCallback;

            content.Clear();

            // Create the notes
            for (int i = 0; i < notes.Count; i++)
            {
                Label newLabel = new() { name = "NoteLabel" + i };

                newLabel.style.width = Length.Percent(100);
                newLabel.style.fontSize = 8f;
                newLabel.style.unityTextAlign = TextAnchor.UpperCenter;
                newLabel.style.whiteSpace = WhiteSpace.Normal;

                newLabel.style.marginLeft = Length.Auto();
                newLabel.style.marginRight = Length.Auto();
                newLabel.style.marginTop = 0;
                newLabel.style.marginBottom = 5f;

                newLabel.style.paddingLeft = 0;
                newLabel.style.paddingRight = 0;
                newLabel.style.paddingTop = 0;
                newLabel.style.paddingBottom = 0;

                newLabel.text = LOCALE.Get(notes[i]);

                content.Add(newLabel);
            }

            // Open menu
            menuUI.OpenMenu(content, menuType, LOCALE.Get(newTitle));

            menuOpen = true;
        }

        void HandleCallback(GeometryChangedEvent evt)
        {
            root.UnregisterCallback<GeometryChangedEvent>(HandleCallback);

            VisualElement noteMenu = uiData.GetMenuElement(menuType);

            if (menuOpen && callback != null && noteMenu != null && noteMenu.resolvedStyle.display == DisplayStyle.None)
            {
                menuOpen = false;

                callback();
            }
        }
    }
}