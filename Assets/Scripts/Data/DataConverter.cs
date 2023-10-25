using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

namespace Merge
{
    public class DataConverter : MonoBehaviour
    {
        // Instances
        private GameData gameData;
        private DataManager dataManager;
        private ItemHandler itemHandler;
        private I18n LOCALE;

        void Start()
        {
            // Cache instances
            gameData = GameData.Instance;
            dataManager = DataManager.Instance;
            itemHandler = dataManager.GetComponent<ItemHandler>();
            LOCALE = I18n.Instance;
        }

        //// BOARD ////

        public Types.Board[] ConvertBoardFromJson(string boardString)
        {
            Types.BoardJson[] boardJson = JsonConvert.DeserializeObject<Types.BoardJson[]>(boardString);

            Types.Board[] boardData = new Types.Board[boardJson.Length];

            for (int i = 0; i < boardJson.Length; i++)
            {
                Types.Type newType = Glob.ParseEnum<Types.Type>(boardJson[i].type);

                Types.Board newBoardData = new()
                {
                    sprite = gameData.GetSprite(boardJson[i].sprite, newType),
                    state = Glob.ParseEnum<Types.State>(boardJson[i].state),
                    type = newType,
                    group = Glob.ParseEnum<ItemTypes.Group>(boardJson[i].group),
                    genGroup = Glob.ParseEnum<ItemTypes.GenGroup>(boardJson[i].genGroup),
                    collGroup = Glob.ParseEnum<Types.CollGroup>(boardJson[i].collGroup),
                    chestGroup = Glob.ParseEnum<Types.ChestGroup>(boardJson[i].chestGroup),
                    chestItems = boardJson[i].chestItems,
                    chestItemsSet = boardJson[i].chestItemsSet,
                    id = boardJson[i].id,
                    generatesAt = boardJson[i].generatesAt,
                    crate = boardJson[i].crate,
                    gemPopped = boardJson[i].gemPopped,
                    isCompleted = boardJson[i].isCompleted,
                    timerOn = boardJson[i].timerOn,
                    timerStartTime = boardJson[i].timerStartTime,
                    timerSeconds = boardJson[i].timerSeconds,
                };

                boardData[i] = newBoardData;
            }

            return boardData;
        }

        public string ConvertBoardToJson(Types.Board[] boardData, bool initialLoop = false)
        {
            Types.BoardJson[] boardJson = new Types.BoardJson[boardData.Length];

            for (int i = 0; i < boardData.Length; i++)
            {
                int randomInt = Random.Range(0, itemHandler.crateSprites.Length);

                Types.BoardJson newBoardJson = new()
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
                    generatesAt = boardData[i].generatesAt,
                    chestItemsSet = boardData[i].chestItemsSet,
                    crate = initialLoop ? randomInt : boardData[i].crate,
                    gemPopped = boardData[i].gemPopped,
                    isCompleted = boardData[i].isCompleted,
                    timerOn = boardData[i].timerOn,
                    timerStartTime = boardData[i].timerStartTime,
                    timerSeconds = boardData[i].timerSeconds,
                };

                boardJson[i] = newBoardJson;
            }

            return JsonConvert.SerializeObject(boardJson);
        }

        public Types.Board[,] ConvertArrayToBoard(Types.Board[] boardArray)
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
                        collGroup = boardArray[count].collGroup,
                        chestGroup = boardArray[count].chestGroup,
                        state = boardArray[count].state,
                        crate = boardArray[count].crate,
                        order = count,
                        chestItems = boardArray[count].chestItems,
                        chestItemsSet = boardArray[count].chestItemsSet,
                        generatesAt = boardArray[count].generatesAt,
                        id = boardArray[count].id,
                        gemPopped = boardArray[count].gemPopped,
                        isCompleted = boardArray[count].isCompleted,
                        timerOn = boardArray[count].timerOn,
                        timerStartTime = boardArray[count].timerStartTime,
                        timerSeconds = boardArray[count].timerSeconds,
                    };

                    count++;
                }
            }

            return newBoardData;
        }

        public Types.Board[] ConvertBoardToArray(Types.Board[,] boardData)
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
                    chestGroup = boardItem.chestGroup,
                    state = boardItem.state,
                    crate = boardItem.crate,
                    chestItems = boardItem.chestItems,
                    chestItemsSet = boardItem.chestItemsSet,
                    id = boardItem.id,
                    generatesAt = boardItem.generatesAt,
                    gemPopped = boardItem.gemPopped,
                    isCompleted = boardItem.isCompleted,
                    timerOn = boardItem.timerOn,
                    timerStartTime = boardItem.timerStartTime,
                    timerSeconds = boardItem.timerSeconds,
                };

                count++;
            }

            return newBoardArray;
        }

        //// BONUS ////

        public List<Types.Bonus> ConvertBonusFromJson(string bonusString)
        {
            Types.BonusJson[] bonusJson = JsonConvert.DeserializeObject<Types.BonusJson[]>(bonusString);

            List<Types.Bonus> bonusData = new();

            for (int i = 0; i < bonusJson.Length; i++)
            {
                Types.Type newType = Glob.ParseEnum<Types.Type>(bonusJson[i].type);

                Types.Bonus newBonusData = new()
                {
                    sprite = gameData.GetSprite(bonusJson[i].sprite, newType),
                    type = newType,
                    group = Glob.ParseEnum<ItemTypes.Group>(bonusJson[i].group),
                    genGroup = Glob.ParseEnum<ItemTypes.GenGroup>(bonusJson[i].genGroup)
                };

                bonusData.Add(newBonusData);
            }

            return bonusData;
        }

        public string ConvertBonusToJson(List<Types.Bonus> bonusData)
        {
            Types.BonusJson[] bonusJson = new Types.BonusJson[bonusData.Count];

            for (int i = 0; i < bonusData.Count; i++)
            {
                Types.BonusJson newBonusJson = new()
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

        public List<Types.Inventory> ConvertInventoryFromJson(string inventoryString)
        {
            Types.InventoryJson[] inventoryJson = JsonConvert.DeserializeObject<Types.InventoryJson[]>(
                inventoryString
            );

            List<Types.Inventory> inventoryData = new();

            for (int i = 0; i < inventoryJson.Length; i++)
            {
                Types.Type newType = Glob.ParseEnum<Types.Type>(inventoryJson[i].type);

                Types.Inventory newInventoryData = new()
                {
                    sprite = gameData.GetSprite(inventoryJson[i].sprite, newType),
                    type = newType,
                    group = Glob.ParseEnum<ItemTypes.Group>(inventoryJson[i].group),
                    genGroup = Glob.ParseEnum<ItemTypes.GenGroup>(inventoryJson[i].genGroup),
                    chestGroup = Glob.ParseEnum<Types.ChestGroup>(inventoryJson[i].chestGroup),
                    isCompleted = inventoryJson[i].isCompleted
                };

                inventoryData.Add(newInventoryData);
            }

            return inventoryData;
        }

        public string ConvertInventoryToJson(List<Types.Inventory> inventoryData)
        {
            Types.InventoryJson[] inventoryJson = new Types.InventoryJson[inventoryData.Count];

            for (int i = 0; i < inventoryData.Count; i++)
            {
                Types.InventoryJson newInventoryJson = new()
                {
                    sprite = inventoryData[i].sprite == null ? "" : inventoryData[i].sprite.name,
                    type = inventoryData[i].type.ToString(),
                    group = inventoryData[i].group.ToString(),
                    genGroup = inventoryData[i].genGroup.ToString(),
                    chestGroup = inventoryData[i].chestGroup.ToString(),
                    isCompleted = inventoryData[i].isCompleted
                };

                inventoryJson[i] = newInventoryJson;
            }

            return JsonConvert.SerializeObject(inventoryJson);
        }

        //// AREAS ////

        public List<WorldTypes.Area> ConvertJsonToArea(string areasString)
        {
            WorldTypes.AreaJson[] areaJson = JsonConvert.DeserializeObject<WorldTypes.AreaJson[]>(areasString);

            List<WorldTypes.Area> areasData = new();

            for (int i = 0; i < areaJson.Length; i++)
            {
                WorldTypes.Area newAreasData = new()
                {
                    name = areaJson[i].name,
                    isLocked = areaJson[i].isLocked,
                    isRoom = areaJson[i].isRoom,
                    wallLeftOrder = areaJson[i].wallLeftOrder,
                    wallRightOrder = areaJson[i].wallRightOrder,
                    floorOrder = areaJson[i].floorOrder,
                    furniture = JsonConvert.DeserializeObject<List<WorldTypes.Furniture>>(areaJson[i].furniture),
                };

                areasData.Add(newAreasData);
            }

            return areasData;
        }

        public string ConvertAreaToJson(List<WorldTypes.Area> areasData)
        {
            WorldTypes.AreaJson[] areaJson = new WorldTypes.AreaJson[areasData.Count];

            for (int i = 0; i < areasData.Count; i++)
            {
                WorldTypes.AreaJson newAreaJson = new()
                {
                    name = areasData[i].name,
                    isLocked = areasData[i].isLocked,
                    isRoom = areasData[i].isRoom,
                    wallLeftOrder = areasData[i].wallLeftOrder,
                    wallRightOrder = areasData[i].wallRightOrder,
                    floorOrder = areasData[i].floorOrder,
                    furniture = JsonConvert.SerializeObject(areasData[i].furniture),
                };

                areaJson[i] = newAreaJson;
            }

            return JsonConvert.SerializeObject(areaJson);
        }

        //// TASKS ////
        public List<Types.TaskGroup> ConvertTaskGroupsFromJson(string tasksString)
        {
            Types.TaskGroupJson[] tasksJson = JsonConvert.DeserializeObject<Types.TaskGroupJson[]>(
                tasksString
            );

            List<Types.TaskGroup> tasksData = new();

            for (int i = 0; i < tasksJson.Length; i++)
            {
                Types.TaskGroup newTasksData = new()
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

        public string ConvertTaskGroupsToJson(List<Types.TaskGroup> tasksData)
        {
            Types.TaskGroupJson[] tasksJson = new Types.TaskGroupJson[tasksData.Count];

            for (int i = 0; i < tasksData.Count; i++)
            {
                Types.TaskGroupJson newTasksJson = new()
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

        List<Types.Task> ConvertTasksFromJson(string tasksString)
        {
            Types.TaskJson[] tasksJson = JsonConvert.DeserializeObject<Types.TaskJson[]>(
                tasksString
            );

            List<Types.Task> tasksData = new();

            for (int i = 0; i < tasksJson.Length; i++)
            {
                Types.Task newTasksData = new()
                {
                    needs = ConvertTaskItemFromJson(tasksJson[i].needs),
                    rewards = ConvertTaskItemFromJson(tasksJson[i].rewards),
                    id = tasksJson[i].id,
                    taskRefName = tasksJson[i].taskRefName,
                    taskRefType = Glob.ParseEnum<Types.TaskRefType>(tasksJson[i].taskRefType),
                    isTaskRefRight = tasksJson[i].isTaskRefRight,
                    completed = tasksJson[i].completed,
                };

                tasksData.Add(newTasksData);
            }

            return tasksData;
        }

        string ConvertTasksToJson(List<Types.Task> tasksData)
        {
            Types.TaskJson[] taskJson = new Types.TaskJson[tasksData.Count];

            for (int i = 0; i < tasksData.Count; i++)
            {
                Types.TaskJson newTaskJson = new()
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

        Types.TaskItem[] ConvertTaskItemFromJson(string itemsString)
        {
            Types.TaskItemJson[] itemsJson = JsonConvert.DeserializeObject<Types.TaskItemJson[]>(
                itemsString
            );

            Types.TaskItem[] itemsData = new Types.TaskItem[itemsJson.Length];

            for (int i = 0; i < itemsJson.Length; i++)
            {
                Types.Type newType = Glob.ParseEnum<Types.Type>(itemsJson[i].type);

                Types.TaskItem newItemsData = new()
                {
                    sprite = gameData.GetSprite(itemsJson[i].sprite, newType),
                    type = newType,
                    group = Glob.ParseEnum<ItemTypes.Group>(itemsJson[i].group),
                    genGroup = Glob.ParseEnum<ItemTypes.GenGroup>(itemsJson[i].genGroup),
                    collGroup = Glob.ParseEnum<Types.CollGroup>(itemsJson[i].collGroup),
                    chestGroup = Glob.ParseEnum<Types.ChestGroup>(itemsJson[i].chestGroup),
                    amount = itemsJson[i].amount,
                    completed = itemsJson[i].completed,
                };

                itemsData[i] = newItemsData;
            }

            return itemsData;
        }

        string ConvertTaskItemToJson(Types.TaskItem[] itemsData)
        {
            Types.TaskItemJson[] itemsJson = new Types.TaskItemJson[itemsData.Length];

            for (int i = 0; i < itemsData.Length; i++)
            {
                Types.TaskItemJson newItemsJson = new()
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

        //// UNSENT ////

        public List<ApiCalls.UnsentData> ConvertUnsentFromJson(string unsentDataString)
        {
            ApiCalls.UnsentDataJson[] unsentJson = JsonConvert.DeserializeObject<ApiCalls.UnsentDataJson[]>(
                unsentDataString
            );

            List<ApiCalls.UnsentData> unsentData = new List<ApiCalls.UnsentData>();

            for (int i = 0; i < unsentJson.Length; i++)
            {
                ApiCalls.UnsentType newUnsentType = Glob.ParseEnum<ApiCalls.UnsentType>(unsentJson[i].unsentType);

                ApiCalls.UnsentData newUnsentData = new ApiCalls.UnsentData
                {
                    unsentType = newUnsentType,
                    jsonData = unsentJson[i].jsonData,
                    priority = unsentJson[i].priority
                };

                unsentData.Add(newUnsentData);
            }

            return unsentData;

        }

        public string ConvertUnsentToJson(List<ApiCalls.UnsentData> unsentData)
        {
            ApiCalls.UnsentDataJson[] unsentJson = new ApiCalls.UnsentDataJson[unsentData.Count];

            for (int i = 0; i < unsentData.Count; i++)
            {
                ApiCalls.UnsentDataJson newUnsentJson = new ApiCalls.UnsentDataJson
                {
                    unsentType = unsentData[i].unsentType.ToString(),
                    jsonData = unsentData[i].jsonData,
                    priority = unsentData[i].priority,
                };

                unsentJson[i] = newUnsentJson;
            }

            return JsonConvert.SerializeObject(unsentJson);
        }

        //// TIMERS ////

        public List<Types.Timer> ConvertTimersFromJson(string timersString)
        {
            List<Types.Timer> timers = new();

            Types.TimerJson[] timerJson = JsonConvert.DeserializeObject<Types.TimerJson[]>(
                timersString
            );

            for (int i = 0; i < timerJson.Length; i++)
            {
                timers.Add(
                    new Types.Timer
                    {
                        startTime = System.DateTime.Parse(timerJson[i].startTime),
                        seconds = timerJson[i].seconds,
                        on = timerJson[i].on,
                        type = Glob.ParseEnum<Types.TimerType>(timerJson[i].type),
                        id = timerJson[i].id,
                    }
                );
            }

            return timers;
        }

        public string ConvertTimersToJson(List<Types.Timer> timers)
        {
            Types.TimerJson[] timerJson = new Types.TimerJson[timers.Count];

            for (int i = 0; i < timers.Count; i++)
            {
                Types.TimerJson newTimerJson = new()
                {
                    startTime = timers[i].startTime.ToString(),
                    seconds = timers[i].seconds,
                    on = timers[i].on,
                    type = timers[i].type.ToString(),
                    id = timers[i].id,
                };

                timerJson[i] = newTimerJson;
            }

            return JsonConvert.SerializeObject(timerJson);
        }

        //// SHOP CONTENT ////

        public Types.ShopItemsContent[] ConvertShopItemContentFromJson(string shopContentString)
        {
            Types.ShopItemsContentJson[] shopContentJson = JsonConvert.DeserializeObject<Types.ShopItemsContentJson[]>(shopContentString);

            Types.ShopItemsContent[] shopContentData = new Types.ShopItemsContent[shopContentJson.Length];

            for (int i = 0; i < shopContentJson.Length; i++)
            {
                Types.Type newType = Glob.ParseEnum<Types.Type>(shopContentJson[i].type);

                Types.ShopItemsContent newShopContentDataData = new()
                {
                    total = shopContentJson[i].total,
                    price = shopContentJson[i].price,
                    type = newType,
                    group = Glob.ParseEnum<ItemTypes.Group>(shopContentJson[i].group),
                    genGroup = Glob.ParseEnum<ItemTypes.GenGroup>(shopContentJson[i].genGroup),
                    chestGroup = Glob.ParseEnum<Types.ChestGroup>(shopContentJson[i].chestGroup),
                    sprite = gameData.GetSprite(shopContentJson[i].sprite, newType),
                    priceType = Glob.ParseEnum<Types.ShopValuesType>(shopContentJson[i].priceType),
                };
                shopContentData[i] = newShopContentDataData;
            }

            return shopContentData;
        }

        public string ConvertShopItemContentToJson(Types.ShopItemsContent[] shopContentData)
        {
            Types.ShopItemsContentJson[] shopContentJson = new Types.ShopItemsContentJson[shopContentData.Length];

            for (int i = 0; i < shopContentJson.Length; i++)
            {
                Types.ShopItemsContentJson newShopContentJson = new()
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
        public Types.Item[] ConvertItems(Types.Item[] itemsContent)
        {
            Types.Item[] convertedItems = new Types.Item[itemsContent.Length];

            for (int i = 0; i < itemsContent.Length; i++)
            {
                int count = 1;

                Types.Item newObjectData = new()
                {
                    type = itemsContent[i].type,
                    group = itemsContent[i].group,
                    genGroup = itemsContent[i].genGroup,
                    collGroup = itemsContent[i].collGroup,
                    chestGroup = itemsContent[i].chestGroup,
                    hasLevel = itemsContent[i].hasLevel,
                    coolDown = itemsContent[i].coolDown,
                    generatesAt = itemsContent[i].generatesAt,
                    customName = itemsContent[i].customName,
                    parents = itemsContent[i].parents,
                    creates = itemsContent[i].creates,
                    content = new Types.ItemData[itemsContent[i].content.Length]
                };

                for (int j = 0; j < itemsContent[i].content.Length; j++)
                {
                    Types.ItemData newInnerObjectData = new()
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
                        generatesAt = itemsContent[i].generatesAt,
                        itemName = GetItemName(itemsContent[i].content[j], newObjectData, count),
                        level = count,
                        sprite = itemsContent[i].content[j].sprite,
                        unlocked = CheckUnlocked(itemsContent[i].content[j].sprite.name),
                        startTime = itemsContent[i].content[j].startTime,
                        seconds = itemsContent[i].content[j].seconds,
                        isMaxLevel = count == itemsContent[i].content.Length,
                        chestItems = InitChestItems(itemsContent[i].content[j], itemsContent[i], count),
                        chestItemsSet = itemsContent[i].content[j].chestItemsSet,
                        gemPopped = itemsContent[i].content[j].gemPopped,
                    };

                    newObjectData.content[j] = newInnerObjectData;

                    count++;
                }

                convertedItems[i] = newObjectData;
            }

            return convertedItems;
        }

        public Types.Item[] ConvertGensToItems(Types.Gen[] gensContent)
        {
            Types.Item[] convertedItems = new Types.Item[gensContent.Length];

            for (int i = 0; i < gensContent.Length; i++)
            {
                int count = 1;

                Types.Item newObjectData = new()
                {
                    type = Types.Type.Gen,
                    genGroup = gensContent[i].genGroup,
                    hasLevel = gensContent[i].hasLevel,
                    generatesAt = gensContent[i].generatesAt,
                    customName = gensContent[i].customName,
                    creates = gensContent[i].creates,
                    coolDown = gensContent[i].coolDown,
                    content = new Types.ItemData[gensContent[i].content.Length]
                };

                for (int j = 0; j < gensContent[i].content.Length; j++)
                {
                    Types.ItemData newInnerObjectData = new()
                    {
                        type = Types.Type.Gen,
                        genGroup = gensContent[i].genGroup,
                        creates = gensContent[i].creates,
                        coolDown = gensContent[i].coolDown,
                        customName = gensContent[i].content[j].customName,
                        hasLevel = gensContent[i].hasLevel,
                        generatesAt = gensContent[i].generatesAt,
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

        string GetItemName(Types.ItemData itemSingle, Types.Item itemsData, int count)
        {
            // Get name based on custom item name
            if (itemSingle.customName && itemSingle.itemName != "")
            {
                return LOCALE.Get(itemsData.type + "_" + itemSingle.itemName);
            }

            // Get name based on item order
            if (itemsData.customName)
            {
                switch (itemsData.type)
                {
                    case Types.Type.Item:
                        return LOCALE.Get(
                            itemsData.type + "_" + itemsData.group.ToString() + "_" + count
                        );
                    case Types.Type.Gen:
                        return LOCALE.Get(
                            itemsData.type + "_" + itemsData.genGroup.ToString() + "_" + count
                        );
                    case Types.Type.Coll:
                        return "";
                    case Types.Type.Chest:
                        return LOCALE.Get(
                            itemsData.type + "_" + itemsData.chestGroup
                        );
                    default:
                        ErrorManager.Instance.Throw(
                            Types.ErrorType.Code,
                            "Wrong type: " + itemsData.type
                        );

                        return "";
                }
            }

            // Get name based on the type
            if (itemsData.type == Types.Type.Item)
            {
                Debug.Log("Item_" + itemsData.group.ToString());
                Debug.Log("Item_" + itemsData.group); // TODO - Check if enums need to be converted to strings
                return LOCALE.Get("Item_" + itemsData.group.ToString() + "_" + count);
            }

            if (itemsData.type == Types.Type.Gen)
            {

                return LOCALE.Get("Gen_" + itemsData.genGroup.ToString());
            }

            if (itemsData.type == Types.Type.Coll)
            {
                return LOCALE.Get("Coll_" + itemsData.collGroup.ToString(), count);
            }

            if (itemsData.type == Types.Type.Chest)
            {
                return LOCALE.Get("Chest_" + itemsData.chestGroup.ToString());
            }

            // No valid name was found
            ErrorManager.Instance.Throw(Types.ErrorType.Locale, "error_loc_name");

            return LOCALE.Get("Item_error_name");
        }

        int InitChestItems(Types.ItemData itemSingle, Types.Item itemsData, int count)
        {
            int chestItemsCount;

            if (itemsData.type == Types.Type.Chest && !itemSingle.chestItemsSet)
            {
                switch (itemsData.chestGroup)
                {
                    case Types.ChestGroup.Piggy:
                        chestItemsCount = Random.Range(6 + count, 8 + count);
                        break;
                    case Types.ChestGroup.Energy:
                        chestItemsCount = Random.Range(4 + count, 6 + count);
                        break;
                    default: // Types.ChestGroup.Item
                        chestItemsCount = Random.Range(3 + count, 5 + count);
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
        public string GetInitialUnlocked()
        {
            // Get all item unlocks
            string[] initialUnlockedPre = new string[dataManager.initialItems.content.Length];

            for (int i = 0; i < dataManager.initialItems.content.Length; i++)
            {
                if (dataManager.initialItems.content[i].sprite != null)
                {
                    bool found = false;

                    // Check if item is already unlocked
                    for (int j = 0; j < initialUnlockedPre.Length; j++)
                    {
                        if (
                            initialUnlockedPre[j] != null
                            && initialUnlockedPre[j] == dataManager.initialItems.content[i].sprite.name
                        )
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        string unlockedItem = dataManager.initialItems.content[i].sprite.name;

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