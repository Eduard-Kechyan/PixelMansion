using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class ValuesUI : MonoBehaviour
    {
        public Sprite[] levelUpIndicatorSprites = new Sprite[0];

        [HideInInspector]
        public bool loaded = false;

        // Variables
        private bool levelEnabled = true;
        private bool energyEnabled = true;
        private bool goldEnabled = true;
        private bool gemsEnabled = true;

        private bool energyPlusEnabled = true;
        private bool goldPlusEnabled = true;
        private bool gemsPlusEnabled = true;

        private bool enabledSet = false;

        private bool levelBlipping = false;

        private bool isEnergySlashing = false;
        private bool isGoldSlashing = false;
        private bool isGemsSlashing = false;

        // References
        private GameRefs gameRefs;
        private LevelMenu levelMenu;
        private EnergyMenu energyMenu;
        private ShopMenu shopMenu;
        private SafeAreaHandler safeAreaHandler;
        [HideInInspector]
        public EnergyTimer energyTimer;
        private ErrorManager errorManager;

        // Instances
        private GameData gameData;

        // UI
        private VisualElement root;

        private VisualElement valuesBox;

        public Button levelButton;
        private VisualElement levelFill;
        private Label levelValue;
        private VisualElement levelUpIndicator;
        public VisualElement dummyLevelButton;

        public Button energyButton;
        private VisualElement energyPlus;
        private VisualElement energySlash;
        private Label energyTimerLabel;
        public VisualElement dummyEnergyButton;

        public Button goldButton;
        private VisualElement goldPlus;
        private VisualElement goldSlash;
        public VisualElement dummyGoldButton;

        public Button gemsButton;
        private VisualElement gemsPlus;
        private VisualElement gemsSlash;
        public VisualElement dummyGemsButton;

        void Start()
        {
            // Cache
            gameRefs = GameRefs.Instance;
            levelMenu = gameRefs.levelMenu;
            energyMenu = gameRefs.energyMenu;
            shopMenu = gameRefs.shopMenu;
            errorManager = ErrorManager.Instance;

            if (gameRefs.worldUI != null)
            {
                safeAreaHandler = gameRefs.worldUI.GetComponent<SafeAreaHandler>();
            }

            // Cache instances
            gameData = GameData.Instance;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            valuesBox = root.Q<VisualElement>("ValuesBox");

            levelButton = valuesBox.Q<Button>("LevelButton");
            levelFill = levelButton.Q<VisualElement>("Fill");
            levelValue = levelButton.Q<Label>("Value");
            levelUpIndicator = valuesBox.Q<VisualElement>("Indicator");
            dummyLevelButton = valuesBox.Q<VisualElement>("DummyLevelButton");

            energyButton = valuesBox.Q<Button>("EnergyButton");
            energyPlus = energyButton.Q<VisualElement>("Plus");
            energySlash = energyButton.Q<VisualElement>("Slash");
            energyTimerLabel = energyButton.Q<Label>("EnergyTimer");
            dummyEnergyButton = valuesBox.Q<VisualElement>("DummyEnergyButton");

            goldButton = valuesBox.Q<Button>("GoldButton");
            goldPlus = goldButton.Q<VisualElement>("Plus");
            goldSlash = goldButton.Q<VisualElement>("Slash");
            dummyGoldButton = valuesBox.Q<VisualElement>("DummyGoldButton");

            gemsButton = valuesBox.Q<Button>("GemsButton");
            gemsPlus = gemsButton.Q<VisualElement>("Plus");
            gemsSlash = gemsButton.Q<VisualElement>("Slash");
            dummyGemsButton = valuesBox.Q<VisualElement>("DummyGemsButton");

            // Init
            energyTimerLabel.style.display = DisplayStyle.None;

            energySlash.RemoveFromClassList("slashing");
            goldSlash.RemoveFromClassList("slashing");
            gemsSlash.RemoveFromClassList("slashing");

            UpdateValues();

            CheckForTaps();

            root.RegisterCallback<GeometryChangedEvent>(SetLoaded);
        }

        void Update()
        {
            HandleEnergyTimer();
        }

        void SetLoaded(GeometryChangedEvent evt)
        {
            root.UnregisterCallback<GeometryChangedEvent>(SetLoaded);

            loaded = true;
        }

        void CheckForTaps()
        {
            levelButton.clicked += () => levelMenu.Open();
            energyButton.clicked += () => energyMenu.Open();
            goldButton.clicked += () => shopMenu.Open("Gold");
            gemsButton.clicked += () => shopMenu.Open("Gems");
        }

        void HandleEnergyTimer()
        {
            if (energyTimer != null && energyTimer.timerOn)
            {
                if (energyTimer.timeout > 0)
                {
                    energyTimer.timeout -= Time.deltaTime;
                }

                float newTimeout = energyTimer.timeout;

                newTimeout++;

                float minutes = Mathf.FloorToInt(newTimeout / 60);
                float seconds = Mathf.FloorToInt(newTimeout % 60);

                string minutesText = minutes < 10 ? "0" + minutes : minutes.ToString();
                string secondsText = seconds < 10 ? "0" + seconds : seconds.ToString();

                energyTimerLabel.text = string.Format("{0}:{1}", minutesText, secondsText);
            }
        }

        public void ToggleEnergyTimer(bool enable)
        {
            if (enable)
            {
                energyTimerLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                energyTimerLabel.style.display = DisplayStyle.None;
            }
        }

        public void UpdateValues()
        {
            levelFill.style.width = CalcLevelFill();

            levelValue.text = gameData.level.ToString();
            energyButton.text = gameData.energy.ToString("N0");
            goldButton.text = gameData.gold.ToString("N0");
            gemsButton.text = gameData.gems.ToString("N0");

            float energyFontSize = CheckFontSize(energyButton);
            float goldFontSize = CheckFontSize(goldButton);
            float gemsFontSize = CheckFontSize(gemsButton);

            if (energyFontSize != 0)
            {
                energyButton.style.fontSize = energyFontSize;
            }

            if (goldFontSize != 0)
            {
                goldButton.style.fontSize = goldFontSize;
            }

            if (gemsFontSize != 0)
            {
                gemsButton.style.fontSize = gemsFontSize;
            }
        }

        float CheckFontSize(Button button)
        {
            float newFontSize = button.resolvedStyle.fontSize;

            if (button.text.Length > 6)
            {
                newFontSize = 4f;
            }

            if (button.text.Length > 8)
            {
                newFontSize = 3f;
            }

            return newFontSize;
        }

        public void UpdateLevel(Action callback = null)
        {
            levelValue.text = gameData.level.ToString();

            StartCoroutine(UpdateLevelFullAfter(callback));
        }

        IEnumerator UpdateLevelFullAfter(Action callback)
        {
            yield return new WaitForSeconds(0.3f);

            List<TimeValue> durationsZero = new() { new TimeValue(0f, TimeUnit.Second) };

            levelFill.style.transitionDuration = new StyleList<TimeValue>(durationsZero);

            levelFill.style.width = Length.Percent(0);

            List<TimeValue> durationsFull = new() { new TimeValue(0.3f, TimeUnit.Second) };

            levelFill.style.transitionDuration = new StyleList<TimeValue>(durationsFull);

            gameData.CheckExperience();

            callback?.Invoke();

            DataManager.Instance.SaveValue("experience", gameData.experience);

            levelFill.style.width = CalcLevelFill();
        }

        public void ToggleLevelUp(bool canLevelUp)
        {
            if (canLevelUp)
            {
                if (!levelBlipping)
                {
                    levelUpIndicator.style.display = DisplayStyle.Flex;

                    levelBlipping = true;

                    StartCoroutine(BlipLevelUpIndicator());
                }
            }
            else
            {
                if (levelBlipping)
                {
                    levelUpIndicator.style.display = DisplayStyle.None;

                    levelBlipping = false;

                    StopCoroutine(BlipLevelUpIndicator());
                }
            }
        }

        IEnumerator BlipLevelUpIndicator()
        {
            int count = 0;

            while (levelBlipping)
            {
                yield return new WaitForSeconds(0.2f);

                levelUpIndicator.style.backgroundImage = new StyleBackground(levelUpIndicatorSprites[count]);

                if (count == 5)
                {
                    count = 0;
                }
                else
                {
                    count++;
                }
            }
        }

        public Length CalcLevelFill()
        {
            if (gameData == null)
            {
                gameData = GameData.Instance;
            }

            float fillPercent;

            if (gameData.experience < gameData.maxExperience)
            {
                fillPercent = (100f / (float)gameData.maxExperience) * (float)gameData.experience;
            }
            else
            {
                fillPercent = 100f;
            }

            return Length.Percent(fillPercent);
        }

        public void SlashValues(Types.CollGroup type, Action callback = null)
        {
            bool calledBack = false;

            switch (type)
            {
                case Types.CollGroup.Energy:
                    if (!isEnergySlashing)
                    {
                        isEnergySlashing = true;

                        calledBack = true;

                        energySlash.AddToClassList("slashing");

                        Glob.SetTimeout(() =>
                        {
                            isEnergySlashing = false;

                            callback?.Invoke();

                            energySlash.RemoveFromClassList("slashing");
                        }, 0.4f);
                    }
                    break;
                case Types.CollGroup.Gold:
                    if (!isGoldSlashing)
                    {
                        isGoldSlashing = true;

                        calledBack = true;

                        goldSlash.AddToClassList("slashing");

                        Glob.SetTimeout(() =>
                        {
                            isGoldSlashing = false;

                            callback?.Invoke();

                            goldSlash.RemoveFromClassList("slashing");
                        }, 0.4f);
                    }
                    break;
                case Types.CollGroup.Gems:
                    if (!isGemsSlashing)
                    {
                        isGemsSlashing = true;

                        calledBack = true;

                        gemsSlash.AddToClassList("slashing");

                        Glob.SetTimeout(() =>
                        {
                            isGemsSlashing = false;

                            callback?.Invoke();

                            gemsSlash.RemoveFromClassList("slashing");
                        }, 0.4f);
                    }
                    break;
            }

            if (!calledBack)
            {
                callback?.Invoke();
            }
        }

        public void EnableButtons()
        {
            levelButton.SetEnabled(levelEnabled);
            energyButton.SetEnabled(energyEnabled);
            goldButton.SetEnabled(goldEnabled);
            gemsButton.SetEnabled(gemsEnabled);

            if (energyPlusEnabled)
            {
                energyPlus.style.visibility = Visibility.Visible;
                energyPlus.style.opacity = 1f;
            }

            if (goldPlusEnabled)
            {
                goldPlus.style.visibility = Visibility.Visible;
                goldPlus.style.opacity = 1f;
            }

            if (gemsPlusEnabled)
            {
                gemsPlus.style.visibility = Visibility.Visible;
                gemsPlus.style.opacity = 1f;
            }

            enabledSet = false;
        }

        public void DisableButtons()
        {
            if (!enabledSet)
            {
                levelEnabled = levelButton.enabledSelf;
                energyEnabled = energyButton.enabledSelf;
                goldEnabled = goldButton.enabledSelf;
                gemsEnabled = gemsButton.enabledSelf;

                levelButton.SetEnabled(false);
                energyButton.SetEnabled(false);
                goldButton.SetEnabled(false);
                gemsButton.SetEnabled(false);

                energyPlusEnabled = energyPlus.resolvedStyle.visibility == Visibility.Visible;
                goldPlusEnabled = goldPlus.resolvedStyle.visibility == Visibility.Visible;
                gemsPlusEnabled = gemsPlus.resolvedStyle.visibility == Visibility.Visible;

                energyPlus.style.visibility = Visibility.Hidden;
                energyPlus.style.opacity = 0f;
                goldPlus.style.visibility = Visibility.Hidden;
                goldPlus.style.opacity = 0f;
                gemsPlus.style.visibility = Visibility.Hidden;
                gemsPlus.style.opacity = 0f;

                enabledSet = true;
            }
        }

        public void HideButtons()
        {
            if (!PlayerPrefs.HasKey("tutorialFinished"))
            {
                levelButton.style.display = DisplayStyle.None;
                energyButton.style.display = DisplayStyle.None;
                goldButton.style.display = DisplayStyle.None;
                gemsButton.style.display = DisplayStyle.None;

                dummyLevelButton.style.display = DisplayStyle.Flex;
                dummyEnergyButton.style.display = DisplayStyle.Flex;
                dummyGoldButton.style.display = DisplayStyle.Flex;
                dummyGemsButton.style.display = DisplayStyle.Flex;

                energyPlus.style.visibility = Visibility.Hidden;
                energyPlus.style.opacity = 0f;
                goldPlus.style.visibility = Visibility.Hidden;
                goldPlus.style.opacity = 0f;
                gemsPlus.style.visibility = Visibility.Hidden;
                gemsPlus.style.opacity = 0f;
            }
        }

        public void ShowButtons()
        {
            levelButton.style.display = DisplayStyle.Flex;
            energyButton.style.display = DisplayStyle.Flex;
            goldButton.style.display = DisplayStyle.Flex;
            gemsButton.style.display = DisplayStyle.Flex;

            dummyLevelButton.style.display = DisplayStyle.None;
            dummyEnergyButton.style.display = DisplayStyle.None;
            dummyGoldButton.style.display = DisplayStyle.None;
            dummyGemsButton.style.display = DisplayStyle.None;

            energyPlus.style.visibility = Visibility.Visible;
            energyPlus.style.opacity = 1f;
            goldPlus.style.visibility = Visibility.Visible;
            goldPlus.style.opacity = 1f;
            gemsPlus.style.visibility = Visibility.Visible;
            gemsPlus.style.opacity = 1f;
        }

        public void ShowButton(Types.CollGroup collGroup)
        {
            if (!PlayerPrefs.HasKey("tutorialFinished"))
            {
                switch (collGroup)
                {
                    case Types.CollGroup.Experience:
                        levelButton.style.display = DisplayStyle.Flex;
                        dummyLevelButton.style.display = DisplayStyle.None;
                        break;
                    case Types.CollGroup.Energy:
                        energyButton.style.display = DisplayStyle.Flex;
                        dummyEnergyButton.style.display = DisplayStyle.None;
                        break;
                    case Types.CollGroup.Gold:
                        goldButton.style.display = DisplayStyle.Flex;
                        dummyGoldButton.style.display = DisplayStyle.None;
                        break;
                    case Types.CollGroup.Gems:
                        gemsButton.style.display = DisplayStyle.Flex;
                        dummyGemsButton.style.display = DisplayStyle.None;
                        break;
                    default:
                        errorManager.ThrowWarning(Types.ErrorType.Code, GetType().ToString(), "Types.CollGroup " + collGroup + " is not implemented!");
                        break;
                }
            }
        }

        public void SetSortingOrder(int order)
        {
            GetComponent<UIDocument>().sortingOrder = order;
        }

        public void CloseUI()
        {
            List<TimeValue> fullDelay = new() { new TimeValue(0.6f) };

            valuesBox.style.top = -50f;
            valuesBox.style.transitionDelay = fullDelay;
        }

        public void OpenUI()
        {
            List<TimeValue> nullDelay = new() { new TimeValue(0.0f) };

            valuesBox.style.top = safeAreaHandler.topPadding;
            valuesBox.style.transitionDelay = nullDelay;
        }
    }
}