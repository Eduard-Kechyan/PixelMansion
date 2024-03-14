using System;
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

        [Header("Spinner")]
        public float spinnerSpeed = 0.07f;
        public Sprite[] spinnerSprites;
        private bool isMenuOverlayOpen = false;

        private List<MenuItem> menus = new();
        private bool valuesShown;
        private bool closeAllMenus = false;
        private bool closingAllMenus = false;
        [SerializeField]
        private bool isClosing = false;

        private class MenuItem
        {
            public VisualElement menuItem;
            public bool showValues;
        }

        // References
        private ValuesUI valuesUI;

        // UI
        private VisualElement root;
        private VisualElement localeWrapper;
        private VisualElement currentMenu;
        private Label title;
        private VisualElement menuOverlay;
        private VisualElement menuOverlaySpinner;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private Button menuOverlayDebugCloseButton;
#endif

        void Start()
        {
            // Cache
            valuesUI = GameRefs.Instance.valuesUI;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;
            localeWrapper = root.Q<VisualElement>("LocaleWrapper");

            menuOverlay = root.Q<VisualElement>("MenuOverlay");
            menuOverlaySpinner = menuOverlay.Q<VisualElement>("Spinner");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            menuOverlayDebugCloseButton = menuOverlay.Q<Button>("DebugCloseButton");

            // Button taps
            menuOverlayDebugCloseButton.clicked += () => HideMenuOverlay();

            menuOverlayDebugCloseButton.style.display = DisplayStyle.Flex;
#endif

            // Init
            localeWrapper.style.display = DisplayStyle.None;

            HideMenuOverlay();
        }

        // Update the currently opened menu's title
        public void UpdateTitle(string newTitle)
        {
            if (currentMenu.name == "LocaleMenu")
            {
                menus[0].menuItem.Q<VisualElement>("Title").Q<Label>("Value").text = newTitle;
            }
        }

        public bool IsMenuOpen(string menuName)
        {
            if (currentMenu != null && currentMenu.name == menuName)
            {
                return true;
            }

            bool found = false;

            if (menus.Count > 0)
            {
                for (int i = 0; i < menus.Count; i++)
                {
                    if (menus[i].menuItem.name == menuName)
                    {
                        found = true;

                        break;
                    }
                }
            }

            return found;
        }

        // Get ready to open the given menu
        public void OpenMenu(VisualElement menuElement, string newTitle, bool showValues = false, bool closeAll = false, bool ignoreClose = false)
        {
            if (!isClosing)
            {
                // Add the menu to the menu list
                menus.Add(new MenuItem { menuItem = menuElement, showValues = showValues });

                // Set the current menu
                currentMenu = menuElement;

                closeAllMenus = closeAll;

                currentMenu.SetEnabled(true);

                if (showValues)
                {
                    ShowValues();
                }

                CheckMenuOpened();

                ShowMenu(newTitle, ignoreClose);
            }
        }

        void ShowMenu(string newTitle, bool ignoreClose)
        {
            // Show the menu locale container
            localeWrapper.style.display = DisplayStyle.Flex;

            // Show the menu
            currentMenu.style.display = DisplayStyle.Flex;
            currentMenu.style.opacity = 1f;

            if (!ignoreClose)
            {
                // Add background click handler
                VisualElement background = currentMenu.Q<VisualElement>("Background");

                if (background != null)
                {
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
                }

                // Disable the close button
                /* VisualElement closeButton = currentMenu.Q<VisualElement>("Close");
                 VisualElement closeButtonBorder = closeButton.Q<VisualElement>("Border");

                 if (closeButton != null)
                 {
                     closeButton.pickingMode = PickingMode.Ignore;
                     closeButtonBorder.pickingMode = PickingMode.Ignore;
                 }*/
            }

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

        public void CloseMenu(string menuName, Action callback = null)
        {
            isClosing = true;

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

            StartCoroutine(HideMenuAfter(null, 0, callback));
        }

        IEnumerator HideMenuAfter(MenuItem menuItem = null, int index = 0, Action callback = null)
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

            callback?.Invoke();

            CheckMenuClosed();
        }

        IEnumerator StopAction()
        {
            yield return new WaitForSeconds(transitionDuration);

            if (isClosing)
            {
                isClosing = false;
            }
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

                StartCoroutine(StopAction());
            }
            else
            {
                // Set open menu indicator to close
                menuOpen = false;

                isClosing = false;

                // Hide the menu locale container
                localeWrapper.style.display = DisplayStyle.None;

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
            for (int i = menus.Count - 1; i > -1; i--) // NOTE - Counting backwards
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

        //// Overlay menuOverlaySpinner ////

        public void ShowMenuOverlay(Action callback = null, bool delayCallback = false)
        {
            menuOverlay.style.opacity = 1;
            menuOverlay.style.display = DisplayStyle.Flex;

            isMenuOverlayOpen = true;

            StartCoroutine(SpinTheSpinner());

            if (delayCallback)
            {
                Glob.SetTimeout(() =>
                {
                    callback?.Invoke();
                }, 0.3f);
            }
            else
            {
                callback?.Invoke();
            }
        }

        public void HideMenuOverlay(Action callback = null, bool delayCallback = false)
        {
            menuOverlay.style.opacity = 0;
            menuOverlay.style.display = DisplayStyle.None;

            isMenuOverlayOpen = false;

            StopCoroutine(SpinTheSpinner());

            if (delayCallback)
            {
                Glob.SetTimeout(() =>
                {
                    callback?.Invoke();
                }, 0.3f);
            }
            else
            {
                callback?.Invoke();
            }
        }

        IEnumerator SpinTheSpinner()
        {
            WaitForSeconds wait = new(spinnerSpeed);

            int count = 0;

            while (isMenuOverlayOpen)
            {
                menuOverlaySpinner.style.backgroundImage = new StyleBackground(spinnerSprites[count]);

                if (count == spinnerSprites.Length - 1)
                {
                    count = 0;
                }
                else
                {
                    count++;
                }

                yield return wait;
            }
        }
    }
}