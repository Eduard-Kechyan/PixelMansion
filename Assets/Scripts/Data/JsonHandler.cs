using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class JsonHandler : MonoBehaviour
{
    private DataManager dataManager;

    void Start()
    {
        // Cache DataManager
        dataManager = GetComponent<DataManager>();
    }

    //// ITEMS ////

    public Types.Items[] ConvertItemsFromJson(Types.ItemsJson[] jsonData)
    {
        Types.Items[] objectData = new Types.Items[jsonData.Length];

        for (int i = 0; i < jsonData.Length; i++)
        {
            int count = 1;

            Item.Group[] newParents = LoopParentsToObject(jsonData[i].parents);
            Types.Generates[] newGenerates = ConvertStringToGenerates(jsonData[i].generates);

            Types.Items newObjectData = new Types.Items
            {
                group = (Item.Group)System.Enum.Parse(typeof(Item.Group), jsonData[i].group),
                type = (Item.Type)System.Enum.Parse(typeof(Item.Type), jsonData[i].type),
                hasLevel = jsonData[i].hasLevel,
                itemName = jsonData[i].itemName,
                parents = newParents,
                generates = newGenerates,
                content = new Types.ItemsData[jsonData[i].content.Length]
            };

            for (int j = 0; j < jsonData[i].content.Length; j++)
            {
                Types.ItemsData newInnerObjectData = new Types.ItemsData
                {
                    group = (Item.Group)System.Enum.Parse(typeof(Item.Group), jsonData[i].group),
                    type = (Item.Type)System.Enum.Parse(typeof(Item.Type), jsonData[i].type),
                    parents = newParents,
                    generates = newGenerates,
                    hasLevel = jsonData[i].hasLevel,
                    itemName = jsonData[i].content[j].itemName,
                    level = count,
                    sprite = dataManager.GetSprite(jsonData[i].content[j].sprite),
                    unlocked = dataManager.CheckUnlocked(jsonData[i].content[j].sprite),
                    isMaxLavel = count == jsonData[i].content.Length
                };

                newObjectData.content[j] = newInnerObjectData;

                count++;
            }

            objectData[i] = newObjectData;
        }

        return objectData;
    }

    public Types.ItemsJson[] ConvertItemsToJson(Types.Items[] objectData)
    {
        Types.ItemsJson[] jsonData = new Types.ItemsJson[objectData.Length];

        for (int i = 0; i < objectData.Length; i++)
        {
            string[] newParents = LoopParentsToJson(objectData[i].parents);
            string[] newGenerates = ConvertGeneratesToString(objectData[i].generates);

            Types.ItemsJson newJsonData = new Types.ItemsJson
            {
                group = objectData[i].group.ToString(),
                type = objectData[i].type.ToString(),
                hasLevel = objectData[i].hasLevel,
                itemName = objectData[i].itemName,
                parents = newParents,
                generates = newGenerates,
                content = new Types.ItemsDataJson[objectData[i].content.Length],
            };

            for (int j = 0; j < objectData[i].content.Length; j++)
            {
                Types.ItemsDataJson newInnerJsonData = new Types.ItemsDataJson
                {
                    group = objectData[i].group.ToString(),
                    type = objectData[i].type.ToString(),
                    parents = newParents,
                    generates = newGenerates,
                    hasLevel = objectData[i].hasLevel,
                    sprite = objectData[i].content[j].sprite.name,
                    itemName =
                        objectData[i].type == Item.Type.Gen
                            ? objectData[i].itemName
                            : objectData[i].content[j].itemName,
                    level = objectData[i].content[j].level,
                    unlocked = false
                };

                newJsonData.content[j] = newInnerJsonData;
            }

            jsonData[i] = newJsonData;
        }

        return jsonData;
    }

    //// BOARD ////

    public Types.Board[] ConvertBoardFromJson(Types.BoardJson[] jsonData)
    {
        Types.Board[] objectData = new Types.Board[jsonData.Length];

        for (int i = 0; i < jsonData.Length; i++)
        {
            Types.Board newObjectData = new Types.Board
            {
                sprite = dataManager.GetSprite(jsonData[i].sprite),
                state = (Item.State)System.Enum.Parse(typeof(Item.State), jsonData[i].state),
                group = (Item.Group)System.Enum.Parse(typeof(Item.Group), jsonData[i].group),
                crate = jsonData[i].crate,
            };

            objectData[i] = newObjectData;
        }

        return objectData;
    }

    public Types.BoardJson[] ConvertBoardToJson(Types.Board[] objectData)
    {
        Types.BoardJson[] jsonData = new Types.BoardJson[objectData.Length];

        for (int i = 0; i < objectData.Length; i++)
        {
            Types.BoardJson newJsonData = new Types.BoardJson
            {
                sprite = objectData[i].sprite == null ? "" : objectData[i].sprite.name,
                state = objectData[i].state.ToString(),
                group = objectData[i].group.ToString(),
                crate = objectData[i].crate,
            };

            jsonData[i] = newJsonData;
        }

        return jsonData;
    }

    //// OTHER ////

    Item.Group[] LoopParentsToObject(string[] newParents)
    {
        Item.Group[] parents = new Item.Group[newParents.Length];

        for (int i = 0; i < newParents.Length; i++)
        {
            parents[i] = (Item.Group)System.Enum.Parse(typeof(Item.Group), newParents[i]);
        }

        return parents;
    }

    string[] LoopParentsToJson(Item.Group[] newParents)
    {
        string[] parents = new string[newParents.Length];

        for (int i = 0; i < newParents.Length; i++)
        {
            parents[i] = newParents[i].ToString();
        }

        return parents;
    }

    Types.Generates[] ConvertStringToGenerates(string[] newGenerates)
    {
        if (newGenerates.Length > 0)
        {
            Types.Generates[] generates = new Types.Generates[newGenerates.Length];

            for (int i = 0; i < newGenerates.Length; i++)
            {
                string[] splitString = newGenerates[i].Split("_");

                generates[i] = new Types.Generates
                {
                    group = (Item.Group)System.Enum.Parse(typeof(Item.Group), splitString[0]),
                    chance = float.Parse(splitString[1])
                };
            }

            return generates;
        }
        else
        {
            return new Types.Generates[0];
        }
    }

    string[] ConvertGeneratesToString(Types.Generates[] newGenerates)
    {
        if (newGenerates.Length > 0)
        {
            string[] generates = new string[newGenerates.Length];

            for (int i = 0; i < newGenerates.Length; i++)
            {
                string combinedString =
                    newGenerates[i].group.ToString() + "_" + newGenerates[i].chance;

                generates[i] = combinedString;
            }

            return generates;
        }
        else
        {
            return new string[0];
        }
    }
}
