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
        public GameplayUI gameplayUI;
        public HubUI hubUI;
        public HubGameUI hubGameUI;
        public MenuUI menuUI;
        public ValuesUI valuesUI;
        public UIDocument debugUI;

        // UI documents
        [HideInInspector]
        public UIDocument gameplayUIDoc;

        [HideInInspector]
        public UIDocument hubUIDoc;

        [HideInInspector]
        public UIDocument hubGameUIDoc;

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
            if (gameplayUI != null)
            {
                gameplayUIDoc = gameplayUI.GetComponent<UIDocument>();
                safeAreaHandler = gameplayUI.GetComponent<SafeAreaHandler>();
                noteDotHandler = gameplayUI.GetComponent<NoteDotHandler>();
            }

            if (hubUI != null)
            {
                hubUIDoc = hubUI.GetComponent<UIDocument>();
                hubGameUIDoc = hubGameUI.GetComponent<UIDocument>();
                safeAreaHandler = hubUI.GetComponent<SafeAreaHandler>();
                noteDotHandler = hubUI.GetComponent<NoteDotHandler>();
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
                followMenu= menuUI.GetComponent<FollowMenu>();
                noteMenu = menuUI.GetComponent<NoteMenu>();
                settingsMenu = menuUI.GetComponent<SettingsMenu>();
                inputMenu = menuUI.GetComponent<InputMenu>();
                shopMenu = menuUI.GetComponent<ShopMenu>();
                taskMenu = menuUI.GetComponent<TaskMenu>();

                valuePop = menuUI.GetComponent<ValuePop>();
            }

            if (debugUI != null && Logs.Instance != null)
            {
                Logs.Instance.Init(debugUI);
            }

            initialized = true;
        }
    }
}