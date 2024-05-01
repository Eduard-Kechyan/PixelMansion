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
            public Types.Menu menuType;
            public VisualElement element;
            public VisualTreeAsset asset;
        };

        private MenuItem[] menuItems;
        private int menuTypesLength = Enum.GetValues(typeof(Types.Menu)).Length;

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

            LoadUIData();
        }

        async void LoadUIData()
        {
            menuContainerPrefab = await addressableManager.LoadAssetAsync<VisualTreeAsset>("Assets/Addressables/Uxml/MenuContainer.uxml");

            for (int i = 0; i < menuTypesLength; i++)
            {
                Types.Menu menuType = (Types.Menu)i;

                MenuItem newMenuItem;

                if (menuType == Types.Menu.None)
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

        public VisualElement GetMenuAsset(Types.Menu menuType)
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
                        errorManager.ThrowWarning(Types.ErrorType.Code, GetType().ToString(), "Types.Menu " + menuType + " is null!");

                        return null;
                    }
                }
            }

            // ERROR
            errorManager.ThrowWarning(Types.ErrorType.Code, GetType().ToString(), "Types.Menu " + menuType + " not found!");

            return null;
        }

        public VisualElement GetMenuElement(Types.Menu menuType)
        {
            for (int i = 0; i < menuTypesLength; i++)
            {
                if (menuItems[i].menuType == menuType)
                {
                    return menuItems[i].element;
                }
            }

            // ERROR
            errorManager.ThrowWarning(Types.ErrorType.Code, GetType().ToString(), "Types.Menu " + menuType + " not found!");

            return null;
        }

        public void SetMenuElement(Types.Menu menuType, VisualElement element)
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
                errorManager.ThrowWarning(Types.ErrorType.Code, GetType().ToString(), "Types.Menu " + menuType + " not found!");
            }
        }

        public void ClearMenuElement(Types.Menu menuType)
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
                errorManager.ThrowWarning(Types.ErrorType.Code, GetType().ToString(), "Types.Menu " + menuType + " not found!");
            }
        }
    }
}
