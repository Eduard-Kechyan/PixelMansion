using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameRefs : MonoBehaviour
{
    // UI
    public GameplayUI gameplayUI;
    public HubUI hubUI;
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

    // Instance
    public static GameRefs Instance;

    [HideInInspector]
    public bool initialized = false;


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
        }
        if (hubUI != null)
        {
            hubUIDoc = hubUI.GetComponent<UIDocument>();
            safeAreaHandler = hubUI.GetComponent<SafeAreaHandler>();
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
