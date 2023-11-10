using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO - Kaleidoscope

namespace Merge
{
    public class GameData : MonoBehaviour
    {
        // Variables
        public ValuesData valuesData;
        public float countFPS = 10f;
        public float numberIncreaseDuration = 1f;

        [HideInInspector]
        public const int WIDTH = 7;
        [HideInInspector]
        public const int HEIGHT = 9;
        [HideInInspector]
        public const int ITEM_COUNT = WIDTH * HEIGHT;
        [HideInInspector]
        public const int BOARD_ITEM_WIDTH = 24;
        [HideInInspector]
        public const int MAX_ENERGY = 100;
        [HideInInspector]
        public const float GAME_PIXEL_WIDTH = 180f;
        [HideInInspector]
        public static float GAME_PIXEL_HEIGHT = 0f;

        public const string WEB_ADDRESS = "https://solonodegames.com"; // TODO - Replace WEBSITE with proper website name
        public const string STUDIO_NAME = "solonodegames";
        public const string GAME_TITLE = "Pixel Mansion";
        public const string GAME_SUBTITLE = "Merge Mystery";

        [HideInInspector]
        public string playerName;

        // Values
        [Header("Values")]
        [Tooltip("0% - 100%")]
        public int experience = 0; // 0% - 100%
        public int energy = 100;
        public int gold = 100;
        public int gems = 100;

        [HideInInspector]
        public int energyTemp = 100;

        private bool updatingEnergy = false;
        private bool updatingGold = false;
        private bool updatingGems = false;

        [Header("Level")]
        public int level = 0;
        public bool levelTen;
        public bool canLevelUp = false;

        [Header("Experience")]
        public int maxExperience = 10;
        [ReadOnly]
        public int leftoverExperience = 0;

        // Inventory
        [Header("Inventory")]
        public int inventorySpace = 7;
        public int inventorySlotPrice = 50;
        [HideInInspector]
        public int initialInventorySpace = 7;
        public List<Types.Inventory> inventoryData = new();
        public const int maxInventorySpace = 50;

        // Main data
        [HideInInspector]
        public List<Types.Bonus> bonusData = new();
        [HideInInspector]
        public List<Types.TaskGroup> tasksData = new();
        [HideInInspector]
        public List<Types.FinishedTask> finishedTasks = new();
        [HideInInspector]
        public List<WorldTypes.Area> areasData = new();

        // Board
        [HideInInspector]
        public Types.Board[,] boardData;
        [HideInInspector]
        public string[] unlockedData = new string[0];
        [HideInInspector]
        public string[] unlockedRoomsData = new string[0];
        [HideInInspector]
        public Types.Item[] itemsData;
        [HideInInspector]
        public Types.Item[] collectablesData;
        [HideInInspector]
        public Types.Item[] generatorsData;
        [HideInInspector]
        public Types.Item[] chestsData;
        [HideInInspector]
        public List<Types.CollDownCount> coolDowns = new();

        // Timers
        [HideInInspector]
        public List<Types.Timer> timers;

        [Header("Other")]
        [ReadOnly]
        public string userId = "DUMMY_ID";
        [ReadOnly]
        public bool greeted = false;

        [HideInInspector]
        public bool resourcesLoaded = false;

        // Notifications
        public List<Types.Notification> notifications = new();

        // Sprites
        private Sprite[] itemsSprites;
        private Sprite[] generatorsSprites;
        private Sprite[] collectablesSprites;
        private Sprite[] chestsSprites;
        private Sprite[] taskSprites;

        // Events
        public delegate void EnergyUpdatedEvent(bool addTimer);
        public static event EnergyUpdatedEvent EnergyUpdatedEventAction;

        // References
        private ValuesUI valuesUI;
        private GameplayUI gameplayUI;
        private DataManager dataManager;
        private SoundManager soundManager;

        // Instance
        public static GameData Instance;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void Start()
        {
            // Cache
            dataManager = DataManager.Instance;
            soundManager = SoundManager.Instance;

            canLevelUp = PlayerPrefs.GetInt("canLevelUp") == 1;

            initialInventorySpace = inventorySpace;

            CalcGamePixelHeight();

            LoadSprites();
        }

        public void Init(string sceneName)
        {
            valuesUI = GameRefs.Instance.valuesUI;

            if (sceneName == "Gameplay")
            {
                gameplayUI = GameRefs.Instance.gameplayUI;
            }
        }

        // Load sprites from resources
        public void LoadSprites()
        {
            itemsSprites = Resources.LoadAll<Sprite>("Sprites/Items");
            generatorsSprites = Resources.LoadAll<Sprite>("Sprites/Generators");
            collectablesSprites = Resources.LoadAll<Sprite>("Sprites/Collectables");
            chestsSprites = Resources.LoadAll<Sprite>("Sprites/Chests");
            taskSprites = Resources.LoadAll<Sprite>("Sprites/Tasks");

            resourcesLoaded = true;
        }

        //////// SET ////////

        void CalcGamePixelHeight()
        {
            GAME_PIXEL_HEIGHT = Screen.height / (Screen.width / GAME_PIXEL_WIDTH);
        }

        public void SetExperience(int amount)
        {
            experience = amount;

            if (valuesUI != null)
            {
                valuesUI.UpdateValues();
            }

            CheckExperience();
        }

        public void SetLevel(int amount)
        {
            level = amount;

            SetTenthLevel();

            CalcMaxExperience();

            if (valuesUI != null)
            {
                valuesUI.UpdateValues();
            }
        }

        public void SetEnergy(int amount, bool slash = false)
        {
            energy = amount;
            energyTemp = amount;

            if (valuesUI != null)
            {
                valuesUI.UpdateValues();
            }

            if (slash)
            {
                valuesUI.SlashValues(Types.CollGroup.Energy);
            }

            EnergyUpdatedEventAction?.Invoke(true);
        }

        public void SetGold(int amount, bool slash = false)
        {
            gold = amount;

            if (valuesUI != null)
            {
                valuesUI.UpdateValues();
            }

            if (slash)
            {
                valuesUI.SlashValues(Types.CollGroup.Gold);
            }
        }

        public void SetGems(int amount, bool slash = false)
        {
            gems = amount;

            if (valuesUI != null)
            {
                valuesUI.UpdateValues();
            }

            if (slash)
            {
                valuesUI.SlashValues(Types.CollGroup.Gems);
            }
        }

        //////// UPDATE ////////

        void UpdateExperience(int amount)
        {
            experience = amount;

            CheckExperience(true);

            valuesUI.UpdateValues();

            dataManager.writer.Write("experience", experience).Commit();
        }

        public void UpdateLevel(Action callback = null)
        {
            level++;

            experience -= maxExperience;

            SetTenthLevel();

            dataManager.writer.Write("level", level).Write("experience", experience).Commit();

            CalcMaxExperience();

            valuesUI.UpdateLevel(callback);

            if (energy < MAX_ENERGY)
            {
                SetEnergy(MAX_ENERGY, true);
            }
        }

        public void CheckExperience(bool playSound = false)
        {
            if (experience >= maxExperience)
            {
                if (playSound && !canLevelUp)
                {
                    soundManager.PlaySound("LevelUpIndicator");
                }
                ToggleCanLevelUpCheck(true);


                if (valuesUI != null)
                {
                    valuesUI.ToggleLevelUp(true);
                }
            }
            else
            {
                ToggleCanLevelUpCheck(false);

                if (valuesUI != null)
                {
                    valuesUI.ToggleLevelUp(false);
                }
            }
        }

        public bool UpdateValue(int amount, Types.CollGroup type, bool useMultiplier = false, bool updateUI = false)
        {
            int tempAmount = CalcNewAmount(amount, type, useMultiplier);

            switch (type)
            {
                case Types.CollGroup.Energy:
                    if (amount < 0 && energy + amount < 0)
                    {
                        return false;
                    }
                    break;
                case Types.CollGroup.Gold:
                    if (amount < 0 && gold + amount < 0)
                    {
                        return false;
                    }
                    break;
                case Types.CollGroup.Gems:
                    if (amount < 0 && gems + amount < 0)
                    {
                        return false;
                    }
                    break;
                default: //Types.CollGroup.Experience
                    UpdateExperience(tempAmount);
                    return false;
            }

            if (type == Types.CollGroup.Energy)
            {
                energyTemp = CalcNewAmount(amount, type, useMultiplier);

                EnergyUpdatedEventAction?.Invoke(true);
            }

            dataManager.writer.Write(type.ToString().ToLower(), tempAmount).Commit();

            if (updateUI)
            {
                UpdateValueUI(amount, type, useMultiplier);
            }

            return true;
        }

        public void UpdateValueUI(int amount, Types.CollGroup type, bool useMultiplier = false)
        {
            int tempAmount = CalcNewAmount(amount, type, useMultiplier);

            bool shouldFlash = amount > 0 ? true : false;

            int currentAmount;

            bool isUpdating;

            switch (type)
            {
                case Types.CollGroup.Energy:
                    currentAmount = energy;
                    isUpdating = updatingEnergy;
                    break;
                case Types.CollGroup.Gold:
                    currentAmount = gold;
                    isUpdating = updatingGold;
                    break;
                case Types.CollGroup.Gems:
                    currentAmount = gems;
                    isUpdating = updatingGems;
                    break;
                default: //Types.CollGroup.Experience
                    Debug.LogWarning("Types.CollGroup.Experience was given to UpdateValueUI!");
                    return;
            }

            if (isUpdating)
            {
                SetUpdating(type, false);

                StopCoroutine(UpdateNumber(type, currentAmount, tempAmount, shouldFlash)); // Remove energy
            }

            if (!isUpdating)
            {
                SetUpdating(type, true);

                StartCoroutine(UpdateNumber(type, currentAmount, tempAmount, shouldFlash));
            }
        }

        int CalcNewAmount(int amount, Types.CollGroup type, bool useMultiplier = false)
        {
            switch (type)
            {
                case Types.CollGroup.Energy:
                    if (useMultiplier)
                    {
                        return energy + valuesData.energyMultiplier[amount - 1];
                    }
                    else
                    {
                        return energy + amount;
                    }
                case Types.CollGroup.Gold:
                    if (useMultiplier)
                    {
                        return gold + valuesData.goldMultiplier[amount - 1];
                    }
                    else
                    {
                        return gold + amount;
                    }
                case Types.CollGroup.Gems:
                    if (useMultiplier)
                    {
                        return gems + valuesData.gemsMultiplier[amount - 1];
                    }
                    else
                    {
                        return gems + amount;
                    }
                default: //Types.CollGroup.Experience
                    if (useMultiplier)
                    {
                        return experience + valuesData.experienceMultiplier[amount - 1];
                    }
                    else
                    {
                        return experience + amount;
                    }
            }
        }

        IEnumerator UpdateNumber(Types.CollGroup type, int amount, int newAmount, bool slash)
        {
            int prevAmount = amount;
            int stepAmount;

            if (newAmount - prevAmount < 0)
            {
                stepAmount = Mathf.FloorToInt((newAmount - prevAmount) / (countFPS * numberIncreaseDuration));
            }
            else
            {
                stepAmount = Mathf.CeilToInt((newAmount - prevAmount) / (countFPS * numberIncreaseDuration));
            }

            if (prevAmount < newAmount)
            {
                while (prevAmount < newAmount)
                {
                    prevAmount += stepAmount;

                    if (prevAmount > newAmount)
                    {
                        prevAmount = newAmount;
                    }

                    SetUpdateNumber(type, prevAmount);

                    valuesUI.UpdateValues();

                    yield return new WaitForSeconds(1f / countFPS);
                }

                if (slash)
                {
                    valuesUI.SlashValues(type);
                }

                SetUpdating(type, false);
            }
            else
            {
                while (prevAmount > newAmount)
                {
                    prevAmount += stepAmount;

                    if (prevAmount < newAmount)
                    {
                        prevAmount = newAmount;
                    }

                    SetUpdateNumber(type, prevAmount);

                    valuesUI.UpdateValues();

                    yield return new WaitForSeconds(1f / countFPS);
                }

                if (slash)
                {
                    valuesUI.SlashValues(type);
                }

                SetUpdating(type, false);
            }
        }

        void SetUpdateNumber(Types.CollGroup type, int amount)
        {
            switch (type)
            {
                case Types.CollGroup.Energy:
                    energy = amount;
                    break;
                case Types.CollGroup.Gold:
                    gold = amount;
                    break;
                case Types.CollGroup.Gems:
                    gems = amount;
                    break;
            }
        }

        void SetUpdating(Types.CollGroup type, bool value)
        {
            switch (type)
            {
                case Types.CollGroup.Energy:
                    updatingEnergy = value;
                    break;
                case Types.CollGroup.Gold:
                    updatingGold = value;
                    break;
                case Types.CollGroup.Gems:
                    updatingGems = value;
                    break;
            }
        }

        //////// BONUS ////////

        public void AddToBonus(Item item, bool check = true)
        {
            Types.Bonus newBonus = new()
            {
                sprite = item.sprite,
                type = item.type,
                group = item.group,
                genGroup = item.genGroup,
                chestGroup = item.chestGroup
            };

            bonusData.Add(newBonus);

            if (check)
            {
                gameplayUI.CheckBonusButton();
            }

            dataManager.SaveBonus();
        }

        public Types.Bonus GetAndRemoveLatestBonus()
        {
            int latestIndex = bonusData.Count - 1;

            Types.Bonus newBonus = bonusData[latestIndex];

            bonusData.RemoveAt(latestIndex);

            gameplayUI.CheckBonusButton();

            return newBonus;
        }

        //////// OTHER ////////

        void ToggleCanLevelUpCheck(bool canLevelUpCheck)
        {
            canLevelUp = canLevelUpCheck;

            PlayerPrefs.SetInt("canLevelUp", canLevelUp ? 1 : 0);
            PlayerPrefs.Save();
        }

        // Get Sprite from sprite name
        public Sprite GetSprite(string name, Types.Type type)
        {
            switch (type)
            {
                case Types.Type.Item:
                    if (type == Types.Type.Item)
                    {
                        foreach (Sprite sprite in itemsSprites)
                        {
                            if (sprite.name == name)
                            {
                                return sprite;
                            }
                        }
                    }
                    break;
                case Types.Type.Gen:
                    foreach (Sprite sprite in generatorsSprites)
                    {
                        if (sprite.name == name)
                        {
                            return sprite;
                        }
                    }
                    break;
                case Types.Type.Coll:
                    foreach (Sprite sprite in collectablesSprites)
                    {
                        if (sprite.name == name)
                        {
                            return sprite;
                        }
                    }
                    break;
                case Types.Type.Chest:
                    foreach (Sprite sprite in chestsSprites)
                    {
                        if (sprite.name == name)
                        {
                            return sprite;
                        }
                    }
                    break;
                default:
                    ErrorManager.Instance.Throw(Types.ErrorType.Code, "Wrong type: " + type);
                    break;
            }

            return null;
        }

        public Sprite GetTaskSprite(string name)
        {
            foreach (Sprite sprite in taskSprites)
            {
                if (sprite.name == name)
                {
                    return sprite;
                }
            }

            return null;
        }

        void CalcMaxExperience()
        {
            maxExperience = valuesData.maxExperienceMultiplier[level];
        }

        void SetTenthLevel()
        {

            if (level > 1 && level % 10 == 0)
            {
                levelTen = true;
            }
            else
            {
                levelTen = false;
            }
        }
    }
}