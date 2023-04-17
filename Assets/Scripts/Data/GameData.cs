using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// TODO - Kaleidoscope

public class GameData : MonoBehaviour
{
    public ValuesData valuesData;

    // Board width in items
    public const int WIDTH = 7;
    public const int HEIGHT = 9;
    public const int ITEM_COUNT = WIDTH * HEIGHT;
    public const int MAX_ENERGY = 100;
    public const float GAME_PIXEL_WIDTH = 180f;

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

    // Timers
    public List<Types.Timer> timers;

    // Items data
    public Types.Items[] itemsData;
    public Types.Collectables[] collectablesData;
    public Types.Generators[] generatorsData;
    public List<Types.Bonus> bonusData = new List<Types.Bonus>();

    //public  Types.Items[] storageData;
    public Types.Board[,] boardData;
    public string[] unlockedData = new string[0];

    private Values values;
    private DataManager dataManager;
    private TimeManager timeManager;
    private LevelMenu levelMenu;
    private GamePlayButtons gamePlayButtons;

    private Sprite[] itemsSprites;
    private Sprite[] generatorsSprites;
    private Sprite[] collectablesSprites;

    public static GameData Instance;

    [HideInInspector]
    public bool canLevelUp = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        dataManager = DataManager.Instance;

        energyTimeOut = energyTime;

        canLevelUp = PlayerPrefs.GetInt("canLevelUp") == 1 ? true : false;

        CalcMaxExperience();
    }

    void Update()
    {
        /*if (energyTimerOn)
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
        }*/
    }

    public void InitializeGamedataCache(bool check=false)
    {
        timeManager = GetComponent<TimeManager>();

        levelMenu = MenuManager.Instance.GetComponent<LevelMenu>();

        values = dataManager.GetComponent<Values>();
        
        if(check){
        gamePlayButtons = GameObject.Find("GamePlayUI").GetComponent<GamePlayButtons>();
        }
    }

    //////// SET ////////

    public void SetExperience(int amount, bool initial = false)
    {
        experience = amount;

        if (!initial)
        {
            values.UpdateValues();
        }
    }

    public void SetLevel(int amount, bool initial = false)
    {
        level = amount;

        CalcMaxExperience();

        if (!initial)
        {
            values.UpdateValues();
        }
    }

    public void SetEnergy(int amount, bool initial = false)
    {
        energy = amount;

        if (!initial)
        {
            values.UpdateValues();

            CheckEnergy();
        }
    }

    public void SetGold(int amount, bool initial = false)
    {
        gold = amount;

        if (!initial)
        {
            values.UpdateValues();
        }
    }

    public void SetGems(int amount, bool initial = false)
    {
        gems = amount;

        if (!initial)
        {
            values.UpdateValues();
        }
    }

    //////// UPDATE ////////

    public void UpdateExperience(int amount = 1, bool useMultiplier = false)
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

        if (experience < 0)
        {
            experience = 0;
        }

        values.UpdateValues();

        levelMenu.UpdateLevelMenu();

        dataManager.writer.Write("experience", experience).Commit();
    }

    public void UpdateLevel()
    {
        level++;

        dataManager.writer.Write("level", level).Commit();

        ToggleCanLevelUpCheck(false);

        CalcMaxExperience();

        values.UpdateLevel();
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

            values.UpdateValues();

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

            values.UpdateValues();

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

            values.UpdateValues();

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
            gamePlayButtons.CheckBonusButton();
        }

        dataManager.SaveBonus();
    }

    public Types.Bonus GetAndRemoveLatestBonus()
    {
        int latestIndex = bonusData.Count - 1;

        Types.Bonus newBonus = bonusData[latestIndex];

        bonusData.RemoveAt(latestIndex);

        dataManager.SaveBonus();

        gamePlayButtons.CheckBonusButton();

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
                Debug.Log("Wrong type!");
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
                if (values.energyTimer != null)
                {
                    values.energyTimer.style.display = DisplayStyle.None;
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

        values.energyTimer.text = energyTimerText;
        values.energyTimer.style.display = DisplayStyle.Flex;
    }

    void CalcMaxExperience()
    {
        maxExperience = valuesData.maxExperienceMultiplier[level];
    }
}
