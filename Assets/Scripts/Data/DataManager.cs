using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CI.QuickSave;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

namespace Merge
{
    public class DataManager : MonoBehaviour
    {
        // For testing only
        public bool ignoreInitialCheck = false;

        // Initial items
        public Items items;
        public Items generators;
        public Items collectables;
        public Items chests;
        public InitialItems initialItems;

        public delegate void BoardSaveEvent();
        public static event BoardSaveEvent boardSaveEvent;
        public delegate void CheckProgressEvent();
        public static event CheckProgressEvent checkProgressEvent;

        // Whether data has been fully loaded
        public bool loaded;

        private bool isEditor = false;

        // Quick Save
        private QuickSaveSettings saveSettings;
        public QuickSaveWriter writer;
        public QuickSaveReader reader;

        private JsonHandler jsonHandler;
        private GameData gameData;
        private TimeManager timeManager;
        private ApiCalls apiCalls;
        private DataConverter dataConverter;

        [HideInInspector]
        public string initialJsonData;
        private string bonusData;
        private string inventoryData;
        private string tasksData;
        [HideInInspector]
        public string finishedTasksJsonData;
        private string timersJsonData;
        [HideInInspector]
        public string unlockedJsonData;
        private string unsentJsonData;

        // Instance
        public static DataManager Instance;

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

        void Start()
        {
            // References
            gameData = GameData.Instance;
            apiCalls = ApiCalls.Instance;
            dataConverter = GetComponent<DataConverter>();
            jsonHandler = GetComponent<JsonHandler>();
            timeManager = gameData.GetComponent<TimeManager>();

            // Set up Quick Save
            saveSettings = new QuickSaveSettings() { CompressionMode = CompressionMode.None }; //TODO -  Set CompressionMode in the final game to Gzip
            writer = QuickSaveWriter.Create("Root", saveSettings);

            gameData.InitAlt();

#if UNITY_EDITOR
            isEditor = true;

            // Make this script run if we aren't starting from the Loading scene
            if (
                !loaded
                && (
                    SceneManager.GetActiveScene().name == "Gameplay"
                    || SceneManager.GetActiveScene().name == "Hub"
                )
            )
            {
                CheckInitialData();
            }

            if (PlayerPrefs.HasKey("userId"))
            {
                GameData.Instance.userId = PlayerPrefs.GetString("userId");
            }
#endif
        }

        // Check if we need to save initial data to disk
        public void CheckInitialData(Action callback = null)
        {
            if ((ignoreInitialCheck && isEditor) || (!PlayerPrefs.HasKey("Loaded") && !writer.Exists("rootSet")))
            {
                initialJsonData = jsonHandler.ConvertBoardToJson(initialItems.content, true);
                bonusData = jsonHandler.ConvertBonusToJson(gameData.bonusData);
                inventoryData = jsonHandler.ConvertInventoryToJson(gameData.inventoryData);
                tasksData = jsonHandler.ConvertTaskGroupsToJson(gameData.tasksData);
                timersJsonData = jsonHandler.ConvertTimersToJson(gameData.timers);
                unsentJsonData = jsonHandler.ConvertUnsentToJson(apiCalls.unsentData);
                finishedTasksJsonData = JsonConvert.SerializeObject(gameData.finishedTasks);

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
                    .Write("tasksData", tasksData)
                    .Write("finishedTasks", finishedTasksJsonData)
                    .Write("unlockedData", dataConverter.GetInitialUnlocked())
                    .Write("timers", timersJsonData)
                    // Other
                    .Write("inventorySpace", gameData.inventorySpace)
                    .Write("inventorySlotPrice", gameData.inventorySlotPrice)
                    .Write("unsentData", unsentJsonData)
                    .Commit();

                GetData(true, callback);
            }
            else
            {
                reader = QuickSaveReader.Create("Root", saveSettings);

                GetData(false, callback);
            }
        }

        // Get object data from the initial data
        void GetData(bool initialLoad, Action callback = null)
        {
            string newBoardData = "";
            string newBonusData = "";
            string newInventoryData = "";
            string newTasksData = "";
            string newFinishedTasks = "";
            string newTimersData = "";
            string newUnlockedData = "";
            string newUnsentData = "";

            if (initialLoad)
            {
                // Get json string from the initial json file
                newBoardData = initialJsonData;
                newBonusData = bonusData;
                newInventoryData = inventoryData;
                newTasksData = tasksData;
                newFinishedTasks = finishedTasksJsonData;
                newTimersData = timersJsonData;
                newUnlockedData = unlockedJsonData;
                newUnsentData = unsentJsonData;
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
                reader.Read<string>("tasksData", r => newTasksData = r);
                reader.Read<string>("finishedTasks", r => newFinishedTasks = r);
                reader.Read<string>("timers", r => newTimersData = r);
                reader.Read<string>("unlockedData", r => newUnlockedData = r);
                reader.Read<string>("unsentData", r => newUnsentData = r);

                // Other
                gameData.inventorySpace = reader.Read<int>("inventorySpace");
                gameData.inventorySlotPrice = reader.Read<int>("inventorySlotPrice");
            }

            string[] unlockedDataTemp = JsonConvert.DeserializeObject<string[]>(newUnlockedData);

            gameData.unlockedData.CopyTo(unlockedDataTemp, 0);
            gameData.unlockedData = unlockedDataTemp;

            gameData.finishedTasks = JsonConvert.DeserializeObject<List<string>>(newFinishedTasks);

            // Convert data
            gameData.itemsData = dataConverter.ConvertItems(items.content);
            gameData.generatorsData = dataConverter.ConvertItems(generators.content);
            gameData.collectablesData = dataConverter.ConvertItems(collectables.content);
            gameData.chestsData = dataConverter.ConvertItems(chests.content);

            gameData.timers = jsonHandler.ConvertTimersFromJson(newTimersData);
            gameData.boardData = dataConverter.ConvertArrayToBoard(jsonHandler.ConvertBoardFromJson(newBoardData));
            gameData.bonusData = jsonHandler.ConvertBonusFromJson(newBonusData);
            gameData.inventoryData = jsonHandler.ConvertInventoryFromJson(newInventoryData);
            gameData.tasksData = jsonHandler.ConvertTaskGroupsFromJson(newTasksData);

            apiCalls.unsentData = jsonHandler.ConvertUnsentFromJson(newUnsentData);

            apiCalls.canCheckForUnsent = true;

            //timeManager.CheckTimers();

            checkProgressEvent();

            // Finish Task
            loaded = true;

            if (initialLoad)
            {
                PlayerPrefs.SetInt("Loaded", 1);
                PlayerPrefs.Save();
            }

            if (callback != null)
            {
                callback();
            }
        }

        //// SAVE ////

        public void SaveBoard(bool fireEvent = true)
        {
            string newBoardData = jsonHandler.ConvertBoardToJson(
                dataConverter.ConvertBoardToArray(gameData.boardData)
            );

            writer.Write("boardData", newBoardData).Commit();

            if (fireEvent && boardSaveEvent != null)
            {
                boardSaveEvent();
            }
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

        public void SaveTasks()
        {
            string newTasksData = jsonHandler.ConvertTaskGroupsToJson(gameData.tasksData);

            writer.Write("tasksData", newTasksData).Commit();
        }

        public void SaveFinishedTasks()
        {
            string newFinishedTasksData = JsonConvert.SerializeObject(gameData.finishedTasks);

            writer.Write("finishedTasks", newFinishedTasksData).Commit();
        }

        public void SaveUnsentData()
        {
            string newUnsentData = jsonHandler.ConvertUnsentToJson(apiCalls.unsentData);

            writer.Write("unsentData", newUnsentData).Commit();
        }

        public void SaveInventory(bool saveSpace = false)
        {
            if (saveSpace)
            {
                writer.Write("inventorySpace", gameData.inventorySpace).Commit();
                writer.Write("inventorySlotPrice", gameData.inventorySlotPrice).Commit();
            }
            else
            {
                //inventorySlotPrice
                string newInventorySpace = jsonHandler.ConvertInventoryToJson(gameData.inventoryData);

                writer.Write("inventoryData", newInventorySpace).Commit();
            }
        }

        //// OTHER ////

        // Unlock item
        public bool UnlockItem(
            string spriteName,
            Types.Type type,
            ItemTypes.Group group,
            ItemTypes.GenGroup genGroup,
            Types.CollGroup collGroup,
            Types.ChestGroup chestGroup
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
                case Types.Type.Chest:
                    for (int i = 0; i < gameData.chestsData.Length; i++)
                    {
                        if (gameData.chestsData[i].chestGroup == chestGroup)
                        {
                            for (int j = 0; j < gameData.chestsData[i].content.Length; j++)
                            {
                                if (gameData.chestsData[i].content[j].sprite.name == spriteName)
                                {
                                    gameData.chestsData[i].content[j].unlocked = true;
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

        public int GetGroupCount(ItemTypes.Group group)
        {
            int count = 0;

            for (int i = 0; i < gameData.itemsData.Length; i++)
            {
                if (gameData.itemsData[i].group == group)
                {
                    count = gameData.itemsData[i].content.Length;
                }
            }

            return count;
        }
    }
}