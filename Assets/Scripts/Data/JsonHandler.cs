using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

namespace Merge
{
    public class JsonHandler : MonoBehaviour
    {
        // Instances
        private GameData gameData;
        private ItemHandler itemHandler;

        void Start()
        {
            // Cache instances
            gameData = GameData.Instance;
            itemHandler = DataManager.Instance.GetComponent<ItemHandler>();
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
                    generatesAt = boardJson[i].generatesAt,
                    crate = boardJson[i].crate,
                    gemPopped = boardJson[i].gemPopped,
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
                    generatesAt = boardData[i].generatesAt,
                    chestItemsSet = boardData[i].chestItemsSet,
                    crate = initialLoop ? randomInt : boardData[i].crate,
                    gemPopped = boardData[i].gemPopped,
                };

                boardJson[i] = newBoardJson;
            }

            return JsonConvert.SerializeObject(boardJson);
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
                    chestGroup = Glob.ParseEnum<Types.ChestGroup>(inventoryJson[i].chestGroup)
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
                };

                inventoryJson[i] = newInventoryJson;
            }

            return JsonConvert.SerializeObject(inventoryJson);
        }

        //// INVENTORY ////

        public List<Types.Tasks> ConvertTasksFromJson(string tasksString)
        {
            Types.TasksJson[] tasksJson = JsonConvert.DeserializeObject<Types.TasksJson[]>(
                tasksString
            );

            List<Types.Tasks> tasksData = new();

            for (int i = 0; i < tasksJson.Length; i++)
            {
                Types.Tasks newTasksData = new()
                {
                    sprite = gameData.GetTaskSprite(tasksJson[i].sprite),
                    name = tasksJson[i].name,
                    needs = ConvertTaskItemsFromJson(tasksJson[i].needs),
                    rewards = ConvertTaskItemsFromJson(tasksJson[i].rewards),
                    completed = tasksJson[i].completed,
                };

                tasksData.Add(newTasksData);
            }

            return tasksData;
        }

        public string ConvertTasksToJson(List<Types.Tasks> tasksData)
        {
            Types.TasksJson[] tasksJson = new Types.TasksJson[tasksData.Count];

            for (int i = 0; i < tasksData.Count; i++)
            {
                Types.TasksJson newTasksJson = new()
                {
                    sprite = tasksData[i].sprite.name,
                    name = tasksData[i].name,
                    needs = ConvertTaskItemsToJson(tasksData[i].needs),
                    rewards = ConvertTaskItemsToJson(tasksData[i].rewards),
                    completed = tasksData[i].completed,
                };

                tasksJson[i] = newTasksJson;
            }

            return JsonConvert.SerializeObject(tasksJson);
        }

        Types.TaskItems[] ConvertTaskItemsFromJson(string itemsString)
        {
            Types.TaskItemsJson[] itemsJson = JsonConvert.DeserializeObject<Types.TaskItemsJson[]>(
                itemsString
            );

           Types.TaskItems[] itemsData = new Types.TaskItems[itemsJson.Length];

            for (int i = 0; i < itemsJson.Length; i++)
            {
                Types.Type newType=Glob.ParseEnum<Types.Type>(itemsJson[i].type);

                Types.TaskItems newItemsData = new()
                {
                    sprite = gameData.GetSprite(itemsJson[i].sprite, newType),
                    type = newType,
                    count = itemsJson[i].count
                };

                itemsData[i]=newItemsData;
            }

            return itemsData;
        }

        string ConvertTaskItemsToJson(Types.TaskItems[] itemsData)
        {
            Types.TaskItemsJson[] itemsJson = new Types.TaskItemsJson[itemsData.Length];

            for (int i = 0; i < itemsData.Length; i++)
            {
                Types.TaskItemsJson newItemsJson = new()
                {
                    sprite = itemsData[i].sprite.name,
                    type = itemsData[i].type.ToString(),
                    count = itemsData[i].count
                };

                itemsJson[i] = newItemsJson;
            }

            return JsonConvert.SerializeObject(itemsJson);
        }

        Sprite GetTaskSprite(string spriteName)
        {
            // TODO - Get Task sprites
            return null;
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
                        startDate = System.DateTime.Parse(timerJson[i].startDate),
                        seconds = timerJson[i].seconds,
                        on = timerJson[i].on,
                        type = Glob.ParseEnum<Types.TimerType>(timerJson[i].type),
                        timerName = timerJson[i].timerName,
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
                    startDate = timers[i].startDate.ToString(),
                    seconds = timers[i].seconds,
                    on = timers[i].on,
                    type = timers[i].type.ToString(),
                    timerName = timers[i].timerName,
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
                    left = shopContentJson[i].left,
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

        public string ConvertShopItemContentToJson(Types.ShopItemsContent[] shopContentData, bool ignoreLast = false)
        {
            int length = ignoreLast ? shopContentData.Length - 1 : shopContentData.Length;

            Types.ShopItemsContentJson[] shopContentJson = new Types.ShopItemsContentJson[length];

            for (int i = 0; i < length; i++)
            {
                Types.ShopItemsContentJson newShopContentJson = new()
                {
                    left = shopContentData[i].left,
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

        // TODO - Are these functions needed?
        /* ItemTypes.GenGroup[] LoopParentsToObject(string[] newParents)
         {
             ItemTypes.GenGroup[] parents = new ItemTypes.GenGroup[newParents.Length];

             for (int i = 0; i < newParents.Length; i++)
             {
                 parents[i] = Glob.ParseEnum<ItemTypes.GenGroup>(newParents[i]);
             }

             return parents;
         }

         string[] LoopParentsToJson(ItemTypes.GenGroup[] newParents)
         {
             string[] parents = new string[newParents.Length];

             for (int i = 0; i < newParents.Length; i++)
             {
                 parents[i] = newParents[i].ToString();
             }

             return parents;
         }

         Types.Creates[] ConvertStringToCreates(string[] newCreates)
         {
             if (newCreates.Length > 0)
             {
                 Types.Creates[] creates = new Types.Creates[newCreates.Length];

                 for (int i = 0; i < newCreates.Length; i++)
                 {
                     string[] splitString = newCreates[i].Split("_");

                     creates[i] = new Types.Creates
                     {
                         group = Glob.ParseEnum<ItemTypes.Group>(splitString[i]),
                         chance = float.Parse(splitString[1])
                     };
                 }

                 return creates;
             }
             else
             {
                 return new Types.Creates[0];
             }
         }

         string[] ConvertCreatesToString(Types.Creates[] newCreates)
         {
             if (newCreates.Length > 0)
             {
                 string[] creates = new string[newCreates.Length];

                 for (int i = 0; i < newCreates.Length; i++)
                 {
                     string combinedString = newCreates[i].group.ToString() + "_" + newCreates[i].chance;

                     creates[i] = combinedString;
                 }

                 return creates;
             }
             else
             {
                 return new string[0];
             }
         }*/
    }
}