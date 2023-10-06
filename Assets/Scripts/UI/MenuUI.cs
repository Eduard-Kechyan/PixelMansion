using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class MenuUI : MonoBehaviour
    {
        // Variables
        public BoardInteractions boardInteractions;
        public float transitionDuration = 0.1f;
        public float menuDecreaseOffset = 0.8f;
        public bool menuOpen = false;

        private List<MenuItem> menus = new();
        private bool valuesShown;
        private bool closeAllMenus = false;
        private bool closingAllMenus = false;

        private class MenuItem
        {
            public VisualElement menuItem;
            public bool showValues;
        }

        // References
        private ValuesUI valuesUI;

        // UI
        private VisualElement root;
        private VisualElement menuLocaleWrapper;
        private VisualElement background;
        private VisualElement currentMenu;
        private Label title;

        void Start()
        {
            // References
            valuesUI = GameRefs.Instance.valuesUI;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;
            menuLocaleWrapper = root.Q<VisualElement>("LocaleWrapper");

            // Init
            menuLocaleWrapper.style.display = DisplayStyle.None;
        }

        // Update the currently opened menu's title
        public void UpdateTitle(string newTitle)
        {
            if (currentMenu.name == "LocaleMenu")
            {
                menus[0].menuItem.Q<VisualElement>("Title").Q<Label>("Value").text = newTitle;
            }
        }

        // Get ready to open the given menu
        public void OpenMenu(VisualElement newMenu, string newTitle, bool showValues = false, bool closeAll = false)
        {
            // Add the menu to the menu list
            menus.Add(new MenuItem { menuItem = newMenu, showValues = showValues });

            // Set the current menu
            currentMenu = newMenu;

            closeAllMenus = closeAll;

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
            VisualElement newMenu = new();

            // Show the menu locale container
            menuLocaleWrapper.style.display = DisplayStyle.Flex;

            // Show the menu
            currentMenu.style.display = DisplayStyle.Flex;
            currentMenu.style.opacity = 1f;

            // Add background click handler
            background = currentMenu.Q<VisualElement>("Background");

            background.AddManipulator(new Clickable(evt =>
            {
                if (closeAllMenus)
                {
                    CloseAllMenus();
                }
                else
                {
                    CloseMenu(currentMenu.name);
                }
            }));

            // Disable the close button
            currentMenu.Q<VisualElement>("Close").pickingMode = PickingMode.Ignore;

            // Set the menu's title
            title = currentMenu.Q<VisualElement>("Title").Q<Label>("Value");

            title.text = newTitle;

            // Set open menu indicator to open
            menuOpen = true;

            // Disable the board
            if (boardInteractions != null)
            {
                boardInteractions.DisableInteractions();
            }
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

        IEnumerator HideMenuAfter(MenuItem menuItem = null, int index = 0)
        {
            yield return new WaitForSeconds(transitionDuration);

            // Hide and remove the current menu
            if (menuItem == null)
            {
                currentMenu.style.display = DisplayStyle.None;

                currentMenu = null;
            }
            else
            {
                menuItem.menuItem.style.display = DisplayStyle.None;

                menus.RemoveAt(index);
            }

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

                // Hide the menu locale container
                menuLocaleWrapper.style.display = DisplayStyle.None;

                // Enable the board
                if (boardInteractions != null)
                {
                    boardInteractions.EnableInteractions();
                }

                if (valuesShown)
                {
                    HideValues();
                }
            }
        }

        public void CloseAllMenus()
        {
            if (!closingAllMenus)
            {
                closingAllMenus = true;

                StartCoroutine(CloseAllAfter());
            }
        }

        IEnumerator CloseAllAfter()
        {
            for (int i = menus.Count - 1; i > -1; i--) // NOTE - We are counting backwards
            {
                menus[i].menuItem.SetEnabled(false);
                menus[i].menuItem.style.opacity = 0f;
                StartCoroutine(HideMenuAfter(menus[i], i));
                yield return new WaitForSeconds(transitionDuration * 2);
            }

            closeAllMenus = false;
            closingAllMenus = false;

            CheckMenuClosed();

            yield return null;
        }

        void ShowValues()
        {
            // Show the values over the menu and disable the buttons
            valuesShown = true;

            valuesUI.SetSortingOrder(12);

            valuesUI.DisableButtons();
        }

        void HideValues()
        {
            // Reset values order in hierarchy and enable the buttons
            valuesShown = false;

            valuesUI.SetSortingOrder(10);

            valuesUI.EnableButtons();
        }
    }
}