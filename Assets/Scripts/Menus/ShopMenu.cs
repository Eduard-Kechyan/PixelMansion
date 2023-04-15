using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class ShopMenu : MonoBehaviour
{
    public ShopData shopData;
    public Sprite smallGoldSprite;
    public Sprite smallGemSprite;
    public bool purchaseFailed = false;

    private MenuManager menuManager;
    private NoteMenu noteMenu;
    private InfoMenu infoMenu;
    private ValuePop valuePop;
    private GameData gameData;
    private ItemHandler itemHandler;

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

    private string scrollLocation;

    private I18n LOCALE = I18n.Instance;

    void Start()
    {
        // Cache
        menuManager = MenuManager.Instance;
        noteMenu = GetComponent<NoteMenu>();
        infoMenu = GetComponent<InfoMenu>();
        valuePop = GetComponent<ValuePop>();
        gameData = GameData.Instance;
        itemHandler = DataManager.Instance.GetComponent<ItemHandler>();

        // Cache UI
        root = menuManager.menuUI.rootVisualElement;

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

        InitializeShopMenu();
    }

    void InitializeShopMenu()
    {
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
        Types.ShopItemsContent[] dailyContent = shopData.dailyContent;

        for (int i = 0; i < dailyContent.Length; i++)
        {
            VisualElement shopBox = dailyBoxes.Q<VisualElement>("DailyBox" + i);
            Label topLabel = shopBox.Q<Label>("TopLabel");
            VisualElement image = shopBox.Q<VisualElement>("Image");
            Button infoButton = shopBox.Q<Button>("InfoButton" + i);
            Button buyButton = shopBox.Q<Button>("BuyButton" + i);
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

            infoButton.clicked += () => ShowInfo(infoButton.name);

            buyButton.clicked += () =>
                BuyItem(buyButton.name, image.resolvedStyle.backgroundImage.sprite.name);
        }
    }

    void InitializeItems()
    {
        Types.ShopItemsContent[] itemsContent = shopData.itemsContent;

        for (int i = 0; i < itemsContent.Length; i++)
        {
            VisualElement shopBox = itemsBoxes.Q<VisualElement>("ItemBox" + i);
            Label topLabel = shopBox.Q<Label>("TopLabel");
            VisualElement image = shopBox.Q<VisualElement>("Image");
            Button infoButton = shopBox.Q<Button>("InfoButton" + i);
            Button buyButton = shopBox.Q<Button>("BuyButton" + i);
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

            infoButton.clicked += () => ShowInfo(infoButton.name);

            buyButton.clicked += () =>
                BuyItem(buyButton.name, image.resolvedStyle.backgroundImage.sprite.name);
        }
    }

    void InitializeGems()
    {
        Types.ShopValuesContent[] gemsContent = shopData.gemsContent;

        for (int i = 0; i < gemsContent.Length; i++)
        {
            VisualElement shopBox = gemsBoxes.Q<VisualElement>("GemsBox" + i);
            Label topLabel = shopBox.Q<Label>("TopLabel");
            VisualElement image = shopBox.Q<VisualElement>("Image");
            Button buyButton = shopBox.Q<Button>("BuyButton" + i);
            VisualElement bonus = shopBox.Q<VisualElement>("Bonus");
            Label bonusLabel = bonus.Q<Label>("BonusLabel");
            VisualElement popular = shopBox.Q<VisualElement>("Popular");
            Label popularLabel = popular.Q<Label>("PopularLabel");

            topLabel.text = gemsContent[i].amount.ToString();

            image.style.backgroundImage = new StyleBackground(gemsContent[i].sprite);

            buyButton.text = GetPrice(gemsContent[i].price, gemsContent[i].type);

            buyButton.clicked += () => BuyGems(buyButton.name);

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
            VisualElement shopBox = goldBoxes.Q<VisualElement>("GoldBox" + i);
            Label topLabel = shopBox.Q<Label>("TopLabel");
            VisualElement image = shopBox.Q<VisualElement>("Image");
            Button buyButton = shopBox.Q<Button>("BuyButton" + i);
            VisualElement popular = shopBox.Q<VisualElement>("Popular");
            Label popularLabel = popular.Q<Label>("PopularLabel");

            topLabel.text = goldContent[i].amount.ToString();

            image.style.backgroundImage = new StyleBackground(goldContent[i].sprite);

            buyButton.text = GetPrice(goldContent[i].price, goldContent[i].type);

            buyButton.clicked += () => BuyGold(buyButton.name);

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
        menuManager.OpenMenu(shopMenu, title, true);

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

    void ShowInfo(string buttonName)
    {
        int order = int.Parse(buttonName[(buttonName.LastIndexOf('n') + 1)..]);

        Types.ShopItemsContent shopItemsContent = shopData.itemsContent[order];
        
        infoMenu.Open(
            itemHandler.CreateItemTemp(
                shopItemsContent.group,
                shopItemsContent.type,
                shopItemsContent.sprite.name
            )
        );
    }

    void BuyItem(string buttonName, string spriteName)
    {
        int order = int.Parse(buttonName[(buttonName.LastIndexOf('n') + 1)..]);

        if (shopData.itemsContent[order].price != 0)
        {
            if (shopData.itemsContent[order].priceType == Types.ShopValuesType.Gold)
            {
                gameData.UpdateGold(-shopData.itemsContent[order].price);
            }
            else
            {
                gameData.UpdateGems(-shopData.itemsContent[order].price);
            }
        }

        // TODO - Add item to the board or to the bonus button
        Debug.Log("Bought Item: " + spriteName);

        menuManager.CloseMenu(shopMenu.name);
    }

    void BuyGems(string buttonName)
    {
        int order = int.Parse(buttonName[(buttonName.LastIndexOf('n') + 1)..]);

        Types.ShopValuesContent shopGem = shopData.gemsContent[order];

        // shopGem.price;

        // TODO - Check if successfuly bought
        if (!purchaseFailed) // NOTE
        {
            valuePop.PopValue(shopGem.amount, "Gems");

            if (shopGem.hasBonus)
            {
                valuePop.PopValue(shopData.gemsContent[order].bonusAmount, "Energy");
            }

            menuManager.CloseMenu(shopMenu.name);
        }
        else
        {
            string[] notes = new string[] { "note_menu_purchase_failed_text" };

            noteMenu.Open("note_menu_purchase_failed", notes);
        }
    }

    void BuyGold(string buttonName)
    {
        int order = int.Parse(buttonName[(buttonName.LastIndexOf('n') + 1)..]);

        Types.ShopValuesContent shopGold = shopData.goldContent[order];

        // shopGold.price;

        // TODO - Check if successfuly bought
        if (!purchaseFailed) // NOTE
        {
            valuePop.PopValue(shopGold.amount, "Gold");

            menuManager.CloseMenu(shopMenu.name);
            ;
        }
        else
        {
            string[] notes = new string[] { "note_menu_purchase_failed_text" };

            noteMenu.Open("note_menu_purchase_failed", notes);
        }
    }

    void Restore(string type)
    {
        Debug.Log("Restore " + type);
    }
}
