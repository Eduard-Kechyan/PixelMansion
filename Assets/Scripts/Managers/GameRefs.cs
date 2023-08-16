using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class GameRefs : MonoBehaviour
{
    // UI
    public GameplayUI gameplayUI;
    public HubUI hubUI;
    public HubGameUI hubGameUI;
    public MenuUI menuUI;
    public ValuesUI valuesUI;

    [HideInInspector]
    public SafeAreaHandler safeAreaHandler;

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
    public EnergyMenu energyMenu;

    [HideInInspector]
    public InfoMenu infoMenu;

    [HideInInspector]
    public InventoryMenu inventoryMenu;

    [HideInInspector]
    public LevelMenu levelMenu;

    [HideInInspector]
    public NoteMenu noteMenu;

    [HideInInspector]
    public SettingsMenu settingsMenu;

    [HideInInspector]
    public ShopMenu shopMenu;

    [HideInInspector]
    public TaskMenu taskMenu;

    [HideInInspector]
    public ValuePop valuePop;

    // Other
    [HideInInspector]
    public bool initialized = false;

    // References
    [HideInInspector]
    public PopupManager popupManager;

    // Instance
    public static GameRefs Instance;

    void Awake()
    {
        Instance = this;

        Init();
    }

    void Init()
    {
        // References
        popupManager = GetComponent<PopupManager>();

        // UI documents
        if (gameplayUI != null)
        {
            gameplayUIDoc = gameplayUI.GetComponent<UIDocument>();
            safeAreaHandler = gameplayUI.GetComponent<SafeAreaHandler>();
        }
        if (hubUI != null)
        {
            hubUIDoc = hubUI.GetComponent<UIDocument>();
            safeAreaHandler = hubUI.GetComponent<SafeAreaHandler>();
        }
        if (hubGameUI != null)
        {
            hubGameUIDoc = hubGameUI.GetComponent<UIDocument>();
        }
        if (valuesUI != null)
        {
            valuesUIDoc = valuesUI.GetComponent<UIDocument>();
        }

        // Menus
        if (menuUI != null)
        {
            menuUIDoc = menuUI.GetComponent<UIDocument>();

            energyMenu = menuUI.GetComponent<EnergyMenu>();
            infoMenu = menuUI.GetComponent<InfoMenu>();
            inventoryMenu = menuUI.GetComponent<InventoryMenu>();
            levelMenu = menuUI.GetComponent<LevelMenu>();
            noteMenu = menuUI.GetComponent<NoteMenu>();
            settingsMenu = menuUI.GetComponent<SettingsMenu>();
            shopMenu = menuUI.GetComponent<ShopMenu>();
            taskMenu = menuUI.GetComponent<TaskMenu>();

            valuePop = menuUI.GetComponent<ValuePop>();
        }

        initialized = true;
    }
}
}