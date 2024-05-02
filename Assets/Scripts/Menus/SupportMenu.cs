using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class SupportMenu : MonoBehaviour
    {
        // Variables
        private MenuUI.Menu menuType = MenuUI.Menu.Support;

        // References
        private MenuUI menuUI;
        // private I18n LOCALE;
        private UIData uiData;

        // UI
        private VisualElement content;

        void Start()
        {
            // Cache
            menuUI = GetComponent<MenuUI>();
            // LOCALE = I18n.Instance;
            uiData = GameData.Instance.GetComponent<UIData>();

            DataManager.Instance.CheckLoaded(() =>
            {
                // UI
                content = uiData.GetMenuAsset(menuType);
            });
        }

        public void Open()
        {
            // Check menu
            if (menuUI.IsMenuOpen(menuType))
            {
                return;
            }

            // Set menu content

            // Open menu
            menuUI.OpenMenu(content, menuType);
        }
    }
}
