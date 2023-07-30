using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class InventoryMenu : MonoBehaviour
{
    // Variables
    public BoardManager boardManager;
    public Sprite slotSprite;
    public Color slotItemColor;
    public int newSlotPrice = 50;
    public float slotPriceMultiplier = 0.5f;

    // References
    private MenuUI menuUI;
    private ShopMenu shopMenu;
    private ConfirmMenu confirmMenu;
    private GameData gameData;
    private DataManager dataManager;
    private BoardPopup boardPopup;
    private SelectionManager selectionManager;
    private ValuePop valuePop;
    private GameplayUI gameplayUI;

    // Instances
    private I18n LOCALE;

    // UI
    private VisualElement root;
    private VisualElement inventoryMenu;
    private VisualElement slotsContainer;
    private Label amountLabel;
    private Label descriptionLabel;
    private VisualElement buyMoreContainer;
    private Button buyMoreButton;
    private Label buyMoreLabel;

    void Start()
    {
        // Cache
        menuUI = GetComponent<MenuUI>();
        shopMenu = GetComponent<ShopMenu>();
        confirmMenu = GetComponent<ConfirmMenu>();
        gameData = GameData.Instance;
        dataManager = DataManager.Instance;
        valuePop = GameRefs.Instance.valuePop;
        gameplayUI = GameRefs.Instance.gameplayUI;

        if (boardManager != null)
        {
            boardPopup = boardManager.GetComponent<BoardPopup>();
            selectionManager = boardManager.GetComponent<SelectionManager>();
        }

        // Cache instances
        LOCALE = I18n.Instance;

        // Cache UI
        root = GetComponent<UIDocument>().rootVisualElement;

        inventoryMenu = root.Q<VisualElement>("InventoryMenu");

        slotsContainer = inventoryMenu.Q<VisualElement>("SlotsContainer");

        amountLabel = inventoryMenu.Q<Label>("AmountLabel");
        descriptionLabel = inventoryMenu.Q<Label>("DescriptionLabel");

        buyMoreContainer = inventoryMenu.Q<VisualElement>("BuyMoreContainer");
        buyMoreLabel = buyMoreContainer.Q<Label>("BuyMoreLabel");
        buyMoreButton = buyMoreContainer.Q<Button>("BuyMoreButton");

        buyMoreButton.clicked += () => BuyMoreSpace();

        Init();
    }

    void Init()
    {
        // Make sure the menu is closed
        inventoryMenu.style.display = DisplayStyle.None;
        inventoryMenu.style.opacity = 0;
    }

    public void Open()
    {
        ClearData();

        SetUI();

        string title = LOCALE.Get("menu_inventory_title");

        // Open menu
        menuUI.OpenMenu(inventoryMenu, title, true);
    }

    void SetUI()
    {
        // Amount
        amountLabel.text = gameData.inventoryData.Count + "/" + gameData.inventorySpace;

        // Description
        descriptionLabel.text = LOCALE.Get("menu_inventory_description");

        // Container content
        for (int i = 0; i < gameData.inventorySpace; i++)
        {
            string nameOrder = i.ToString();

            VisualElement slot = new VisualElement { name = "InventorySlot" + nameOrder };

            slot.style.width = 24f;
            slot.style.height = 24f;
            slot.style.backgroundImage = new StyleBackground(slotSprite);
            slot.style.marginLeft = 2f;
            slot.style.marginRight = 2f;
            slot.style.marginBottom = 4f;

            if (gameData.inventoryData.Count > 0 && gameData.inventoryData.Count > i)
            {
                Button slotItem = new Button { name = "InventorySlotItem" + nameOrder };

                slotItem.style.width = 24f;
                slotItem.style.height = 24f;
                slotItem.style.backgroundImage = new StyleBackground(
                    gameData.inventoryData[i].sprite
                );
                slotItem.style.backgroundColor = slotItemColor;
                slotItem.style.borderLeftWidth = 0f;
                slotItem.style.borderRightWidth = 0f;
                slotItem.style.borderTopWidth = 0f;
                slotItem.style.borderBottomWidth = 0f;
                slotItem.style.marginLeft = 0f;
                slotItem.style.marginRight = 0f;
                slotItem.style.marginTop = 0f;
                slotItem.style.marginBottom = 0f;
                slotItem.style.paddingLeft = 0f;
                slotItem.style.paddingRight = 0f;
                slotItem.style.paddingTop = 0f;
                slotItem.style.paddingBottom = 0f;

                slotItem.clicked += () => AddItemToBoard(nameOrder);

                slot.Add(slotItem);
            }

            slotsContainer.Add(slot);
        }

        // Buy more
        if (gameData.inventorySpace < GameData.maxInventorySpace)
        {
            buyMoreContainer.style.display = DisplayStyle.Flex;

            buyMoreButton.text = CalcNewSlotPrice();

            buyMoreLabel.text = LOCALE.Get("menu_inventory_buy_more");
        }
        else
        {
            buyMoreButton.style.display = DisplayStyle.None;

            buyMoreLabel.text = LOCALE.Get("menu_inventory_max_slots");
        }
    }

    void BuyMoreSpace()
    {
        int amount = int.Parse(buyMoreButton.text);

        if (gameData.UpdateGold(-amount))
        {
            gameData.inventorySpace++;

            ClearData();

            SetUI();

            dataManager.SaveInventory(true);
        }
        else
        {
            confirmMenu.Open("need_gold", () => {
                confirmMenu.Close();

                Glob.SetTimout(()=>{
                    shopMenu.Open();
                },0.35f);
            });
        }
    }

    string CalcNewSlotPrice()
    {
        int boughtSlots = gameData.inventorySpace - gameData.initialInventorySpace;

        int newPrice = gameData.inventorySlotPrice;

        if (boughtSlots > 0)
        {
            if (boughtSlots % 2 == 1)
            {
                newPrice += 7 * boughtSlots;
            }
            else
            {
                newPrice += 10 * (boughtSlots-1);
            }

            if (gameData.inventorySlotPrice != newPrice)
            {
                gameData.inventorySlotPrice = newPrice;

                dataManager.SaveInventory(true);
            }
        }

        return newPrice.ToString();
    }

    void AddItemToBoard(string nameOrder)
    {
        List<Types.BoardEmpty> emptyBoard = boardManager.GetEmptyBoardItems(Vector2Int.zero, false);

        if (emptyBoard.Count > 0)
        {
            Button slotItem = slotsContainer.Q<Button>("InventorySlotItem" + nameOrder);

            int order = int.Parse(nameOrder);

            VisualElement slot = slotsContainer.Q<VisualElement>("InventorySlot" + nameOrder);

            // Pop out the item
            valuePop.PopInventoryItem(
                gameData.inventoryData[order].sprite,
                slot.worldBound.position,
                gameplayUI.bonusButtonPosition
            );

            gameData.inventoryData.RemoveAt(order);

            dataManager.SaveInventory();

            ClearData();

            SetUI();

            StartCoroutine(ClearSlot(slot));
        }
    }

    IEnumerator ClearSlot(VisualElement slot)
    {
        yield return new WaitForSeconds(0.1f);

        // Remove the item from the data
        slot.Clear();
    }

    void ClearData()
    {
        if (slotsContainer.childCount > 0)
        {
            slotsContainer.Clear();
        }
    }

    public bool CheckInventoryButton(Item item, GameObject initialTile, float scaleSpeed)
    {
        // Get dragged position on the UI
        Vector2 newUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
            root.panel,
            item.transform.position,
            Camera.main
        );

        var pickedElement = root.panel.Pick(newUIPos);

        // Check if the item's type is the correct one
        if (item.type != Types.Type.Coll)
        {
            // Check if the element under the position is the one we need
            if (pickedElement != null && pickedElement.name == "InventoryButton")
            {
                // Check if there is enough space in the inventory
                if (gameData.inventoryData.Count < gameData.inventorySpace)
                {
                    // Add the item to the inventory
                    Types.Inventory newInventoryItem = new Types.Inventory
                    {
                        sprite = item.sprite,
                        type = item.type,
                        group = item.group,
                        genGroup = item.genGroup,
                    };

                    gameData.inventoryData.Add(newInventoryItem);

                    dataManager.SaveInventory();

                    // Remove and unselect the item
                    item.ScaleToSize(Vector2.zero, scaleSpeed, false);

                    Vector2Int loc = boardManager.GetBoardLocation(0, initialTile);

                    int order = gameData.boardData[loc.x, loc.y].order;

                    gameData.boardData[loc.x, loc.y] = new Types.Board { order = order };

                    selectionManager.Unselect("Other");

                    return true;
                }
                else
                {
                    boardPopup.AddPop(
                        LOCALE.Get("pop_inventory_full"),
                        item.transform.position,
                        true,
                        "Buzz"
                    );
                    return false;
                }
            }
        }

        return false;
    }
}
