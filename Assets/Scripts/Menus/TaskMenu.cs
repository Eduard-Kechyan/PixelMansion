using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace Merge
{
    public class TaskMenu : MonoBehaviour
{
    // References
    private MenuUI menuUI;

    // Instances
    private I18n LOCALE;

    // UI
    private VisualElement root;
    private VisualElement taskMenu;

    void Start()
    {
        // Cache
        menuUI = GetComponent<MenuUI>();

        // Cache instances
        LOCALE = I18n.Instance;

        // Cache UI
        root = GetComponent<UIDocument>().rootVisualElement;

        taskMenu = root.Q<VisualElement>("TaskMenu");

        Init();
    }

    void Init()
    {
        // Make sure the menu is closed
        taskMenu.style.display = DisplayStyle.None;
        taskMenu.style.opacity = 0;
    }

    public void Open()
    {
        // Set the title
        string title = LOCALE.Get("task_menu_title");

        // Open menu
        menuUI.OpenMenu(taskMenu, title);
    }
}
}