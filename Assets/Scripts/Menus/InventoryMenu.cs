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

        // References
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

        // Instances
        private I18n LOCALE;

        // UI
        private VisualElement root;
        private VisualElement inventoryMenu;
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
            menuUI = GetComponent<MenuUI>();
            shopMenu = GetComponent<ShopMenu>();
            confirmMenu = GetComponent<ConfirmMenu>();
            gameData = GameData.Instance;
            dataManager = DataManager.Instance;
            popupManager = PopupManager.Instance;
            valuePop = GetComponent<ValuePop>();
            mergeUI = GameRefs.Instance.mergeUI;
            soundManager = SoundManager.Instance;
            uiButtons = gameData.GetComponent<UIButtons>();
            timeManager = GameRefs.Instance.timeManager;
            boardManager = GameRefs.Instance.boardManager;
            boardSelection = GameRefs.Instance.boardSelection;

            // Cache instances
            LOCALE = I18n.Instance;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            inventoryMenu = root.Q<VisualElement>("InventoryMenu");

            slotsContainer = inventoryMenu.Q<VisualElement>("SlotsContainer");

            amountLabel = inventoryMenu.Q<Label>("AmountLabel");
            descriptionLabel = inventoryMenu.Q<Label>("DescriptionLabel");
            pauseLabel = inventoryMenu.Q<Label>("PauseLabel");

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
            if (menuUI.IsMenuOpen(inventoryMenu.name))
            {
                return;
            }

            ClearData();

            SetUI();

            string title = LOCALE.Get("menu_inventory_title");

            // Open menu
            menuUI.OpenMenu(inventoryMenu, title, true);
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

            if (gameData.UpdateValue(-amount, Types.CollGroup.Gold, false, true))
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
            List<Types.TileEmpty> emptyBoard = boardManager.GetEmptyTileItems(Vector2Int.zero, true);

            int order = int.Parse(nameOrder);

            VisualElement slot = slotsContainer.Q<VisualElement>("InventorySlot" + nameOrder);

            float halfWidth = slot.worldBound.width / 2;
            Vector2 initialPosition = new(slot.worldBound.position.x + halfWidth, slot.worldBound.position.y + halfWidth);

            if (emptyBoard.Count > 0)
            {
                slot.Clear();

                Types.Inventory inventoryItem = gameData.inventoryData[order];

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
                            if (inventoryItem.timerOn && inventoryItem.type == Types.Type.Gen)
                            {
                                timeManager.ItemTakenOutOfInventoryAfter(position, inventoryItem.id);
                            }
                        });

                        ClearData();

                        SetUI();
                    }
                );

                if (inventoryItem.timerOn && inventoryItem.type == Types.Type.Gen)
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
                    Types.SoundType.Buzz
                );
            }
        }

        Types.ItemData GetItemData(Types.Inventory inventoryItem)
        {
            Types.ItemData newItemData = new();

            if (inventoryItem.type == Types.Type.Item)
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

            if (inventoryItem.type == Types.Type.Gen)
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
            if (item.type != Types.Type.Coll)
            {
                // Check if the element under the position is the one we need
                if (pickedElement != null && pickedElement.name == "InventoryButton")
                {
                    // Check if there is enough space in the inventory
                    if (gameData.inventoryData.Count < gameData.inventorySpace)
                    {
                        // Add the item to the inventory
                        Types.Inventory newInventoryItem = new()
                        {
                            sprite = item.sprite,
                            type = item.type,
                            group = item.group,
                            genGroup = item.genGroup,
                            chestGroup = item.chestGroup,
                            id = item.id,
                            gemPopped = item.gemPopped,
                        };

                        if (item.type == Types.Type.Gen && item.timerOn)
                        {
                            DateTime altTime = DateTime.UtcNow;

                            newInventoryItem.timerOn = true;
                            newInventoryItem.timerAltTime = altTime;

                            timeManager.ItemPutIntoInventory(item.id, altTime);
                        }

                        gameData.inventoryData.Add(newInventoryItem);

                        dataManager.SaveInventory();

                        soundManager.PlaySound(Types.SoundType.OpenCrate); // FIX SOUND - Set proper sound

                        mergeUI.BlipInventoryIndicator();

                        // Remove and unselect the item
                        item.ScaleToSize(Vector2.zero, scaleSpeed, true);

                        Vector2Int loc = boardManager.GetBoardLocation(0, initialTile);

                        int order = gameData.boardData[loc.x, loc.y].order;

                        gameData.boardData[loc.x, loc.y] = new Types.Tile { order = order };


                        dataManager.SaveBoard();

                        boardSelection.Unselect(Types.SelectType.None);

                        return true;
                    }
                    else
                    {
                        popupManager.Pop(
                            LOCALE.Get("pop_inventory_full"),
                            item.transform.position,
                            Types.SoundType.Buzz,
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