using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Merge
{
    public class DataConverter : MonoBehaviour
    {
        // References
        private GameData gameData;
        private DataManager dataManager;
        private ItemHandler itemHandler;
        private I18n LOCALE;

        [Serializable]
        public class InitialItemData
        {
            public string sprite;
            public string type;
            public string state;
            public string group;
            public string genGroup;
            public string collGroup;
            public string chestGroup;
        }

        void Start()
        {
            // Cache
            gameData = GameData.Instance;
            dataManager = DataManager.Instance;
            LOCALE = I18n.Instance;

            if (dataManager != null)
            {
                itemHandler = dataManager.GetComponent<ItemHandler>();
            }
        }

        //// INITIAL ITEMS ////

        public BoardManager.Tile[] ConvertInitialItemsToBoard(string initialJson)
        {
            InitialItemData[] initialItemDataJson = JsonConvert.DeserializeObject<InitialItemData[]>(initialJson);

            BoardManager.Tile[] boardData = new BoardManager.Tile[initialItemDataJson.Length];

            for (int i = 0; i < initialItemDataJson.Length; i++)
            {
                Item.Type newType = Glob.ParseEnum<Item.Type>(initialItemDataJson[i].type);

                BoardManager.Tile newBoardData = new()
                {
                    sprite = initialItemDataJson[i].sprite == "" ? null : gameData.GetSprite(initialItemDataJson[i].sprite, newType),
                    state = Glob.ParseEnum<Item.State>(initialItemDataJson[i].state),
                    type = newType,
                    id = Guid.NewGuid().ToString(),
                    group = Glob.ParseEnum<Item.Group>(initialItemDataJson[i].group),
                    genGroup = Glob.ParseEnum<Item.GenGroup>(initialItemDataJson[i].genGroup),
                    collGroup = Glob.ParseEnum<Item.CollGroup>(initialItemDataJson[i].collGroup),
                    chestGroup = Glob.ParseEnum<Item.ChestGroup>(initialItemDataJson[i].chestGroup),
                };

                boardData[i] = newBoardData;
            }

            return boardData;
        }

        public string ConvertBoardToInitialItems(BoardManager.Tile[] boardData)
        {
            InitialItemData[] initialItemDataJson = new InitialItemData[boardData.Length];

            for (int i = 0; i < boardData.Length; i++)
            {
                InitialItemData newInitialItemJson = new()
                {
                    sprite = boardData[i].sprite == null ? "" : boardData[i].sprite.name,
                    state = boardData[i].state.ToString(),
                    type = boardData[i].type.ToString(),
                    group = boardData[i].group.ToString(),
                    genGroup = boardData[i].genGroup.ToString(),
                    collGroup = boardData[i].collGroup.ToString(),
                    chestGroup = boardData[i].chestGroup.ToString(),
                };

                initialItemDataJson[i] = newInitialItemJson;
            }

            return JsonConvert.SerializeObject(initialItemDataJson);
        }

        //// BOARD ////

        public BoardManager.Tile[] ConvertBoardFromJson(string boardString)
        {
            BoardManager.TileJson[] tileJson = JsonConvert.DeserializeObject<BoardManager.TileJson[]>(boardString);

            BoardManager.Tile[] boardData = new BoardManager.Tile[tileJson.Length];

            for (int i = 0; i < tileJson.Length; i++)
            {
                Item.Type newType = Glob.ParseEnum<Item.Type>(tileJson[i].type);

                BoardManager.Tile newBoardData = new()
                {
                    sprite = gameData.GetSprite(tileJson[i].sprite, newType),
                    state = Glob.ParseEnum<Item.State>(tileJson[i].state),
                    type = newType,
                    group = Glob.ParseEnum<Item.Group>(tileJson[i].group),
                    genGroup = Glob.ParseEnum<Item.GenGroup>(tileJson[i].genGroup),
                    collGroup = Glob.ParseEnum<Item.CollGroup>(tileJson[i].collGroup),
                    chestGroup = Glob.ParseEnum<Item.ChestGroup>(tileJson[i].chestGroup),
                    chestItems = tileJson[i].chestItems,
                    chestItemsSet = tileJson[i].chestItemsSet,
                    chestOpen = tileJson[i].chestOpen,
                    id = tileJson[i].id,
                    generatesAtLevel = tileJson[i].generatesAtLevel,
                    crate = tileJson[i].crate,
                    gemPopped = tileJson[i].gemPopped,
                    isCompleted = tileJson[i].isCompleted,
                    timerOn = tileJson[i].timerOn,
                };

                boardData[i] = newBoardData;
            }

            return boardData;
        }

        public string ConvertBoardToJson(BoardManager.Tile[] boardData, bool initialLoop = false)
        {
            BoardManager.TileJson[] tileJson = new BoardManager.TileJson[boardData.Length];

            for (int i = 0; i < boardData.Length; i++)
            {
                int randomInt = UnityEngine.Random.Range(0, itemHandler.crateSprites.Length);

                BoardManager.TileJson newTileJson = new()
                {
                    sprite = boardData[i].sprite == null ? "" : boardData[i].sprite.name,
                    state = boardData[i].state.ToString(),
                    type = boardData[i].type.ToString(),
                    group = boardData[i].group.ToString(),
                    genGroup = boardData[i].genGroup.ToString(),
                    collGroup = boardData[i].collGroup.ToString(),
                    chestGroup = boardData[i].chestGroup.ToString(),
                    chestItems = boardData[i].chestItems,
                    id = boardData[i].id,
                    generatesAtLevel = boardData[i].generatesAtLevel,
                    chestItemsSet = boardData[i].chestItemsSet,
                    chestOpen = boardData[i].chestOpen,
                    crate = initialLoop ? randomInt : boardData[i].crate,
                    gemPopped = boardData[i].gemPopped,
                    isCompleted = boardData[i].isCompleted,
                    timerOn = boardData[i].timerOn,
                };

                tileJson[i] = newTileJson;
            }

            return JsonConvert.SerializeObject(tileJson);
        }

        public BoardManager.Tile[,] ConvertArrayToBoard(BoardManager.Tile[] boardArray)
        {
            BoardManager.Tile[,] newBoardData = new BoardManager.Tile[GameData.WIDTH, GameData.HEIGHT];

            int count = 0;

            for (int i = 0; i < GameData.WIDTH; i++)
            {
                for (int j = 0; j < GameData.HEIGHT; j++)
                {
                    newBoardData[i, j] = new BoardManager.Tile
                    {
                        sprite = boardArray[count].sprite,
                        type = boardArray[count].type,
                        group = boardArray[count].group,
                        genGroup = boardArray[count].genGroup,
                        collGroup = boardArray[count].collGroup,
                        chestGroup = boardArray[count].chestGroup,
                        state = boardArray[count].state,
                        crate = boardArray[count].crate,
                        order = count,
                        chestItems = boardArray[count].chestItems,
                        chestItemsSet = boardArray[count].chestItemsSet,
                        chestOpen = boardArray[count].chestOpen,
                        generatesAtLevel = boardArray[count].generatesAtLevel,
                        id = boardArray[count].id,
                        gemPopped = boardArray[count].gemPopped,
                        isCompleted = boardArray[count].isCompleted,
                        timerOn = boardArray[count].timerOn,
                    };

                    count++;
                }
            }

            return newBoardData;
        }

        public BoardManager.Tile[] ConvertBoardToArray(BoardManager.Tile[,] boardData)
        {
            BoardManager.Tile[] newBoardArray = new BoardManager.Tile[GameData.ITEM_COUNT];

            int count = 0;

            foreach (BoardManager.Tile boardItem in boardData)
            {
                newBoardArray[count] = new BoardManager.Tile
                {
                    sprite = boardItem.sprite,
                    type = boardItem.type,
                    group = boardItem.group,
                    genGroup = boardItem.genGroup,
                    collGroup = boardItem.collGroup,
                    chestGroup = boardItem.chestGroup,
                    state = boardItem.state,
                    crate = boardItem.crate,
                    chestItems = boardItem.chestItems,
                    chestItemsSet = boardItem.chestItemsSet,
                    chestOpen = boardItem.chestOpen,
                    id = boardItem.id,
                    generatesAtLevel = boardItem.generatesAtLevel,
                    gemPopped = boardItem.gemPopped,
                    isCompleted = boardItem.isCompleted,
                    timerOn = boardItem.timerOn,
                };

                count++;
            }

            return newBoardArray;
        }

        //// BONUS ////

        public List<BonusManager.Bonus> ConvertBonusFromJson(string bonusString)
        {
            BonusManager.BonusJson[] bonusJson = JsonConvert.DeserializeObject<BonusManager.BonusJson[]>(bonusString);

            List<BonusManager.Bonus> bonusData = new();

            for (int i = 0; i < bonusJson.Length; i++)
            {
                Item.Type newType = Glob.ParseEnum<Item.Type>(bonusJson[i].type);

                BonusManager.Bonus newBonusData = new()
                {
                    sprite = gameData.GetSprite(bonusJson[i].sprite, newType),
                    type = newType,
                    group = Glob.ParseEnum<Item.Group>(bonusJson[i].group),
                    genGroup = Glob.ParseEnum<Item.GenGroup>(bonusJson[i].genGroup)
                };

                bonusData.Add(newBonusData);
            }

            return bonusData;
        }

        public string ConvertBonusToJson(List<BonusManager.Bonus> bonusData)
        {
            BonusManager.BonusJson[] bonusJson = new BonusManager.BonusJson[bonusData.Count];

            for (int i = 0; i < bonusData.Count; i++)
            {
                BonusManager.BonusJson newBonusJson = new()
                {
                    sprite = bonusData[i].sprite == null ? "" : bonusData[i].sprite.name,
                    type = bonusData[i].type.ToString(),
                    group = bonusData[i].group.ToString(),
                    genGroup = bonusData[i].genGroup.ToString(),
                };

                bonusJson[i] = newBonusJson;
            }

            return JsonConvert.SerializeObject(bonusJson);
        }

        //// INVENTORY ////

        public List<InventoryMenu.Inventory> ConvertInventoryFromJson(string inventoryString)
        {
            InventoryMenu.InventoryJson[] inventoryJson = JsonConvert.DeserializeObject<InventoryMenu.InventoryJson[]>(
                inventoryString
            );

            List<InventoryMenu.Inventory> inventoryData = new();

            for (int i = 0; i < inventoryJson.Length; i++)
            {
                Item.Type newType = Glob.ParseEnum<Item.Type>(inventoryJson[i].type);

                InventoryMenu.Inventory newInventoryData = new()
                {
                    sprite = gameData.GetSprite(inventoryJson[i].sprite, newType),
                    type = newType,
                    group = Glob.ParseEnum<Item.Group>(inventoryJson[i].group),
                    genGroup = Glob.ParseEnum<Item.GenGroup>(inventoryJson[i].genGroup),
                    chestGroup = Glob.ParseEnum<Item.ChestGroup>(inventoryJson[i].chestGroup),
                    id = inventoryJson[i].id,
                    isCompleted = inventoryJson[i].isCompleted,
                    timerOn = inventoryJson[i].timerOn,
                    timerAltTime = System.DateTime.Parse(inventoryJson[i].timerAltTime),
                    gemPopped = inventoryJson[i].gemPopped
                };

                inventoryData.Add(newInventoryData);
            }

            return inventoryData;
        }

        public string ConvertInventoryToJson(List<InventoryMenu.Inventory> inventoryData)
        {
            InventoryMenu.InventoryJson[] inventoryJson = new InventoryMenu.InventoryJson[inventoryData.Count];

            for (int i = 0; i < inventoryData.Count; i++)
            {
                InventoryMenu.InventoryJson newInventoryJson = new()
                {
                    sprite = inventoryData[i].sprite == null ? "" : inventoryData[i].sprite.name,
                    type = inventoryData[i].type.ToString(),
                    group = inventoryData[i].group.ToString(),
                    genGroup = inventoryData[i].genGroup.ToString(),
                    chestGroup = inventoryData[i].chestGroup.ToString(),
                    id = inventoryData[i].id,
                    isCompleted = inventoryData[i].isCompleted,
                    timerOn = inventoryData[i].timerOn,
                    timerAltTime = inventoryData[i].timerAltTime.ToString(),
                    gemPopped = inventoryData[i].gemPopped
                };

                inventoryJson[i] = newInventoryJson;
            }

            return JsonConvert.SerializeObject(inventoryJson);
        }

        //// AREAS ////

        public List<WorldDataManager.Area> ConvertJsonToArea(string areasString)
        {
            WorldDataManager.AreaJson[] areaJson = JsonConvert.DeserializeObject<WorldDataManager.AreaJson[]>(areasString);

            List<WorldDataManager.Area> areasData = new();

            for (int i = 0; i < areaJson.Length; i++)
            {
                WorldDataManager.Area newAreasData = new()
                {
                    name = areaJson[i].name,
                    isLocked = areaJson[i].isLocked,
                    isRoom = areaJson[i].isRoom,
                    wallLeftOrder = areaJson[i].wallLeftOrder,
                    wallRightOrder = areaJson[i].wallRightOrder,
                    floorOrder = areaJson[i].floorOrder,
                    furniture = JsonConvert.DeserializeObject<List<WorldDataManager.Furniture>>(areaJson[i].furniture),
                    props = JsonConvert.DeserializeObject<List<WorldDataManager.Prop>>(areaJson[i].props),
                    filth = JsonConvert.DeserializeObject<List<WorldDataManager.Filth>>(areaJson[i].filth),
                };

                areasData.Add(newAreasData);
            }

            return areasData;
        }

        public string ConvertAreaToJson(List<WorldDataManager.Area> areasData)
        {
            WorldDataManager.AreaJson[] areaJson = new WorldDataManager.AreaJson[areasData.Count];

            for (int i = 0; i < areasData.Count; i++)
            {
                WorldDataManager.AreaJson newAreaJson = new()
                {
                    name = areasData[i].name,
                    isLocked = areasData[i].isLocked,
                    isRoom = areasData[i].isRoom,
                    wallLeftOrder = areasData[i].wallLeftOrder,
                    wallRightOrder = areasData[i].wallRightOrder,
                    floorOrder = areasData[i].floorOrder,
                    furniture = JsonConvert.SerializeObject(areasData[i].furniture),
                    props = JsonConvert.SerializeObject(areasData[i].props),
                    filth = JsonConvert.SerializeObject(areasData[i].filth),
                };

                areaJson[i] = newAreaJson;
            }

            return JsonConvert.SerializeObject(areaJson);
        }

        //// TASKS ////
        public List<TaskManager.TaskGroup> ConvertTaskGroupsFromJson(string tasksString)
        {
            TaskManager.TaskGroupJson[] tasksJson = JsonConvert.DeserializeObject<TaskManager.TaskGroupJson[]>(
                tasksString
            );

            List<TaskManager.TaskGroup> tasksData = new();

            for (int i = 0; i < tasksJson.Length; i++)
            {
                TaskManager.TaskGroup newTasksData = new()
                {
                    id = tasksJson[i].id,
                    tasks = ConvertTasksFromJson(tasksJson[i].tasks),
                    total = tasksJson[i].total,
                    completed = tasksJson[i].completed,
                };

                tasksData.Add(newTasksData);
            }

            return tasksData;
        }

        public string ConvertTaskGroupsToJson(List<TaskManager.TaskGroup> tasksData)
        {
            TaskManager.TaskGroupJson[] tasksJson = new TaskManager.TaskGroupJson[tasksData.Count];

            for (int i = 0; i < tasksData.Count; i++)
            {
                TaskManager.TaskGroupJson newTasksJson = new()
                {
                    id = tasksData[i].id,
                    tasks = ConvertTasksToJson(tasksData[i].tasks),
                    total = tasksData[i].total,
                    completed = tasksData[i].completed,
                };

                tasksJson[i] = newTasksJson;
            }

            return JsonConvert.SerializeObject(tasksJson);
        }

        List<TaskManager.Task> ConvertTasksFromJson(string tasksString)
        {
            TaskManager.TaskJson[] tasksJson = JsonConvert.DeserializeObject<TaskManager.TaskJson[]>(
                tasksString
            );

            List<TaskManager.Task> tasksData = new();

            for (int i = 0; i < tasksJson.Length; i++)
            {
                TaskManager.Task newTasksData = new()
                {
                    needs = ConvertTaskItemFromJson(tasksJson[i].needs),
                    rewards = ConvertTaskItemFromJson(tasksJson[i].rewards),
                    id = tasksJson[i].id,
                    taskRefName = tasksJson[i].taskRefName,
                    taskRefType = Glob.ParseEnum<TaskManager.TaskRefType>(tasksJson[i].taskRefType),
                    isTaskRefRight = tasksJson[i].isTaskRefRight,
                    completed = tasksJson[i].completed,
                };

                tasksData.Add(newTasksData);
            }

            return tasksData;
        }

        string ConvertTasksToJson(List<TaskManager.Task> tasksData)
        {
            TaskManager.TaskJson[] taskJson = new TaskManager.TaskJson[tasksData.Count];

            for (int i = 0; i < tasksData.Count; i++)
            {
                TaskManager.TaskJson newTaskJson = new()
                {
                    needs = ConvertTaskItemToJson(tasksData[i].needs),
                    rewards = ConvertTaskItemToJson(tasksData[i].rewards),
                    id = tasksData[i].id,
                    taskRefName = tasksData[i].taskRefName,
                    taskRefType = tasksData[i].taskRefType.ToString(),
                    isTaskRefRight = tasksData[i].isTaskRefRight,
                    completed = tasksData[i].completed,
                };

                taskJson[i] = newTaskJson;
            }

            return JsonConvert.SerializeObject(taskJson);
        }

        TaskManager.TaskItem[] ConvertTaskItemFromJson(string itemsString)
        {
            TaskManager.TaskItemJson[] itemsJson = JsonConvert.DeserializeObject<TaskManager.TaskItemJson[]>(
                itemsString
            );

            TaskManager.TaskItem[] itemsData = new TaskManager.TaskItem[itemsJson.Length];

            for (int i = 0; i < itemsJson.Length; i++)
            {
                Item.Type newType = Glob.ParseEnum<Item.Type>(itemsJson[i].type);

                TaskManager.TaskItem newItemsData = new()
                {
                    sprite = gameData.GetSprite(itemsJson[i].sprite, newType),
                    type = newType,
                    group = Glob.ParseEnum<Item.Group>(itemsJson[i].group),
                    genGroup = Glob.ParseEnum<Item.GenGroup>(itemsJson[i].genGroup),
                    collGroup = Glob.ParseEnum<Item.CollGroup>(itemsJson[i].collGroup),
                    chestGroup = Glob.ParseEnum<Item.ChestGroup>(itemsJson[i].chestGroup),
                    amount = itemsJson[i].amount,
                    completed = itemsJson[i].completed,
                };

                itemsData[i] = newItemsData;
            }

            return itemsData;
        }

        string ConvertTaskItemToJson(TaskManager.TaskItem[] itemsData)
        {
            TaskManager.TaskItemJson[] itemsJson = new TaskManager.TaskItemJson[itemsData.Length];

            for (int i = 0; i < itemsData.Length; i++)
            {
                TaskManager.TaskItemJson newItemsJson = new()
                {
                    sprite = itemsData[i].sprite.name,
                    type = itemsData[i].type.ToString(),
                    group = itemsData[i].group.ToString(),
                    genGroup = itemsData[i].genGroup.ToString(),
                    collGroup = itemsData[i].collGroup.ToString(),
                    chestGroup = itemsData[i].chestGroup.ToString(),
                    amount = itemsData[i].amount,
                    completed = itemsData[i].completed,
                };

                itemsJson[i] = newItemsJson;
            }

            return JsonConvert.SerializeObject(itemsJson);
        }

        //// TIMERS ////

        public List<TimeManager.Timer> ConvertTimersFromJson(string timersString)
        {
            List<TimeManager.Timer> timers = new();

            if (timersString.Contains("[") || timersString == "")
            {
                TimeManager.TimerJson[] timerJson = JsonConvert.DeserializeObject<TimeManager.TimerJson[]>(
                    timersString
                );

                for (int i = 0; i < timerJson.Length; i++)
                {
                    timers.Add(
                        new TimeManager.Timer
                        {
                            startTime = System.DateTime.Parse(timerJson[i].startTime),
                            seconds = timerJson[i].seconds,
                            running = timerJson[i].running,
                            timerType = Glob.ParseEnum<TimeManager.TimerType>(timerJson[i].timerType),
                            id = timerJson[i].id,
                            notificationId = timerJson[i].notificationId,
                            notificationType = Glob.ParseEnum<NotificsManager.NotificationType>(timerJson[i].notificationType),
                        }
                    );
                }
            }
            else
            {
                Debug.LogError("timersString is wrong!!!");
                Debug.LogError(timersString);
            }

            return timers;
        }

        public string ConvertTimersToJson(List<TimeManager.Timer> timers)
        {
            TimeManager.TimerJson[] timerJson = new TimeManager.TimerJson[timers.Count];

            for (int i = 0; i < timers.Count; i++)
            {
                TimeManager.TimerJson newTimerJson = new()
                {
                    startTime = timers[i].startTime.ToString(),
                    seconds = timers[i].seconds,
                    running = timers[i].running,
                    timerType = timers[i].timerType.ToString(),
                    id = timers[i].id,
                    notificationId = timers[i].notificationId,
                    notificationType = timers[i].notificationType.ToString(),
                };

                timerJson[i] = newTimerJson;
            }

            return JsonConvert.SerializeObject(timerJson);
        }

        //// COOL DOWNS ////

        public List<TimeManager.CoolDownCount> ConvertCoolDownsFromJson(string coolDownsString)
        {
            List<TimeManager.CoolDownCount> coolDowns = new();

            TimeManager.CoolDownCountJson[] timerJson = JsonConvert.DeserializeObject<TimeManager.CoolDownCountJson[]>(
                coolDownsString
            );

            for (int i = 0; i < timerJson.Length; i++)
            {
                coolDowns.Add(
                    new TimeManager.CoolDownCount
                    {
                        level = timerJson[i].level,
                        count = timerJson[i].count,
                        id = timerJson[i].id,
                    }
                );
            }

            return coolDowns;
        }

        public string ConvertCoolDownsToJson(List<TimeManager.CoolDownCount> coolDowns)
        {
            TimeManager.CoolDownCountJson[] coolDownJson = new TimeManager.CoolDownCountJson[coolDowns.Count];

            for (int i = 0; i < coolDowns.Count; i++)
            {
                TimeManager.CoolDownCountJson neCoolDownsJson = new()
                {
                    level = coolDowns[i].level,
                    count = coolDowns[i].count,
                    id = coolDowns[i].id,
                };

                coolDownJson[i] = neCoolDownsJson;
            }

            return JsonConvert.SerializeObject(coolDownJson);
        }

        //// NOTIFICATIONS ////

        public List<NotificsManager.Notification> ConvertNotificationsFromJson(string notificationsString)
        {
            List<NotificsManager.Notification> notifications = new();

            NotificsManager.NotificationJson[] notificationsJson = JsonConvert.DeserializeObject<NotificsManager.NotificationJson[]>(
                notificationsString
            );

            for (int i = 0; i < notificationsJson.Length; i++)
            {
                notifications.Add(
                    new NotificsManager.Notification
                    {
                        id = notificationsJson[i].id,
                        fireTime = System.DateTime.Parse(notificationsJson[i].fireTime),
                        type = Glob.ParseEnum<NotificsManager.NotificationType>(notificationsJson[i].type),
                        itemName = notificationsJson[i].itemName,
                    }
                );
            }

            return notifications;
        }

        public string ConvertNotificationsToJson(List<NotificsManager.Notification> notifications)
        {
            NotificsManager.NotificationJson[] notificationJson = new NotificsManager.NotificationJson[notifications.Count];

            for (int i = 0; i < notifications.Count; i++)
            {
                NotificsManager.NotificationJson newNotificationJson = new()
                {
                    id = notifications[i].id,
                    fireTime = notifications[i].fireTime.ToString(),
                    type = notifications[i].type.ToString(),
                    itemName = notifications[i].itemName,
                };

                notificationJson[i] = newNotificationJson;
            }

            return JsonConvert.SerializeObject(notificationJson);
        }

        //// SHOP CONTENT ////

        public ShopMenu.ShopItemsContent[] ConvertShopItemContentFromJson(string shopContentString)
        {
            ShopMenu.ShopItemsContentJson[] shopContentJson = JsonConvert.DeserializeObject<ShopMenu.ShopItemsContentJson[]>(shopContentString);

            ShopMenu.ShopItemsContent[] shopContentData = new ShopMenu.ShopItemsContent[shopContentJson.Length];

            for (int i = 0; i < shopContentJson.Length; i++)
            {
                Item.Type newType = Glob.ParseEnum<Item.Type>(shopContentJson[i].type);

                ShopMenu.ShopItemsContent newShopContentDataData = new()
                {
                    total = shopContentJson[i].total,
                    price = shopContentJson[i].price,
                    type = newType,
                    group = Glob.ParseEnum<Item.Group>(shopContentJson[i].group),
                    genGroup = Glob.ParseEnum<Item.GenGroup>(shopContentJson[i].genGroup),
                    chestGroup = Glob.ParseEnum<Item.ChestGroup>(shopContentJson[i].chestGroup),
                    sprite = gameData.GetSprite(shopContentJson[i].sprite, newType),
                    priceType = Glob.ParseEnum<ShopMenu.ShopValuesType>(shopContentJson[i].priceType),
                };
                shopContentData[i] = newShopContentDataData;
            }

            return shopContentData;
        }

        public string ConvertShopItemContentToJson(ShopMenu.ShopItemsContent[] shopContentData)
        {
            ShopMenu.ShopItemsContentJson[] shopContentJson = new ShopMenu.ShopItemsContentJson[shopContentData.Length];

            for (int i = 0; i < shopContentJson.Length; i++)
            {
                ShopMenu.ShopItemsContentJson newShopContentJson = new()
                {
                    total = shopContentData[i].total,
                    price = shopContentData[i].price,
                    type = shopContentData[i].type.ToString(),
                    group = shopContentData[i].group.ToString(),
                    genGroup = shopContentData[i].genGroup.ToString(),
                    chestGroup = shopContentData[i].chestGroup.ToString(),
                    priceType = shopContentData[i].priceType.ToString(),
                    sprite = shopContentData[i].sprite.name,
                };

                shopContentJson[i] = newShopContentJson;
            }

            return JsonConvert.SerializeObject(shopContentJson);
        }

        //// OTHER ////

        // Convert scriptable object data to gameplay data
        public BoardManager.TypeItem[] ConvertItems(BoardManager.TypeItem[] itemsContent)
        {
            BoardManager.TypeItem[] convertedItems = new BoardManager.TypeItem[itemsContent.Length];

            for (int i = 0; i < itemsContent.Length; i++)
            {
                int count = 1;

                BoardManager.TypeItem newObjectData = new()
                {
                    type = itemsContent[i].type,
                    group = itemsContent[i].group,
                    genGroup = itemsContent[i].genGroup,
                    collGroup = itemsContent[i].collGroup,
                    chestGroup = itemsContent[i].chestGroup,
                    hasLevel = itemsContent[i].hasLevel,
                    coolDown = itemsContent[i].coolDown,
                    generatesAtLevel = itemsContent[i].generatesAtLevel,
                    customName = itemsContent[i].customName,
                    parents = itemsContent[i].parents,
                    creates = itemsContent[i].creates,
                    content = new BoardManager.ItemData[itemsContent[i].content.Length]
                };

                for (int j = 0; j < itemsContent[i].content.Length; j++)
                {
                    int chestItemsCount = itemsContent[i].type == Item.Type.Chest ? InitChestItems(itemsContent[i].content[j], itemsContent[i], count) : 0;

                    BoardManager.ItemData newInnerObjectData = new()
                    {
                        type = itemsContent[i].type,
                        group = itemsContent[i].group,
                        genGroup = itemsContent[i].genGroup,
                        collGroup = itemsContent[i].collGroup,
                        chestGroup = itemsContent[i].chestGroup,
                        creates = itemsContent[i].creates,
                        customName = itemsContent[i].content[j].customName,
                        parents = itemsContent[i].parents,
                        hasLevel = itemsContent[i].hasLevel,
                        coolDown = itemsContent[i].coolDown,
                        generatesAtLevel = itemsContent[i].generatesAtLevel,
                        itemName = GetItemName(itemsContent[i].content[j], newObjectData, count),
                        level = count,
                        sprite = itemsContent[i].content[j].sprite,
                        unlocked = CheckUnlocked(itemsContent[i].content[j].sprite.name),
                        startTime = itemsContent[i].content[j].startTime,
                        seconds = itemsContent[i].content[j].seconds,
                        isMaxLevel = count == itemsContent[i].content.Length,
                        chestItems = chestItemsCount,
                        chestItemsSet = chestItemsCount == 0 ? itemsContent[i].content[j].chestItemsSet : true,
                        gemPopped = itemsContent[i].content[j].gemPopped,
                    };

                    newObjectData.content[j] = newInnerObjectData;

                    count++;
                }

                convertedItems[i] = newObjectData;
            }

            return convertedItems;
        }

        public BoardManager.TypeItem[] ConvertGensToItems(BoardManager.TypeGen[] gensContent)
        {
            BoardManager.TypeItem[] convertedItems = new BoardManager.TypeItem[gensContent.Length];

            for (int i = 0; i < gensContent.Length; i++)
            {
                int count = 1;

                BoardManager.TypeItem newObjectData = new()
                {
                    type = Item.Type.Gen,
                    genGroup = gensContent[i].genGroup,
                    hasLevel = gensContent[i].hasLevel,
                    generatesAtLevel = gensContent[i].generatesAtLevel,
                    customName = gensContent[i].customName,
                    creates = gensContent[i].creates,
                    coolDown = gensContent[i].coolDown,
                    parents = gensContent[i].parents,
                    content = new BoardManager.ItemData[gensContent[i].content.Length]
                };

                for (int j = 0; j < gensContent[i].content.Length; j++)
                {
                    BoardManager.ItemData newInnerObjectData = new()
                    {
                        type = Item.Type.Gen,
                        genGroup = gensContent[i].genGroup,
                        creates = gensContent[i].creates,
                        coolDown = gensContent[i].coolDown,
                        customName = gensContent[i].content[j].customName,
                        hasLevel = gensContent[i].hasLevel,
                        parents = gensContent[i].parents,
                        generatesAtLevel = gensContent[i].generatesAtLevel,
                        itemName = GetItemName(gensContent[i].content[j], newObjectData, count),
                        level = count,
                        sprite = gensContent[i].content[j].sprite,
                        unlocked = CheckUnlocked(gensContent[i].content[j].sprite.name),
                        startTime = gensContent[i].content[j].startTime,
                        seconds = gensContent[i].content[j].seconds,
                        isMaxLevel = count == gensContent[i].content.Length,
                    };

                    newObjectData.content[j] = newInnerObjectData;

                    count++;
                }

                convertedItems[i] = newObjectData;
            }

            return convertedItems;
        }

        public BoardManager.TypeItem[] ConvertChestsToItems(BoardManager.TypeChest[] chestContent)
        {
            BoardManager.TypeItem[] convertedItems = new BoardManager.TypeItem[chestContent.Length];

            for (int i = 0; i < chestContent.Length; i++)
            {
                int count = 1;

                BoardManager.TypeItem newObjectData = new()
                {
                    type = Item.Type.Chest,
                    chestGroup = chestContent[i].chestGroup,
                    customName = chestContent[i].customName,
                    hasLevel = chestContent[i].hasLevel,
                    creates = chestContent[i].creates,
                    content = new BoardManager.ItemData[chestContent[i].content.Length]
                };

                for (int j = 0; j < chestContent[i].content.Length; j++)
                {
                    BoardManager.ItemData newInnerObjectData = new()
                    {
                        type = Item.Type.Chest,
                        creates = chestContent[i].creates,
                        chestGroup = chestContent[i].chestGroup,
                        hasLevel = chestContent[i].hasLevel,
                        customName = chestContent[i].content[j].customName,
                        itemName = GetItemName(chestContent[i].content[j], newObjectData, count),
                        level = count,
                        sprite = chestContent[i].content[j].sprite,
                        unlocked = CheckUnlocked(chestContent[i].content[j].sprite.name),
                        isMaxLevel = count == chestContent[i].content.Length,
                    };

                    newObjectData.content[j] = newInnerObjectData;

                    count++;
                }

                convertedItems[i] = newObjectData;
            }

            return convertedItems;
        }

        public BoardManager.TypeItem[] ConvertCollsToItems(BoardManager.TypeColl[] collsContent)
        {
            BoardManager.TypeItem[] convertedItems = new BoardManager.TypeItem[collsContent.Length];

            for (int i = 0; i < collsContent.Length; i++)
            {
                int count = 1;

                BoardManager.TypeItem newObjectData = new()
                {
                    type = Item.Type.Coll,
                    collGroup = collsContent[i].collGroup,
                    customName = collsContent[i].customName,
                    hasLevel = collsContent[i].hasLevel,
                    parents = collsContent[i].parents,
                    content = new BoardManager.ItemData[collsContent[i].content.Length]
                };

                for (int j = 0; j < collsContent[i].content.Length; j++)
                {
                    BoardManager.ItemData newInnerObjectData = new()
                    {
                        type = Item.Type.Coll,
                        collGroup = collsContent[i].collGroup,
                        hasLevel = collsContent[i].hasLevel,
                        parents = collsContent[i].parents,
                        customName = collsContent[i].content[j].customName,
                        itemName = GetItemName(collsContent[i].content[j], newObjectData, count),
                        level = count,
                        sprite = collsContent[i].content[j].sprite,
                        unlocked = CheckUnlocked(collsContent[i].content[j].sprite.name),
                        isMaxLevel = count == collsContent[i].content.Length,
                    };

                    newObjectData.content[j] = newInnerObjectData;

                    count++;
                }

                convertedItems[i] = newObjectData;
            }

            return convertedItems;
        }

        string GetItemName(BoardManager.ItemData itemSingle, BoardManager.TypeItem itemsData, int count)
        {
            // Get name based on custom item name
            if (itemSingle.customName && itemSingle.itemName != "")
            {
                switch (itemsData.type)
                {
                    case Item.Type.Item:
                        return LOCALE.Get("Item_" + itemSingle.group + "_" + count);
                    case Item.Type.Gen:
                        return LOCALE.Get("Gen_" + itemSingle.genGroup + "_" + count);
                    case Item.Type.Coll:
                        return LOCALE.Get("Item_" + itemSingle.collGroup + "_" + count);
                    case Item.Type.Chest:
                        return LOCALE.Get("Item_" + itemSingle.chestGroup + "_" + count);
                }

            }

            // Get name based on item order
            if (itemsData.customName)
            {
                switch (itemsData.type)
                {
                    case Item.Type.Item:
                        return LOCALE.Get(
                            itemsData.type + "_" + itemsData.group + "_" + count
                        );
                    case Item.Type.Gen:
                        return LOCALE.Get(
                            itemsData.type + "_" + itemsData.genGroup + "_" + count
                        );
                    case Item.Type.Coll:
                        return "";
                    case Item.Type.Chest:
                        return LOCALE.Get(
                            itemsData.type + "_" + itemsData.chestGroup
                        );
                    default:
                        // ERROR
                        ErrorManager.Instance.Throw(ErrorManager.ErrorType.Code, "DataConverter.cs -> GetItemName()", "Wrong type: " + itemsData.type);
                        return "";
                }
            }

            // Get name based on the type
            if (itemsData.type == Item.Type.Item)
            {
                return LOCALE.Get("Item_" + itemsData.group + "_" + count);
            }

            if (itemsData.type == Item.Type.Gen)
            {
                int genGroupItemsCount = LOCALE.GetLength("Gen_" + itemsData.genGroup + "_", 1);

                if (genGroupItemsCount > 0)
                {
                    return LOCALE.Get("Gen_" + itemsData.genGroup + "_" + count);
                }
                else
                {
                    return LOCALE.Get("Gen_" + itemsData.genGroup);
                }
            }

            if (itemsData.type == Item.Type.Coll)
            {
                return LOCALE.Get("Coll_" + itemsData.collGroup, count);
            }

            if (itemsData.type == Item.Type.Chest)
            {
                return LOCALE.Get("Chest_" + itemsData.chestGroup);
            }

            // ERROR - No valid name was found
            ErrorManager.Instance.Throw(ErrorManager.ErrorType.Locale, "DataConverter.cs -> GetItemName()", "error_loc_name");

            return LOCALE.Get("Item_error_name");
        }

        int InitChestItems(BoardManager.ItemData itemSingle, BoardManager.TypeItem itemsData, int count)
        {
            int chestItemsCount;

            if (itemsData.type == Item.Type.Chest && !itemSingle.chestItemsSet)
            {
                switch (itemsData.chestGroup)
                {
                    case Item.ChestGroup.Piggy:
                        chestItemsCount = UnityEngine.Random.Range(6 + count, 8 + count);
                        break;
                    case Item.ChestGroup.Energy:
                        chestItemsCount = UnityEngine.Random.Range(4 + count, 6 + count);
                        break;
                    default: // Item.ChestGroup.Item
                        chestItemsCount = UnityEngine.Random.Range(5 + count, 7 + count);
                        break;
                }
            }
            else
            {
                chestItemsCount = itemSingle.chestItems;
            }

            return chestItemsCount;
        }

        // Check if item is unlocked
        public bool CheckUnlocked(string spriteName)
        {
            bool found = false;

            for (int i = 0; i < gameData.unlockedItems.Length; i++)
            {
                if (spriteName == gameData.unlockedItems[i])
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        // Get initial unlocked items
        public string GetInitialUnlocked()
        {
            // Get all item unlocks
            string[] initialUnlockedPre = new string[dataManager.initialBoardData.Length];

            for (int i = 0; i < dataManager.initialBoardData.Length; i++)
            {
                if (dataManager.initialBoardData[i].sprite != null)
                {
                    bool found = false;

                    // Check if item is already unlocked
                    for (int j = 0; j < initialUnlockedPre.Length; j++)
                    {
                        if (
                            initialUnlockedPre[j] != null
                            && initialUnlockedPre[j] == dataManager.initialBoardData[i].sprite.name
                        )
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        string unlockedItem = dataManager.initialBoardData[i].sprite.name;

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

            dataManager.unlockedJsonData = JsonConvert.SerializeObject(initialUnlocked);

            return dataManager.unlockedJsonData;
        }
    }
}