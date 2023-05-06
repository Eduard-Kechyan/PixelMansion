using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CI.QuickSave;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Locale;

public class DataManager : MonoBehaviour
{
    // Instance
    public static DataManager Instance;

    // For testing only
    public bool ignoreInitialCheck = false;

    // Initial items
    public Items items;
    public Items generators;
    public Items collectables;
    public InitialItems initialItems;

    // Whether data has been fully loaded
    public bool loaded;
    //private bool isEditor = false;

    // Quick Save
    private QuickSaveSettings saveSettings;
    public QuickSaveWriter writer;
    public QuickSaveReader reader;

    private JsonHandler jsonHandler;
    private GameData gameData;
    private TimeManager timeManager;

    private string initialJsonData;
    private string bonusData;
    private string inventoryData;
    private string timersJsonData;
    private string unlockedJsonData;

    private I18n LOCALE = I18n.Instance;

    // Handle instance
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

    async void Start()
    {
        // Set up Quick Save
        saveSettings = new QuickSaveSettings() { CompressionMode = CompressionMode.None }; //TODO -  Set CompressionMode in the final game to Gzip
        writer = QuickSaveWriter.Create("Root", saveSettings);

        // Cache JsonHandler
        jsonHandler = GetComponent<JsonHandler>();

        gameData = GameData.Instance;

        timeManager = gameData.GetComponent<TimeManager>();

        gameData.LoadSprites();

#if UNITY_EDITOR
       // isEditor = true;

        // Make this script run if we arn't starting from the Loading scene
        if (
            !loaded
            && (
                SceneManager.GetActiveScene().name == "Gameplay"
                || SceneManager.GetActiveScene().name == "Hub"
            )
        )
        {
            await CheckInitialData();
        }
#endif
    }

    // Check if we need to save initial data to disk
    public async Task CheckInitialData()
    {
        Debug.Log("DataManager: "+writer.Exists("rootSet"));

        if (writer.Exists("rootSet"))
        {
            reader = QuickSaveReader.Create("Root", saveSettings);

            await GetData(false);
        }
        else
        {
            initialJsonData = jsonHandler.ConvertBoardToJson(initialItems.content, true);
            bonusData = jsonHandler.ConvertBonusToJson(gameData.bonusData);
            inventoryData = jsonHandler.ConvertInventoryToJson(gameData.inventoryData);
            timersJsonData = jsonHandler.ConvertTimersToJson(gameData.timers);

            writer
                .Write("rootSet", true)
                //////// Values ////////
                .Write("experience", gameData.experience)
                .Write("level", gameData.level)
                .Write("energy", gameData.energy)
                .Write("gold", gameData.gold)
                .Write("gems", gameData.gems)
                //////// Data ////////
                .Write("boardData", initialJsonData)
                .Write("bonusData", bonusData)
                .Write("inventoryData", inventoryData)
                .Write("unlockedData", GetInitialUnlocked())
                .Write("timers", timersJsonData)
                // Other
                .Write("inventorySpace", gameData.inventorySpace)
                .Commit();

            await GetData(true);
        }
        /*if (!writer.Exists("rootSet") || (ignoreInitialCheck && isEditor))
        {
            
        }
        else
        {
            reader = QuickSaveReader.Create("Root", saveSettings);

            await GetData(false);
        }*/
    }

    // Get object data from the initial data
    async Task GetData(bool initialLoad)
    {
        string newBoardData = "";
        string newBonusData = "";
        string newInventoryData = "";
        string newTimersData = "";
        string newUnlockedData = "";

        if (initialLoad)
        {
            // Get json string from the initial json file
            newBoardData = initialJsonData;
            newBonusData = bonusData;
            newInventoryData = inventoryData;
            newTimersData = timersJsonData;
            newUnlockedData = unlockedJsonData;
        }
        else
        {
            gameData.SetExperience(reader.Read<int>("experience"), true);
            gameData.SetLevel(reader.Read<int>("level"), true);
            gameData.SetEnergy(reader.Read<int>("energy"), true);
            gameData.SetGold(reader.Read<int>("gold"), true);
            gameData.SetGems(reader.Read<int>("gems"), true);

            // Get json string from the saved json file
            reader.Read<string>("boardData", r => newBoardData = r);
            reader.Read<string>("bonusData", r => newBonusData = r);
            reader.Read<string>("inventoryData", r => newInventoryData = r);
            reader.Read<string>("timers", r => newTimersData = r);
            reader.Read<string>("unlockedData", r => newUnlockedData = r);

            gameData.inventorySpace = reader.Read<int>("inventorySpace");
        }

        string[] unlockedDataTemp = JsonConvert.DeserializeObject<string[]>(newUnlockedData);

        gameData.unlockedData.CopyTo(unlockedDataTemp, 0);
        gameData.unlockedData = unlockedDataTemp;

        // Convert data
        gameData.itemsData = ConvertItems(items.content);
        gameData.generatorsData = ConvertItems(generators.content);
        gameData.collectablesData = ConvertItems(collectables.content);

        gameData.timers = jsonHandler.ConvertTimersFromJson(newTimersData);
        gameData.boardData = ConvertArrayToBoard(jsonHandler.ConvertBoardFromJson(newBoardData));
        gameData.bonusData = jsonHandler.ConvertBonusFromJson(newBonusData);
        gameData.inventoryData = jsonHandler.ConvertInventoryFromJson(newInventoryData);

        //timeManager.CheckTimers();

        // Finish Task
        loaded = true;

        await Task.Delay(500);
    }

    Types.Items[] ConvertItems(Types.Items[] itemsContent)
    {
        Types.Items[] convertedItems = new Types.Items[itemsContent.Length];

        for (int i = 0; i < itemsContent.Length; i++)
        {
            int count = 1;

            Types.Items newObjectData = new Types.Items
            {
                type = itemsContent[i].type,
                group = itemsContent[i].group,
                genGroup = itemsContent[i].genGroup,
                collGroup = itemsContent[i].collGroup,
                hasLevel = itemsContent[i].hasLevel,
                customName = itemsContent[i].customName,
                parents = itemsContent[i].parents,
                creates = itemsContent[i].creates,
                content = new Types.ItemsData[itemsContent[i].content.Length]
            };

            for (int j = 0; j < itemsContent[i].content.Length; j++)
            {
                Types.ItemsData newInnerObjectData = new Types.ItemsData
                {
                    type = itemsContent[i].type,
                    group = itemsContent[i].group,
                    genGroup = itemsContent[i].genGroup,
                    collGroup = itemsContent[i].collGroup,
                    creates = itemsContent[i].creates,
                    customName = itemsContent[i].content[j].customName,
                    parents = itemsContent[i].parents,
                    hasLevel = itemsContent[i].hasLevel,
                    itemName = GetItemName(itemsContent[i].content[j], newObjectData, count),
                    level = count,
                    sprite = itemsContent[i].content[j].sprite,
                    unlocked = CheckUnlocked(itemsContent[i].content[j].sprite.name),
                    isMaxLavel = count == itemsContent[i].content.Length
                };

                newObjectData.content[j] = newInnerObjectData;

                count++;
            }

            convertedItems[i] = newObjectData;
        }

        return convertedItems;
    }

    string GetItemName(Types.ItemsData itemSingle, Types.Items itemsData, int count)
    {
        if (itemSingle.customName && itemSingle.itemName != "")
        {
            return LOCALE.Get(itemsData.type + "_" + itemSingle.itemName);
        }

        if (itemsData.customName)
        {
            switch (itemsData.type)
            {
                case Types.Type.Item:
                    return LOCALE.Get(
                        itemsData.type + "_" + itemsData.group.ToString() + "_" + (count - 1)
                    );
                case Types.Type.Gen:
                    return LOCALE.Get(
                        itemsData.type + "_" + itemsData.genGroup.ToString() + "_" + (count - 1)
                    );
                case Types.Type.Coll:
                    return "";

                default:
                    ErrorManager.Instance.Throw(
                        Types.ErrorType.Code,
                        "Wrong type: " + itemsData.type
                    );

                    return "";
            }
        }

        if (itemsData.type == Types.Type.Item)
        {
            return LOCALE.Get("Item_" + itemsData.group.ToString() + "_" + 0);
        }

        if (itemsData.type == Types.Type.Gen)
        {
            return LOCALE.Get("Gen_" + itemsData.genGroup.ToString() + "_" + 0);
        }

        if (itemsData.type == Types.Type.Coll)
        {
            return LOCALE.Get("Coll_" + itemsData.collGroup.ToString(), count);
        }

        ErrorManager.Instance.Throw(Types.ErrorType.Locale, "error_loc_name");

        return LOCALE.Get("Item_error_name");
    }

    // Check if item is unlocked
    bool CheckUnlocked(string spriteName)
    {
        bool found = false;

        for (int i = 0; i < gameData.unlockedData.Length; i++)
        {
            if (spriteName == gameData.unlockedData[i])
            {
                found = true;
                break;
            }
        }

        return found;
    }

    // Get initial unlocked items
    string GetInitialUnlocked()
    {
        // Get all item unlocks
        string[] initialUnlockedPre = new string[initialItems.content.Length];

        for (int i = 0; i < initialItems.content.Length; i++)
        {
            if (initialItems.content[i].sprite != null)
            {
                bool found = false;

                // Check if item is already unlocked
                for (int j = 0; j < initialUnlockedPre.Length; j++)
                {
                    if (
                        initialUnlockedPre[j] != null
                        && initialUnlockedPre[j] == initialItems.content[i].sprite.name
                    )
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    string unlockedItem = initialItems.content[i].sprite.name;

                    initialUnlockedPre[i] = unlockedItem;
                }
            }
        }

        // Clear array from nulls
        int nullFreeLength = 0; // Note the 1

        for (int i = 0; i < initialUnlockedPre.Length; i++)
        {
            if (initialUnlockedPre[i] != null)
            {
                nullFreeLength++;
            }
        }

        string[] initialUnlocked = new string[nullFreeLength];

        int count = 0;

        for (int i = 0; i < initialUnlockedPre.Length; i++)
        {
            if (initialUnlockedPre[i] != null)
            {
                initialUnlocked[count] = initialUnlockedPre[i];

                count++;
            }
        }

        unlockedJsonData = JsonConvert.SerializeObject(initialUnlocked);

        return unlockedJsonData;
    }

    // Unlock item
    public bool UnlockItem(
        string spriteName,
        Types.Type type,
        Types.Group group,
        Types.GenGroup genGroup,
        Types.CollGroup collGroup
    )
    {
        bool found = false;

        for (int i = 0; i < gameData.unlockedData.Length; i++)
        {
            if (spriteName == gameData.unlockedData[i])
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            string[] newUnlockedData = new string[gameData.unlockedData.Length + 1];

            for (int i = 0; i < gameData.unlockedData.Length; i++)
            {
                newUnlockedData[i] = gameData.unlockedData[i];
            }

            newUnlockedData[gameData.unlockedData.Length] = spriteName;

            gameData.unlockedData.CopyTo(newUnlockedData, 0);
            gameData.unlockedData = newUnlockedData;

            writer
                .Write("unlockedData", JsonConvert.SerializeObject(gameData.unlockedData))
                .Commit();
        }

        switch (type)
        {
            case Types.Type.Item:
                for (int i = 0; i < gameData.itemsData.Length; i++)
                {
                    if (gameData.itemsData[i].group == group)
                    {
                        for (int j = 0; j < gameData.itemsData[i].content.Length; j++)
                        {
                            if (gameData.itemsData[i].content[j].sprite.name == spriteName)
                            {
                                gameData.itemsData[i].content[j].unlocked = true;
                            }
                        }
                    }
                }
                break;
            case Types.Type.Gen:
                for (int i = 0; i < gameData.generatorsData.Length; i++)
                {
                    if (gameData.generatorsData[i].genGroup == genGroup)
                    {
                        for (int j = 0; j < gameData.generatorsData[i].content.Length; j++)
                        {
                            if (gameData.generatorsData[i].content[j].sprite.name == spriteName)
                            {
                                gameData.generatorsData[i].content[j].unlocked = true;
                            }
                        }
                    }
                }
                break;
            case Types.Type.Coll:
                for (int i = 0; i < gameData.collectablesData.Length; i++)
                {
                    if (gameData.collectablesData[i].collGroup == collGroup)
                    {
                        for (int j = 0; j < gameData.collectablesData[i].content.Length; j++)
                        {
                            if (gameData.collectablesData[i].content[j].sprite.name == spriteName)
                            {
                                gameData.collectablesData[i].content[j].unlocked = true;
                            }
                        }
                    }
                }
                break;
            default:
                ErrorManager.Instance.Throw(Types.ErrorType.Code, "Wrong type: " + type);
                break;
        }

        return found;
    }

    //// SAVE ////

    // Save board data to disk
    public void SaveBoard()
    {
        string newBoardData = jsonHandler.ConvertBoardToJson(
            ConvertBoardToArray(gameData.boardData)
        );

        writer.Write("boardData", newBoardData).Commit();
    }

    public void SaveTimers()
    {
        string newTimers = jsonHandler.ConvertTimersToJson(gameData.timers);

        writer.Write("timers", newTimers).Commit();
    }

    public void SaveBonus()
    {
        string newBonusData = jsonHandler.ConvertBonusToJson(gameData.bonusData);

        writer.Write("bonusData", newBonusData).Commit();
    }

    public void SaveInventory(bool saveSpace = false)
    {
        if (saveSpace)
        {
            writer.Write("inventorySpace", gameData.inventorySpace).Commit();
        }
        else
        {
            string newInventoryData = jsonHandler.ConvertInventoryToJson(gameData.inventoryData);

            writer.Write("inventoryData", newInventoryData).Commit();
        }
    }

    //// OTHER ////

    [ContextMenu("Loop Board Data")]
    public void LoopBoardData()
    {
        foreach (Types.Board boardItem in gameData.boardData)
        {
            Debug.Log(boardItem.sprite);
        }
    }

    Types.Board[,] ConvertArrayToBoard(Types.Board[] boardArray)
    {
        Types.Board[,] newBoardData = new Types.Board[GameData.WIDTH, GameData.HEIGHT];

        int count = 0;

        for (int i = 0; i < GameData.WIDTH; i++)
        {
            for (int j = 0; j < GameData.HEIGHT; j++)
            {
                newBoardData[i, j] = new Types.Board
                {
                    sprite = boardArray[count].sprite,
                    type = boardArray[count].type,
                    group = boardArray[count].group,
                    genGroup = boardArray[count].genGroup,
                    state = boardArray[count].state,
                    crate = boardArray[count].crate,
                    order = count
                };

                count++;
            }
        }

        return newBoardData;
    }

    Types.Board[] ConvertBoardToArray(Types.Board[,] boardData)
    {
        Types.Board[] newBoardArray = new Types.Board[GameData.ITEM_COUNT];

        int count = 0;

        foreach (Types.Board boardItem in boardData)
        {
            newBoardArray[count] = new Types.Board
            {
                sprite = boardItem.sprite,
                type = boardItem.type,
                group = boardItem.group,
                genGroup = boardItem.genGroup,
                collGroup = boardItem.collGroup,
                state = boardItem.state,
                crate = boardItem.crate
            };

            count++;
        }

        return newBoardArray;
    }
}
