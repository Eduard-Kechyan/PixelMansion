using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace Merge
{
    public class InventoryMenu : MonoBehaviour
    {
        // Variables
        public int newSlotPrice = 50;
        public float slotPriceMultiplier = 0.5f;

        private MenuUI.Menu menuType = MenuUI.Menu.Inventory;

        // Enums
        public class Inventory
        {
            public Sprite sprite;
            public Item.Type type;
            public Item.Group group;
            public Item.GenGroup genGroup;
            public Item.ChestGroup chestGroup;
            public string id;
            public bool isCompleted;
            public bool timerOn;
            public DateTime timerAltTime;
            public bool gemPopped;
        }

        public class InventoryJson
        {
            public string sprite;
            public string type;
            public string group;
            public string genGroup;
            public string chestGroup;
            public string id;
            public bool isCompleted;
            public bool timerOn;
            public string timerAltTime;
            public bool gemPopped;
        }

        // References
        private GameRefs gameRefs;
        private MenuUI menuUI;
        private ShopMenu shopMenu;
        private ConfirmMenu confirmMenu;
        private GameData gameData;
        private DataManager dataManager;
        private PopupManager popupManager;
        private BoardSelection boardSelection;
        private ValuePop valuePop;
        private MergeUI mergeUI;
        private SoundManager soundManager;
        private UIButtons uiButtons;
        private TimeManager timeManager;
        private BoardManager boardManager;
        private UIData uiData;
        private I18n LOCALE;

        // UI
        private VisualElement root;
        private VisualElement content;
        private VisualElement slotsContainer;
        private Label amountLabel;
        private Label descriptionLabel;
        private Label pauseLabel;
        private VisualElement buyMoreContainer;
        private Button buyMoreButton;
        private Label buyMoreLabel;

        void Start()
        {
            // Cache
            gameRefs = GameRefs.Instance;
            menuUI = GetComponent<MenuUI>();
            shopMenu = GetComponent<ShopMenu>();
            confirmMenu = GetComponent<ConfirmMenu>();
            gameData = GameData.Instance;
            dataManager = DataManager.Instance;
            popupManager = PopupManager.Instance;
            valuePop = GetComponent<ValuePop>();
            mergeUI = gameRefs.mergeUI;
            soundManager = SoundManager.Instance;
            uiButtons = gameData.GetComponent<UIButtons>();
            timeManager = gameRefs.timeManager;
            boardManager = gameRefs.boardManager;
            boardSelection = gameRefs.boardSelection;
            LOCALE = I18n.Instance;
            uiData = GameData.Instance.GetComponent<UIData>();

            DataManager.Instance.CheckLoaded(() =>
            {

                // UI
                content = uiData.GetMenuAsset(menuType);

                slotsContainer = content.Q<VisualElement>("SlotsContainer");

                amountLabel = content.Q<Label>("AmountLabel");
                descriptionLabel = content.Q<Label>("DescriptionLabel");
                pauseLabel = content.Q<Label>("PauseLabel");

                buyMoreContainer = content.Q<VisualElement>("BuyMoreContainer");
                buyMoreLabel = buyMoreContainer.Q<Label>("BuyMoreLabel");
                buyMoreButton = buyMoreContainer.Q<Button>("BuyMoreButton");

                buyMoreButton.clicked += () => BuyMoreSpace();
            });
        }

        public void Open()
        {
            // Check menu
            if (menuUI.IsMenuOpen(menuType))
            {
                return;
            }

            // Set menu content
            ClearData();

            SetUI();

            // Open menu
            menuUI.OpenMenu(content, menuType, "", true);
        }

        void SetUI()
        {
            // Description
            descriptionLabel.text = LOCALE.Get("menu_inventory_description");

            // Pause
            pauseLabel.text = LOCALE.Get("menu_inventory_pause");

            // Amount
            amountLabel.text = gameData.inventoryData.Count + "/" + gameData.inventorySpace;

            // Container content
            for (int i = 0; i < gameData.inventorySpace; i++)
            {
                string nameOrder = i.ToString();

                VisualElement slot = new() { name = "InventorySlot" + nameOrder };

                slot.AddToClassList("slot");

                if (gameData.inventoryData.Count > 0 && gameData.inventoryData.Count > i)
                {
                    Button slotItem = new() { name = "InventorySlotItem" + nameOrder };

                    slotItem.style.backgroundImage = new StyleBackground(
                        gameData.inventoryData[i].sprite
                    );

                    slotItem.AddToClassList("slot_item");

                    slotItem.clicked += () => AddItemToBoard(nameOrder);

                    if (gameData.inventoryData[i].isCompleted)
                    {
                        VisualElement check = new() { name = "Check" };

                        check.AddToClassList("check");

                        slotItem.Add(check);
                    }

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

            if (gameData.UpdateValue(-amount, Item.CollGroup.Gold, false, true))
            {
                gameData.inventorySpace++;

                ClearData();

                SetUI();

                dataManager.SaveInventory(true);
            }
            else
            {
                confirmMenu.Open("need_gold", () =>
                {
                    confirmMenu.Close();

                    Glob.SetTimeout(() =>
                    {
                        shopMenu.Open("Gold");
                    }, 0.35f);
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
                    newPrice += 10 * (boughtSlots - 1);
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
            List<BoardManager.TileEmpty> emptyBoard = boardManager.GetEmptyTileItems(Vector2Int.zero, true);

            int order = int.Parse(nameOrder);

            VisualElement slot = slotsContainer.Q<VisualElement>("InventorySlot" + nameOrder);

            float halfWidth = slot.worldBound.width / 2;
            Vector2 initialPosition = new(slot.worldBound.position.x + halfWidth, slot.worldBound.position.y + halfWidth);

            if (emptyBoard.Count > 0)
            {
                slot.Clear();

                Inventory inventoryItem = gameData.inventoryData[order];

                // Pop out the item
                valuePop.PopInventoryItem(
                    gameData.inventoryData[order].sprite,
                    initialPosition,
                    uiButtons.mergeBonusButtonPos,
                    () =>
                    {
                        Debug.Log(inventoryItem.id);
                        boardManager.CreateItemOnEmptyTile(null, emptyBoard[0], uiButtons.mergeBonusButtonPos, false, false, inventoryItem, (Vector2 position) =>
                        {
                            if (inventoryItem.timerOn && inventoryItem.type == Item.Type.Gen)
                            {
                                timeManager.ItemTakenOutOfInventoryAfter(position, inventoryItem.id);
                            }
                        });

                        ClearData();

                        SetUI();
                    }
                );

                if (inventoryItem.timerOn && inventoryItem.type == Item.Type.Gen)
                {
                    timeManager.ItemTakenOutOfInventory(inventoryItem.id);
                }

                gameData.inventoryData.RemoveAt(order);

                dataManager.SaveInventory();
            }
            else
            {
                popupManager.Pop(
                    LOCALE.Get("pop_board_full"),
                    initialPosition,
                    SoundManager.SoundType.Buzz
                );
            }
        }

        BoardManager.ItemData GetItemData(Inventory inventoryItem)
        {
            BoardManager.ItemData newItemData = new();

            if (inventoryItem.type == Item.Type.Item)
            {
                for (int i = 0; i < gameData.itemsData.Length; i++)
                {
                    if (gameData.itemsData[i].group == inventoryItem.group)
                    {
                        for (int j = 0; j < gameData.itemsData[i].content.Length; j++)
                        {
                            if (gameData.itemsData[i].content[j].sprite == inventoryItem.sprite)
                            {
                                newItemData = gameData.itemsData[i].content[j];
                            }
                        }
                    }
                }
            }

            if (inventoryItem.type == Item.Type.Gen)
            {
                for (int i = 0; i < gameData.generatorsData.Length; i++)
                {
                    if (gameData.generatorsData[i].group == inventoryItem.group)
                    {
                        for (int j = 0; j < gameData.generatorsData[i].content.Length; j++)
                        {
                            if (gameData.generatorsData[i].content[j].sprite == inventoryItem.sprite)
                            {
                                newItemData = gameData.generatorsData[i].content[j];
                            }
                        }
                    }
                }
            }

            return newItemData;
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
            if (item.type != Item.Type.Coll)
            {
                // Check if the element under the position is the one we need
                if (pickedElement != null && pickedElement.name == "InventoryButton")
                {
                    // Check if there is enough space in the inventory
                    if (gameData.inventoryData.Count < gameData.inventorySpace)
                    {
                        // Add the item to the inventory
                        Inventory newInventoryItem = new()
                        {
                            sprite = item.sprite,
                            type = item.type,
                            group = item.group,
                            genGroup = item.genGroup,
                            chestGroup = item.chestGroup,
                            id = item.id,
                            gemPopped = item.gemPopped,
                        };

                        if (item.type == Item.Type.Gen && item.timerOn)
                        {
                            DateTime altTime = DateTime.UtcNow;

                            newInventoryItem.timerOn = true;
                            newInventoryItem.timerAltTime = altTime;

                            timeManager.ItemPutIntoInventory(item.id, altTime);
                        }

                        gameData.inventoryData.Add(newInventoryItem);

                        dataManager.SaveInventory();

                        soundManager.PlaySound(SoundManager.SoundType.OpenCrate); // FIX SOUND - Set proper sound

                        mergeUI.BlipInventoryIndicator();

                        // Remove and unselect the item
                        item.ScaleToSize(Vector2.zero, scaleSpeed, true);

                        Vector2Int loc = boardManager.GetBoardLocation(0, initialTile);

                        int order = gameData.boardData[loc.x, loc.y].order;

                        gameData.boardData[loc.x, loc.y] = new BoardManager.Tile { order = order };


                        dataManager.SaveBoard();

                        boardSelection.Unselect(BoardSelection.SelectType.None);

                        return true;
                    }
                    else
                    {
                        popupManager.Pop(
                            LOCALE.Get("pop_inventory_full"),
                            item.transform.position,
                            SoundManager.SoundType.Buzz,
                            true
                        );

                        return false;
                    }
                }
            }

            return false;
        }
    }
}