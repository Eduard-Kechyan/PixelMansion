using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// TODO - Kaleidoscope

public class GameData : MonoBehaviour
{
    // Variables
    public ValuesData valuesData;

    public const int WIDTH = 7;
    public const int HEIGHT = 9;
    public const int ITEM_COUNT = WIDTH * HEIGHT;
    public const int MAX_ENERGY = 100;
    public const float GAME_PIXEL_WIDTH = 180f;
    public static float GAME_PIXEL_HEIGHT = 0f;

    public int maxExperience = 10;
    public int leftoverExperience = 0;
    public int maxLevel = 99;
    public int maxOther = 9999;

    public float energyTime = 120f;
    private float energyTimeOut;

    //private bool energyTimerOn = false;

    [SerializeField]
    private string energyTimerText = "00:00";

    // Values
    public int experience = 0; // 0% - 100%
    public int level = 0;
    public int energy = 100;
    public int gold = 100;
    public int gems = 100;

    public bool canLevelUp = false;

    public int inventorySpace = 7;
    public const int maxInventorySpace = 50;

    // Timers
    public List<Types.Timer> timers;

    // Items data
    public Types.Items[] itemsData;
    public Types.Items[] collectablesData;
    public Types.Items[] generatorsData;
    public List<Types.Bonus> bonusData = new List<Types.Bonus>();
    public List<Types.Inventory> inventoryData = new List<Types.Inventory>();

    //public  Types.Items[] storageData;
    public Types.Board[,] boardData;
    public string[] unlockedData = new string[0];

    private Sprite[] itemsSprites;
    private Sprite[] generatorsSprites;
    private Sprite[] collectablesSprites;

    // References
    private TimeManager timeManager;
    private LevelMenu levelMenu;
    private ValuesUI valuesUI;
    private GameplayUI gameplayUI;
    private DataManager dataManager;

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
        timeManager = GetComponent<TimeManager>();

        // Cache instances
        dataManager = DataManager.Instance;

        energyTimeOut = energyTime;

        canLevelUp = PlayerPrefs.GetInt("canLevelUp") == 1 ? true : false;

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

    /* void Update()
     {
         if (energyTimerOn)
         {
             if (energyTimeOut > 0)
             {
                 energyTimeOut -= Time.deltaTime;
                 UpdateEnergyTimer(energyTimeOut);
             }
             else
             {
                 UpdateEnergy();
 
                 CheckEnergy(true);
             }
         }
     }*/

    //////// SET ////////

    void CalcGamePixelHeight()
    {
        GAME_PIXEL_HEIGHT = Screen.height / (Screen.width / GAME_PIXEL_WIDTH);
    }

    public void SetExperience(int amount, bool initial = false)
    {
        experience = amount;

        if (!initial)
        {
            valuesUI.UpdateValues();
        }
    }

    public void SetLevel(int amount, bool initial = false)
    {
        level = amount;

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

            CheckEnergy();
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

                experience = maxExperience;

                ToggleCanLevelUpCheck(true);
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

    public void UpdateLevel()
    {
        level++;

        dataManager.writer.Write("level", level).Commit();

        experience = leftoverExperience;

        ToggleCanLevelUpCheck(false);

        CalcMaxExperience();

        valuesUI.UpdateLevel();
    }

    public bool UpdateEnergy(int amount = 1, bool useMultiplier = false)
    {
        if (amount > 0 && energy < maxOther || amount < 0 && energy >= amount)
        {
            if (useMultiplier)
            {
                energy += valuesData.energyMultiplier[amount - 1];
            }
            else
            {
                energy += amount;
            }

            if (energy > maxOther)
            {
                energy = maxOther;
            }

            if (energy < 0)
            {
                energy = 0;
            }

            CheckEnergy();

            dataManager.writer.Write("energy", energy).Commit();

            valuesUI.UpdateValues();

            return true;
        }

        return false;
    }

    public bool UpdateGold(int amount = 1, bool useMultiplier = false)
    {
        if (amount > 0 && gold < maxOther || amount < 0 && gold >= amount)
        {
            if (useMultiplier)
            {
                gold += valuesData.goldMultiplier[amount - 1];
            }
            else
            {
                gold += amount;
            }

            if (gold > maxOther)
            {
                gold = maxOther;
            }

            if (gold < 0)
            {
                gold = 0;
            }

            dataManager.writer.Write("gold", gold).Commit();

            valuesUI.UpdateValues();

            return true;
        }

        return false;
    }

    public bool UpdateGems(int amount = 1, bool useMultiplier = false)
    {
        if (amount > 0 && gems < maxOther || amount < 0 && gems >= amount)
        {
            if (useMultiplier)
            {
                gems += valuesData.gemsMultiplier[amount - 1];
            }
            else
            {
                gems += amount;
            }

            if (gems > maxOther)
            {
                gems = maxOther;
            }

            if (gems < 0)
            {
                gems = 0;
            }

            dataManager.writer.Write("gems", gems).Commit();

            valuesUI.UpdateValues();

            return true;
        }

        return false;
    }

    //////// BONUS ////////

    public void AddToBonus(Item item, bool check = true)
    {
        Types.Bonus newBonus = new Types.Bonus
        {
            sprite = item.sprite,
            type = item.type,
            group = item.group,
            genGroup = item.genGroup
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

    public void LoadSprites()
    {
        // Load spirtes from resources
        itemsSprites = Resources.LoadAll<Sprite>("Sprites/Items");
        generatorsSprites = Resources.LoadAll<Sprite>("Sprites/Generators");
        collectablesSprites = Resources.LoadAll<Sprite>("Sprites/Collectables");
    }

    void ToggleCanLevelUpCheck(bool canLevelUpCheck)
    {
        canLevelUp = canLevelUpCheck;

        PlayerPrefs.SetInt("canLevelUp", canLevelUp ? 1 : 0);
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
            default:
                ErrorManager.Instance.Throw(Types.ErrorType.Code, "Wrong type: " + type);
                break;
        }

        return null;
    }

    public void CheckEnergy(bool fromTimer = false)
    {
        /*if(energy < MAX_ENERGY){
            timeManager.AddEnergyTimer();
        }*/


        energyTimeOut = energyTime;

        // Increase energy after set time if energy is less than 100
        if (energy < 100)
        {
            // energyTimerOn = true;
        }
        else
        {
            // End the timer and notify the plat that the energy is full
            // energyTimerOn = false;

            if (fromTimer)
            {
                if (energy >= 100)
                {
                    Debug.Log("Energy full!");

                    // TODO = Add notification for a full energy
                }
            }
            else
            {
                if (valuesUI.energyTimer != null)
                {
                    valuesUI.energyTimer.style.display = DisplayStyle.None;
                }
            }
        }
    }

    void UpdateEnergyTimer(float currentTime)
    {
        currentTime++;

        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        string minutesText = minutes < 10 ? "0" + minutes : minutes.ToString();
        string secondsText = seconds < 10 ? "0" + seconds : seconds.ToString();

        energyTimerText = minutesText + ":" + secondsText;

        valuesUI.energyTimer.text = energyTimerText;
        valuesUI.energyTimer.style.display = DisplayStyle.Flex;
    }

    void CalcMaxExperience()
    {
        maxExperience = valuesData.maxExperienceMultiplier[level];
    }
}
