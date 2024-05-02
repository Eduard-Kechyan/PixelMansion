using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class ConflictMenu : MonoBehaviour
    {
        // Variables
        private Action<bool> conflictCallback;

        [HideInInspector]
        public bool hasLoadingEvent = true;

        [HideInInspector]
        public MenuUI.Menu menuType = MenuUI.Menu.Conflict;

        // References
        private MenuUI menuUI;
        private I18n LOCALE;
        private UIData uiData;

        // UI
        private VisualElement content;
        private Label conflictLabelA;
        private Label conflictLabelB;
        private Button conflictNewButton;
        private Button conflictPreviousButton;

        void Start()
        {
            // Cache
            menuUI = GetComponent<MenuUI>();
            LOCALE = I18n.Instance;
            uiData = GameData.Instance.GetComponent<UIData>();

            DataManager.Instance.CheckLoaded(() =>
            {
                // UI
                content = uiData.GetMenuAsset(menuType);

                conflictLabelA = content.Q<Label>("ConflictLabelA");
                conflictLabelB = content.Q<Label>("ConflictLabelB");
                conflictNewButton = content.Q<Button>("NewButton");
                conflictPreviousButton = content.Q<Button>("PreviousButton");

                // UI Taps
                conflictNewButton.clicked += () =>
                {
                    ResolveConflict(true);
                };
                conflictPreviousButton.clicked += () =>
                {
                    ResolveConflict(false);
                };

                Init();
            });
        }

        void Init()
        {
            conflictLabelA.text = LOCALE.Get("conflict_label_a");
            conflictLabelB.text = LOCALE.Get("conflict_label_b");

            conflictNewButton.text = LOCALE.Get("conflict_create_new");
            conflictPreviousButton.text = LOCALE.Get("conflict_use_previous");

            menuUI.ThrowMenuLoadedEvent();
        }

        void OnDestroy()
        {
            uiData.ClearMenuElement(menuType);
        }

        public void Open(Action<bool> callback = null)
        {
            // Check menu
            if (menuUI.IsMenuOpen(menuType))
            {
                return;
            }

            // Set menu content
            conflictCallback = callback;

            // Open menu
            menuUI.OpenMenu(content, menuType);
        }

        // Handle the player resolving the conflict
        void ResolveConflict(bool forceLinking)
        {
            menuUI.CloseMenu(menuType, () =>
            {
                conflictCallback?.Invoke(forceLinking);
            });
        }
    }
}
