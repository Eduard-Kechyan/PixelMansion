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
    // Board width in items
    const int WIDTH = 7;
    const int HEIGHT = 9;
    public const int ITEM_COUNT = WIDTH * HEIGHT;

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

    // Quick Save
    private QuickSaveSettings settings;
    public QuickSaveWriter writer;
    public QuickSaveReader reader;

    private JsonHandler jsonHandler;
    private static Values values;

    /*private string itemsJsonData;
    private string generatorJsonData;*/
    private string initialJsonData;
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

        // Cache Values
        values = GetComponent<Values>();

        GameData.LoadSprites();

#if (UNITY_EDITOR)
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
        if (!writer.Exists("rootSet") || ignoreInitialCheck)
        {
            writer
                .Write("rootSet", true)
                //////// Values ////////
                .Write("experience", GameData.experience)
                .Write("level", GameData.level)
                .Write("energy", GameData.energy)
                .Write("gold", GameData.gold)
                .Write("gems", GameData.gems)
                //////// Items ////////
                /*.Write("itemsData", ConvertItemsToJson(Types.Type.Default))
                .Write("generatorsData", ConvertItemsToJson(Types.Type.Gen))*/
                .Write("boardData", ConvertInitialItemsToJson())
                .Write("unlockedData", GetInitialUnlocked())
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
        /*string newItemsData = "";
        string newGeneratorsData = "";*/
        string newBoardData = "";
        string newUnlockedData = "";

        if (initialLoad)
        {
            // Get json string from the initial json file
            /* newItemsData = itemsJsonData;
             newGeneratorsData = generatorJsonData;*/
            newBoardData = initialJsonData;
            newUnlockedData = unlockedJsonData;
        }
        else
        {
            // Get json string from the saved json file
            /*reader.Read<string>("itemsData", r => newItemsData = r);
            reader.Read<string>("generatorsData", r => newGeneratorsData = r);*/
            reader.Read<string>("boardData", r => newBoardData = r);
            reader.Read<string>("unlockedData", r => newUnlockedData = r);

            GameData.SetExperience(reader.Read<float>("experience"), true);
            GameData.SetLevel(reader.Read<int>("level"), true);
            GameData.SetEnergy(reader.Read<int>("energy"), true);
            GameData.SetGold(reader.Read<int>("gold"), true);
            GameData.SetGems(reader.Read<int>("gems"), true);

            values.UpdateValues();
        }

        // Get object from json
        /* Types.ItemsJson[] itemsDataJson = JsonConvert.DeserializeObject<Types.ItemsJson[]>(
             newItemsData
         );
         Types.GeneratorsJson[] generatorsDataJson =
             JsonConvert.DeserializeObject<Types.GeneratorsJson[]>(newGeneratorsData);*/
        Types.BoardJson[] boardDataJson = JsonConvert.DeserializeObject<Types.BoardJson[]>(
            newBoardData
        );
        string[] unlockedDataTemp = JsonConvert.DeserializeObject<string[]>(newUnlockedData);

        GameData.unlockedData.CopyTo(unlockedDataTemp, 0);
        GameData.unlockedData = unlockedDataTemp;

        // Convert data
        GameData.itemsData = ConvertItems(items.content);
        GameData.generatorsData = ConvertGenerators(generators.content);
        /*GameData.itemsData = jsonHandler.ConvertItemsFromJson(itemsDataJson);
        GameData.generatorsData = jsonHandler.ConvertGeneratorsFromJson(generatorsDataJson);*/
        GameData.boardData = jsonHandler.ConvertBoardFromJson(boardDataJson);

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

        for (int i = 0; i < GameData.unlockedData.Length; i++)
        {
            if (spriteName == GameData.unlockedData[i])
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
    public void UnlockItem(string spriteName)
    {
        bool found = false;

        for (int i = 0; i < GameData.unlockedData.Length; i++)
        {
            if (spriteName == GameData.unlockedData[i])
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            string[] newUnlockedData = new string[GameData.unlockedData.Length + 1];

            for (int i = 0; i < GameData.unlockedData.Length; i++)
            {
                newUnlockedData[i] = GameData.unlockedData[i];
            }

            newUnlockedData[GameData.unlockedData.Length] = spriteName;

            GameData.unlockedData.CopyTo(newUnlockedData, 0);
            GameData.unlockedData = newUnlockedData;
        }
    }

    //// SAVE ////

    // Save board data to disk
    public void SaveBoard()
    {
        string newBoardData = JsonConvert.SerializeObject(
            jsonHandler.ConvertBoardToJson(GameData.boardData)
        );

        writer.Write("boardData", newBoardData).Commit();
    }

    //// CONVERT ////

    /*sstring ConvertItemsToJson(Types.Type type)
    {
        witch (type)
        {
            case Types.Type.Default:
                itemsJsonData = JsonConvert.SerializeObject(
                    jsonHandler.ConvertItemsToJson(items.content)
                );
                return itemsJsonData;
            case Types.Type.Gen:
                generatorJsonData = JsonConvert.SerializeObject(
                    jsonHandler.ConvertGeneratorsToJson(generators.content)
                );
                return generatorJsonData;
        }

        return "";
    }*/

    string ConvertInitialItemsToJson()
    {
        initialJsonData = JsonConvert.SerializeObject(
            jsonHandler.ConvertBoardToJson(initialItems.content)
        );

        return initialJsonData;
    }

    //// OTHER ////

    [ContextMenu("Loop Board Data")]
    public void LoopBoardData()
    {
        for (int i = 0; i < GameData.boardData.Length; i++)
        {
            Debug.Log(GameData.boardData[i].sprite);
        }
    }
}
