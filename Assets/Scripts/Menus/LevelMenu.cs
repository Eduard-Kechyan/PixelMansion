using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class LevelMenu : MonoBehaviour
{
    public ShopData shopData;

    private MenuManager menuManager;
    private InfoMenu infoMenu;
    private GameData gameData;
    private Values values;
    private ItemHandler itemHandler;
    private BonusManager bonusManager;
    private ValuePop valuePop;

    private VisualElement root;
    private VisualElement levelMenu;
    private Label levelLabel;
    private Label levelValue;
    private VisualElement levelFill;
    private Label levelFillLabel;
    private VisualElement levelRewards;
    private Label levelRewardsLabel;
    private Button levelUpButton;
    private Label levelUpLabel;

    private I18n LOCALE = I18n.Instance;

    void Start()
    {
        // Cache
        menuManager = GetComponent<MenuManager>();
        infoMenu = menuManager.GetComponent<InfoMenu>();
        valuePop = GetComponent<ValuePop>();
        gameData = GameData.Instance;
        values = DataManager.Instance.GetComponent<Values>();
        itemHandler = DataManager.Instance.GetComponent<ItemHandler>();

        // Cache UI
        root = menuManager.menuUI.rootVisualElement;

        levelMenu = root.Q<VisualElement>("LevelMenu");

        levelLabel = levelMenu.Q<Label>("LevelLabel");
        levelValue = levelMenu.Q<VisualElement>("LevelIndicator").Q<Label>("Value");
        levelFill = levelMenu.Q<VisualElement>("Fill");
        levelFillLabel = levelMenu.Q<Label>("FillLabel");
        levelRewards = levelMenu.Q<VisualElement>("LevelRewards");
        levelRewardsLabel = levelRewards.Q<Label>("Label");
        levelUpButton = levelMenu.Q<Button>("LevelUpButton");
        levelUpLabel = levelUpButton.Q<Label>("LevelUpLabel");

        levelUpButton.clicked += () => UpdateLevel();
    }

    public void InitializeLevelMenuCache()
    {
        bonusManager = GameObject.Find("GamePlayUI").GetComponent<BonusManager>();
    }

    public void Open()
    {
        // Set the title
        string title = LOCALE.Get("level_menu_title");

        levelLabel.text = LOCALE.Get("level_menu_label");

        levelRewardsLabel.text = LOCALE.Get("level_menu_rewards_label");

        levelUpLabel.text = LOCALE.Get("level_menu_level_up_button");

        UpdateLevelMenu();

        HandleRewards();

        // Open menu
        menuManager.OpenMenu(levelMenu, title);
    }

    public void UpdateLevelMenu()
    {
        levelValue.text = gameData.level.ToString();

        levelFill.style.width = values.CalcLevelFill();

        levelFillLabel.text = gameData.experience + "/" + gameData.maxExperience;

        levelUpButton.SetEnabled(gameData.canLevelUp);
    }

    void HandleRewards()
    {
        Types.ShopItemsContent[] rewardContent = shopData.levelRewardContent;

        for (int i = 0; i < rewardContent.Length; i++)
        {
            VisualElement levelRewardBox = levelRewards.Q<VisualElement>("LevelReward" + i);
            Button infoButton = levelRewardBox.Q<Button>("InfoButton");

            levelRewardBox.style.backgroundImage = new StyleBackground(rewardContent[i].sprite);

            infoButton.clicked += () => ShowInfo(levelRewardBox.name, 'd');
        }
    }

    void ShowInfo(string name, char nameChar)
    {
        int order = int.Parse(name[(name.LastIndexOf(nameChar) + 1)..]);

        Types.ShopItemsContent rewardContent = shopData.levelRewardContent[order];

        infoMenu.Open(
            itemHandler.CreateItemTemp(
                rewardContent.group,
                rewardContent.type,
                rewardContent.sprite.name
            )
        );
    }

    void UpdateLevel()
    {
        gameData.UpdateLevel();

        for (int i = 0; i < shopData.levelRewardContent.Length; i++)
        {
            bool check = false;

            if (shopData.levelRewardContent.Length - 1 == i)
            {
                check = true;
            }

            StartCoroutine(PopOutBonus(i * 0.2f, i, check));
        }

        menuManager.CloseMenu(levelMenu.name);
    }

    IEnumerator PopOutBonus(float seconds, int order, bool check = true)
    {
        yield return new WaitForSeconds(seconds);

        Item newItem = itemHandler.CreateItemTemp(
            shopData.levelRewardContent[order].group,
            shopData.levelRewardContent[order].type,
            shopData.levelRewardContent[order].sprite.name
        );

        valuePop.PopBonus(newItem, bonusManager.bonusButtonPosition, check);
    }
}
