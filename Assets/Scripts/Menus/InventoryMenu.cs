using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class InventoryMenu : MonoBehaviour
{
    private Sprite slotSprite;

    private MenuManager menuManager;
    private GameData gameData;

    private VisualElement root;

    private VisualElement inventoryMenu;
    private VisualElement container;

    private Label amountLabel;
    private Label descriptionLabel;

    private VisualElement buyMoreContainer;
    private Button buyMoreButton;
    private Label buyMoreLabel;

    private I18n LOCALE = I18n.Instance;

    void Start()
    {
        // Cache
        menuManager = GetComponent<MenuManager>();

        gameData = DataManager.Instance.GetComponent<GameData>();

        // Cache UI
        root = menuManager.menuUI.rootVisualElement;

        inventoryMenu = root.Q<VisualElement>("InventoryMenu");
        container = root.Q<VisualElement>("Container");

        amountLabel = inventoryMenu.Q<Label>("AmountLabel");
        descriptionLabel = inventoryMenu.Q<Label>("DescriptionLabel");

        buyMoreContainer = inventoryMenu.Q<VisualElement>("BuyMoreContainer");
        buyMoreLabel = buyMoreContainer.Q<Label>("BuyMoreLabel");
        buyMoreButton = buyMoreContainer.Q<Button>("BuyMoreButton");

        InitializeMenu();
    }

    void InitializeMenu()
    {
        inventoryMenu.style.display = DisplayStyle.None;
    }

    public void Open()
    {
        ClearData();

        // Unlocked items
        SetMenuUI();

        string title = LOCALE.Get("menu_inventory_title");

        // Open menu
        menuManager.OpenMenu(inventoryMenu, title, true);
    }

    void SetMenuUI()
    {
        // Amount
        amountLabel.text = 00 + "/" + 00; // TODO - Change the first 00 to the number of slots used and set the second 00 to the total number of slots

        // Description
        descriptionLabel.text = LOCALE.Get("menu_inventory_description");

        // Buy more
        buyMoreContainer.style.display = DisplayStyle.Flex; // TODO -  Add a check here

        buyMoreButton.text = "00"; // TODO - Add cost of the new slot

        buyMoreLabel.text = LOCALE.Get("menu_inventory_buy_more");
    }

    void ClearData()
    {
        if (container.childCount > 0)
        {
            container.Clear();
        }
    }
}
