using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class NoteMenu : MonoBehaviour
{
    // References
    private MenuUI menuUI;

// UI
    private VisualElement root;
    private VisualElement noteMenu;
    private VisualElement menuContent;

    private I18n LOCALE = I18n.Instance;

    void Start()
    {
        // Cache
        menuUI = GetComponent<MenuUI>();

        // Cache UI
        root = GetComponent<UIDocument>().rootVisualElement;

        noteMenu = root.Q<VisualElement>("NoteMenu");
        menuContent = noteMenu.Q<VisualElement>("Content");
    }

    public void Open(string newTitle, string[] notes)
    {
        // Clear children if there are any
        if (menuContent.childCount > 0)
        {
            menuContent.Clear();
        }

        // Set the title
        string title = LOCALE.Get(newTitle);

        // Create the notes
        for (int i = 0; i < notes.Length; i++)
        {
            Label newLabel = new Label { name = "NoteLabel" + i };

            newLabel.style.width = Length.Percent(80);
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
    }
}
