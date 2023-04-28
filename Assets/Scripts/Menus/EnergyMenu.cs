using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class EnergyMenu : MonoBehaviour
{
    // Variables
    public int energyBuyAmount = 100;
    public int energyWatchAmount = 20;
    public int gemsCost = 5;

    // References
    private MenuUI menuUI;
    private ShopMenu shopMenu;
    private ValuePop valuePop;

    // Instances
    private GameData gameData;
    private I18n LOCALE;

    // UI
    private VisualElement root;
    private VisualElement energyMenu;
    private Label energyLabelA;
    private Label energyLabelB;
    private Label energyBuyLabel;
    private Label energyWatchLabel;
    private Button buyButton;
    private Button watchButton;

    void Start()
    {
        // Cache
        menuUI = GetComponent<MenuUI>();
        shopMenu = GetComponent<ShopMenu>();
        valuePop = GetComponent<ValuePop>();

        // Cache instances
        gameData = GameData.Instance;
        LOCALE = I18n.Instance;

        // Cache UI
        root = GetComponent<UIDocument>().rootVisualElement;

        energyMenu = root.Q<VisualElement>("EnergyMenu");

        energyLabelB = energyMenu.Q<Label>("EnergyLabelB");
        energyLabelA = energyMenu.Q<Label>("EnergyLabelA");

        energyWatchLabel = energyMenu.Q<VisualElement>("EnergyBoxes").Q<Label>("WatchLabel");
        energyBuyLabel = energyMenu.Q<VisualElement>("EnergyBoxes").Q<Label>("BuyLabel");

        watchButton = energyMenu.Q<VisualElement>("EnergyBoxes").Q<Button>("WatchButton");
        buyButton = energyMenu.Q<VisualElement>("EnergyBoxes").Q<Button>("BuyButton");

        watchButton.clicked += () => WatchAdHandle();
        buyButton.clicked += () => BuyEnegyHandler();

        InitMenu();
    }

    void InitMenu()
    {
        energyLabelB.text = LOCALE.Get("energy_menu_label_b");
        energyLabelA.text = LOCALE.Get("energy_menu_label_a");

        energyWatchLabel.text = "+" + energyWatchAmount;
        energyBuyLabel.text = "+" + energyBuyAmount;

        watchButton.text = LOCALE.Get("energy_menu_watch_ad");
        buyButton.text = gemsCost.ToString();
    }

    public void Open()
    {
        // Title
        string title = LOCALE.Get("energy_menu_title");

        watchButton.SetEnabled(false);

        // Open menu
        menuUI.OpenMenu(energyMenu, title, true);
    }

    // Add energy after successfuly watching an ad
    void WatchAdHandle()
    {
        // TODO - Add a check here to see if the ad was successfuly watched

        Debug.Log("Ad watched!");

        gameData.UpdateEnergy(energyWatchAmount);

        menuUI.CloseMenu(energyMenu.name);
    }

    // Add energy after buyinh it
    void BuyEnegyHandler()
    {
        // Check if we have enough energy
        if (gameData.gems >= gemsCost)
        {
            gameData.UpdateGems(-gemsCost); // Note the -

            valuePop.PopValue(energyBuyAmount, "Energy");

            menuUI.CloseMenu(energyMenu.name);
        }
        else
        {
            shopMenu.Open("Gems");
        }
    }
}
