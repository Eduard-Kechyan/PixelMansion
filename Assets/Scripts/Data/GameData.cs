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
        public BuildData buildData;
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

        public const string WEB_ADDRESS = "https://solonodegames.com"; // FIX - Replace WEBSITE with proper website name
        public const string STUDIO_NAME = "solonodegames";

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
        public List<InventoryMenu.Inventory> inventoryData = new();
        public const int maxInventorySpace = 50;

        // Main data
        [HideInInspector]
        public List<BonusManager.Bonus> bonusData = new();
        [HideInInspector]
        public List<TaskManager.TaskGroup> tasksData = new();
        [HideInInspector]
        public List<TaskManager.FinishedTask> finishedTasks = new();
        [HideInInspector]
        public List<WorldDataManager.Area> areasData = new();

        // Board
        [HideInInspector]
        public BoardManager.Tile[,] boardData;
        [HideInInspector]
        public string[] unlockedData = new string[0];
        [HideInInspector]
        public string[] unlockedRoomsData = new string[0];
        [HideInInspector]
        public BoardManager.TypeItem[] itemsData;
        [HideInInspector]
        public BoardManager.TypeItem[] collectablesData;
        [HideInInspector]
        public BoardManager.TypeItem[] generatorsData;
        [HideInInspector]
        public BoardManager.TypeItem[] chestsData;
        [HideInInspector]
        public List<TimeManager.CoolDownCount> coolDowns = new();

        // Timers
        [HideInInspector]
        public List<TimeManager.Timer> timers;

        [Header("Other")]
        [ReadOnly]
        public bool greeted = false;

        [HideInInspector]
        public bool dataLoaded = false;

        // Notifications
        public List<NotificsManager.Notification> notifications = new();

        // Sprites
        private Sprite[] itemSprites;
        private Sprite[] generatorSprites;
        private Sprite[] collectableSprites;
        private Sprite[] chestSprites;
        private Sprite[] taskSprites;

        private Sprite[] furnitureSprites;
        private Sprite[] floorSprites;
        private Sprite[] wallSprites;
        private Sprite[] propsSprites;

        // Other
        public SceneLoader.SceneType lastScene = SceneLoader.SceneType.None;

        [HideInInspector]
        public string termsHtml = "";
        [HideInInspector]
        public string privacyHtml = "";
        [HideInInspector]
        public bool gettingLegalData = false; // TODO - Set to true

        // Debug
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        public List<Logs.LogData> logsData = new();
#endif

        // Events
        public delegate void EnergyUpdatedEvent(bool addTimer);
        public static event EnergyUpdatedEvent EnergyUpdatedEventAction;

        // References
        private ValuesUI valuesUI;
        private MergeUI mergeUI;
        private DataManager dataManager;
        private SoundManager soundManager;
        private CloudSave cloudSave;
        private AddressableManager addressableManager;

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
            cloudSave = Services.Instance.GetComponent<CloudSave>();
            addressableManager = dataManager.GetComponent<AddressableManager>();

            canLevelUp = PlayerPrefs.GetInt("canLevelUp") == 1;

            initialInventorySpace = inventorySpace;

            CalcGamePixelHeight();

            LoadGameData();
        }

        public void Init(SceneLoader.SceneType scene)
        {
            valuesUI = GameRefs.Instance.valuesUI;

            if (scene == SceneLoader.SceneType.Merge)
            {
                mergeUI = GameRefs.Instance.mergeUI;
            }
        }

        async void LoadGameData()
        {
            // Items
            itemSprites = await addressableManager.LoadAssetAllArrayAsync<Sprite>("items");
            generatorSprites = await addressableManager.LoadAssetAllArrayAsync<Sprite>("generators");
            collectableSprites = await addressableManager.LoadAssetAllArrayAsync<Sprite>("collectables");
            chestSprites = await addressableManager.LoadAssetAllArrayAsync<Sprite>("chests");

            // Tasks
            taskSprites = await addressableManager.LoadAssetAllArrayAsync<Sprite>("tasks");

            // Selectables
            furnitureSprites = await addressableManager.LoadAssetAllArrayAsync<Sprite>("furniture");
            floorSprites = await addressableManager.LoadAssetAllArrayAsync<Sprite>("floors");
            wallSprites = await addressableManager.LoadAssetAllArrayAsync<Sprite>("walls");
            propsSprites = await addressableManager.LoadAssetAllArrayAsync<Sprite>("props");

            dataLoaded = true;
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
                valuesUI.SlashValues(Item.CollGroup.Energy);
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
                valuesUI.SlashValues(Item.CollGroup.Gold);
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
                valuesUI.SlashValues(Item.CollGroup.Gems);
            }
        }

        //////// UPDATE ////////

        void UpdateExperience(int amount)
        {
            experience = amount;

            CheckExperience(true);

            valuesUI.UpdateValues();

            dataManager.SaveValue("experience", experience);
        }

        public void UpdateLevel(Action callback = null)
        {
            level++;

            experience -= maxExperience;

            SetTenthLevel();

            dataManager.SaveValue("level", level);
            dataManager.SaveValue("experience", experience);

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
                    soundManager.PlaySound(SoundManager.SoundType.LevelUpIndicator);
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

        public bool UpdateValue(int amount, Item.CollGroup type, bool useMultiplier = false, bool updateUI = false)
        {
            int tempAmount = CalcNewAmount(amount, type, useMultiplier);

            switch (type)
            {
                case Item.CollGroup.Energy:
                    if (amount < 0 && energy + amount < 0)
                    {
                        return false;
                    }
                    break;
                case Item.CollGroup.Gold:
                    if (amount < 0 && gold + amount < 0)
                    {
                        return false;
                    }
                    break;
                case Item.CollGroup.Gems:
                    if (amount < 0 && gems + amount < 0)
                    {
                        return false;
                    }
                    break;
                default: //Item.CollGroup.Experience
                    UpdateExperience(tempAmount);
                    return false;
            }

            if (type == Item.CollGroup.Energy)
            {
                energyTemp = CalcNewAmount(amount, type, useMultiplier);

                EnergyUpdatedEventAction?.Invoke(true);
            }

            dataManager.SaveValue(type.ToString().ToLower(), tempAmount);

            if (updateUI)
            {
                UpdateValueUI(amount, type, useMultiplier);
            }

            return true;
        }

        public void UpdateValueUI(int amount, Item.CollGroup type, bool useMultiplier = false, Action callback = null)
        {
            int tempAmount = CalcNewAmount(amount, type, useMultiplier);

            bool shouldFlash = amount > 0 ? true : false;

            int currentAmount;

            bool isUpdating;

            switch (type)
            {
                case Item.CollGroup.Energy:
                    currentAmount = energy;
                    isUpdating = updatingEnergy;
                    break;
                case Item.CollGroup.Gold:
                    currentAmount = gold;
                    isUpdating = updatingGold;
                    break;
                case Item.CollGroup.Gems:
                    currentAmount = gems;
                    isUpdating = updatingGems;
                    break;
                default: //Item.CollGroup.Experience
                    Debug.LogWarning("Item.CollGroup.Experience was given to UpdateValueUI!");
                    return;
            }

            if (isUpdating)
            {
                SetUpdating(type, false);

                StopCoroutine(UpdateNumber(type, currentAmount, tempAmount, shouldFlash, callback)); // Remove energy
            }
            else
            {
                SetUpdating(type, true);

                StartCoroutine(UpdateNumber(type, currentAmount, tempAmount, shouldFlash, callback));
            }
        }

        int CalcNewAmount(int amount, Item.CollGroup type, bool useMultiplier = false)
        {
            switch (type)
            {
                case Item.CollGroup.Energy:
                    if (useMultiplier)
                    {
                        return energy + valuesData.energyMultiplier[amount - 1];
                    }
                    else
                    {
                        return energy + amount;
                    }
                case Item.CollGroup.Gold:
                    if (useMultiplier)
                    {
                        return gold + valuesData.goldMultiplier[amount - 1];
                    }
                    else
                    {
                        return gold + amount;
                    }
                case Item.CollGroup.Gems:
                    if (useMultiplier)
                    {
                        return gems + valuesData.gemsMultiplier[amount - 1];
                    }
                    else
                    {
                        return gems + amount;
                    }
                default: //Item.CollGroup.Experience
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

        IEnumerator UpdateNumber(Item.CollGroup type, int amount, int newAmount, bool slash, Action callback = null)
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
                    valuesUI.SlashValues(type, callback);
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
                    valuesUI.SlashValues(type, callback);
                }

                SetUpdating(type, false);
            }

            if (!slash)
            {
                callback?.Invoke();
            }
        }

        void SetUpdateNumber(Item.CollGroup type, int amount)
        {
            switch (type)
            {
                case Item.CollGroup.Energy:
                    energy = amount;
                    break;
                case Item.CollGroup.Gold:
                    gold = amount;
                    break;
                case Item.CollGroup.Gems:
                    gems = amount;
                    break;
            }
        }

        void SetUpdating(Item.CollGroup type, bool value)
        {
            switch (type)
            {
                case Item.CollGroup.Energy:
                    updatingEnergy = value;
                    break;
                case Item.CollGroup.Gold:
                    updatingGold = value;
                    break;
                case Item.CollGroup.Gems:
                    updatingGems = value;
                    break;
            }
        }

        //////// BONUS ////////

        public void AddToBonus(Item item)
        {
            BonusManager.Bonus newBonus = new()
            {
                sprite = item.sprite,
                type = item.type,
                group = item.group,
                genGroup = item.genGroup,
                chestGroup = item.chestGroup
            };

            bonusData.Add(newBonus);

            dataManager.SaveBonus();
        }

        public void CheckBonus()
        {
            mergeUI.CheckBonusButton();
        }

        public BonusManager.Bonus GetAndRemoveLatestBonus()
        {
            int latestIndex = bonusData.Count - 1;

            BonusManager.Bonus newBonus = bonusData[latestIndex];

            bonusData.RemoveAt(latestIndex);

            mergeUI.CheckBonusButton();

            return newBonus;
        }

        //////// OTHER ////////

        void ToggleCanLevelUpCheck(bool canLevelUpCheck)
        {
            canLevelUp = canLevelUpCheck;

            PlayerPrefs.SetInt("canLevelUp", canLevelUp ? 1 : 0);
            PlayerPrefs.Save();

            cloudSave.SaveDataAsync("canLevelUp", canLevelUp ? 1 : 0);
        }

        // Get item sprite from sprite name
        public Sprite GetSprite(string name, Item.Type type)
        {
            if (name == null || name == "")
            {
                return null;
            }

            switch (type)
            {
                case Item.Type.Item:
                    return FindSpriteByName(itemSprites, name);
                case Item.Type.Gen:
                    return FindSpriteByName(generatorSprites, name);
                case Item.Type.Coll:
                    return FindSpriteByName(collectableSprites, name);
                case Item.Type.Chest:
                    return FindSpriteByName(chestSprites, name);
                default:
                    // ERROR
                    ErrorManager.Instance.Throw(ErrorManager.ErrorType.Code, GetType() + " // Item", "Wrong type: " + type);
                    return null;
            }
        }

        // Get task sprite from sprite name
        public Sprite GetSprite(string name)
        {
            return FindSpriteByName(taskSprites, name);
        }

        // Get selectable from sprite name
        public Sprite GetSprite(string name, Selectable.Type type, bool isOption = false)
        {
            switch (type)
            {
                case Selectable.Type.Floor:
                    return FindSpriteByName(floorSprites, name);
                //return FindSpriteByName(isOption ? floorOptionSprites : floorSprites, name);
                case Selectable.Type.Wall:
                    return FindSpriteByName(wallSprites, name);
                case Selectable.Type.Furniture:
                    return FindSpriteByName(furnitureSprites, name);
                case Selectable.Type.Prop:
                    return FindSpriteByName(propsSprites, name);
                default:
                    // ERROR
                    ErrorManager.Instance.Throw(ErrorManager.ErrorType.Code, GetType() + " // Selectable", "Wrong type: " + type);
                    return null;
            }
        }

        // Find the given sprite
        Sprite FindSpriteByName(Sprite[] sprites, string name)
        {
            foreach (Sprite sprite in sprites)
            {
                if (sprite.name == name)
                {
                    return sprite;
                }
            }

            // ERROR
            ErrorManager.Instance.Throw(ErrorManager.ErrorType.Code, GetType().ToString(), "Sprite not found with name: " + name);

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