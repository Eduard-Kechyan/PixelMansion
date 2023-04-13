using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class TaskMenu : MonoBehaviour
{
    private MenuManager menuManager;

    private VisualElement root;
    private VisualElement taskMenu;

    private I18n LOCALE = I18n.Instance;
    
    void Start()
    {
        // Cache
        menuManager = GetComponent<MenuManager>();

        // Cache UI
        root = menuManager.menuUI.rootVisualElement;

        taskMenu = root.Q<VisualElement>("TaskMenu");
        
    }

    public void Open()
    {
        // Set the title
        string title = LOCALE.Get("task_menu_title");

        // Open menu
        menuManager.OpenMenu(taskMenu, title);
    }
}
