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
        public Generators generators;
        public Colls colls;
        public Chests chests;
        public InitialItems initialItems;
        public WorldDataManager worldDataManager;

        public delegate void BoardSaveEvent();
        public static event BoardSaveEvent BoardSaveEventAction;
        public delegate void BoardSaveUndoEvent(bool unselect);
        public static event BoardSaveUndoEvent BoardSaveUndoEventAction;
        public delegate void CheckProgressEvent();
        public static event CheckProgressEvent CheckProgressEventAction;

        // Whether data has been fully loaded
        public bool loaded;

        private bool isEditor = false;

        // Quick Save
        private QuickSaveSettings saveSettings;
        public QuickSaveWriter writer;
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
        private string unsentJsonData;

        // References
        private ApiCalls apiCalls;
        private GameData gameData;
        private DataConverter dataConverter;
        private RateMenu rateMenu;
        private FollowMenu followMenu;

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
            // Cache
            gameData = GameData.Instance;
            apiCalls = ApiCalls.Instance;
            dataConverter = GetComponent<DataConverter>();
            rateMenu = GameRefs.Instance.rateMenu;
            followMenu = GameRefs.Instance.followMenu;

            // Set up Quick Save
            saveSettings = new QuickSaveSettings() { CompressionMode = CompressionMode.None }; // TODO -  Set CompressionMode in the final game to Gzip
            writer = QuickSaveWriter.Create("Root", saveSettings);

#if UNITY_EDITOR
            isEditor = true;

            string sceneName = SceneManager.GetActiveScene().name;

            // Make this script run if we aren't starting from the Loading scene
            if (!loaded && (sceneName == "Gameplay" || sceneName == "Hub"))
            {
                StartCoroutine(WaitForLoadedResources());
            }

            if (PlayerPrefs.HasKey("userId"))
            {
                GameData.Instance.userId = PlayerPrefs.GetString("userId");
            }
#endif
        }

        public void CheckForLoadedResources(Action callback = null)
        {
            StartCoroutine(WaitForLoadedResources(callback));
        }

        public IEnumerator WaitForLoadedResources(Action callback = null)
        {
            while (!gameData.resourcesLoaded)
            {
                yield return null;
            }

            CheckInitialData(callback);
        }

        // Check if we need to save initial data to disk
        void CheckInitialData(Action callback)
        {
            if ((ignoreInitialCheck && isEditor) || (!PlayerPrefs.HasKey("Loaded") && !writer.Exists("rootSet")))
            {
                boardJsonData = dataConverter.ConvertBoardToJson(initialItems.content, true);
                bonusData = dataConverter.ConvertBonusToJson(gameData.bonusData);
                inventoryData = dataConverter.ConvertInventoryToJson(gameData.inventoryData);
                tasksJsonData = dataConverter.ConvertTaskGroupsToJson(gameData.tasksData);
                timersJsonData = dataConverter.ConvertTimersToJson(gameData.timers);
                coolDownsJsonData = dataConverter.ConvertCoolDownsToJson(gameData.coolDowns);
                notificationsJsonData = dataConverter.ConvertNotificationsToJson(gameData.notifications);
                unsentJsonData = dataConverter.ConvertUnsentToJson(apiCalls.unsentData);
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
                    .Write("boardData", boardJsonData)
                    .Write("bonusData", bonusData)
                    .Write("inventoryData", inventoryData)
                    .Write("tasksData", tasksJsonData)
                    .Write("finishedTasks", finishedTasksJsonData)
                    .Write("unlockedData", dataConverter.GetInitialUnlocked())
                    .Write("unlockedRoomsData", worldDataManager != null ? worldDataManager.GetInitialUnlockedRooms() : "[]")
                    .Write("timers", timersJsonData)
                    .Write("coolDowns", coolDownsJsonData)
                    .Write("notificationsData", notificationsJsonData)
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
            string newUnsentData = "";

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
                newUnsentData = unsentJsonData;
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
                reader.Read<string>("unsentData", r => newUnsentData = r);

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

            gameData.finishedTasks = JsonConvert.DeserializeObject<List<Types.FinishedTask>>(newFinishedTasks);

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

            apiCalls.unsentData = dataConverter.ConvertUnsentFromJson(newUnsentData);

            apiCalls.canCheckForUnsent = true;

            CheckProgressEventAction();

            // Finish Task
            loaded = true;

            if (SceneManager.GetActiveScene().name == "Hub")
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
                PlayerPrefs.SetInt("Loaded", 1);
                PlayerPrefs.Save();
            }

            callback?.Invoke();
        }

        //// SAVE ////

        public void SaveBoard(bool fireEvent = true, bool fireEventForUndo = true)
        {
            string newBoardData = dataConverter.ConvertBoardToJson(
                dataConverter.ConvertBoardToArray(gameData.boardData)
            );

            writer.Write("boardData", newBoardData).Commit();

            if (fireEvent && BoardSaveEventAction != null)
            {
                BoardSaveEventAction();
            }

            if (fireEventForUndo && BoardSaveUndoEventAction != null)
            {
                BoardSaveUndoEventAction(true);
            }
        }

        public void SaveTimers()
        {
            string newTimers = dataConverter.ConvertTimersToJson(gameData.timers);

            writer.Write("timers", newTimers).Commit();
        }

        public void SaveCollDowns()
        {
            string newCoolDowns = dataConverter.ConvertCoolDownsToJson(gameData.coolDowns);

            writer.Write("coolDowns", newCoolDowns).Commit();
        }

        public void SaveNotifications()
        {
            string newNotifications = dataConverter.ConvertNotificationsToJson(gameData.notifications);

            writer.Write("notificationsData", newNotifications).Commit();
        }

        public void SaveBonus()
        {
            string newBonusData = dataConverter.ConvertBonusToJson(gameData.bonusData);

            writer.Write("bonusData", newBonusData).Commit();
        }

        public void SaveTasks()
        {
            string newTasksData = dataConverter.ConvertTaskGroupsToJson(gameData.tasksData);

            writer.Write("tasksData", newTasksData).Commit();
        }

        public void SaveFinishedTasks()
        {
            string newFinishedTasksData = JsonConvert.SerializeObject(gameData.finishedTasks);

            writer.Write("finishedTasks", newFinishedTasksData).Commit();
        }

        public void SaveUnsentData()
        {
            string newUnsentData = dataConverter.ConvertUnsentToJson(apiCalls.unsentData);

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
                string newInventorySpace = dataConverter.ConvertInventoryToJson(gameData.inventoryData);

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