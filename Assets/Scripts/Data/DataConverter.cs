using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;

namespace Merge
{
    public class DataConverter : MonoBehaviour
{
    // References
    private DataManager dataManager;
    private GameData gameData;
    private I18n LOCALE = I18n.Instance;

    void Start()
    {
        dataManager = GetComponent<DataManager>();
        gameData = GameData.Instance;
    }

    public Types.Items[] ConvertItems(Types.Items[] itemsContent)
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
                chestGroup = itemsContent[i].chestGroup,
                hasLevel = itemsContent[i].hasLevel,
                hasTimer = itemsContent[i].hasTimer,
                generatesAt = itemsContent[i].generatesAt,
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
                    chestGroup = itemsContent[i].chestGroup,
                    creates = itemsContent[i].creates,
                    customName = itemsContent[i].content[j].customName,
                    parents = itemsContent[i].parents,
                    hasLevel = itemsContent[i].hasLevel,
                    hasTimer = itemsContent[i].hasTimer,
                    generatesAt = itemsContent[i].generatesAt,
                    itemName = GetItemName(itemsContent[i].content[j], newObjectData, count),
                    level = count,
                    sprite = itemsContent[i].content[j].sprite,
                    unlocked = CheckUnlocked(itemsContent[i].content[j].sprite.name),
                    startTime = itemsContent[i].content[j].startTime,
                    seconds = itemsContent[i].content[j].seconds,
                    isMaxLavel = count == itemsContent[i].content.Length,
                    chestItems = InitChestItems(itemsContent[i].content[j], itemsContent[i], count),
                    chestItemsSet = itemsContent[i].content[j].chestItemsSet,
                    gemPoped = itemsContent[i].content[j].gemPoped,
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

    int InitChestItems(Types.ItemsData itemSingle, Types.Items itemsData, int count)
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
                    gemPoped = boardArray[count].gemPoped
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
                generatesAt = boardItem.generatesAt,
                gemPoped = boardItem.gemPoped
            };

            count++;
        }

        return newBoardArray;
    }
}
}