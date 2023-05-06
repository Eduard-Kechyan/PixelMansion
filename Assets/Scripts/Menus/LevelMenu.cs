using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;
using UnityEngine.SceneManagement;

public class LevelMenu : MonoBehaviour
{
    // Variables
    public bool gameplayScene = false;
    public HubUI hubUI;
    public ShopData shopData;

    // References
    private MenuUI menuUI;
    private InfoMenu infoMenu;
    private ValuePop valuePop;
    private ValuesUI valuesUI;
    private GameplayUI gameplayUI;

    // Instances
    private GameData gameData;
    private ItemHandler itemHandler;
    private I18n LOCALE;

    // UI
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

    void Start()
    {
        // Cache
        menuUI = GetComponent<MenuUI>();
        infoMenu = menuUI.GetComponent<InfoMenu>();
        valuePop = GetComponent<ValuePop>();
        valuesUI = GameRefs.Instance.valuesUI;
        gameplayUI = GameRefs.Instance.gameplayUI;

        // Cache instances
        gameData = GameData.Instance;
        itemHandler = DataManager.Instance.GetComponent<ItemHandler>();
        LOCALE = I18n.Instance;

        // Cache UI
        root = GetComponent<UIDocument>().rootVisualElement;

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

        Init();
    }

    void Init()
    {
        // Make sure the menu is closed
        levelMenu.style.display = DisplayStyle.None;
        levelMenu.style.opacity = 0;
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
        menuUI.OpenMenu(levelMenu, title);
    }

    public void UpdateLevelMenu()
    {
        levelValue.text = gameData.level.ToString();

        levelFill.style.width = valuesUI.CalcLevelFill();

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

        infoMenu.Open(itemHandler.CreateItemTemp(rewardContent));
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

        menuUI.CloseMenu(levelMenu.name);
    }

    IEnumerator PopOutBonus(float seconds, int order, bool newCheck = true)
    {
        yield return new WaitForSeconds(seconds);

        bool check = newCheck;

        Item newItem = itemHandler.CreateItemTemp(shopData.levelRewardContent[order]);

        Vector2 buttonPosition;

        if (gameplayScene)
        {
            buttonPosition = gameplayUI.bonusButtonPosition;
        }
        else
        {
            buttonPosition = hubUI.playButtonPosition;

            check = false;
        }

        valuePop.PopBonus(newItem, buttonPosition, check, false, false);
    }
}
