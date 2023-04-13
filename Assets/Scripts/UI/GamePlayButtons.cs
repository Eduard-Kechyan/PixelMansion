using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class GamePlayButtons : MonoBehaviour
{
    public SceneLoader sceneLoader;

    private VisualElement root;
    private Button homeButton;
    private Button inventoryButton;
    private Button bonusButton;
    private Button shopButton;
    private Button taskButton;

    private InventoryMenu inventoryMenu;
    private ShopMenu shopMenu;
    private TaskMenu taskMenu;

    private BonusItemsManager bonusItemsManager;

    void Start()
    {
        bonusItemsManager = DataManager.Instance.GetComponent<BonusItemsManager>();

        // UI
        root = GetComponent<UIDocument>().rootVisualElement;

        homeButton = root.Q<Button>("HomeButton");
        inventoryButton = root.Q<Button>("InventoryButton");
        bonusButton = root.Q<Button>("BonusButton");
        shopButton = root.Q<Button>("ShopButton");
        taskButton = root.Q<Button>("TaskButton");

        // Menus
        inventoryMenu = MenuManager.Instance.GetComponent<InventoryMenu>();
        shopMenu = MenuManager.Instance.GetComponent<ShopMenu>();
        taskMenu = MenuManager.Instance.GetComponent<TaskMenu>();

        // Button taps
        homeButton.clicked += () => sceneLoader.Load(1);
        inventoryButton.clicked += () => inventoryMenu.Open();
        bonusButton.clicked += () => bonusItemsManager.GetBonus();
        shopButton.clicked += () => shopMenu.Open();
        taskButton.clicked += () => taskMenu.Open();
    }    
}
