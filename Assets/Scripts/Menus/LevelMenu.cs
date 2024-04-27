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
        public bool mergeScene = false;
        public float levelButtonOffset = 9f;
        public LevelData levelData;
        [ReadOnly]
        public bool isRewarding = false;

        private Types.ShopItemsContent[] rewardContent = new Types.ShopItemsContent[0];

        private string levelRewardText0;
        private string levelRewardText1;
        private string levelRewardText2;

        private readonly List<TimeValue> nullTransition = new();
        private readonly List<TimeValue> fullTransition = new();

        private bool isButtonSlashing = false;

        private Scale nullScale = new(new Vector2(0f, 0f));
        private Scale fullScale = new(new Vector2(1f, 1f));

        private Coroutine levelFillTimeout;

        // References
        private GameData gameData;
        private ItemHandler itemHandler;
        private SoundManager soundManager;
        private I18n LOCALE;
        private MenuUI menuUI;
        private InfoMenu infoMenu;
        private ValuePop valuePop;
        private ValuesUI valuesUI;
        private WorldUI worldUI;
        private UIButtons uiButtons;

        // UI
        private VisualElement root;
        private VisualElement levelMenu;
        private Label levelLabel;
        private Label levelValue;
        private VisualElement levelFill;
        private Label levelRewardsLabel;
        private Button levelUpButton;
        private VisualElement levelUpButtonSlash;
        private Label levelUpLabel;
        private Button levelUpDummyButton;
        private Label levelUpDummyLabel;

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
            worldUI = GameRefs.Instance.worldUI;
            uiButtons = gameData.GetComponent<UIButtons>();

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            levelMenu = root.Q<VisualElement>("LevelMenu");

            levelLabel = levelMenu.Q<Label>("LevelLabel");
            levelValue = levelMenu.Q<VisualElement>("LevelIndicator").Q<Label>("Value");
            levelFill = levelMenu.Q<VisualElement>("Fill");
            levelFillLabel = levelMenu.Q<Label>("FillLabel");
            levelUpButton = levelMenu.Q<Button>("LevelUpButton");
            levelUpButtonSlash = levelUpButton.Q<VisualElement>("ButtonSlash");
            levelUpLabel = levelUpButton.Q<Label>("LevelUpLabel");
            levelUpDummyButton = levelMenu.Q<Button>("LevelUpDummyButton");
            levelUpDummyLabel = levelUpDummyButton.Q<Label>("LevelUpLabel");

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

            // Initialize level up button slash
            levelUpButtonSlash.RemoveFromClassList("button_slash_slashing");

            // Set transitions
            nullTransition.Add(new TimeValue(0f, TimeUnit.Second));
            fullTransition.Add(new TimeValue(0.3f, TimeUnit.Second));
        }

        public void Open()
        {
            if (menuUI.IsMenuOpen(levelMenu.name))
            {
                return;
            }

            if (!isRewarding)
            {
                // Set the title
                string title = LOCALE.Get("level_menu_title");

                levelLabel.text = LOCALE.Get("level_menu_label");

                levelRewardsLabel.text = LOCALE.Get("level_menu_rewards_label");

                levelUpLabel.text = LOCALE.Get("level_menu_level_up_button");
                levelUpDummyLabel.text = LOCALE.Get("level_menu_level_up_button");

                UpdateLevelMenu(true);

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
                menuUI.OpenMenu(levelMenu, title, true);
            }
        }

        public void UpdateLevelMenu(bool init = false)
        {
            if (init)
            {
                levelValue.text = gameData.level.ToString();

                levelFill.style.transitionDuration = new StyleList<TimeValue>(fullTransition);
                levelFill.style.width = valuesUI.CalcLevelFill();
            }
            else
            {
                StartCoroutine(BloopLevelValue());

                Glob.StopTimeout(levelFillTimeout);

                levelFill.style.transitionDuration = new StyleList<TimeValue>(nullTransition);
                levelFill.style.width = 0;

                levelFillTimeout = Glob.SetTimeout(() =>
                {
                    levelFill.style.transitionDuration = new StyleList<TimeValue>(fullTransition);
                    levelFill.style.width = valuesUI.CalcLevelFill();
                }, 0.3f);
            }

            levelFillLabel.text = gameData.experience + "/" + gameData.maxExperience;

            if (gameData.canLevelUp)
            {
                levelUpButton.style.display = DisplayStyle.Flex;
                levelUpDummyButton.style.display = DisplayStyle.None;

                if (!isButtonSlashing)
                {
                    isButtonSlashing = true;

                    StartCoroutine(SlashLevelUpButton());
                }
            }
            else
            {
                levelUpButton.style.display = DisplayStyle.None;
                levelUpDummyButton.style.display = DisplayStyle.Flex;

                isButtonSlashing = false;

                StopCoroutine(SlashLevelUpButton());

                levelUpButtonSlash.RemoveFromClassList("button_slash_slashing");
            }
        }

        // Bloop the level value
        IEnumerator BloopLevelValue()
        {
            yield return new WaitForSeconds(0.2f);

            levelValue.AddToClassList("level_value_bloop");

            yield return new WaitForSeconds(0.2f);

            levelValue.text = gameData.level.ToString();

            levelValue.RemoveFromClassList("level_value_bloop");
        }

        IEnumerator SlashLevelUpButton()
        {
            bool slash = true;

            while (isButtonSlashing)
            {
                if (slash)
                {
                    levelUpButtonSlash.AddToClassList("button_slash_slashing");

                    yield return new WaitForSeconds(1f);

                    slash = false;
                }
                else
                {
                    levelUpButtonSlash.RemoveFromClassList("button_slash_slashing");

                    yield return new WaitForSeconds(0.5f);

                    slash = true;
                }
            }
        }

        void HandleRewards(bool newRewards = true)
        {
            // FIX -  Also update the new rewards if "newRewards"

            levelReward0.style.backgroundImage = new StyleBackground(rewardContent[0].sprite);
            levelReward1.style.backgroundImage = new StyleBackground(rewardContent[1].sprite);

            levelRewardText0 = levelReward0.name;
            levelRewardText1 = levelReward1.name;

            if (rewardContent.Length > 2)
            {
                levelReward2.style.backgroundImage = new StyleBackground(rewardContent[2].sprite);
                levelRewardText2 = levelReward2.name;
            }
            else
            {
                levelReward2.style.display = DisplayStyle.None;
            }

            if (newRewards)
            {
                levelReward0.style.scale = new StyleScale(fullScale);
                levelRewardButton0.style.scale = new StyleScale(fullScale);

                levelReward1.style.scale = new StyleScale(fullScale);
                levelRewardButton0.style.scale = new StyleScale(fullScale);

                levelReward2.style.scale = new StyleScale(fullScale);
                levelRewardButton0.style.scale = new StyleScale(fullScale);
            }
        }

        void ShowInfo(string name, char nameChar)
        {
            int order = int.Parse(name[(name.LastIndexOf(nameChar) + 1)..]);

            infoMenu.Open(itemHandler.CreateItemTemp(rewardContent[order]));
        }

        void UpdateLevel()
        {
            levelUpButton.style.display = DisplayStyle.None;
            levelUpDummyButton.style.display = DisplayStyle.Flex;

            gameData.UpdateLevel(() =>
            {
                UpdateLevelMenu();
            });

            for (int i = 0; i < rewardContent.Length; i++)
            {
                bool check = false;

                if (rewardContent.Length - 1 == i)
                {
                    check = true;

                    Glob.SetTimeout(() =>
                    {
                        HandleRewards(true);
                    }, (rewardContent.Length + 1) * 0.4f);
                }

                StartCoroutine(PopOutBonus(i * 0.4f, i, check));
            }

            soundManager.PlaySound(Types.SoundType.LevelUp);
        }

        IEnumerator PopOutBonus(float seconds, int order, bool newCheck = true)
        {
            yield return new WaitForSeconds(seconds);

            bool check = newCheck;

            Item newItem = itemHandler.CreateItemTemp(rewardContent[order]);

            Vector2 initialPosition;
            Vector2 buttonPosition;

            if (order == 0)
            {
                initialPosition = new Vector2(levelReward0.worldBound.x + (GameData.BOARD_ITEM_WIDTH / 2), levelReward0.worldBound.y + (GameData.BOARD_ITEM_WIDTH / 2));

                levelReward0.style.scale = new StyleScale(nullScale);
                levelRewardButton0.style.scale = new StyleScale(nullScale);
            }
            else if (order == 1)
            {
                initialPosition = new Vector2(levelReward1.worldBound.x + (GameData.BOARD_ITEM_WIDTH / 2), levelReward1.worldBound.y + (GameData.BOARD_ITEM_WIDTH / 2));

                levelReward1.style.scale = new StyleScale(nullScale);
                levelRewardButton0.style.scale = new StyleScale(nullScale);
            }
            else
            {
                initialPosition = new Vector2(levelReward2.worldBound.x + (GameData.BOARD_ITEM_WIDTH / 2), levelReward2.worldBound.y + (GameData.BOARD_ITEM_WIDTH / 2));

                levelReward2.style.scale = new StyleScale(nullScale);
                levelRewardButton0.style.scale = new StyleScale(nullScale);
            }

            if (mergeScene)
            {
                buttonPosition = uiButtons.mergeBonusButtonPos;
            }
            else
            {
                buttonPosition = uiButtons.worldPlayButtonPos;

                check = false;
            }

            valuePop.PopBonus(newItem, initialPosition, buttonPosition, true);
        }
    }
}