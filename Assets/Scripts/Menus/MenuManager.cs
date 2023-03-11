using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    public UIDocument uiDoc;
    public BoardInteractions boardInteractions;
    public float transitionDuration = 0.5f;
    public bool menuOpen;

    public static MenuManager Instance;

    private VisualElement root;

    private VisualElement menuBackground;
    private VisualElement menuContainer;
    private Label menuTitle;
    private Button closeButton;

    private VisualElement currentMenu;

    private string title;

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
        root = uiDoc.rootVisualElement;

        menuBackground = root.Q<VisualElement>("MenuBackground");
        menuContainer = root.Q<VisualElement>("MenuContainer");
        menuTitle = root.Q<VisualElement>("MenuTitle").Q<Label>("Value");
        closeButton = root.Q<Button>("MenuCloseButton");

        menuBackground.AddManipulator(new Clickable(evt => CloseMenu()));
        closeButton.clickable.clicked += () => CloseMenu();

        InitializeMenus();

        currentMenu = null;
    }

    void InitializeMenus()
    {
        menuBackground.style.display = DisplayStyle.None;
        menuBackground.style.opacity = 0f;
        menuContainer.style.display = DisplayStyle.None;
        menuContainer.style.opacity = 0f;
    }

    public void OpenMenu(VisualElement newMenu, string newTitle)
    {
        title = newTitle;

        currentMenu = newMenu;

        ShowMenu();
    }

    public void CloseMenu()
    {
        if (currentMenu != null)
        {
            HideMenu();
        }
    }

    void ShowMenu()
    {
        menuBackground.style.display = DisplayStyle.Flex;
        menuBackground.style.opacity = 1f;
        menuContainer.style.display = DisplayStyle.Flex;
        menuContainer.style.opacity = 1f;

        currentMenu.style.display = DisplayStyle.Flex;

        menuTitle.text = title;

        menuOpen = true;

        boardInteractions.DisableInteractions();
    }

    void HideMenu()
    {
        menuBackground.style.opacity = 0f;
        menuContainer.style.opacity = 0f;

        StartCoroutine(HideMenuAfter());
    }

    IEnumerator HideMenuAfter()
    {
        yield return new WaitForSeconds(transitionDuration);

        currentMenu.style.display = DisplayStyle.None;

        currentMenu = null;

        menuBackground.style.display = DisplayStyle.None;
        menuContainer.style.display = DisplayStyle.None;

        menuOpen = false;

        boardInteractions.EnableInteractions();
    }
}
