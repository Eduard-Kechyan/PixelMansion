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

        // References
        private MenuUI menuUI;
        private I18n LOCALE;

        // UI
        private VisualElement root;
        private VisualElement noteMenu;
        private VisualElement menuContent;

        void Start()
        {
            // Cache
            menuUI = GetComponent<MenuUI>();
            LOCALE = I18n.Instance;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            noteMenu = root.Q<VisualElement>("NoteMenu");
            menuContent = noteMenu.Q<VisualElement>("Content");

            root.RegisterCallback<GeometryChangedEvent>(HandleCallback);

            Init();
        }

        void Init()
        {
            // Make sure the menu is closed
            noteMenu.style.display = DisplayStyle.None;
            noteMenu.style.opacity = 0;
        }

        public void Open(string newTitle, List<string> notes, Action newCallback = null)
        {
            if (menuUI.IsMenuOpen(noteMenu.name))
            {
                return;
            }

            callback = newCallback;

            // Clear children if there are any
            if (menuContent.childCount > 0)
            {
                menuContent.Clear();
            }

            // Set the title
            string title = LOCALE.Get(newTitle);

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

                menuContent.Add(newLabel);
            }

            // Open menu
            menuUI.OpenMenu(noteMenu, title);

            menuOpen = true;
        }

        void HandleCallback(GeometryChangedEvent evt)
        {
            root.UnregisterCallback<GeometryChangedEvent>(HandleCallback);

            if (menuOpen && callback != null && noteMenu.resolvedStyle.display == DisplayStyle.None)
            {
                menuOpen = false;

                callback();
            }
        }
    }
}