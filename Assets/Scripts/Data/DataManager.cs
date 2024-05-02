using System;
using System.Collections;
using System.Collections.Generic;
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
        public Generators generators;
        public Colls colls;
        public Chests chests;
        public InitialItems initialItems;
        public InitialItems initialItemsSkip;

        // Events
        public delegate void BoardSaveEvent();
        public static event BoardSaveEvent OnBoardSave;
        public delegate void BoardSaveUndoEvent(bool unselect);
        public static event BoardSaveUndoEvent OnBoardSaveUndo;

        // Whether data has been fully loaded
        public bool loaded = false;

        private bool isEditor = false;

        private string[] saveDataKeys;

        // Quick Save
        private QuickSaveSettings saveSettings;
        private QuickSaveWriter writer;
        public QuickSaveReader reader;

        [HideInInspector]
        public string boardJsonData;
        private string bonusData;
        private string inventoryData;
        private string tasksJsonData;
        [HideInInspector]
        public string finishedTasksJsonData;
        private string timersJsonData;
        private string coolDownsJsonData;
        private string notificationsJsonData;
        [HideInInspector]
        public string unlockedJsonData;
        public string unlockedRoomsJsonData;

        // References
        private GameRefs gameRefs;
        private GameData gameData;
        private UIData uiData;
        private DataConverter dataConverter;
        private RateMenu rateMenu;
        private FollowMenu followMenu;
        private Services services;
        private CloudSave cloudSave;
        private ErrorManager errorManager;
        private WorldDataManager worldDataManager;

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

#if UNITY_EDITOR|| DEVELOPMENT_BUILD
                SetSaveDataKeys();
#endif

                // Set up Quick Save
                // Compression mode will be based on the game debug mode
                saveSettings = new QuickSaveSettings() { CompressionMode = Debug.isDebugBuild ? CompressionMode.None : CompressionMode.Gzip };
                writer = QuickSaveWriter.Create("Root", saveSettings);
            }
        }

        void Start()
        {
            // Cache
            gameRefs = GameRefs.Instance;
            gameData = GameData.Instance;
            uiData = gameData.GetComponent<UIData>();
            dataConverter = GetComponent<DataConverter>();
            rateMenu = gameRefs.rateMenu;
            followMenu = gameRefs.followMenu;
            services = Services.Instance;
            cloudSave = services.GetComponent<CloudSave>();
            errorManager = ErrorManager.Instance;
            worldDataManager = gameRefs.worldDataManager;

#if UNITY_EDITOR
            isEditor = true;

            string sceneName = SceneManager.GetActiveScene().name;

            // Make this script run if we aren't starting from the Loading scene
            if (!loaded && (sceneName == SceneLoader.SceneType.Merge.ToString() || sceneName == SceneLoader.SceneType.World.ToString()))
            {
                StartCoroutine(WaitForLoadedData());
            }
#endif
        }

        public void CheckForLoadedData(Action callback = null)
        {
            StartCoroutine(WaitForLoadedData(callback));
        }

        public IEnumerator WaitForLoadedData(Action callback = null)
        {
            while (!gameData.dataLoaded || !uiData.dataLoaded)
            {
                yield return null;
            }

            CheckInitialData(callback);
        }

        public void CheckLoaded(Action callback)
        {
            StartCoroutine(WaitForData(callback));
        }

        public IEnumerator WaitForData(Action callback)
        {
            while (!loaded)
            {
                yield return null;
            }

            callback();
        }

        // Check if we need to save initial data to disk
        void CheckInitialData(Action callback)
        {
            if ((ignoreInitialCheck && isEditor) || (!PlayerPrefs.HasKey("dataLoaded") && !writer.Exists("rootSet")))
            {
                boardJsonData = dataConverter.ConvertBoardToJson(PlayerPrefs.HasKey("tutorialFinished") ? initialItemsSkip.content : initialItems.content, true);
                bonusData = dataConverter.ConvertBonusToJson(gameData.bonusData);
                inventoryData = dataConverter.ConvertInventoryToJson(gameData.inventoryData);
                tasksJsonData = dataConverter.ConvertTaskGroupsToJson(gameData.tasksData);
                timersJsonData = dataConverter.ConvertTimersToJson(gameData.timers);
                coolDownsJsonData = dataConverter.ConvertCoolDownsToJson(gameData.coolDowns);
                notificationsJsonData = dataConverter.ConvertNotificationsToJson(gameData.notifications);
                finishedTasksJsonData = JsonConvert.SerializeObject(gameData.finishedTasks);

                Dictionary<string, object> playerData = new()
                {
                    //////// Main ////////
                    {"rootSet", true},
                    //////// Values ////////
                    {"experience", gameData.experience},
                    {"level", gameData.level},
                    {"energy", gameData.energy},
                    {"gold", gameData.gold},
                    {"gems", gameData.gems},
                    //////// Data ////////
                    {"boardData", boardJsonData},
                    {"bonusData", bonusData},
                    {"inventoryData", inventoryData},
                    {"tasksData", tasksJsonData},
                    {"finishedTasks", finishedTasksJsonData},
                    {"unlockedData", dataConverter.GetInitialUnlocked()},
                    {"unlockedRoomsData", worldDataManager != null ? worldDataManager.GetInitialUnlockedRooms() : "[]"},
                    {"timers", timersJsonData},
                    {"coolDowns", coolDownsJsonData},
                    {"notificationsData", notificationsJsonData},
                    {"inventorySpace", gameData.inventorySpace},
                    {"inventorySlotPrice", gameData.inventorySlotPrice}
                };

                foreach (var dataItem in playerData)
                {
                    writer.Write(dataItem.Key, dataItem.Value);
                }

                writer.Commit();

                cloudSave.SaveDataAsync(playerData);

                GetData(true, callback);
            }
            else
            {
                reader = QuickSaveReader.Create("Root", saveSettings);

                GetData(false, callback);
            }
        }

        // Get object data from the initial data
        void GetData(bool initialLoad, Action callback)
        {
            string newBoardData = "";
            string newBonusData = "";
            string newInventoryData = "";
            string newTasksData = "";
            string newFinishedTasks = "";
            string newTimersData = "";
            string newCoolDownsData = "";
            string newNotificationsData = "";
            string newUnlockedData = "";
            string newUnlockedRoomsData = "";

            if (initialLoad)
            {
                // Get json string from the initial json file
                newBoardData = boardJsonData;
                newBonusData = bonusData;
                newInventoryData = inventoryData;
                newTasksData = tasksJsonData;
                newFinishedTasks = finishedTasksJsonData;
                newTimersData = timersJsonData;
                newCoolDownsData = coolDownsJsonData;
                newNotificationsData = notificationsJsonData;
                newUnlockedData = unlockedJsonData;
                newUnlockedRoomsData = unlockedRoomsJsonData;
            }
            else
            {
                // ! - Level should come before experience
                gameData.SetLevel(reader.Read<int>("level"));
                gameData.SetExperience(reader.Read<int>("experience"));
                gameData.SetEnergy(reader.Read<int>("energy"));
                gameData.SetGold(reader.Read<int>("gold"));
                gameData.SetGems(reader.Read<int>("gems"));

                // Get json string from the saved json file
                reader.Read<string>("boardData", r => newBoardData = r);
                reader.Read<string>("bonusData", r => newBonusData = r);
                reader.Read<string>("inventoryData", r => newInventoryData = r);
                reader.Read<string>("tasksData", r => newTasksData = r);
                reader.Read<string>("finishedTasks", r => newFinishedTasks = r);
                reader.Read<string>("timers", r => newTimersData = r);
                reader.Read<string>("coolDowns", r => newCoolDownsData = r);
                reader.Read<string>("notificationsData", r => newNotificationsData = r);
                reader.Read<string>("unlockedData", r => newUnlockedData = r);
                reader.Read<string>("unlockedRoomsData", r => newUnlockedRoomsData = r);

                // Other
                gameData.inventorySpace = reader.Read<int>("inventorySpace");
                gameData.inventorySlotPrice = reader.Read<int>("inventorySlotPrice");
            }

            if (newUnlockedData != "")
            {
                string[] unlockedDataTemp = JsonConvert.DeserializeObject<string[]>(newUnlockedData);

                gameData.unlockedData.CopyTo(unlockedDataTemp, 0);
                gameData.unlockedData = unlockedDataTemp;
            }

            if (newUnlockedRoomsData != "")
            {
                string[] unlockedRoomsDataTemp = JsonConvert.DeserializeObject<string[]>(newUnlockedRoomsData);

                gameData.unlockedRoomsData.CopyTo(unlockedRoomsDataTemp, 0);
                gameData.unlockedRoomsData = unlockedRoomsDataTemp;
            }

            gameData.finishedTasks = JsonConvert.DeserializeObject<List<TaskManager.FinishedTask>>(newFinishedTasks);

            // Convert data
            gameData.itemsData = dataConverter.ConvertItems(items.content);
            gameData.generatorsData = dataConverter.ConvertItems(dataConverter.ConvertGensToItems(generators.content));
            gameData.collectablesData = dataConverter.ConvertItems(dataConverter.ConvertCollsToItems(colls.content));
            gameData.chestsData = dataConverter.ConvertItems(dataConverter.ConvertChestsToItems(chests.content));

            gameData.timers = dataConverter.ConvertTimersFromJson(newTimersData);
            gameData.coolDowns = dataConverter.ConvertCoolDownsFromJson(newCoolDownsData);
            gameData.notifications = dataConverter.ConvertNotificationsFromJson(newNotificationsData);
            gameData.boardData = dataConverter.ConvertArrayToBoard(dataConverter.ConvertBoardFromJson(newBoardData));
            gameData.bonusData = dataConverter.ConvertBonusFromJson(newBonusData);
            gameData.inventoryData = dataConverter.ConvertInventoryFromJson(newInventoryData);
            gameData.tasksData = dataConverter.ConvertTaskGroupsFromJson(newTasksData);

            if (PlayerPrefs.HasKey("playerName"))
            {
                gameData.playerName = PlayerPrefs.GetString("playerName");

                cloudSave.SaveDataAsync("playerName", gameData.playerName);
            }

            // Finish Task
            loaded = true;

            if (SceneManager.GetActiveScene().name == SceneLoader.SceneType.World.ToString())
            {
                if (rateMenu.shouldShow)
                {
                    rateMenu.Open();
                }

                if (followMenu.shouldShow)
                {
                    followMenu.Open();
                }
            }

            if (initialLoad)
            {
                PlayerPrefs.SetInt("dataLoaded", 1);
                PlayerPrefs.Save();

                cloudSave.SaveDataAsync("dataLoaded", 1);
            }

            callback?.Invoke();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        void SetSaveDataKeys()
        {
            saveDataKeys = new[]{
                //////// Main ////////
                "dataLoaded", // Prefs
                "rootSet",
                //////// Values ////////
                "experience",
                "level",
                "energy",
                "gold",
                "gems",
                //////// Data ////////
                "boardData",
                "bonusData",
                "inventoryData",
                "tasksData",
                "finishedTasks",
                "unlockedData",
                "unlockedRoomsData",
                "timers",
                "coolDowns",
                "notificationsData",
                "inventorySpace",
                "inventorySlotPrice",
                //////// World ////////
                "areaSet", // Prefs, Data
                "areas",
                "doorSet", // Prefs, Data
                "unlockedDoors",
                //////// Player Prefs ////////
                "playerName",
                "tutorialFinished",
                "tutorialStep",
                "initialTaskDataSet",
                "progressStep",
                "locale",
                "sound",
                "music",
                "vibration",
                "notifications",
                "followResult",
                "rateResult",
                "canLevelUp",
                "termsAccepted",
                "preMansionRemoved",
            };
        }

        void CheckSaveDataKey(string key)
        {
            bool found = false;

            for (int i = 0; i < saveDataKeys.Length; i++)
            {
                if (saveDataKeys[i] == key)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                // WARNING
                errorManager.ThrowWarning(
                    ErrorManager.ErrorType.Code,
                    GetType().Name,
                    "Save data key doesn't exist: " + key
                );
            }
        }
#endif

        //// SAVE ////

        public void SaveValue<T>(string key, T value, bool saveToCloud = true)
        {
            SaveValue(new Dictionary<string, object> { { key, value } }, saveToCloud);
        }

        public void SaveValue(Dictionary<string, object> values, bool saveToCloud = true)
        {
            foreach (var valueItem in values)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                CheckSaveDataKey(valueItem.Key);
#endif

                if (valueItem.Value is string || valueItem.Value is int || valueItem.Value is float || valueItem.Value is bool)
                {
                    writer.Write(valueItem.Key, valueItem.Value).Commit();
                }
                else
                {
                    // ERROR
                    errorManager.Throw(
                        ErrorManager.ErrorType.Code,
                        "DataManager.cs -> SaveValue()",
                        "Value is fo type: " + valueItem.Value + ". It needs to be converted to a STRING, INT, FLOAT or a BOOL!"
                    );
                }
            }

            if (saveToCloud)
            {
                cloudSave.SaveDataAsync(values);
            }
        }

        public void SaveBoard(bool fireEvent = true, bool fireEventForUndo = true)
        {
            string newBoardData = dataConverter.ConvertBoardToJson(
                dataConverter.ConvertBoardToArray(gameData.boardData)
            );

            writer.Write("boardData", newBoardData).Commit();

            cloudSave.SaveDataAsync("boardData", newBoardData);

            if (fireEvent)
            {
                OnBoardSave?.Invoke();
            }

            if (fireEventForUndo)
            {
                OnBoardSaveUndo?.Invoke(true);
            }
        }

        public void SaveTimers()
        {
            string newTimers = dataConverter.ConvertTimersToJson(gameData.timers);

            writer.Write("timers", newTimers).Commit();

            cloudSave.SaveDataAsync("timers", newTimers);
        }

        public void SaveCoolDowns()
        {
            string newCoolDowns = dataConverter.ConvertCoolDownsToJson(gameData.coolDowns);

            writer.Write("coolDowns", newCoolDowns).Commit();

            cloudSave.SaveDataAsync("coolDowns", newCoolDowns);
        }

        public void SaveNotifications()
        {
            string newNotifications = dataConverter.ConvertNotificationsToJson(gameData.notifications);

            writer.Write("notificationsData", newNotifications).Commit();

            cloudSave.SaveDataAsync("notificationsData", newNotifications);
        }

        public void SaveBonus()
        {
            string newBonusData = dataConverter.ConvertBonusToJson(gameData.bonusData);

            writer.Write("bonusData", newBonusData).Commit();

            cloudSave.SaveDataAsync("bonusData", newBonusData);
        }

        public void SaveTasks()
        {
            string newTasksData = dataConverter.ConvertTaskGroupsToJson(gameData.tasksData);

            writer.Write("tasksData", newTasksData).Commit();

            cloudSave.SaveDataAsync("tasksData", newTasksData);
        }

        public void SaveFinishedTasks()
        {
            string newFinishedTasksData = JsonConvert.SerializeObject(gameData.finishedTasks);

            writer.Write("finishedTasks", newFinishedTasksData).Commit();

            cloudSave.SaveDataAsync("finishedTasks", newFinishedTasksData);
        }

        public void SaveInventory(bool saveSpace = false)
        {
            if (saveSpace)
            {
                writer
                .Write("inventorySpace", gameData.inventorySpace)
                .Write("inventorySlotPrice", gameData.inventorySlotPrice)
                .Commit();

                cloudSave.SaveDataAsync(new(){
                    { "inventorySpace", gameData.inventorySpace },
                    { "inventorySlotPrice", gameData.inventorySlotPrice }
                });
            }
            else
            {
                //inventorySlotPrice
                string newInventorySpace = dataConverter.ConvertInventoryToJson(gameData.inventoryData);

                writer.Write("inventoryData", newInventorySpace).Commit();

                cloudSave.SaveDataAsync("inventoryData", newInventorySpace);
            }
        }

        public void SaveUnsentData(Dictionary<string, object> unsentData)
        {
            string newUnsentData = JsonConvert.SerializeObject(unsentData);

            writer.Write("unsentData", newUnsentData).Commit();
        }

        //// LOAD ////

        public T LoadValue<T>(string key)
        {
            T newData = default;

            if (reader != null)
            {
                reader.Read<T>(key, r => newData = r);
            }
            else
            {
                // ERROR
                errorManager.Throw(
                    ErrorManager.ErrorType.Code,
                    GetType().Name,
                    "Reader is null for key: " + key
                );
            }

            return newData;
        }

        public Dictionary<string, object> LoadUnsentData()
        {
            string unsentData = "";

            if (reader != null)
            {
                reader.Read<string>("unsentData", r => unsentData = r);
            }
            else
            {
                // ERROR
                errorManager.Throw(
                    ErrorManager.ErrorType.Code,
                    GetType().Name,
                    "Reader is null for key: unsentData"
                );
            }

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(unsentData);
        }

        //// SET ////

        public void SetValue(string key, object value)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            CheckSaveDataKey(key);
#endif

            string valueString = value.ToString();

            if (valueString != "Unity.Services.CloudSave.Internal.Http.JsonObject")
            {
                switch (key)
                {
                    //////// Main ////////
                    case "dataLoaded":
                        PlayerPrefs.SetInt("dataLoaded", 1);
                        break;
                    case "rootSet":
                        writer.Write(key, true).Commit();
                        break;
                    //////// Values ////////
                    case "experience":
                        gameData.experience = int.Parse(valueString);
                        writer.Write(key, int.Parse(valueString)).Commit();
                        break;
                    case "level":
                        gameData.level = int.Parse(valueString);
                        writer.Write(key, int.Parse(valueString)).Commit();
                        break;
                    case "energy":
                        gameData.energy = int.Parse(valueString);
                        writer.Write(key, int.Parse(valueString)).Commit();
                        break;
                    case "gold":
                        gameData.gold = int.Parse(valueString);
                        writer.Write(key, int.Parse(valueString)).Commit();
                        break;
                    case "gems":
                        gameData.gems = int.Parse(valueString);
                        writer.Write(key, int.Parse(valueString)).Commit();
                        break;
                    //////// Data ////////
                    case "boardData":
                        gameData.boardData = dataConverter.ConvertArrayToBoard(dataConverter.ConvertBoardFromJson(valueString));
                        writer.Write(key, valueString).Commit();
                        break;
                    case "bonusData":
                        gameData.bonusData = dataConverter.ConvertBonusFromJson(valueString);
                        writer.Write(key, valueString).Commit();
                        break;
                    case "inventoryData":
                        gameData.inventoryData = dataConverter.ConvertInventoryFromJson(valueString);
                        writer.Write(key, valueString).Commit();
                        break;
                    case "finishedTasks":
                        gameData.finishedTasks = JsonConvert.DeserializeObject<List<TaskManager.FinishedTask>>(valueString);
                        writer.Write(key, valueString).Commit();
                        break;
                    case "tasksData":
                        gameData.tasksData = dataConverter.ConvertTaskGroupsFromJson(valueString);
                        writer.Write(key, valueString).Commit();
                        break;
                    case "unlockedData":
                        string[] unlockedDataTemp = JsonConvert.DeserializeObject<string[]>(valueString);

                        gameData.unlockedData.CopyTo(unlockedDataTemp, 0);
                        gameData.unlockedData = unlockedDataTemp;

                        writer.Write(key, valueString).Commit();
                        break;
                    case "unlockedRoomsData":
                        string[] unlockedRoomsDataTemp = JsonConvert.DeserializeObject<string[]>(valueString);

                        gameData.unlockedRoomsData.CopyTo(unlockedRoomsDataTemp, 0);
                        gameData.unlockedRoomsData = unlockedRoomsDataTemp;

                        writer.Write(key, valueString).Commit();
                        break;
                    case "timers":
                        gameData.timers = dataConverter.ConvertTimersFromJson(valueString);
                        writer.Write(key, valueString).Commit();
                        break;
                    case "coolDowns":
                        gameData.coolDowns = dataConverter.ConvertCoolDownsFromJson(valueString);
                        writer.Write(key, valueString).Commit();
                        break;
                    case "notificationsData":
                        gameData.notifications = dataConverter.ConvertNotificationsFromJson(valueString);
                        writer.Write(key, valueString).Commit();
                        break;
                    case "inventorySpace":
                        gameData.inventorySpace = int.Parse(valueString);
                        writer.Write(key, int.Parse(valueString)).Commit();
                        break;
                    case "inventorySlotPrice":
                        gameData.inventorySlotPrice = int.Parse(valueString);
                        writer.Write(key, int.Parse(valueString)).Commit();
                        break;
                    //////// World ////////
                    case "areaSet":
                        writer.Write(key, true).Commit();
                        PlayerPrefs.SetInt("areaSet", 1);
                        break;
                    case "areas":
                        gameData.areasData = dataConverter.ConvertJsonToArea(valueString);
                        writer.Write(key, valueString).Commit();
                        break;
                    case "doorSet":
                        writer.Write(key, true).Commit();
                        PlayerPrefs.SetInt("doorSet", 1);
                        break;
                    case "unlockedDoors":
                        writer.Write(key, valueString).Commit();
                        break;
                    //////// Player Prefs ////////
                    case "playerName":
                        PlayerPrefs.SetString("playerName", valueString);
                        break;
                    case "tutorialFinished":
                        PlayerPrefs.SetInt("tutorialFinished", 1);
                        break;
                    case "tutorialStep":
                        PlayerPrefs.SetString("tutorialStep", valueString);
                        break;
                    case "initialTaskDataSet":
                        PlayerPrefs.SetInt("initialTaskDataSet", 1);
                        break;
                    case "progressStep":
                        PlayerPrefs.SetString("progressStep", valueString);
                        break;
                    case "sound":
                        PlayerPrefs.SetInt("sound", int.Parse(valueString));
                        break;
                    case "locale":
                        PlayerPrefs.SetString("locale", valueString);
                        break;
                    case "music":
                        PlayerPrefs.SetInt("music", int.Parse(valueString));
                        break;
                    case "vibration":
                        PlayerPrefs.SetInt("vibration", int.Parse(valueString));
                        break;
                    case "notifications":
                        PlayerPrefs.SetInt("notifications", int.Parse(valueString));
                        break;
                    case "followResult":
                        PlayerPrefs.SetInt("followResult", 1);
                        break;
                    case "rateResult":
                        PlayerPrefs.SetInt("rateResult", 1);
                        break;
                    case "canLevelUp":
                        PlayerPrefs.SetInt("canLevelUp", int.Parse(valueString));
                        break;
                    case "termsAccepted":
                        services.termsAccepted = true;

                        PlayerPrefs.SetInt("termsAccepted", 1);
                        break;
                    case "preMansionRemoved":
                        services.termsAccepted = true;

                        PlayerPrefs.SetInt("preMansionRemoved", 1);
                        break;
                    default:
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "Wrong key given: " + key
                        );
                        break;
                }
            }
            else
            {
                // ERROR
                errorManager.Throw(
                    ErrorManager.ErrorType.Code,
                    GetType().Name,
                    "Wrong value given for key: " + key + ". Value: " + valueString
                );
            }
        }

        public void FinishSettingValues()
        {
            writer.Commit();

            PlayerPrefs.Save();
        }

        //// OTHER ////

        // Unlock item
        public bool UnlockItem(
            string spriteName,
            Item.Type type,
            Item.Group group,
            Item.GenGroup genGroup,
            Item.CollGroup collGroup,
            Item.ChestGroup chestGroup
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
                case Item.Type.Item:
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
                case Item.Type.Gen:
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
                case Item.Type.Coll:
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
                case Item.Type.Chest:
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
                    // ERROR
                    ErrorManager.Instance.Throw(ErrorManager.ErrorType.Code, "DataManager.cs -> UnlockItem()", "Wrong type: " + type);
                    break;
            }

            return found;
        }

        public bool UnlockRoom(string roomName)
        {
            bool found = false;

            for (int i = 0; i < gameData.unlockedRoomsData.Length; i++)
            {
                if (roomName == gameData.unlockedRoomsData[i])
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                string[] newUnlockedRoomsData = new string[gameData.unlockedRoomsData.Length + 1];

                for (int i = 0; i < gameData.unlockedRoomsData.Length; i++)
                {
                    newUnlockedRoomsData[i] = gameData.unlockedRoomsData[i];
                }

                newUnlockedRoomsData[gameData.unlockedRoomsData.Length] = roomName;

                gameData.unlockedRoomsData.CopyTo(newUnlockedRoomsData, 0);
                gameData.unlockedRoomsData = newUnlockedRoomsData;

                writer
                    .Write("unlockedRoomsData", JsonConvert.SerializeObject(gameData.unlockedData))
                    .Commit();
            }

            for (int i = 0; i < gameData.areasData.Count; i++)
            {
                if (gameData.areasData[i].name == roomName && gameData.areasData[i].isLocked)
                {
                    gameData.areasData[i].isLocked = false;

                    writer
                        .Write("areas", dataConverter.ConvertAreaToJson(gameData.areasData))
                        .Commit();

                    break;
                }
            }

            return found;
        }

        public int GetGroupItemsCount(Item.Group group)
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