using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using UnityEngine.SceneManagement;

namespace Merge
{
    public class LevelMenu : MonoBehaviour
    {
        // Variables
        public bool gameplayScene = false;
        public float levelButtonOffset = 9f;
        public HubUI hubUI;
        public LevelData levelData;
        [ReadOnly]
        public bool isRewarding = false;

        private Types.ShopItemsContent[] rewardContent = new Types.ShopItemsContent[0];

        private string levelRewardText0;
        private string levelRewardText1;
        private string levelRewardText2;

        // References
        private GameData gameData;
        private ItemHandler itemHandler;
        private SoundManager soundManager;
        private I18n LOCALE;
        private MenuUI menuUI;
        private InfoMenu infoMenu;
        private ValuePop valuePop;
        private ValuesUI valuesUI;
        private UIButtons uiButtons;

        // UI
        private VisualElement root;
        private VisualElement levelMenu;
        private Label levelLabel;
        private Label levelValue;
        private VisualElement levelFill;
        private Label levelRewardsLabel;
        private Button levelUpButton;
        private Label levelUpLabel;

        private Label levelFillLabel;
        private VisualElement levelRewards;
        private VisualElement levelReward0;
        private Button levelRewardButton0;
        private VisualElement levelReward1;
        private Button levelRewardButton1;
        private VisualElement levelReward2;
        private Button levelRewardButton2;

        void Start()
        {
            // Cache
            gameData = GameData.Instance;
            itemHandler = DataManager.Instance.GetComponent<ItemHandler>();
            soundManager = SoundManager.Instance;
            LOCALE = I18n.Instance;
            menuUI = GetComponent<MenuUI>();
            infoMenu = menuUI.GetComponent<InfoMenu>();
            valuePop = GetComponent<ValuePop>();
            valuesUI = GameRefs.Instance.valuesUI;
            uiButtons = gameData.GetComponent<UIButtons>();

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            levelMenu = root.Q<VisualElement>("LevelMenu");

            levelLabel = levelMenu.Q<Label>("LevelLabel");
            levelValue = levelMenu.Q<VisualElement>("LevelIndicator").Q<Label>("Value");
            levelFill = levelMenu.Q<VisualElement>("Fill");
            levelFillLabel = levelMenu.Q<Label>("FillLabel");
            levelUpButton = levelMenu.Q<Button>("LevelUpButton");
            levelUpLabel = levelUpButton.Q<Label>("LevelUpLabel");

            levelRewards = levelMenu.Q<VisualElement>("LevelRewards");
            levelRewardsLabel = levelRewards.Q<Label>("Label");
            levelReward0 = levelRewards.Q<VisualElement>("LevelReward0");
            levelRewardButton0 = levelReward0.Q<Button>("InfoButton");
            levelReward1 = levelRewards.Q<VisualElement>("LevelReward1");
            levelRewardButton1 = levelReward1.Q<Button>("InfoButton");
            levelReward2 = levelRewards.Q<VisualElement>("LevelReward2");
            levelRewardButton2 = levelReward2.Q<Button>("InfoButton");

            levelRewardButton0.clicked += () =>
            {
                ShowInfo(levelRewardText0, 'd');
            };

            levelRewardButton1.clicked += () =>
            {
                ShowInfo(levelRewardText1, 'd');
            };

            levelRewardButton2.clicked += () =>
            {
                ShowInfo(levelRewardText2, 'd');
            };

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
            if (!isRewarding)
            {
                // Set the title
                string title = LOCALE.Get("level_menu_title");

                levelLabel.text = LOCALE.Get("level_menu_label");

                levelRewardsLabel.text = LOCALE.Get("level_menu_rewards_label");

                levelUpLabel.text = LOCALE.Get("level_menu_level_up_button");

                UpdateLevelMenu();

                if (gameData.levelTen)
                {
                    rewardContent = levelData.levelTenRewardContent;
                }
                else
                {
                    rewardContent = levelData.levelRewardContent;
                }

                HandleRewards();

                // Open menu
                menuUI.OpenMenu(levelMenu, title);
            }
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
            levelReward0.style.backgroundImage = new StyleBackground(rewardContent[0].sprite);
            levelReward1.style.backgroundImage = new StyleBackground(rewardContent[1].sprite);

            levelRewardText0 = levelReward0.name;
            levelRewardText1 = levelReward1.name;

            if (rewardContent.Length == 2)
            {
                levelReward2.style.display = DisplayStyle.None;
            }
            else
            {
                levelReward2.style.backgroundImage = new StyleBackground(rewardContent[2].sprite);
                levelRewardText2 = levelReward2.name;
            }
        }

        void ShowInfo(string name, char nameChar)
        {
            int order = int.Parse(name[(name.LastIndexOf(nameChar) + 1)..]);

            infoMenu.Open(itemHandler.CreateItemTemp(rewardContent[order]));
        }

        void UpdateLevel()
        {
            gameData.UpdateLevel();

            for (int i = 0; i < rewardContent.Length; i++)
            {
                bool check = false;

                if (rewardContent.Length - 1 == i)
                {
                    check = true;
                }

                StartCoroutine(PopOutBonus(i * 0.4f, i, check));
            }

            soundManager.PlaySound("LevelUp");

            //UpdateLevelMenu();

            menuUI.CloseMenu(levelMenu.name);

            HandleRewards();
        }

        IEnumerator PopOutBonus(float seconds, int order, bool newCheck = true)
        {
            yield return new WaitForSeconds(seconds);

            bool check = newCheck;

            Item newItem = itemHandler.CreateItemTemp(rewardContent[order]);

            Vector2 initialPosition = new(valuesUI.levelButton.worldBound.x + levelButtonOffset, valuesUI.levelButton.worldBound.y + levelButtonOffset);
            Vector2 buttonPosition;

            if (gameplayScene)
            {
                buttonPosition = uiButtons.gameplayBonusButtonPos;
            }
            else
            {
                buttonPosition = uiButtons.hubPlayButtonPos;

                check = false;
            }

            valuePop.PopBonus(newItem, initialPosition, buttonPosition, check, true);
        }
    }
}