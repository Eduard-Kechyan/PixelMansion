using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Purchasing;
using System.Linq;

namespace Merge
{
    public class ShopMenu : MonoBehaviour
    {
        // Variables
        public bool mergeScene = false;
        public ShopData shopData;
        public Sprite smallGoldSprite;
        public Sprite smallGemSprite;

        private string scrollLocation;

        // private bool purchasing = false;

        private MenuUI.Menu menuType = MenuUI.Menu.Shop;

        // Classes
        [Serializable]
        public class ShopItemsContent
        {
            public int total;
            public int price;
            public Item.Type type;
            public Item.Group group;
            public Item.GenGroup genGroup;
            public Item.ChestGroup chestGroup;
            public ShopValuesType priceType;
            public Sprite sprite;
        }

        [Serializable]
        public class ShopItemsContentJson
        {
            public int total;
            public int price;
            public string type;
            public string group;
            public string genGroup;
            public string chestGroup;
            public string priceType;
            public string sprite;
        }

        [Serializable]
        public class ShopValuesContent
        {
            public string id;
            public Sprite sprite;
            public bool isPopular;
        }

        // Enums
        public enum ShopValuesType
        {
            Gems,
            Gold
        };

        public enum ShopItemType
        {
            Daily,
            Item,
            Gold,
            Gems
        }

        // References
        private GameData gameData;
        private UIData uiData;
        private DailyData dailyData;
        private ItemHandler itemHandler;
        private I18n LOCALE;
        private MenuUI menuUI;
        private NoteMenu noteMenu;
        private InfoMenu infoMenu;
        private ValuePop valuePop;
        private Services services;
        private PaymentsManager paymentsManager;
        private UIButtons uiButtons;
        private AddressableManager addressableManager;
        private MergeUI mergeUI;
        private BoardManager boardManager;
        private SceneLoader sceneLoader;

        // UI
        private VisualElement content;
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

        void Start()
        {
            // Cache
            gameData = GameData.Instance;
            uiData = gameData.GetComponent<UIData>();
            dailyData = DataManager.Instance.GetComponent<DailyData>();
            LOCALE = I18n.Instance;
            itemHandler = DataManager.Instance.GetComponent<ItemHandler>();
            menuUI = GetComponent<MenuUI>();
            noteMenu = GetComponent<NoteMenu>();
            infoMenu = GetComponent<InfoMenu>();
            valuePop = GetComponent<ValuePop>();
            services = Services.Instance;
            paymentsManager = services.GetComponent<PaymentsManager>();
            uiButtons = gameData.GetComponent<UIButtons>();
            addressableManager = DataManager.Instance.GetComponent<AddressableManager>();
            mergeUI = GameRefs.Instance.mergeUI;
            boardManager = GameRefs.Instance.boardManager;
            sceneLoader = GameRefs.Instance.sceneLoader;

            DataManager.Instance.CheckLoaded(() =>
            {
                // UI
                content = uiData.GetMenuAsset(menuType);

                scrollContainer = content.Q<ScrollView>("ScrollContainer");

                dailySubtitle = content.Q<VisualElement>("DailySubtitle").Q<Label>("Subtitle");
                itemsSubtitle = content.Q<VisualElement>("ItemsSubtitle").Q<Label>("Subtitle");
                gemsSubtitle = content.Q<VisualElement>("GemsSubtitle").Q<Label>("Subtitle");
                goldSubtitle = content.Q<VisualElement>("GoldSubtitle").Q<Label>("Subtitle");

                dailyBoxes = content.Q<VisualElement>("DailyBoxes");
                itemsBoxes = content.Q<VisualElement>("ItemsBoxes");
                gemsBoxes = content.Q<VisualElement>("GemsBoxes");
                goldBoxes = content.Q<VisualElement>("GoldBoxes");

#if UNITY_IOS
            SetRestore();
#endif

                Init();
            });
        }

        void Init()
        {
            InitializeShopItems(ShopItemType.Item);
            InitializeShopCurrency(ShopItemType.Gold);
            InitializeShopCurrency(ShopItemType.Gems);

            StartCoroutine(WaitForDailyContent());
        }

        void SetRestore()
        {
            restoreGems = content.Q<Button>("RestoreGems");
            restoreGold = content.Q<Button>("RestoreGold");

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

            InitializeShopItems(ShopItemType.Daily);
        }

        void InitializeShopItems(ShopItemType shopItemType)
        {
            ShopItemsContent[] shopItems = new ShopItemsContent[0];

            if (shopItemType == ShopItemType.Daily)
            {
                shopItems = dailyData.dailyContent;

                dailyBoxes.Clear();
            }
            else
            {
                shopItems = shopData.itemsContent;

                itemsBoxes.Clear();
            }

            for (int i = 0; i < shopItems.Length; i++)
            {
                // Initialize
                string nameOrder = i.ToString();
                var newShopItemBox = uiData.shopItemBoxPrefab.CloneTree();

                // Shop box
                VisualElement shopBox = newShopItemBox.Q<VisualElement>("ShopItemBox");
                shopBox.name = shopItemType.ToString() + "ShopBox" + i;
                shopBox.AddToClassList("shop_item_box_" + shopItemType.ToString().ToLower());

                // Top label // FIX - Add a check for the items left in the shop
                shopBox.Q<Label>("TopLabel").text = dailyData.GetLeftCount(nameOrder, shopItems[i].total, shopItemType) + "/" + shopItems[i].total;

                // Popular
                shopBox.Q<VisualElement>("Popular").style.display = DisplayStyle.None;

                // Image
                shopBox.Q<VisualElement>("Image").style.backgroundImage = new StyleBackground(shopItems[i].sprite);

                // Bonus
                shopBox.Q<VisualElement>("Bonus").style.display = DisplayStyle.None;

                // Buy button
                Button buyButton = shopBox.Q<Button>("BuyButton");
                VisualElement buyButtonValue = buyButton.Q<VisualElement>("Value");
                Label buyButtonLabel = buyButton.Q<Label>("Label");

                // Check if this is the daily data
                // Also check for button value and label
                if (shopItemType == ShopItemType.Daily && shopItems[i].price == 0)
                {
                    buyButtonValue.style.display = DisplayStyle.None;

                    buyButtonLabel.style.width = Length.Percent(100);

                    buyButtonLabel.text = LOCALE.Get("shop_menu_free");
                }
                else
                {
                    buyButtonValue.style.backgroundImage = new StyleBackground(
                        shopItems[i].priceType == ShopValuesType.Gold
                            ? smallGoldSprite
                            : smallGemSprite
                    );

                    buyButtonLabel.text = shopItems[i].price.ToString();
                }

                // Check if this is the daily data
                // Also check if we already got the free daily item
                if (shopItemType == ShopItemType.Daily && (i == 0 && dailyData.dailyItem1 || i == 1 && dailyData.dailyItem2))
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
                if (shopItems[i].type == Item.Type.Coll)
                {
                    newShopItemBox.Q<Button>("InfoButton").style.display = DisplayStyle.None;
                }
                else
                {
                    newShopItemBox.Q<Button>("InfoButton").clicked += () => ShowInfo(nameOrder);
                }

                // Add to container
                if (shopItemType == ShopItemType.Daily)
                {
                    dailyBoxes.Add(newShopItemBox);
                }
                else
                {
                    itemsBoxes.Add(newShopItemBox);
                }
            }
        }

        void InitializeShopCurrency(ShopItemType shopItemType)
        {
            ShopValuesContent[] shopValues;
            string idCheckValue;

            if (shopItemType == ShopItemType.Gold)
            {
                goldBoxes.Clear();

                shopValues = shopData.goldContent;

                idCheckValue = "gold_";
            }
            else
            {
                gemsBoxes.Clear();

                shopValues = shopData.gemsContent;

                idCheckValue = "gems_";
            }

            ICollection<ProductCatalogItem> products = paymentsManager.catalog.allProducts;

            int count = 0;

            foreach (var product in products)
            {
                if (product.id.StartsWith(idCheckValue))
                {
                    ShopValuesContent shopValue = GetShopItem(shopValues, product.id);

                    if (shopValue == null)
                    {
                        Debug.LogWarning("Shop item " + product.id + " not found in shopValues");
                    }
                    else
                    {
                        ProductCatalogPayout payout = product.Payouts.First();
                        ProductCatalogPayout payoutBonus = GetProductBonus(product.Payouts);

                        // Initialize
                        var newShopItemBox = uiData.shopItemBoxPrefab.CloneTree();

                        // Shop box
                        VisualElement shopBox = newShopItemBox.Q<VisualElement>("ShopItemBox");
                        shopBox.name = shopItemType.ToString() + "ShopBox" + count;
                        shopBox.AddToClassList("shop_item_box_" + shopItemType.ToString().ToLower());

                        // Top label 
                        Label topLabel = newShopItemBox.Q<Label>("TopLabel");
                        topLabel.text = payout.quantity.ToString("N0");

                        // Popular
                        VisualElement popular = newShopItemBox.Q<VisualElement>("Popular");
                        Label popularLabel = popular.Q<Label>("PopularLabel");

                        if (shopValue.isPopular)
                        {
                            popularLabel.text = LOCALE.Get("shop_menu_popular");

                            topLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

                            switch (Settings.Instance.currentLocale)
                            {
                                case I18n.Locale.Armenian:
                                    popularLabel.style.fontSize = 3;
                                    break;
                                case I18n.Locale.Japanese:
                                    popularLabel.style.fontSize = 3;
                                    break;
                                case I18n.Locale.Korean:
                                    popularLabel.style.fontSize = 3;
                                    break;
                                case I18n.Locale.Chinese:
                                    popularLabel.style.fontSize = 3;
                                    break;
                                default:
                                    popularLabel.style.fontSize = 3;

                                    if (Settings.Instance.currentLocale != I18n.Locale.German)
                                    {
                                        popularLabel.style.fontSize = 5;
                                    }
                                    break;
                            }

                            if (payout.quantity > 999)
                            {
                                topLabel.style.paddingLeft = 4f;

                                if (shopItemType == ShopItemType.Gold)
                                {
                                    if (payout.quantity > 9999)
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
                        newShopItemBox.Q<VisualElement>("Image").style.backgroundImage = new StyleBackground(shopValue.sprite);

                        // Bonus
                        if (shopItemType == ShopItemType.Gems && payoutBonus != null)
                        {
                            newShopItemBox.Q<VisualElement>("Bonus").Q<Label>("BonusLabel").text = "+" + payoutBonus.quantity;
                        }
                        else
                        {
                            newShopItemBox.Q<VisualElement>("Bonus").style.display = DisplayStyle.None;
                        }

                        // Buy button
                        Button buyButton = newShopItemBox.Q<Button>("BuyButton");
                        Label buyButtonLabel = buyButton.Q<Label>("Label");
                        buyButton.Q<VisualElement>("Value").style.display = DisplayStyle.None;

                        if (services.iapAvailable)
                        {
                            string price = paymentsManager.GetPrice(product.id);

                            if (price == "")
                            {
                                buyButton.SetEnabled(false);

                                buyButtonLabel.text = LOCALE.Get("error");
                            }
                            else
                            {
                                buyButton.SetEnabled(true);

                                buyButtonLabel.text = price;
                            }
                        }
                        else
                        {
                            buyButton.SetEnabled(false);

                            buyButtonLabel.text = LOCALE.Get("shop_menu_buy_button_loading");
                        }

                        buyButtonLabel.AddToClassList("shop_box_buy_button_label_full");

                        string shopItemId = product.id;

                        buyButton.clicked += () =>
                        {
                            StartCoroutine(PrePurchase(() =>
                            {
                                BuyCurrency(shopItemId);
                            }));
                        };

                        // Info button
                        newShopItemBox.Q<Button>("InfoButton").style.display = DisplayStyle.None;

                        // Add to container
                        if (shopItemType == ShopItemType.Gold)
                        {
                            goldBoxes.Add(newShopItemBox);
                        }
                        else
                        {
                            gemsBoxes.Add(newShopItemBox);
                        }

                        count++;
                    }
                }
            }
        }

        ShopValuesContent GetShopItem(ShopValuesContent[] shopValues, string productId)
        {
            for (int i = 0; i < shopValues.Length; i++)
            {
                if (shopValues[i].id == productId)
                {
                    return shopValues[i];
                }
            }

            return null;
        }

        ProductCatalogPayout GetProductBonus(IList<ProductCatalogPayout> payouts)
        {
            int payoutCount = 0;

            if (payouts.Count > 1)
            {
                foreach (var payoutItem in payouts)
                {
                    if (payoutCount == 1)
                    {
                        return payoutItem;
                    }

                    payoutCount++;
                }
            }

            return null;
        }

        public void Open(string newLocation = "")
        {
            // Check menu
            if (menuUI.IsMenuOpen(menuType))
            {
                // Check if we need to scroll to a specific location
                if (newLocation != "")
                {
                    scrollLocation = newLocation;

                    content.RegisterCallback<GeometryChangedEvent>(ScrollCallback);
                }

                return;
            }

            // Subtitles
            dailySubtitle.text = LOCALE.Get("shop_menu_subtitle_daily");
            itemsSubtitle.text = LOCALE.Get("shop_menu_subtitle_items");
            gemsSubtitle.text = LOCALE.Get("shop_menu_subtitle_gems");
            goldSubtitle.text = LOCALE.Get("shop_menu_subtitle_gold");

            // Open menu
            menuUI.OpenMenu(content, menuType, "");

            // Reset scroll position
            scrollContainer.scrollOffset = Vector2.zero;

            // Check if we need to scroll to a specific location
            if (newLocation != "")
            {
                scrollLocation = newLocation;

                content.RegisterCallback<GeometryChangedEvent>(ScrollCallback);
            }
        }

        void ScrollCallback(GeometryChangedEvent evt)
        {
            content.UnregisterCallback<GeometryChangedEvent>(ScrollCallback);

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

            ShopItemsContent shopItemsContent = shopData.itemsContent[order];

            infoMenu.Open(itemHandler.CreateItemTemp(shopItemsContent));
        }

        void BuyItem(string nameOrder, ShopItemType shopItemType)
        {
            int order = int.Parse(nameOrder);

            if (shopData.itemsContent[order].price != 0)
            {
                if (shopData.itemsContent[order].priceType == ShopValuesType.Gold)
                {
                    if (gameData.gold >= shopData.itemsContent[order].price)
                    {
                        gameData.UpdateValue(-shopData.itemsContent[order].price, Item.CollGroup.Gold, false, true);
                    }
                    else
                    {
                        noteMenu.Open("note_menu_not_enough_gold_title", new List<string>() { "note_menu_not_enough_gold_text" });
                    }
                }
                else
                {
                    if (gameData.gems >= shopData.itemsContent[order].price)
                    {
                        gameData.UpdateValue(-shopData.itemsContent[order].price, Item.CollGroup.Gems, false, true);
                    }
                    else
                    {
                        noteMenu.Open("note_menu_not_enough_gems_title", new List<string>() { "note_menu_not_enough_gems_text" });
                    }
                }
            }

            if (shopItemType == ShopItemType.Daily)
            {
                HandleDailyItem(order);
            }

            if (sceneLoader.scene == SceneLoader.SceneType.Merge)
            {
                StartCoroutine(AddItemToBoardOrBonusButton(order, shopItemType));
            }
            else
            {
                StartCoroutine(AddItemToPlayButton(order, shopItemType));
            }

            dailyData.SetBoughtItem(nameOrder, shopItemType);

            InitializeShopItems(shopItemType);

            menuUI.CloseMenu(menuType);
        }

        void BuyCurrency(string productId)
        {
            paymentsManager.Purchase(productId, (bool thankPlayer) =>
            {
                StartCoroutine(PostPurchase(() =>
                {
                    if (thankPlayer)
                    {
                        Glob.SetTimeout(() =>
                        {
                            noteMenu.Open("note_menu_purchase_thank_title", new List<string>() { "note_menu_purchase_thank_text_a", "note_menu_purchase_thank_text_b" });
                        }, 0.3f);
                    }
                }));
            }, (string preFix) =>
            {
                StartCoroutine(PostPurchase(() =>
                {
                    noteMenu.Open("note_menu_purchase_failed_title", new List<string>() { "note_menu_purchase_failed_" + preFix });
                }));
            });
        }

        IEnumerator PrePurchase(Action callback)
        {
            menuUI.ShowMenuOverlay();

            // purchasing = true;

            yield return new WaitForSeconds(0.3f);

            callback();
        }

        IEnumerator PostPurchase(Action callback)
        {
            menuUI.HideMenuOverlay();

            yield return new WaitForSeconds(0.3f);

            //  purchasing = false;

            callback();
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

        IEnumerator AddItemToBoardOrBonusButton(int order, ShopItemType shopItemType)
        {
            yield return new WaitForSeconds(0.5f);

            // Add item to the board or to the bonus button
            List<BoardManager.TileEmpty> emptyBoard = boardManager.GetEmptyTileItems(Vector2Int.zero, false);

            BoardManager.ItemData boardItem;
            Item newItem;

            if (shopItemType == ShopItemType.Item)
            {
                boardItem = new BoardManager.ItemData
                {
                    sprite = shopData.itemsContent[order].sprite,
                    type = shopData.itemsContent[order].type,
                    group = shopData.itemsContent[order].group,
                    genGroup = shopData.itemsContent[order].genGroup,
                    chestGroup = shopData.itemsContent[order].chestGroup,
                    collGroup = Item.CollGroup.Experience,
                };

                newItem = itemHandler.CreateItemTemp(shopData.itemsContent[order]);
            }
            else
            {
                boardItem = new BoardManager.ItemData
                {
                    sprite = shopData.dailyContent[order].sprite,
                    type = shopData.dailyContent[order].type,
                    group = shopData.dailyContent[order].group,
                    genGroup = shopData.dailyContent[order].genGroup,
                    chestGroup = shopData.dailyContent[order].chestGroup,
                    collGroup = Item.CollGroup.Experience,
                };

                newItem = itemHandler.CreateItemTemp(shopData.dailyContent[order]);
            }

            // Check if the board is full
            if (emptyBoard.Count > 0)
            {
                emptyBoard.Sort((p1, p2) => p1.distance.CompareTo(p2.distance));

                boardManager.CreateItemOnEmptyTile(boardItem, emptyBoard[0], uiButtons.mergeShopButtonPos);
            }
            else
            {
                valuePop.PopBonus(newItem, uiButtons.mergeShopButtonPos, uiButtons.mergeBonusButtonPos);
            }
        }

        IEnumerator AddItemToPlayButton(int order, ShopItemType shopItemType)
        {
            yield return new WaitForSeconds(0.5f);

            Item newItem;

            if (shopItemType == ShopItemType.Item)
            {
                newItem = itemHandler.CreateItemTemp(shopData.itemsContent[order]);
            }
            else
            {
                newItem = itemHandler.CreateItemTemp(shopData.dailyContent[order]);
            }

            valuePop.PopBonus(newItem, uiButtons.worldShopButtonPos, uiButtons.worldPlayButtonPos);
        }
    }
}