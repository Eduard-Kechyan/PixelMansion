using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    // Board width in items
    public const int WIDTH = 7;
    public const int HEIGHT = 9;
    public const int ITEM_COUNT = WIDTH * HEIGHT;

    public static float maxExperience = 100f;
    public static int maxLevel = 99;
    public static int maxOther = 9999;

    // Values
    public static float experience = 0; // 0% - 100%
    public static int level = 0;
    public static int energy = 100;
    public static int gold = 100;
    public static int gems = 100;

    // Items data
    public static Types.Items[] itemsData;
    public static Types.Generators[] generatorsData;

    //// TEST DATA ////
    public static Types.TestTile[,] testTilesPre = new Types.TestTile[WIDTH, HEIGHT];
    public static Types.TestTile[,] testTiles = new Types.TestTile[WIDTH, HEIGHT];

    //public static Types.Items[] storageData;
    public static Types.Board[,] boardData;
    public static string[] unlockedData = new string[0];

    private static Values values;
    private static DataManager dataManager;

    private static Sprite[] itemsSprites;
    private static Sprite[] generatorsSprites;

    public static GameData Instance;

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
        values = GetComponent<Values>();
        dataManager = GetComponent<DataManager>();
    }

    //////// SET ////////

    public static void SetExperience(float amount, bool initial = false)
    {
        experience = amount;

        if (!initial)
        {
            values.UpdateValues();
        }
    }

    public static void SetLevel(int amount, bool initial = false)
    {
        level = amount;

        if (!initial)
        {
            values.UpdateValues();
        }
    }

    public static void SetEnergy(int amount, bool initial = false)
    {
        energy = amount;

        if (!initial)
        {
            values.UpdateValues();
        }
    }

    public static void SetGold(int amount, bool initial = false)
    {
        gold = amount;

        if (!initial)
        {
            values.UpdateValues();
        }
    }

    public static void SetGems(int amount, bool initial = false)
    {
        gems = amount;

        if (!initial)
        {
            values.UpdateValues();
        }
    }

    //////// UPDATE ////////

    public static bool UpdateExperience(float amount = 1)
    {
        experience += amount;

        if (experience >= maxExperience)
        {
            experience = maxExperience;

            // TODO - Update level here

            return true;
        }

        if (experience < 0)
        {
            experience = 0;
        }

        return false;
    }

    public static void UpdateLevel()
    {
        level++;

        dataManager.writer.Write("level", level).Commit();

        values.UpdateValues();
    }

    public static bool UpdateEnergy(int amount = 1)
    {
        if (amount > 0 && energy < maxOther || amount < 0 && energy >= amount)
        {
            energy += amount;

            if (energy > maxOther)
            {
                energy = maxOther;
            }

            if (energy < 0)
            {
                energy = 0;
            }

            dataManager.writer.Write("energy", energy).Commit();

            values.UpdateValues();

            return true;
        }

        return false;
    }

    public static bool UpdateGold(int amount = 1)
    {
        if (amount > 0 && gold < maxOther || amount < 0 && gold >= amount)
        {
            gold += amount;

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

    public static bool UpdateGems(int amount = 1)
    {
        if (amount > 0 && gems < maxOther || amount < 0 && gems >= amount)
        {
            gems += amount;

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

    //////// OTHER ////////

    public static void LoadSprites()
    {
        // Load spirtes from resources
        itemsSprites = Resources.LoadAll<Sprite>("Sprites/Items");
        generatorsSprites = Resources.LoadAll<Sprite>("Sprites/Generators");
    }

    // Get Sprite from sprite name
    public static Sprite GetSprite(string name, Types.Type type)
    {
        if (type == Types.Type.Default)
        {
            foreach (Sprite sprite in itemsSprites)
            {
                if (sprite.name == name)
                {
                    return sprite;
                }
            }
        }
        else if ((type == Types.Type.Gen))
        {
            foreach (Sprite sprite in generatorsSprites)
            {
                if (sprite.name == name)
                {
                    return sprite;
                }
            }
        }

        return null;
    }
}
