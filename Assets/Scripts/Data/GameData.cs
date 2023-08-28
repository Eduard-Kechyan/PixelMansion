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
        public EnergyTimer energyTimer;

        public const int WIDTH = 7;
        public const int HEIGHT = 9;
        public const int ITEM_COUNT = WIDTH * HEIGHT;
        public const int MAX_ENERGY = 100;
        public const float GAME_PIXEL_WIDTH = 180f;
        public static float GAME_PIXEL_HEIGHT = 0f;

        public const string WEB_ADDRESS = "https://WEBSITE.com"; // TODO - Replace WEBSITE with proper the website name
        public const string STUDIO_NAME = "unonodegames"; // TODO - Replace SOCIAL_MEDIA_NAME with the proper studio name
        public const string GAME_TITLE = "Pixel Merge"; // TODO - Replace GAME_TITLE with the proper game title

        public int maxExperience = 10;
        public int leftoverExperience = 0;

        // Values
        public int experience = 0; // 0% - 100%
        public int level = 0;
        public int energy = 100;
        public int gold = 100;
        public int gems = 100;

        public bool levelTen;
        public bool canLevelUp = false;

        // Inventory
        public int inventorySpace = 7;
        public const int maxInventorySpace = 50;
        public int inventorySlotPrice = 50;

        [HideInInspector]
        public int initialInventorySpace = 7;

        // Timers
        public List<Types.Timer> timers;

        // Main data
        public Types.Items[] itemsData;
        public Types.Items[] collectablesData;
        public Types.Items[] generatorsData;
        public Types.Items[] chestsData;
        public List<Types.Bonus> bonusData = new();
        public List<Types.Inventory> inventoryData = new();
        public List<Types.Tasks> tasksData = new();

        public Types.Board[,] boardData;
        public string[] unlockedData = new string[0];

        // User
        public string userId;

        private Sprite[] itemsSprites;
        private Sprite[] generatorsSprites;
        private Sprite[] collectablesSprites;
        private Sprite[] chestsSprites;
        private Sprite[] taskSprites;

        // References
        private LevelMenu levelMenu;
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

            CalcMaxExperience();

            CalcGamePixelHeight();
        }

        public void Init(string sceneName)
        {
            levelMenu = GameRefs.Instance.levelMenu;
            valuesUI = GameRefs.Instance.valuesUI;

            if (sceneName == "Gameplay")
            {
                gameplayUI = GameRefs.Instance.gameplayUI;
            }
        }

        public void InitAlt()
        {
            // Load sprites from resources
            itemsSprites = Resources.LoadAll<Sprite>("Sprites/Items");
            generatorsSprites = Resources.LoadAll<Sprite>("Sprites/Generators");
            collectablesSprites = Resources.LoadAll<Sprite>("Sprites/Collectables");
            chestsSprites = Resources.LoadAll<Sprite>("Sprites/Chests");
            taskSprites= Resources.LoadAll<Sprite>("Sprites/Tasks");
        }

        //////// SET ////////

        void CalcGamePixelHeight()
        {
            GAME_PIXEL_HEIGHT = Screen.height / (Screen.width / GAME_PIXEL_WIDTH);
        }

        public void SetExperience(int amount, bool initial = false, bool checkCanLevelUp = false)
        {
            experience = amount;

            if (!initial)
            {
                valuesUI.UpdateValues();
            }

            if (checkCanLevelUp)
            {
                // SetExperienceReady();
            }
        }

        public void SetLevel(int amount, bool initial = false)
        {
            level = amount;

            SetTenthLevel();

            CalcMaxExperience();

            if (!initial)
            {
                valuesUI.UpdateValues();
            }
        }

        public void SetEnergy(int amount, bool initial = false)
        {
            energy = amount;

            if (!initial)
            {
                valuesUI.UpdateValues();
            }
        }

        public void SetGold(int amount, bool initial = false)
        {
            gold = amount;

            if (!initial)
            {
                valuesUI.UpdateValues();
            }
        }

        public void SetGems(int amount, bool initial = false)
        {
            gems = amount;

            if (!initial)
            {
                valuesUI.UpdateValues();
            }
        }

        //////// UPDATE ////////

        public void UpdateExperience(int amount = 1, bool useMultiplier = false)
        {
            if (canLevelUp)
            {
                if (useMultiplier)
                {
                    leftoverExperience += valuesData.experienceMultiplier[amount - 1];
                }
                else
                {
                    leftoverExperience += amount;
                }
            }
            else
            {
                if (useMultiplier)
                {
                    experience += valuesData.experienceMultiplier[amount - 1];
                }
                else
                {
                    experience += amount;
                }

                if (experience >= maxExperience)
                {
                    leftoverExperience = experience - maxExperience;

                    //  experience = maxExperience;

                    SetExperienceReady();
                }
            }

            if (experience < 0)
            {
                experience = 0;
            }

            valuesUI.UpdateValues();

            levelMenu.UpdateLevelMenu();

            dataManager.writer.Write("experience", experience).Commit();
        }

        void SetExperienceReady()
        {
            ToggleCanLevelUpCheck(true);

            soundManager.PlaySound("LevelUpIndicator");

            valuesUI.ToggleLevelUp(true);
        }

        public void UpdateLevel()
        {
            level++;

            SetTenthLevel();

            dataManager.writer.Write("level", level).Commit();

            ToggleCanLevelUpCheck(false);

            valuesUI.ToggleLevelUp(false);

            CalcMaxExperience();

            valuesUI.UpdateLevel();
        }

        public bool UpdateEnergy(int amount = 1, bool useMultiplier = false)
        {
            if (amount < 0 && gems + energy < 0)
            {
                return false;
            }
            else
            {
                if (useMultiplier)
                {
                    energy += valuesData.energyMultiplier[amount - 1];
                }
                else
                {
                    energy += amount;
                }

                if (energy < 0)
                {
                    energy = 0;
                }

                energyTimer.Check();

                dataManager.writer.Write("energy", energy).Commit();

                valuesUI.UpdateValues();

                return true;
            }
        }

        public bool UpdateGold(int amount = 1, bool useMultiplier = false)
        {
            if (amount < 0 && gold + amount < 0)
            {
                return false;
            }
            else
            {
                if (useMultiplier)
                {
                    gold += valuesData.goldMultiplier[amount - 1];
                }
                else
                {
                    gold += amount;
                }

                if (gold < 0)
                {
                    gold = 0;
                }

                dataManager.writer.Write("gold", gold).Commit();

                valuesUI.UpdateValues();

                return true;
            }
        }

        public bool UpdateGems(int amount = 1, bool useMultiplier = false)
        {
            if (amount < 0 && gems + amount < 0)
            {
                return false;
            }
            else
            {
                if (useMultiplier)
                {
                    gems += valuesData.gemsMultiplier[amount - 1];
                }
                else
                {
                    gems += amount;
                }

                if (gems < 0)
                {
                    gems = 0;
                }

                dataManager.writer.Write("gems", gems).Commit();

                valuesUI.UpdateValues();

                return true;
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

            dataManager.SaveBonus();

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