using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Locale;
using Newtonsoft.Json;

public class DataConverter : MonoBehaviour
{
    // References
    private DataManager dataManager;
    private GameData gameData;
    private I18n LOCALE = I18n.Instance;

    void Start() {
        dataManager=GetComponent<DataManager>();
        gameData=GameData.Instance;
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

    public string GetItemName(Types.ItemsData itemSingle, Types.Items itemsData, int count)
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
        dataManager=GetComponent<DataManager>();
        string[] initialUnlockedPre = new string[dataManager.initialItems.content.Length];

        dataManager=GetComponent<DataManager>();
        for (int i = 0; i < dataManager.initialItems.content.Length; i++)
        {
        dataManager=GetComponent<DataManager>();
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
                    state = boardArray[count].state,
                    crate = boardArray[count].crate,
                    order = count
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
                state = boardItem.state,
                crate = boardItem.crate
            };

            count++;
        }

        return newBoardArray;
    }
}
