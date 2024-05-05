using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// NOTE - Get the data using: gameData.menuContainerPrefab.CloneTree();

namespace Merge
{
    public class UIData : MonoBehaviour
    {
        // Variables
        [HideInInspector]
        public bool dataLoaded = false;

        class MenuItem
        {
            public MenuUI.Menu menuType;
            public VisualElement element;
            public VisualTreeAsset asset;
        };

        private MenuItem[] menuItems;
        private int menuTypesLength = Enum.GetValues(typeof(MenuUI.Menu)).Length;

        // Menus
        [HideInInspector]
        public VisualTreeAsset menuContainerPrefab;

        [HideInInspector]
        public VisualTreeAsset shopItemBoxPrefab;

        [HideInInspector]
        public VisualTreeAsset taskGroupPrefab;
        [HideInInspector]
        public VisualTreeAsset taskPrefab;
        [HideInInspector]
        public VisualTreeAsset taskNeedPrefab;

        // References
        private DataManager dataManager;
        private AddressableManager addressableManager;
        private ErrorManager errorManager;

        void Start()
        {
            // Cache
            dataManager = DataManager.Instance;
            addressableManager = dataManager.GetComponent<AddressableManager>();
            errorManager = ErrorManager.Instance;

            menuItems = new MenuItem[menuTypesLength];

            StartCoroutine(WaitForInitialization());
        }

        IEnumerator WaitForInitialization()
        {
            while (!addressableManager.initialized)
            {
                yield return null;
            }

            LoadUIData();
        }

        async void LoadUIData()
        {
            menuContainerPrefab = await addressableManager.LoadAssetAsync<VisualTreeAsset>("Assets/Addressables/Uxml/MenuContainer.uxml");

            for (int i = 0; i < menuTypesLength; i++)
            {
                MenuUI.Menu menuType = (MenuUI.Menu)i;

                MenuItem newMenuItem;

                if (menuType == MenuUI.Menu.None)
                {
                    newMenuItem = new()
                    {
                        menuType = menuType,
                        asset = null
                    };
                }
                else
                {
                    newMenuItem = new()
                    {
                        menuType = menuType,
                        asset = await addressableManager.LoadAssetAsync<VisualTreeAsset>("Assets/Addressables/Uxml/Menus/" + menuType + "Menu.uxml")
                    };
                }

                menuItems[i] = newMenuItem;
            }

            shopItemBoxPrefab = await addressableManager.LoadAssetAsync<VisualTreeAsset>("Assets/Addressables/Uxml/ShopItemBox.uxml");

            taskGroupPrefab = await addressableManager.LoadAssetAsync<VisualTreeAsset>("Assets/Addressables/Uxml/TaskGroup.uxml");
            taskPrefab = await addressableManager.LoadAssetAsync<VisualTreeAsset>("Assets/Addressables/Uxml/Task.uxml");
            taskNeedPrefab = await addressableManager.LoadAssetAsync<VisualTreeAsset>("Assets/Addressables/Uxml/TaskNeed.uxml");

            dataLoaded = true;
        }

        public VisualElement GetMenuAsset(MenuUI.Menu menuType)
        {
            for (int i = 0; i < menuTypesLength; i++)
            {
                if (menuItems[i].menuType == menuType)
                {
                    if (menuItems[i].asset != null)
                    {
                        return menuItems[i].asset.CloneTree().Q<VisualElement>("Content");
                    }
                    else
                    {
                        // ERROR
                        errorManager.ThrowWarning(ErrorManager.ErrorType.Code, GetType().ToString(), "MenuUI.Menu " + menuType + " is null!");

                        return null;
                    }
                }
            }

            // ERROR
            errorManager.ThrowWarning(ErrorManager.ErrorType.Code, GetType().ToString(), "MenuUI.Menu " + menuType + " not found!");

            return null;
        }

        public VisualElement GetMenuElement(MenuUI.Menu menuType)
        {
            for (int i = 0; i < menuTypesLength; i++)
            {
                if (menuItems[i].menuType == menuType)
                {
                    return menuItems[i].element;
                }
            }

            // ERROR
            errorManager.ThrowWarning(ErrorManager.ErrorType.Code, GetType().ToString(), "MenuUI.Menu " + menuType + " not found!");

            return null;
        }

        public void SetMenuElement(MenuUI.Menu menuType, VisualElement element)
        {
            bool found = false;

            for (int i = 0; i < menuTypesLength; i++)
            {
                if (menuItems[i].menuType == menuType)
                {
                    menuItems[i].element = element;

                    found = true;

                    break;
                }
            }

            if (!found)
            {
                // ERROR
                errorManager.ThrowWarning(ErrorManager.ErrorType.Code, GetType().ToString(), "MenuUI.Menu " + menuType + " not found!");
            }
        }

        public void ClearMenuElement(MenuUI.Menu menuType)
        {
            bool found = false;

            for (int i = 0; i < menuTypesLength; i++)
            {
                if (menuItems[i].menuType == menuType)
                {
                    menuItems[i].element = null;
                    menuItems[i].asset = null;

                    found = true;

                    break;
                }
            }

            if (!found)
            {
                // ERROR
                errorManager.ThrowWarning(ErrorManager.ErrorType.Code, GetType().ToString(), "MenuUI.Menu " + menuType + " not found!");
            }
        }
    }
}
