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
        private bool levelEnabled = false;
        private bool energyEnabled = false;
        private bool goldEnabled = false;
        private bool gemsEnabled = false;

        private bool energyPlusEnabled = false;
        private bool goldPlusEnabled = false;
        private bool gemsPlusEnabled = false;

        private bool enabledSet = false;

        private bool levelBlipping = false;

        private bool isEnergySlashing = false;
        private bool isGoldSlashing = false;
        private bool isGemsSlashing = false;

        // References
        private LevelMenu levelMenu;
        private EnergyMenu energyMenu;
        private ShopMenu shopMenu;
        private SafeAreaHandler safeAreaHandler;
        [HideInInspector]
        public EnergyTimer energyTimer;

        // Instances
        private GameData gameData;

        // UI
        private VisualElement root;

        private VisualElement valuesBox;

        public Button levelButton;
        private VisualElement levelFill;
        private Label levelValue;
        private VisualElement levelUpIndicator;

        public Button energyButton;
        private VisualElement energyPlus;
        private VisualElement energySlash;

        private Label energyTimerLabel;

        public Button goldButton;
        private VisualElement goldPlus;
        private VisualElement goldSlash;

        public Button gemsButton;
        private VisualElement gemsPlus;
        private VisualElement gemsSlash;

        void Start()
        {
            // Cache
            levelMenu = GameRefs.Instance.levelMenu;
            energyMenu = GameRefs.Instance.energyMenu;
            shopMenu = GameRefs.Instance.shopMenu;

            if (GameRefs.Instance.worldUI != null)
            {
                safeAreaHandler = GameRefs.Instance.worldUI.GetComponent<SafeAreaHandler>();
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

            energyButton = valuesBox.Q<Button>("EnergyButton");
            energyPlus = energyButton.Q<VisualElement>("Plus");
            energyTimerLabel = energyButton.Q<Label>("EnergyTimer");
            energySlash = energyButton.Q<VisualElement>("Slash");

            energyTimerLabel.style.display = DisplayStyle.None;

            goldButton = valuesBox.Q<Button>("GoldButton");
            goldPlus = goldButton.Q<VisualElement>("Plus");
            goldSlash = goldButton.Q<VisualElement>("Slash");

            gemsButton = valuesBox.Q<Button>("GemsButton");
            gemsPlus = gemsButton.Q<VisualElement>("Plus");
            gemsSlash = gemsButton.Q<VisualElement>("Slash");

            // Init
            energySlash.RemoveFromClassList("slashing");
            goldSlash.RemoveFromClassList("slashing");
            gemsSlash.RemoveFromClassList("slashing");

            SetValues();

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

        void SetValues()
        {
            // FIX - Remove comments and set to false
            //levelButton.SetEnabled(false);
            //energyButton.SetEnabled(false);
            //goldButton.SetEnabled(false);
            //gemsButton.SetEnabled(false);

            // FIX - Add a check for the plusses
            energyPlus.style.display = DisplayStyle.None;
            goldPlus.style.display = DisplayStyle.None;
            gemsPlus.style.display = DisplayStyle.None;

            UpdateValues();
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

        public void SlashValues(Types.CollGroup type)
        {
            switch (type)
            {
                case Types.CollGroup.Energy:
                    if (!isEnergySlashing)
                    {
                        isEnergySlashing = true;

                        energySlash.AddToClassList("slashing");

                        Glob.SetTimeout(() =>
                        {
                            isEnergySlashing = false;

                            energySlash.RemoveFromClassList("slashing");
                        }, 0.4f);
                    }
                    break;
                case Types.CollGroup.Gold:
                    if (!isGoldSlashing)
                    {
                        isGoldSlashing = true;

                        goldSlash.AddToClassList("slashing");

                        Glob.SetTimeout(() =>
                        {
                            isGoldSlashing = false;

                            goldSlash.RemoveFromClassList("slashing");
                        }, 0.4f);
                    }
                    break;
                case Types.CollGroup.Gems:
                    if (!isGemsSlashing)
                    {
                        isGemsSlashing = true;

                        gemsSlash.AddToClassList("slashing");

                        Glob.SetTimeout(() =>
                        {
                            isGemsSlashing = false;

                            gemsSlash.RemoveFromClassList("slashing");
                        }, 0.4f);
                    }
                    break;
            }
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

        public void DisableButtonsAlt()
        {
            /* levelButton.SetEnabled(false);
             energyButton.SetEnabled(false);
             goldButton.SetEnabled(false);
             gemsButton.SetEnabled(false);

             energyPlus.style.visibility = Visibility.Hidden;
             energyPlus.style.opacity = 0f;
             goldPlus.style.visibility = Visibility.Hidden;
             goldPlus.style.opacity = 0f;
             gemsPlus.style.visibility = Visibility.Hidden;
             gemsPlus.style.opacity = 0f;*/
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

        public void EnableButtonsAlt()
        {
            levelButton.SetEnabled(true);
            energyButton.SetEnabled(true);
            goldButton.SetEnabled(true);
            gemsButton.SetEnabled(true);

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

            //  enabledSet = false;
        }

        public void HideButtons()
        {
            /*if (!PlayerPrefs.HasKey("valuesLevelButtonShowing"))
            {
                levelButton.style.display = DisplayStyle.None;
            }

            if (!PlayerPrefs.HasKey("valuesEnergyButtonShowing"))
            {
                energyButton.style.display = DisplayStyle.None;
            }

            if (!PlayerPrefs.HasKey("valuesGoldButtonShowing"))
            {
                goldButton.style.display = DisplayStyle.None;
            }

            if (!PlayerPrefs.HasKey("valuesGemsButtonShowing"))
            {
                gemsButton.style.display = DisplayStyle.None;
            }*/
        }

        public void ShowButtons()
        {
            levelButton.style.display = DisplayStyle.Flex;
            energyButton.style.display = DisplayStyle.Flex;
            goldButton.style.display = DisplayStyle.Flex;
            gemsButton.style.display = DisplayStyle.Flex;
        }

        public void ShowButton(string name)
        {
            switch (name)
            {
                case "level":
                    levelButton.style.display = DisplayStyle.Flex;
                    PlayerPrefs.SetInt("valuesLevelButtonShowing", 1);
                    break;
                case "energy":
                    energyButton.style.display = DisplayStyle.Flex;
                    PlayerPrefs.SetInt("valuesEnergyButtonShowing", 1);
                    break;
                case "gold":
                    goldButton.style.display = DisplayStyle.Flex;
                    PlayerPrefs.SetInt("valuesGoldButtonShowing", 1);
                    break;
                case "gems":
                    gemsButton.style.display = DisplayStyle.Flex;
                    PlayerPrefs.SetInt("valuesGemsButtonShowing", 1);
                    break;
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