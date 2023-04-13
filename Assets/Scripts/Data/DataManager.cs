using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CI.QuickSave;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class DataManager : MonoBehaviour
{
    // Instance
    public static DataManager Instance;

    // For testing only
    public bool ignoreInitialCheck = false;

    // Initial items
    public Items items;
    public Generators generators;
    public InitialItems initialItems;

    // Whether data has been fully loaded
    public bool loaded;
    private bool isEditor = false;

    // Quick Save
    private QuickSaveSettings settings;
    public QuickSaveWriter writer;
    public QuickSaveReader reader;

    private JsonHandler jsonHandler;
    private GameData gameData;
    private TimeManager timeManager;

    private string initialJsonData;
    private string timersJsonData;
    private string unlockedJsonData;

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
        settings = new QuickSaveSettings() { CompressionMode = CompressionMode.None }; //TODO -  Set CompressionMode to in the final game Gzip
        writer = QuickSaveWriter.Create("Root", settings);

        // Cache JsonHandler
        jsonHandler = GetComponent<JsonHandler>();

        gameData = GameData.Instance;

        timeManager = gameData.GetComponent<TimeManager>();

        gameData.LoadSprites();

#if (UNITY_EDITOR)
        isEditor = true;

        // Make this script run if we not starting from the Losding scene
        if (!loaded && SceneManager.GetActiveScene().name == "GamePlay")
        {
            await CheckInitialData();
        }
#endif
    }

    // Check if we need to save initial data to disk
    public async Task CheckInitialData()
    {
        if (!writer.Exists("rootSet") || (ignoreInitialCheck && isEditor))
        {
            initialJsonData = jsonHandler.ConvertBoardToJson(initialItems.content,true);
            timersJsonData = jsonHandler.ConvertTimersToJson(gameData.timers);

            writer
                .Write("rootSet", true)
                //////// Values ////////
                .Write("experience", gameData.experience)
                .Write("level", gameData.level)
                .Write("energy", gameData.energy)
                .Write("gold", gameData.gold)
                .Write("gems", gameData.gems)
                //////// Items ////////
                .Write("boardData", initialJsonData)
                .Write("unlockedData", GetInitialUnlocked())
                .Write("timers", timersJsonData)
                .Commit();

            await GetData(true);
        }
        else
        {
            reader = QuickSaveReader.Create("Root", settings);

            await GetData(false);
        }
    }

    // Get object data from the initial data
    async Task GetData(bool initialLoad)
    {
        string newBoardData = "";
        string newTimersData = "";
        string newUnlockedData = "";

        if (initialLoad)
        {
            // Get json string from the initial json file
            newBoardData = initialJsonData;
            newTimersData = timersJsonData;
            newUnlockedData = unlockedJsonData;
        }
        else
        {
            // Get json string from the saved json file
            reader.Read<string>("boardData", r => newBoardData = r);
            reader.Read<string>("timers", r => newTimersData = r);
            reader.Read<string>("unlockedData", r => newUnlockedData = r);

            gameData.SetExperience(reader.Read<float>("experience"), true);
            gameData.SetLevel(reader.Read<int>("level"), true);
            gameData.SetEnergy(reader.Read<int>("energy"), true);
            gameData.SetGold(reader.Read<int>("gold"), true);
            gameData.SetGems(reader.Read<int>("gems"), true);
        }

        string[] unlockedDataTemp = JsonConvert.DeserializeObject<string[]>(newUnlockedData);

        gameData.unlockedData.CopyTo(unlockedDataTemp, 0);
        gameData.unlockedData = unlockedDataTemp;

        // Convert data
        gameData.itemsData = ConvertItems(items.content);
        gameData.generatorsData = ConvertGenerators(generators.content);

        gameData.timers = jsonHandler.ConvertTimersFromJson(newTimersData);
        gameData.boardData = ConvertArrayToBoard(jsonHandler.ConvertBoardFromJson(newBoardData));

        //timeManager.CheckTimers();

        // Finish Task
        loaded = true;
        await Task.Delay(200);
    }

    Types.Items[] ConvertItems(Types.Items[] itemsContent)
    {
        Types.Items[] convertedItems = new Types.Items[itemsContent.Length];

        for (int i = 0; i < itemsContent.Length; i++)
        {
            int count = 1;

            Types.Items newObjectData = new Types.Items
            {
                group = itemsContent[i].group,
                hasLevel = itemsContent[i].hasLevel,
                itemName = itemsContent[i].itemName,
                parents = itemsContent[i].parents,
                content = new Types.ItemsData[itemsContent[i].content.Length]
            };

            for (int j = 0; j < itemsContent[i].content.Length; j++)
            {
                Types.ItemsData newInnerObjectData = new Types.ItemsData
                {
                    group = itemsContent[i].group,
                    parents = itemsContent[i].parents,
                    hasLevel = itemsContent[i].hasLevel,
                    itemName =
                        itemsContent[i].content[j].itemName != ""
                            ? itemsContent[i].content[j].itemName
                            : items.content[i].itemName,
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

    Types.Generators[] ConvertGenerators(Types.Generators[] generatorsCotent)
    {
        Types.Generators[] convertedGenerators = new Types.Generators[generatorsCotent.Length];

        for (int i = 0; i < generatorsCotent.Length; i++)
        {
            int count = 1;

            Types.Generators newObjectData = new Types.Generators
            {
                genGroup = generatorsCotent[i].genGroup,
                hasLevel = generatorsCotent[i].hasLevel,
                itemName = generatorsCotent[i].itemName,
                creates = generatorsCotent[i].creates,
                content = new Types.GeneratorsData[generatorsCotent[i].content.Length]
            };

            for (int j = 0; j < generatorsCotent[i].content.Length; j++)
            {
                Types.GeneratorsData newInnerObjectData = new Types.GeneratorsData
                {
                    genGroup = generatorsCotent[i].genGroup,
                    creates = generatorsCotent[i].creates,
                    hasLevel = generatorsCotent[i].hasLevel,
                    itemName =
                        generatorsCotent[i].content[j].itemName != ""
                            ? generatorsCotent[i].content[j].itemName
                            : items.content[i].itemName,
                    level = count,
                    sprite = generatorsCotent[i].content[j].sprite,
                    unlocked = CheckUnlocked(generatorsCotent[i].content[j].sprite.name),
                    isMaxLavel = count == generatorsCotent[i].content.Length
                };

                newObjectData.content[j] = newInnerObjectData;

                count++;
            }

            convertedGenerators[i] = newObjectData;
        }

        return convertedGenerators;
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
    public void UnlockItem(
        string spriteName,
        Types.Type type,
        Types.Group group,
        Types.GenGroup genGroup
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

            writer.Write("unlockedData", JsonConvert.SerializeObject(gameData.unlockedData)).Commit();
        }

        if (type == Types.Type.Item)
        {
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
        }
        else if (type == Types.Type.Gen)
        {
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
        }
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
                state = boardItem.state,
                crate = boardItem.crate
            };

            count++;
        }

        return newBoardArray;
    }
}
