using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    public UIDocument menuUI;
    public UIDocument valuesUI;
    public BoardInteractions boardInteractions;
    public float transitionDuration = 0.5f;
    public float menuDecreaseOffset = 0.8f;
    public bool menuOpen;

    private class MenuItem
    {
        public VisualElement menuItem;
        public bool showValues;
    }

    public static MenuManager Instance;

    private Values values;

    private VisualElement background;
    private Label title;

    private List<MenuItem> menus = new List<MenuItem>();
    private VisualElement currentMenu;

    private bool valuesShown;

    // Set Singelton
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        // Cache
        values = DataManager.Instance.GetComponent<Values>();
    }

    public void OpenMenu(
        VisualElement newMenu,
        string newTitle,
        bool showValues = false
    )
    {
        // Add the menu to the menu list
        menus.Add(new MenuItem { menuItem = newMenu, showValues = showValues });

        // Set the current menu
        currentMenu = newMenu;

        currentMenu.SetEnabled(true);

        if (showValues)
        {
            ShowValues();
        }

        CheckMenuOpened();

        ShowMenu(newTitle);
    }

    void ShowMenu(string newTitle)
    {
        VisualElement newMenu = new VisualElement();

        // Show the menu
        currentMenu.style.display = DisplayStyle.Flex;
        currentMenu.style.opacity = 1f;

        // Add background click handler
        background = currentMenu.Q<VisualElement>("Background");

        background.AddManipulator(new Clickable(evt => CloseMenu(currentMenu.name)));

        // Disable the close button
        currentMenu.Q<VisualElement>("Close").pickingMode = PickingMode.Ignore;

        // Set the menu's itle
        title = currentMenu.Q<VisualElement>("Title").Q<Label>("Value");

        title.text = newTitle;

        // Set open menu indicator to open
        menuOpen = true;

        // Disable the board
        boardInteractions.DisableInteractions();
    }

    public void CloseMenu(string menuName)
    {
        // Disable the menu to make it unclickable
        currentMenu.SetEnabled(false);

        // Hide the menu
        currentMenu.style.opacity = 0f;

        // Remove the menu from the menu list
        int currentMenuIndex = 0;

        for (int i = 0; i < menus.Count; i++)
        {
            if (menus[i].menuItem.name == menuName)
            {
                currentMenuIndex = i;
            }
        }

        menus.RemoveAt(currentMenuIndex);

        StartCoroutine(HideMenuAfter());
    }

    IEnumerator HideMenuAfter()
    {
        yield return new WaitForSeconds(transitionDuration);

        // Hide and remove the current menu
        currentMenu.style.display = DisplayStyle.None;

        currentMenu = null;

        CheckMenuClosed();
    }

    void CheckMenuOpened()
    {
        // Check if there are more than 1 menu's open
        if (menus.Count > 1)
        {
            // Decrease the menu's size
            Scale scale = new Scale(new Vector2(menuDecreaseOffset, menuDecreaseOffset));

            menus[menus.Count - 2].menuItem.style.scale = new StyleScale(scale);

            // Hide close button
            menus[menus.Count - 2].menuItem.Q<VisualElement>("Close").style.opacity = 0f;
        }
    }

    void CheckMenuClosed()
    {
        // Check if there are any menu's open
        if (menus.Count > 0)
        {
            // Set the current menu
            currentMenu = menus[menus.Count - 1].menuItem;

            // Reset the menu's size
            Scale scale = new Scale(new Vector2(1f, 1f));

            currentMenu.style.scale = new StyleScale(scale);

            // Show close button
            currentMenu.Q<VisualElement>("Close").style.opacity = 1f;

            if (menus[menus.Count - 1].showValues)
            {
                ShowValues();
            }
        }
        else
        {
            // Set open menu indicator to close
            menuOpen = false;

            // Enable the board
            boardInteractions.EnableInteractions();

            if (valuesShown)
            {
                HideValues();
            }
        }
    }

    void ShowValues()
    {
        // Show the values over the menu and disable the buttons
        valuesShown = true;

        valuesUI.sortingOrder = 12;

        values.DisableButtons();
    }

    void HideValues()
    {
        // Reset values order in hierarchy and enable the buttons
        valuesShown = false;

        valuesUI.sortingOrder = 10;

        values.EnableButtons();
    }
}
