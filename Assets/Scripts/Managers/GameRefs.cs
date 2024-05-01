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
        [Header("All Scenes")]
        public ProgressManager progressManager;
        public TaskManager taskManager;
        public TutorialManager tutorialManager;
        public PointerHandler pointerHandler;
        public TimeManager timeManager;
        public SceneLoader sceneLoader;
        public FeedbackManager feedbackManager;

        [Header("World Scene")]
        public WorldUI worldUI;
        public WorldGameUI worldGameUI;
        public WorldDataManager worldDataManager;
        public ConvoUIHandler convoUIHandler;
        public NavMeshManager navMeshManager;

        [Header("Merge Scene")]
        public MergeUI mergeUI;
        public BoardIndication boardIndication;
        public BoardInitialization boardInitialization;
        public BoardInteractions boardInteractions;
        public BoardManager boardManager;
        public BoardSelection boardSelection;
        public InfoBox infoBox;
        public ClockManager clockManager;

        [Header("UI")]
        public MenuUI menuUI;
        public ValuesUI valuesUI;

        [Header("Sprites")]
        public SpriteArray[] crateBreakSprites;
        public Sprite[] lockOpenSprites;
        public Sprite[] bubblePopSprites;

        [Header("Other")]
        public float readySpeed = 1f;

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
        public DebugMenu debugMenu;

        [HideInInspector]
        public ValuePop valuePop;

        // Other
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
                debugMenu = menuUI.GetComponent<DebugMenu>();

                valuePop = menuUI.GetComponent<ValuePop>();
            }

            initialized = true;

            ready = true;
        }
    }
}