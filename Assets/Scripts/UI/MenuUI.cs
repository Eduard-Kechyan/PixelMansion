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
        public float transitionDuration = 0.2f;
        public float menuDecreaseOffset = 0.8f;
        public bool menuOpen = false;

        [Header("Options")]
        public MenuOptionsData menuOptionsData;

        [Header("Spinner")]
        public float spinnerSpeed = 0.07f;
        public Sprite[] spinnerSprites;
        private bool isMenuOverlayOpen = false;

        [HideInInspector]
        public int menusToLoad = 0;
        private int menusLoaded = 0;
        private Action menusLoadedCallback;

        private List<MenuItem> menus = new();
        private bool valuesShown;
        private bool closeAllMenus = false;
        private bool closingAllMenus = false;
        [SerializeField]
        private bool isClosing = false;

        private Menu currentMenuType;
        private VisualElement currentMenuUI;

        // Classes
        private class MenuItem
        {
            public VisualElement menuUI;
            public Menu menuType;
            public bool showValues;
        }

        [Serializable]
        public class MenuOptions
        {
            public string name;
            public Menu menuType = Menu.None;
            public bool showValues = false;
            public bool isSmall = false;
            public bool canClose = true;
            public bool keepInMemory = true;
        }

        // Events
        public delegate void OpenEvent();
        public static event OpenEvent OnMenuOpen;
        public delegate void CloseEvent();
        public static event CloseEvent OnMenuClose;
        public delegate void MenuLoadedEvent();
        public static event MenuLoadedEvent OnMenuLoaded;

        // Enums
        public enum Menu
        {
            None,
            Confirm,
            Note,
            Info,
            Inventory,
            Task,
            Shop,
            Level,
            Energy,
            Locale,
            Settings,
            Support,
            Input,
            Follow,
            Rate,
            Message,
            Debug,
            Terms,
            Conflict,
            Update
        }

        // References
        private ValuesUI valuesUI;
        private I18n LOCALE;
        private UIData uiData;
        private BoardInteractions boardInteractions;
        private ErrorManager errorManager;

        // UI
        private VisualElement root;
        private VisualElement localeWrapper;
        private VisualElement menuOverlay;
        private VisualElement menuOverlaySpinner;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private Button menuOverlayDebugCloseButton;
#endif

        void Start()
        {
            // Cache
            valuesUI = GameRefs.Instance.valuesUI;
            LOCALE = I18n.Instance;
            uiData = GameData.Instance.GetComponent<UIData>();
            boardInteractions = GameRefs.Instance.boardInteractions;
            errorManager = ErrorManager.Instance;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;
            localeWrapper = root.Q<VisualElement>("LocaleWrapper");

            menuOverlay = root.Q<VisualElement>("MenuOverlay");
            menuOverlaySpinner = menuOverlay.Q<VisualElement>("Spinner");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            menuOverlayDebugCloseButton = menuOverlay.Q<Button>("DebugCloseButton");

            // UI taps
            menuOverlayDebugCloseButton.clicked += () => HideMenuOverlay();

            menuOverlayDebugCloseButton.style.display = DisplayStyle.Flex;
#endif

            // Init
            localeWrapper.style.display = DisplayStyle.None;

            HideMenuOverlay();
        }

        void OnEnable()
        {
            // Subscribe to events
            MenuUI.OnMenuLoaded += MenuLoaded;
        }

        void OnDisable()
        {
            // Unsubscribe to events
            MenuUI.OnMenuLoaded -= MenuLoaded;
        }

        // Get ready to open the given menu
        public VisualElement OpenMenu(VisualElement menuUI, Menu menuType, string altTitle = "", bool closeAll = false, bool keepInMemory = true)
        {
            if (!isClosing)
            {
                // Set the current menu
                currentMenuType = menuType;

                MenuOptions menuOptions = GetMenuOptions(menuType);

                VisualElement menuElement = null;

                if (menuOptions.keepInMemory)
                {
                    menuElement = uiData.GetMenuElement(menuType);
                }

                if (menuElement != null)
                {
                    currentMenuUI = menuElement;
                }
                else
                {
                    // Create a new menu                
                    currentMenuUI = uiData.menuContainerPrefab.CloneTree().Q<VisualElement>("Menu");

                    // Add the content to the newly created menu
                    currentMenuUI.Q<VisualElement>("Container").Insert(0, menuUI);

                    // Add the related menu class
                    currentMenuUI.AddToClassList(currentMenuType + "_menu");

                    if (menuOptions.isSmall)
                    {
                        currentMenuUI.Q<VisualElement>("Container").AddToClassList("small_menu");
                    }
                }

                // Set or update the menu's title
                string newTitle = altTitle != "" ? altTitle : LOCALE.Get(currentMenuType + "_menu_title");

                currentMenuUI.Q<Label>("TitleValue").text = newTitle;

                // Add the menu to the menu list
                menus.Add(new MenuItem
                {
                    menuUI = currentMenuUI,
                    menuType = currentMenuType,
                    showValues = menuOptions.showValues
                });

                if (keepInMemory && menuOptions.keepInMemory)
                {
                    uiData.SetMenuElement(menuType, currentMenuUI);
                }

                // Add menu to the ui
                localeWrapper.Insert(localeWrapper.childCount - 1, currentMenuUI);

                // Close all menus if needed
                closeAllMenus = closeAll;

                if (menuOptions.showValues)
                {
                    ShowValues();
                }

                CheckMenuOpened();

                OnMenuOpen?.Invoke();

                ShowMenu(menuOptions.canClose);

                return currentMenuUI;
            }

            return null;
        }

        void ShowMenu(bool canClose)
        {
            // Show the menu locale container
            localeWrapper.style.display = DisplayStyle.Flex;

            // Show the menu
            currentMenuUI.style.display = DisplayStyle.Flex;
            currentMenuUI.style.opacity = 1f;

            // Enable the menu to make it clickable
            currentMenuUI.SetEnabled(true);

            // Add background click handler
            VisualElement background = currentMenuUI.Q<VisualElement>("Background");
            VisualElement closeButton = currentMenuUI.Q<VisualElement>("Close");

            if (PlayerPrefs.HasKey("tutorialFinished") && canClose)
            {
                background.AddManipulator(new Clickable(evt =>
                {
                    HandleBackgroundClose();
                }));

                closeButton.style.display = DisplayStyle.Flex;
                closeButton.style.opacity = 1;
            }
            else
            {
                background.RemoveManipulator(new Clickable(evt =>
                {
                    HandleBackgroundClose();
                }));

                closeButton.style.display = DisplayStyle.None;
                closeButton.style.opacity = 0;
            }

            // Set open menu indicator to open
            menuOpen = true;

            // Disable the board
            if (boardInteractions != null)
            {
                boardInteractions.DisableInteractions();
            }
        }

        void HandleBackgroundClose()
        {
            if (closeAllMenus)
            {
                CloseAllMenus();
            }
            else
            {
                CloseMenu(currentMenuType);
            }
        }

        public void CloseMenu(Menu menuType, Action callback = null)
        {
            OnMenuClose?.Invoke();

            isClosing = true;

            // Disable the menu to make it unclickable
            currentMenuUI.SetEnabled(false);

            // Hide the menu
            currentMenuUI.style.opacity = 0f;

            // Remove the menu from the menu list
            int currentMenuIndex = 0;

            for (int i = 0; i < menus.Count; i++)
            {
                if (menus[i].menuType == menuType)
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
                currentMenuUI.style.display = DisplayStyle.None;

                localeWrapper.Remove(currentMenuUI);

                currentMenuUI = null;

                currentMenuType = Menu.None;
            }
            else
            {
                menuItem.menuUI.style.display = DisplayStyle.None;

                localeWrapper.Remove(menuItem.menuUI);

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

                menus[menus.Count - 2].menuUI.style.scale = new StyleScale(scale);

                // Hide close button
                menus[menus.Count - 2].menuUI.Q<VisualElement>("Close").style.opacity = 0f;
            }
        }

        void CheckMenuClosed()
        {
            // Check if there are any menu's open
            if (menus.Count > 0)
            {
                // Set the current menu
                currentMenuUI = menus[menus.Count - 1].menuUI;
                currentMenuType = menus[menus.Count - 1].menuType;

                // Reset the menu's size
                Scale scale = new Scale(new Vector2(1f, 1f));

                currentMenuUI.style.scale = new StyleScale(scale);

                // Show close button
                currentMenuUI.Q<VisualElement>("Close").style.opacity = 1f;

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

                currentMenuUI = null;
                currentMenuType = Menu.None;

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

                OnMenuClose?.Invoke();

                StartCoroutine(CloseAllAfter());
            }
        }

        IEnumerator CloseAllAfter()
        {
            for (int i = menus.Count - 1; i > -1; i--) // NOTE - Counting backwards
            {
                menus[i].menuUI.SetEnabled(false);
                menus[i].menuUI.style.opacity = 0f;
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

        MenuOptions GetMenuOptions(Menu menuType)
        {
            foreach (MenuOptions menuOption in menuOptionsData.data)
            {
                if (menuOption.menuType == menuType)
                {
                    return menuOption;
                }
            }

            // ERROR
            errorManager.ThrowWarning(ErrorManager.ErrorType.Code, GetType().ToString(), "menuType " + menuType + " not found in menuOptionsData!");

            return null;
        }

        public bool IsMenuOpen(Menu menu)
        {
            if (currentMenuType != Menu.None && currentMenuType == menu)
            {
                return true;
            }

            bool found = false;

            if (menus.Count > 0)
            {
                for (int i = 0; i < menus.Count; i++)
                {
                    if (menus[i].menuType == menu)
                    {
                        found = true;

                        break;
                    }
                }
            }

            return found;
        }

        //// Menus ////

        public void CheckLoadingSceneMenus(Action callback = null)
        {
            menusLoadedCallback = callback;

            Component[] components = gameObject.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                Type type = components[i].GetType();

                if (type.GetField("menuType") != null && type.GetField("hasLoadingEvent") != null)
                {
                    menusToLoad++;
                }
            }
        }

        void MenuLoaded()
        {
            menusLoaded++;

            if (menusToLoad == menusLoaded)
            {
                menusLoadedCallback?.Invoke();
            }
        }

        public void ThrowMenuLoadedEvent()
        {
            OnMenuLoaded?.Invoke();
        }

        //// Overlay ////

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