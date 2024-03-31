using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class GameRefs : MonoBehaviour
    {
        // UI scripts
        [Header("UI Scripts")]
        public MergeUI mergeUI;
        public WorldUI worldUI;
        public WorldGameUI worldGameUI;
        public MenuUI menuUI;
        public ValuesUI valuesUI;

        // UI documents
        [HideInInspector]
        public UIDocument mergeUIDoc;

        [HideInInspector]
        public UIDocument worldUIDoc;

        [HideInInspector]
        public UIDocument worldGameUIDoc;

        [HideInInspector]
        public UIDocument menuUIDoc;

        [HideInInspector]
        public UIDocument valuesUIDoc;

        // Menus
        [HideInInspector]
        public ConfirmMenu confirmMenu;

        [HideInInspector]
        public EnergyMenu energyMenu;

        [HideInInspector]
        public InfoMenu infoMenu;

        [HideInInspector]
        public InventoryMenu inventoryMenu;

        [HideInInspector]
        public LevelMenu levelMenu;

        [HideInInspector]
        public RateMenu rateMenu;

        [HideInInspector]
        public FollowMenu followMenu;

        [HideInInspector]
        public NoteMenu noteMenu;

        [HideInInspector]
        public SettingsMenu settingsMenu;

        [HideInInspector]
        public InputMenu inputMenu;

        [HideInInspector]
        public ShopMenu shopMenu;

        [HideInInspector]
        public TaskMenu taskMenu;

        [HideInInspector]
        public ValuePop valuePop;

        // Other
        [Header("Other")]
        public float readySpeed = 1f;
        public ClockManager clockManager;
        public InfoBox infoBox;
        public BoardManager boardManager;

        [HideInInspector]
        public bool initialized = false;

        [HideInInspector]
        public SafeAreaHandler safeAreaHandler;

        [HideInInspector]
        public NoteDotHandler noteDotHandler;

        [Serializable]
        public class SpriteArray
        {
            public Sprite[] content;
        }

        public SpriteArray[] crateBreakSprites;

        public Sprite[] lockOpenSprites;

        public Sprite[] bubblePopSprites;

        [HideInInspector]
        public bool ready;

        // Instance
        public static GameRefs Instance;

        void Awake()
        {
            Instance = this;

            Init();
        }

        void Init()
        {
            // UI documents
            if (mergeUI != null)
            {
                mergeUIDoc = mergeUI.GetComponent<UIDocument>();
                safeAreaHandler = mergeUI.GetComponent<SafeAreaHandler>();
                noteDotHandler = mergeUI.GetComponent<NoteDotHandler>();
            }

            if (worldUI != null)
            {
                worldUIDoc = worldUI.GetComponent<UIDocument>();
                worldGameUIDoc = worldGameUI.GetComponent<UIDocument>();
                safeAreaHandler = worldUI.GetComponent<SafeAreaHandler>();
                noteDotHandler = worldUI.GetComponent<NoteDotHandler>();
            }

            if (valuesUI != null)
            {
                valuesUIDoc = valuesUI.GetComponent<UIDocument>();
            }

            // Menus
            if (menuUI != null)
            {
                menuUIDoc = menuUI.GetComponent<UIDocument>();

                confirmMenu = menuUI.GetComponent<ConfirmMenu>();
                energyMenu = menuUI.GetComponent<EnergyMenu>();
                infoMenu = menuUI.GetComponent<InfoMenu>();
                inventoryMenu = menuUI.GetComponent<InventoryMenu>();
                levelMenu = menuUI.GetComponent<LevelMenu>();
                rateMenu = menuUI.GetComponent<RateMenu>();
                followMenu = menuUI.GetComponent<FollowMenu>();
                noteMenu = menuUI.GetComponent<NoteMenu>();
                settingsMenu = menuUI.GetComponent<SettingsMenu>();
                inputMenu = menuUI.GetComponent<InputMenu>();
                shopMenu = menuUI.GetComponent<ShopMenu>();
                taskMenu = menuUI.GetComponent<TaskMenu>();

                valuePop = menuUI.GetComponent<ValuePop>();
            }

            initialized = true;

            ready = true;
        }
    }
}