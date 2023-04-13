using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Values : MonoBehaviour
{
    [HideInInspector]
    public bool set;

    private GameData gameData;

    private UIDocument valuesUI;

    private VisualElement root;

    private VisualElement valuesBox;

    public Button levelButton;
    private Label levelValue;

    public Button energyButton;
    private VisualElement energyPlus;

    [HideInInspector]
    public Label energyTimer;

    public Button goldButton;
    private VisualElement goldPlus;

    public Button gemsButton;
    private VisualElement gemsPlus;

    private EnergyMenu energyMenu;
    private ShopMenu shopMenu;

    private bool levelEnabled = false;
    private bool energyEnabled = false;
    private bool goldEnabled = false;
    private bool gemsEnabled = false;

    private bool energyPlusEnabled = false;
    private bool goldPlusEnabled = false;
    private bool gemsPlusEnabled = false;

    private bool enabledSet = false;

    void Start()
    {
        gameData = GameData.Instance;
    }

    public void InitializeValues()
    {
        // Menus
        energyMenu = MenuManager.Instance.GetComponent<EnergyMenu>();
        shopMenu = MenuManager.Instance.GetComponent<ShopMenu>();

        // Cache UI
        valuesUI = GameObject.Find("ValuesUI").GetComponent<UIDocument>();

        root = valuesUI.rootVisualElement;

        valuesBox = root.Q<VisualElement>("ValuesBox");

        levelButton = valuesBox.Q<Button>("LevelButton");
        levelValue = levelButton.Q<Label>("Value");

        energyButton = valuesBox.Q<Button>("EnergyButton");
        energyPlus = energyButton.Q<VisualElement>("Plus");
        energyTimer = energyButton.Q<Label>("EnergyTimer");

        energyTimer.style.display = DisplayStyle.None;

        goldButton = valuesBox.Q<Button>("GoldButton");
        goldPlus = goldButton.Q<VisualElement>("Plus");

        gemsButton = valuesBox.Q<Button>("GemsButton");
        gemsPlus = gemsButton.Q<VisualElement>("Plus");

        set = true;

        SetValues();

        CheckForTaps();
    }

    void SetValues()
    {
        // TODO - Remove comments and set to false
        //levelButton.SetEnabled(false);
        //energyButton.SetEnabled(false);
        //goldButton.SetEnabled(false);
        //gemsButton.SetEnabled(false);

        // TODO - Add a check for the plusses
        // energyPlus.style.display = DisplayStyle.None;
        //goldPlus.style.display = DisplayStyle.None;
        //gemsPlus.style.display = DisplayStyle.None;

        UpdateValues();
    }

    void CheckForTaps()
    {
        energyButton.clicked += () => energyMenu.Open();
        goldButton.clicked += () => shopMenu.Open("Gold");
        gemsButton.clicked += () => shopMenu.Open("Gems");
    }

    public void UpdateValues()
    {
        levelButton.text = gameData.experience.ToString(); // TODO -  Change this to a fill

        levelValue.text = gameData.level.ToString();
        energyButton.text = gameData.energy.ToString();
        goldButton.text = gameData.gold.ToString();
        gemsButton.text = gameData.gems.ToString();
    }

    public void DisableButtons()
    {
        if (!enabledSet)
        {
            levelEnabled = levelButton.enabledSelf;
            energyEnabled = energyButton.enabledSelf;
            goldEnabled = goldButton.enabledSelf;
            gemsEnabled = gemsButton.enabledSelf;

            levelButton.SetEnabled(false);
            energyButton.SetEnabled(false);
            goldButton.SetEnabled(false);
            gemsButton.SetEnabled(false);

            energyPlusEnabled = energyPlus.resolvedStyle.visibility == Visibility.Visible;
            goldPlusEnabled = goldPlus.resolvedStyle.visibility == Visibility.Visible;
            gemsPlusEnabled = gemsPlus.resolvedStyle.visibility == Visibility.Visible;

            energyPlus.style.visibility = Visibility.Hidden;
            energyPlus.style.opacity = 0f;
            goldPlus.style.visibility = Visibility.Hidden;
            goldPlus.style.opacity = 0f;
            gemsPlus.style.visibility = Visibility.Hidden;
            gemsPlus.style.opacity = 0f;

            enabledSet = true;
        }
    }

    public void EnableButtons()
    {
        levelButton.SetEnabled(levelEnabled);
        energyButton.SetEnabled(energyEnabled);
        goldButton.SetEnabled(goldEnabled);
        gemsButton.SetEnabled(gemsEnabled);

        if (energyPlusEnabled)
        {
            energyPlus.style.visibility = Visibility.Visible;
            energyPlus.style.opacity = 1f;
        }

        if (goldPlusEnabled)
        {
            goldPlus.style.visibility = Visibility.Visible;
            goldPlus.style.opacity = 1f;
        }

        if (gemsPlusEnabled)
        {
            gemsPlus.style.visibility = Visibility.Visible;
            gemsPlus.style.opacity = 1f;
        }

        enabledSet = false;
    }
}
