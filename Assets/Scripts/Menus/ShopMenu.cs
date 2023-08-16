using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace Merge
{
    public class ShopMenu : MonoBehaviour
{
    // Variables
    public bool gameplayScene = false;
    public GameplayUI gameplayUI;
    public HubUI hubUI;
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

        // Cache UI
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

        restoreGems = shopMenu.Q<Button>("RestoreGems");
        restoreGold = shopMenu.Q<Button>("RestoreGold");

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

        // Restore
        string restoreText = LOCALE.Get("shop_menu_restore");
        restoreGems.Q<Label>("RestoreLabel").text = restoreText;
        restoreGold.Q<Label>("RestoreLabel").text = restoreText;

        restoreGems.clicked += () => Restore("Gems");
        restoreGold.clicked += () => Restore("Gold");

        InitializeDaily();

        InitializeItems();

        InitializeGems();

        InitializeGold();
    }

    void InitializeDaily()
    {
        Types.ShopItemsContent[] dailyContent = dailyData.dailyContent;

        for (int i = 0; i < dailyContent.Length; i++)
        {
            string nameOrder = i.ToString();

            VisualElement shopBox = dailyBoxes.Q<VisualElement>("DailyBox" + i);
            Label topLabel = shopBox.Q<Label>("TopLabel");
            VisualElement image = shopBox.Q<VisualElement>("Image");
            Button infoButton = shopBox.Q<Button>("InfoButton");
            Button buyButton = shopBox.Q<Button>("BuyButton");
            VisualElement buyButtonValue = buyButton.Q<VisualElement>("Value");
            Label buyButtonLabel = buyButton.Q<Label>("Label");

            // TODO - Add a check for teh items left in the shop
            topLabel.text = dailyContent[i].left + "/" + dailyContent[i].left;

            image.style.backgroundImage = new StyleBackground(dailyContent[i].sprite);

            if (dailyContent[i].price == 0)
            {
                buyButtonValue.style.display = DisplayStyle.None;

                buyButtonLabel.style.width = Length.Percent(100);

                buyButtonLabel.text = LOCALE.Get("shop_menu_free");
            }
            else
            {
                buyButtonValue.style.backgroundImage = new StyleBackground(
                    dailyContent[i].priceType == Types.ShopValuesType.Gold
                        ? smallGoldSprite
                        : smallGemSprite
                );

                buyButtonLabel.text = dailyContent[i].price.ToString();
            }

            infoButton.clicked += () => ShowInfo(nameOrder);

            if (i == 0 && dailyData.dailyItem1 || i == 1 && dailyData.dailyItem2)
            {
                buyButton.SetEnabled(false);
                buyButtonLabel.text = LOCALE.Get("shop_menu_free_gotten");
            }
            else
            {
                buyButton.clicked += () => BuyItem(nameOrder, "Daily");
            }
        }
    }

    void InitializeItems()
    {
        Types.ShopItemsContent[] itemsContent = shopData.itemsContent;

        for (int i = 0; i < itemsContent.Length; i++)
        {
            string nameOrder = i.ToString();

            VisualElement shopBox = itemsBoxes.Q<VisualElement>("ItemBox" + i);
            Label topLabel = shopBox.Q<Label>("TopLabel");
            VisualElement image = shopBox.Q<VisualElement>("Image");
            Button infoButton = shopBox.Q<Button>("InfoButton");
            Button buyButton = shopBox.Q<Button>("BuyButton");
            VisualElement buyButtonValue = buyButton.Q<VisualElement>("Value");
            Label buyButtonLabel = buyButton.Q<Label>("Label");

            // TODO - Add a check for teh items left in the shop
            topLabel.text = itemsContent[i].left + "/" + itemsContent[i].left;

            image.style.backgroundImage = new StyleBackground(itemsContent[i].sprite);

            buyButtonValue.style.backgroundImage = new StyleBackground(
                itemsContent[i].priceType == Types.ShopValuesType.Gold
                    ? smallGoldSprite
                    : smallGemSprite
            );

            buyButtonLabel.text = itemsContent[i].price.ToString();

            infoButton.clicked += () => ShowInfo(nameOrder);

            buyButton.clicked += () => BuyItem(nameOrder);
        }
    }

    void InitializeGems()
    {
        Types.ShopValuesContent[] gemsContent = shopData.gemsContent;

        for (int i = 0; i < gemsContent.Length; i++)
        {
            string nameOrder = i.ToString();

            VisualElement shopBox = gemsBoxes.Q<VisualElement>("GemsBox" + i);
            Label topLabel = shopBox.Q<Label>("TopLabel");
            VisualElement image = shopBox.Q<VisualElement>("Image");
            Button buyButton = shopBox.Q<Button>("BuyButton");
            VisualElement bonus = shopBox.Q<VisualElement>("Bonus");
            Label bonusLabel = bonus.Q<Label>("BonusLabel");
            VisualElement popular = shopBox.Q<VisualElement>("Popular");
            Label popularLabel = popular.Q<Label>("PopularLabel");

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

            topLabel.text = gemsContent[i].amount.ToString();

            image.style.backgroundImage = new StyleBackground(gemsContent[i].sprite);

            buyButton.text = GetPrice(gemsContent[i].price, gemsContent[i].type);

            buyButton.clicked += () => BuyGems(nameOrder);

            // Bonus
            if (gemsContent[i].hasBonus)
            {
                bonusLabel.text = "+" + gemsContent[i].bonusAmount.ToString();
            }
            else
            {
                bonus.style.display = DisplayStyle.None;
            }

            // Popular
            if (gemsContent[i].isPopular)
            {
                popularLabel.text = LOCALE.Get("shop_menu_popular");

                topLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

                if (gemsContent[i].amount > 999)
                {
                    topLabel.style.paddingLeft = 4f;
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
        }
    }

    void InitializeGold()
    {
        Types.ShopValuesContent[] goldContent = shopData.goldContent;

        for (int i = 0; i < goldContent.Length; i++)
        {
            string nameOrder = i.ToString();

            VisualElement shopBox = goldBoxes.Q<VisualElement>("GoldBox" + i);
            Label topLabel = shopBox.Q<Label>("TopLabel");
            VisualElement image = shopBox.Q<VisualElement>("Image");
            Button buyButton = shopBox.Q<Button>("BuyButton");
            VisualElement popular = shopBox.Q<VisualElement>("Popular");
            Label popularLabel = popular.Q<Label>("PopularLabel");

            topLabel.text = goldContent[i].amount.ToString();

            image.style.backgroundImage = new StyleBackground(goldContent[i].sprite);

            buyButton.text = GetPrice(goldContent[i].price, goldContent[i].type);

            buyButton.clicked += () => BuyGold(nameOrder);

            // Popular
            if (goldContent[i].isPopular)
            {
                popularLabel.text = LOCALE.Get("shop_menu_popular");

                topLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

                if (goldContent[i].amount > 999)
                {
                    topLabel.style.paddingLeft = 4f;

                    if (goldContent[i].amount > 9999)
                    {
                        topLabel.style.paddingTop = 1f;
                        topLabel.style.fontSize = 6f;
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
        }
    }

    string GetPrice(float price, Types.ShopValuesType type)
    {
        // NOTE -  The initial given price is in dollars

        return "$" + price;
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

            shopMenu.RegisterCallback<GeometryChangedEvent>(ScroolCallback);
        }
    }

    void ScroolCallback(GeometryChangedEvent evt)
    {
        shopMenu.UnregisterCallback<GeometryChangedEvent>(ScroolCallback);

        switch (scrollLocation)
        {
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
        float scrollContainerHeight = 0;
        float subtitleBottomMargin = 0;

        if (scrollContainer.scrollOffset.y == 0)
        {
            scrollContainerHeight = scrollContainer.resolvedStyle.height / 2;
            subtitleBottomMargin = 20;
        }
        else
        {
            scrollContainerHeight = scrollContainer.resolvedStyle.height;
            subtitleBottomMargin = 10;
        }

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

    void BuyItem(string nameOrder, string type = "Items")
    {
        int order = int.Parse(nameOrder);

        if (shopData.itemsContent[order].price != 0)
        {
            if (shopData.itemsContent[order].priceType == Types.ShopValuesType.Gold)
            {
                if (gameData.gold >= shopData.itemsContent[order].price)
                {
                    gameData.UpdateGold(-shopData.itemsContent[order].price);
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
                    gameData.UpdateGems(-shopData.itemsContent[order].price);
                }
                else
                {
                    string[] notes = new string[] { "note_menu_not_enough_gems_text" };

                    noteMenu.Open("note_menu_not_enough_gems_title", notes);
                }
            }
        }

        if (type == "Daily")
        {
            HandleDailyItem(order);
        }

        if (sceneLoader.sceneName == "Gameplay")
        {
            StartCoroutine(AddItemToBoardOrBonusButton(order, type));
        }
        else
        {
            StartCoroutine(AddItemToPlayButton(order, type));
        }

        menuUI.CloseMenu(shopMenu.name);
    }

    void BuyGems(string nameOrder)
    {
        int order = int.Parse(nameOrder);

        Types.ShopValuesContent shopGem = shopData.gemsContent[order];

        paymentsManager.Purchase(shopGem.price, () =>
        {
            valuePop.PopValue(shopGem.amount, "Gems");

            if (shopGem.hasBonus)
            {
                valuePop.PopValue(shopData.gemsContent[order].bonusAmount, "Energy");
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

        paymentsManager.Purchase(shopGold.price, () =>
        {
            valuePop.PopValue(shopGold.amount, "Gold");

            menuUI.CloseMenu(shopMenu.name);
        }, () =>
        {
            string[] notes = new string[] { "note_menu_purchase_failed_text" };

            noteMenu.Open("note_menu_purchase_failed", notes);
        });
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

    IEnumerator AddItemToBoardOrBonusButton(int order, string type)
    {
        yield return new WaitForSeconds(0.5f);

        // Add item to the board or to the bonus button
        List<Types.BoardEmpty> emptyBoard = boardManager.GetEmptyBoardItems(Vector2Int.zero, false);

        Types.ItemsData boardItem;
        Item newItem;

        if (type == "Items")
        {
            boardItem = new Types.ItemsData
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
            boardItem = new Types.ItemsData
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

            boardManager.CreateItemOnEmptyTile(boardItem, emptyBoard[0], Vector2.zero, false);
        }
        else
        {
            Vector2 buttonPosition;

            if (gameplayScene)
            {
                buttonPosition = gameplayUI.bonusButtonPosition;
            }
            else
            {
                buttonPosition = hubUI.playButtonPosition;
            }

            valuePop.PopBonus(newItem, buttonPosition, true, true);
        }
    }

    IEnumerator AddItemToPlayButton(int order, string type)
    {
        yield return new WaitForSeconds(0.5f);

        Item newItem;

        if (type == "Items")
        {
            newItem = itemHandler.CreateItemTemp(shopData.itemsContent[order]);
        }
        else
        {
            newItem = itemHandler.CreateItemTemp(shopData.dailyContent[order]);
        }

        valuePop.PopBonus(newItem, hubUI.playButtonPosition, false, true);
    }
}
}