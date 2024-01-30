using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Purchasing;

namespace Merge
{
    public class ShopMenu : MonoBehaviour
    {
        // Variables
        public bool gameplayScene = false;
        public GameplayUI gameplayUI;
        public BoardManager boardManager;
        public SceneLoader sceneLoader;
        public ShopData shopData;
        public Sprite smallGoldSprite;
        public Sprite smallGemSprite;

        private string scrollLocation;

        // References
        private GameData gameData;
        private DailyData dailyData;
        private ItemHandler itemHandler;
        private I18n LOCALE;
        private MenuUI menuUI;
        private NoteMenu noteMenu;
        private InfoMenu infoMenu;
        private ValuePop valuePop;
        private PaymentsManager paymentsManager;
        private UIButtons uiButtons;

        // UI
        private VisualElement root;
        private VisualElement shopMenu;
        private ScrollView scrollContainer;
        private Label dailySubtitle;
        private Label itemsSubtitle;
        private Label gemsSubtitle;
        private Label goldSubtitle;
        private VisualElement dailyBoxes;
        private VisualElement itemsBoxes;
        private VisualElement gemsBoxes;
        private VisualElement goldBoxes;
        private Button restoreGems;
        private Button restoreGold;

        private VisualTreeAsset shopItemBoxPrefab;

        void Start()
        {
            // Cache
            gameData = GameData.Instance;
            dailyData = DataManager.Instance.GetComponent<DailyData>();
            LOCALE = I18n.Instance;
            itemHandler = DataManager.Instance.GetComponent<ItemHandler>();
            menuUI = GetComponent<MenuUI>();
            noteMenu = GetComponent<NoteMenu>();
            infoMenu = GetComponent<InfoMenu>();
            valuePop = GetComponent<ValuePop>();
            paymentsManager = Services.Instance.GetComponent<PaymentsManager>();
            uiButtons = gameData.GetComponent<UIButtons>();

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            shopMenu = root.Q<VisualElement>("ShopMenu");

            scrollContainer = shopMenu.Q<ScrollView>("ScrollContainer");

            dailySubtitle = shopMenu.Q<VisualElement>("DailySubtitle").Q<Label>("Subtitle");
            itemsSubtitle = shopMenu.Q<VisualElement>("ItemsSubtitle").Q<Label>("Subtitle");
            gemsSubtitle = shopMenu.Q<VisualElement>("GemsSubtitle").Q<Label>("Subtitle");
            goldSubtitle = shopMenu.Q<VisualElement>("GoldSubtitle").Q<Label>("Subtitle");

            dailyBoxes = shopMenu.Q<VisualElement>("DailyBoxes");
            itemsBoxes = shopMenu.Q<VisualElement>("ItemsBoxes");
            gemsBoxes = shopMenu.Q<VisualElement>("GemsBoxes");
            goldBoxes = shopMenu.Q<VisualElement>("GoldBoxes");

#if UNITY_IOS
            CheckRestore();
#endif

            shopItemBoxPrefab = Resources.Load<VisualTreeAsset>("Uxml/ShopItemBox");

            Init();
        }

        void Init()
        {
            // Make sure the menu is closed
            shopMenu.style.display = DisplayStyle.None;
            shopMenu.style.opacity = 0;

            // Subtitles
            dailySubtitle.text = LOCALE.Get("shop_menu_subtitle_daily");
            itemsSubtitle.text = LOCALE.Get("shop_menu_subtitle_items");
            gemsSubtitle.text = LOCALE.Get("shop_menu_subtitle_gems");
            goldSubtitle.text = LOCALE.Get("shop_menu_subtitle_gold");

            InitializeShopItems(Types.ShopItemType.Item);
            InitializeShopItems(Types.ShopItemType.Gold);
            InitializeShopItems(Types.ShopItemType.Gems);

            StartCoroutine(WaitForDailyContent());
        }

        void CheckRestore()
        {
            restoreGems = shopMenu.Q<Button>("RestoreGems");
            restoreGold = shopMenu.Q<Button>("RestoreGold");

            string restoreText = LOCALE.Get("shop_menu_restore");

            restoreGems.Q<Label>("RestoreLabel").text = restoreText;
            restoreGold.Q<Label>("RestoreLabel").text = restoreText;

            restoreGems.style.display = DisplayStyle.Flex;
            restoreGold.style.display = DisplayStyle.Flex;

            restoreGems.clicked += () => Restore("Gems");
            restoreGold.clicked += () => Restore("Gold");
        }

        IEnumerator WaitForDailyContent()
        {
            while (!dailyData.dataSet)
            {
                yield return null;
            }

            InitializeShopItems(Types.ShopItemType.Daily);
        }

        void InitializeShopItems(Types.ShopItemType shopItemType)
        {
            Types.ShopItemsContent[] shopItems = new Types.ShopItemsContent[0];
            Types.ShopValuesContent[] shopValues = new Types.ShopValuesContent[0];

            switch (shopItemType)
            {
                case Types.ShopItemType.Daily:
                    shopItems = dailyData.dailyContent;

                    dailyBoxes.Clear();
                    break;
                case Types.ShopItemType.Item:
                    shopItems = shopData.itemsContent;

                    itemsBoxes.Clear();
                    break;
                case Types.ShopItemType.Gold:
                    shopValues = shopData.goldContent;

                    goldBoxes.Clear();
                    break;
                case Types.ShopItemType.Gems:
                    shopValues = shopData.gemsContent;

                    gemsBoxes.Clear();
                    break;
            }

            if (shopItemType == Types.ShopItemType.Daily || shopItemType == Types.ShopItemType.Item)
            {
                for (int i = 0; i < shopItems.Length; i++)
                {
                    // Initialize
                    string nameOrder = i.ToString();
                    var newShopItemBox = shopItemBoxPrefab.CloneTree();

                    // Shop box
                    VisualElement shopBox = newShopItemBox.Q<VisualElement>("ShopItemBox");
                    shopBox.name = shopItemType.ToString() + "ShopBox" + i;
                    shopBox.AddToClassList("shop_item_box_" + shopItemType.ToString().ToLower());

                    // Top label // FIX - Add a check for the items left in the shop
                    newShopItemBox.Q<Label>("TopLabel").text = dailyData.GetLeftCount(nameOrder, shopItems[i].total, shopItemType) + "/" + shopItems[i].total;

                    // Popular
                    newShopItemBox.Q<VisualElement>("Popular").style.display = DisplayStyle.None;

                    // Image
                    newShopItemBox.Q<VisualElement>("Image").style.backgroundImage = new StyleBackground(shopItems[i].sprite);

                    // Bonus
                    newShopItemBox.Q<VisualElement>("Bonus").style.display = DisplayStyle.None;

                    // Buy button
                    Button buyButton = newShopItemBox.Q<Button>("BuyButton");
                    VisualElement buyButtonValue = buyButton.Q<VisualElement>("Value");
                    Label buyButtonLabel = buyButton.Q<Label>("Label");

                    // Check if this is the daily data
                    // Also check for button value and label
                    if (shopItemType == Types.ShopItemType.Daily && shopItems[i].price == 0)
                    {
                        buyButtonValue.style.display = DisplayStyle.None;

                        buyButtonLabel.style.width = Length.Percent(100);

                        buyButtonLabel.text = LOCALE.Get("shop_menu_free");
                    }
                    else
                    {
                        buyButtonValue.style.backgroundImage = new StyleBackground(
                            shopItems[i].priceType == Types.ShopValuesType.Gold
                                ? smallGoldSprite
                                : smallGemSprite
                        );

                        buyButtonLabel.text = shopItems[i].price.ToString();
                    }

                    // Check if this is the daily data
                    // Also check if we already got the free daily item
                    if (shopItemType == Types.ShopItemType.Daily && (i == 0 && dailyData.dailyItem1 || i == 1 && dailyData.dailyItem2))
                    {
                        buyButton.SetEnabled(false);
                        buyButtonLabel.text = LOCALE.Get("shop_menu_free_gotten");
                        buyButtonLabel.AddToClassList("shop_box_buy_button_label_full");
                    }
                    else
                    {
                        buyButton.clicked += () => BuyItem(nameOrder, shopItemType);
                    }

                    // Info button
                    if (shopItems[i].type == Types.Type.Coll)
                    {
                        newShopItemBox.Q<Button>("InfoButton").style.display = DisplayStyle.None;
                    }
                    else
                    {
                        newShopItemBox.Q<Button>("InfoButton").clicked += () => ShowInfo(nameOrder);
                    }

                    // Add to container
                    if (shopItemType == Types.ShopItemType.Daily)
                    {
                        dailyBoxes.Add(newShopItemBox);
                    }
                    else
                    {
                        itemsBoxes.Add(newShopItemBox);
                    }
                }
            }
            else
            {
                for (int i = 0; i < shopValues.Length; i++)
                {
                    // Initialize
                    string nameOrder = i.ToString();
                    var newShopItemBox = shopItemBoxPrefab.CloneTree();

                    // Shop box
                    VisualElement shopBox = newShopItemBox.Q<VisualElement>("ShopItemBox");
                    shopBox.name = shopItemType.ToString() + "ShopBox" + i;
                    shopBox.AddToClassList("shop_item_box_" + shopItemType.ToString().ToLower());

                    // Top label 
                    Label topLabel = newShopItemBox.Q<Label>("TopLabel");
                    topLabel.text = shopValues[i].amount.ToString("N0");

                    // Popular
                    VisualElement popular = newShopItemBox.Q<VisualElement>("Popular");
                    Label popularLabel = popular.Q<Label>("PopularLabel");

                    if (shopValues[i].isPopular)
                    {
                        popularLabel.text = LOCALE.Get("shop_menu_popular");

                        topLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

                        switch (Settings.Instance.currentLocale)
                        {
                            case Types.Locale.Armenian:
                                popularLabel.style.fontSize = 3;
                                break;
                            case Types.Locale.Japanese:
                                popularLabel.style.fontSize = 3;
                                break;
                            case Types.Locale.Korean:
                                popularLabel.style.fontSize = 3;
                                break;
                            case Types.Locale.Chinese:
                                popularLabel.style.fontSize = 3;
                                break;
                            default:
                                popularLabel.style.fontSize = 3;

                                if (Settings.Instance.currentLocale != Types.Locale.German)
                                {
                                    popularLabel.style.fontSize = 5;
                                }
                                break;
                        }

                        if (shopValues[i].amount > 999)
                        {
                            topLabel.style.paddingLeft = 4f;

                            if (shopItemType == Types.ShopItemType.Gold)
                            {
                                if (shopValues[i].amount > 9999)
                                {
                                    topLabel.style.paddingTop = 1f;
                                    topLabel.style.fontSize = 6f;
                                }
                            }
                        }
                        else
                        {
                            topLabel.style.paddingLeft = 8f;
                        }
                    }
                    else
                    {
                        popular.style.display = DisplayStyle.None;
                    }

                    // Image
                    newShopItemBox.Q<VisualElement>("Image").style.backgroundImage = new StyleBackground(shopValues[i].sprite);

                    // Bonus
                    if (shopItemType == Types.ShopItemType.Gems && shopValues[i].hasBonus)
                    {
                        newShopItemBox.Q<VisualElement>("Bonus").Q<Label>("BonusLabel").text = "+" + shopValues[i].bonusAmount.ToString();
                    }
                    else
                    {
                        newShopItemBox.Q<VisualElement>("Bonus").style.display = DisplayStyle.None;
                    }

                    // Buy button
                    Button buyButton = newShopItemBox.Q<Button>("BuyButton");
                    Label buyButtonLabel = buyButton.Q<Label>("Label");
                    buyButton.Q<VisualElement>("Value").style.display = DisplayStyle.None;

                    buyButtonLabel.text = GetPrice(shopValues[i].id);
                    //buyButtonLabel.text = LOCALE.Get("shop_menu_buy_button_loading");
                    buyButtonLabel.AddToClassList("shop_box_buy_button_label_full");

                    buyButton.clicked += () =>
                    {
                        if (shopItemType == Types.ShopItemType.Gems)
                        {
                            BuyGems(nameOrder);
                        }
                        else
                        {
                            BuyGold(nameOrder);
                        }
                    };

                    // Info button
                    newShopItemBox.Q<Button>("InfoButton").style.display = DisplayStyle.None;

                    // Add to container
                    if (shopItemType == Types.ShopItemType.Gold)
                    {
                        goldBoxes.Add(newShopItemBox);
                    }
                    else
                    {
                        gemsBoxes.Add(newShopItemBox);
                    }
                }
            }
        }

        string GetPrice(string id)
        {
            // FIX - Add a real price here
            // NOTE -  The initial price is in dollars

            return "$" + 4;
        }

        public void Open(string newLocation = "")
        {
            // Title
            string title = LOCALE.Get("shop_menu_title");

            // Open menu
            menuUI.OpenMenu(shopMenu, title, true);

            // Reset scroll position
            scrollContainer.scrollOffset = Vector2.zero;

            // Check if we need to scroll to a specific location
            if (newLocation != "")
            {
                scrollLocation = newLocation;

                shopMenu.RegisterCallback<GeometryChangedEvent>(ScrollCallback);
            }
        }

        void ScrollCallback(GeometryChangedEvent evt)
        {
            shopMenu.UnregisterCallback<GeometryChangedEvent>(ScrollCallback);

            switch (scrollLocation)
            {
                case "Daily":
                    scrollContainer.ScrollTo(dailySubtitle);
                    break;
                case "Items":
                    scrollContainer.ScrollTo(itemsSubtitle);
                    break;
                case "Gems":
                    scrollContainer.ScrollTo(gemsSubtitle);
                    break;
                case "Gold":
                    scrollContainer.ScrollTo(goldSubtitle);
                    break;
                default:
                    Debug.Log("Scroll location not found: " + scrollLocation);
                    break;
            }

            // Calculate the scroll location
            float subtitleHeight = gemsSubtitle.resolvedStyle.height;
            float scrollContainerHeight = scrollContainer.resolvedStyle.height;
            float subtitleBottomMargin = 10;

            float scrollOffset = scrollContainerHeight - (subtitleHeight + subtitleBottomMargin);

            scrollContainer.scrollOffset = new Vector2(
                0,
                scrollContainer.scrollOffset.y + scrollOffset
            );
        }

        void ShowInfo(string nameOrder)
        {
            int order = int.Parse(nameOrder);

            Types.ShopItemsContent shopItemsContent = shopData.itemsContent[order];

            infoMenu.Open(itemHandler.CreateItemTemp(shopItemsContent));
        }

        void BuyItem(string nameOrder, Types.ShopItemType shopItemType)
        {
            int order = int.Parse(nameOrder);

            if (shopData.itemsContent[order].price != 0)
            {
                if (shopData.itemsContent[order].priceType == Types.ShopValuesType.Gold)
                {
                    if (gameData.gold >= shopData.itemsContent[order].price)
                    {
                        gameData.UpdateValue(-shopData.itemsContent[order].price, Types.CollGroup.Gold, false, true);
                    }
                    else
                    {
                        string[] notes = new string[] { "note_menu_not_enough_gold_text" };

                        noteMenu.Open("note_menu_not_enough_gold_title", notes);
                    }
                }
                else
                {
                    if (gameData.gems >= shopData.itemsContent[order].price)
                    {
                        gameData.UpdateValue(-shopData.itemsContent[order].price, Types.CollGroup.Gems, false, true);
                    }
                    else
                    {
                        string[] notes = new string[] { "note_menu_not_enough_gems_text" };

                        noteMenu.Open("note_menu_not_enough_gems_title", notes);
                    }
                }
            }

            if (shopItemType == Types.ShopItemType.Daily)
            {
                HandleDailyItem(order);
            }

            if (sceneLoader.scene == Types.Scene.Gameplay)
            {
                StartCoroutine(AddItemToBoardOrBonusButton(order, shopItemType));
            }
            else
            {
                StartCoroutine(AddItemToPlayButton(order, shopItemType));
            }

            dailyData.SetBoughtItem(nameOrder, shopItemType);

            InitializeShopItems(shopItemType);

            menuUI.CloseMenu(shopMenu.name);
        }

        void BuyGems(string nameOrder)
        {
            int order = int.Parse(nameOrder);

            Types.ShopValuesContent shopGem = shopData.gemsContent[order];

            paymentsManager.Purchase(shopGem.id, () =>
            {
                if (sceneLoader.scene == Types.Scene.Gameplay)
                {
                    valuePop.PopValue(shopGem.amount, Types.CollGroup.Gems, uiButtons.gameplayShopButtonPos);

                    if (shopGem.hasBonus)
                    {
                        valuePop.PopValue(shopData.gemsContent[order].bonusAmount, Types.CollGroup.Energy, uiButtons.gameplayShopButtonPos);
                    }
                }
                else
                {
                    valuePop.PopValue(shopGem.amount, Types.CollGroup.Gems, uiButtons.hubShopButtonPos);

                    if (shopGem.hasBonus)
                    {
                        valuePop.PopValue(shopData.gemsContent[order].bonusAmount, Types.CollGroup.Energy, uiButtons.hubShopButtonPos);
                    }
                }

                menuUI.CloseMenu(shopMenu.name);
            }, () =>
            {
                string[] notes = new string[] { "note_menu_purchase_failed_text" };

                noteMenu.Open("note_menu_purchase_failed", notes);
            });
        }

        void BuyGold(string nameOrder)
        {
            int order = int.Parse(nameOrder);

            Types.ShopValuesContent shopGold = shopData.goldContent[order];

            paymentsManager.Purchase(shopGold.id, () =>
            {
                if (sceneLoader.scene == Types.Scene.Gameplay)
                {
                    valuePop.PopValue(shopGold.amount, Types.CollGroup.Gold, uiButtons.gameplayShopButtonPos);
                }
                else
                {
                    valuePop.PopValue(shopGold.amount, Types.CollGroup.Gold, uiButtons.hubShopButtonPos);
                }

                menuUI.CloseMenu(shopMenu.name);
            }, () =>
            {
                string[] notes = new string[] { "note_menu_purchase_failed_text" };

                noteMenu.Open("note_menu_purchase_failed", notes);
            });
        }

        public void FinalizePurchase()
        {

        }

        void Restore(string type)
        {
            Debug.Log("Restore " + type);
        }

        void HandleDailyItem(int order)
        {
            PlayerPrefs.SetInt("dailyItem" + order, 1);

            if (order == 0)
            {
                dailyData.dailyItem1 = true;
            }
            else
            {
                dailyData.dailyItem2 = true;
            }
        }

        IEnumerator AddItemToBoardOrBonusButton(int order, Types.ShopItemType shopItemType)
        {
            yield return new WaitForSeconds(0.5f);

            // Add item to the board or to the bonus button
            List<Types.BoardEmpty> emptyBoard = boardManager.GetEmptyBoardItems(Vector2Int.zero, false);

            Types.ItemData boardItem;
            Item newItem;

            if (shopItemType == Types.ShopItemType.Item)
            {
                boardItem = new Types.ItemData
                {
                    sprite = shopData.itemsContent[order].sprite,
                    type = shopData.itemsContent[order].type,
                    group = shopData.itemsContent[order].group,
                    genGroup = shopData.itemsContent[order].genGroup,
                    chestGroup = shopData.itemsContent[order].chestGroup,
                    collGroup = Types.CollGroup.Experience,
                };

                newItem = itemHandler.CreateItemTemp(shopData.itemsContent[order]);
            }
            else
            {
                boardItem = new Types.ItemData
                {
                    sprite = shopData.dailyContent[order].sprite,
                    type = shopData.dailyContent[order].type,
                    group = shopData.dailyContent[order].group,
                    genGroup = shopData.dailyContent[order].genGroup,
                    chestGroup = shopData.dailyContent[order].chestGroup,
                    collGroup = Types.CollGroup.Experience,
                };

                newItem = itemHandler.CreateItemTemp(shopData.dailyContent[order]);
            }

            // Check if the board is full
            if (emptyBoard.Count > 0)
            {
                emptyBoard.Sort((p1, p2) => p1.distance.CompareTo(p2.distance));

                boardManager.CreateItemOnEmptyTile(boardItem, emptyBoard[0], uiButtons.gameplayShopButtonPos);
            }
            else
            {
                valuePop.PopBonus(newItem, uiButtons.gameplayShopButtonPos, uiButtons.gameplayBonusButtonPos);
            }
        }

        IEnumerator AddItemToPlayButton(int order, Types.ShopItemType shopItemType)
        {
            yield return new WaitForSeconds(0.5f);

            Item newItem;

            if (shopItemType == Types.ShopItemType.Item)
            {
                newItem = itemHandler.CreateItemTemp(shopData.itemsContent[order]);
            }
            else
            {
                newItem = itemHandler.CreateItemTemp(shopData.dailyContent[order]);
            }

            valuePop.PopBonus(newItem, uiButtons.hubShopButtonPos, uiButtons.hubPlayButtonPos);
        }
    }
}