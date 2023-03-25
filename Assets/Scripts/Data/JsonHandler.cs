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
    /*
        public Types.Items[] ConvertItemsFromJson(Types.ItemsJson[] jsonData = null)
        {
            Types.Items[] objectData = new Types.Items[jsonData.Length];
    
            for (int i = 0; i < jsonData.Length; i++)
            {
                int count = 1;
    
                Types.GenGroup[] newParents = LoopParentsToObject(jsonData[i].parents);
    
                Types.Items newObjectData = new Types.Items
                {
                    group = (Types.Group)System.Enum.Parse(typeof(Types.Group), jsonData[i].group),
                    hasLevel = jsonData[i].hasLevel,
                    itemName = jsonData[i].itemName,
                    parents = newParents,
                    content = new Types.ItemsData[jsonData[i].content.Length]
                };
    
                for (int j = 0; j < jsonData[i].content.Length; j++)
                {
                    Types.ItemsData newInnerObjectData = new Types.ItemsData
                    {
                        group = (Types.Group)System.Enum.Parse(typeof(Types.Group), jsonData[i].group),
                        parents = newParents,
                        hasLevel = jsonData[i].hasLevel,
                        itemName = jsonData[i].content[j].itemName,
                        level = count,
                        sprite = GameData.GetSprite(jsonData[i].content[j].sprite, Types.Type.Default),
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
    
                Types.ItemsJson newJsonData = new Types.ItemsJson
                {
                    group = objectData[i].group.ToString(),
                    hasLevel = objectData[i].hasLevel,
                    itemName = objectData[i].itemName,
                    parents = newParents,
                    content = new Types.ItemsDataJson[objectData[i].content.Length],
                };
    
                for (int j = 0; j < objectData[i].content.Length; j++)
                {
                    Types.ItemsDataJson newInnerJsonData = new Types.ItemsDataJson
                    {
                        group = objectData[i].group.ToString(),
                        parents = newParents,
                        hasLevel = objectData[i].hasLevel,
                        sprite = objectData[i].content[j].sprite.name,
                        itemName =
                            objectData[i].content[j].itemName != ""
                                ? objectData[i].content[j].itemName
                                : objectData[i].itemName,
                        level = objectData[i].content[j].level,
                        unlocked = false
                    };
    
                    newJsonData.content[j] = newInnerJsonData;
                }
    
                jsonData[i] = newJsonData;
            }
    
            return jsonData;
        }
    
        //// GENERATORS ////
        public Types.Generators[] ConvertGeneratorsFromJson(Types.GeneratorsJson[] jsonData = null)
        {
            Types.Generators[] objectData = new Types.Generators[jsonData.Length];
    
            for (int i = 0; i < jsonData.Length; i++)
            {
                int count = 1;
    
                Types.Creates[] newCreates = ConvertStringToCreates(jsonData[i].creates);
    
                Types.Generators newObjectData = new Types.Generators
                {
                    genGroup = (Types.GenGroup)
                        System.Enum.Parse(typeof(Types.GenGroup), jsonData[i].genGroup),
                    hasLevel = jsonData[i].hasLevel,
                    itemName = jsonData[i].itemName,
                    creates = newCreates,
                    content = new Types.GeneratorsData[jsonData[i].content.Length]
                };
    
                for (int j = 0; j < jsonData[i].content.Length; j++)
                {
                    Types.GeneratorsData newInnerObjectData = new Types.GeneratorsData
                    {
                        genGroup = (Types.GenGroup)
                            System.Enum.Parse(typeof(Types.GenGroup), jsonData[i].genGroup),
                        hasLevel = jsonData[i].hasLevel,
                        itemName = jsonData[i].content[j].itemName,
                        creates = newCreates,
                        level = count,
                        sprite = GameData.GetSprite(jsonData[i].content[j].sprite, Types.Type.Default),
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
    
        public Types.GeneratorsJson[] ConvertGeneratorsToJson(Types.Generators[] objectData)
        {
            Types.GeneratorsJson[] jsonData = new Types.GeneratorsJson[objectData.Length];
    
            for (int i = 0; i < objectData.Length; i++)
            {
                string[] newCreates = ConvertCreatesToString(objectData[i].creates);
    
                Types.GeneratorsJson newJsonData = new Types.GeneratorsJson
                {
                    genGroup = objectData[i].genGroup.ToString(),
                    hasLevel = objectData[i].hasLevel,
                    itemName = objectData[i].itemName,
                    creates = newCreates,
                    content = new Types.GeneratorsDataJson[objectData[i].content.Length],
                };
    
                for (int j = 0; j < objectData[i].content.Length; j++)
                {
                    Debug.Log(objectData[i].content[j].itemName);
                    Types.GeneratorsDataJson newInnerJsonData = new Types.GeneratorsDataJson
                    {
                        genGroup = objectData[i].genGroup.ToString(),
                        creates = newCreates,
                        hasLevel = objectData[i].hasLevel,
                        sprite = objectData[i].content[j].sprite.name,
                        itemName =
                            objectData[i].content[j].itemName != ""
                                ? objectData[i].content[j].itemName
                                : objectData[i].itemName,
                        level = objectData[i].content[j].level,
                        unlocked = false
                    };
    
                    newJsonData.content[j] = newInnerJsonData;
                }
    
                jsonData[i] = newJsonData;
            }
    
            return jsonData;
        }
    */
    //// BOARD ////

    public Types.Board[] ConvertBoardFromJson(Types.BoardJson[] jsonData)
    {
        Types.Board[] objectData = new Types.Board[jsonData.Length];

        for (int i = 0; i < jsonData.Length; i++)
        {
            Types.Type newType = (Types.Type)
                System.Enum.Parse(typeof(Types.Type), jsonData[i].type);

            Types.Board newObjectData = new Types.Board
            {
                sprite = GameData.GetSprite(jsonData[i].sprite, newType),
                state = (Types.State)System.Enum.Parse(typeof(Types.State), jsonData[i].state),
                type = newType,
                group = (Types.Group)System.Enum.Parse(typeof(Types.Group), jsonData[i].group),
                genGroup = (Types.GenGroup)
                    System.Enum.Parse(typeof(Types.GenGroup), jsonData[i].genGroup),
                crate = jsonData[i].crate,
                order = 0,
                loc = Vector2Int.zero,
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
                type = objectData[i].type.ToString(),
                group = objectData[i].group.ToString(),
                genGroup = objectData[i].genGroup.ToString(),
                crate = objectData[i].crate,
            };

            jsonData[i] = newJsonData;
        }

        return jsonData;
    }

    //// OTHER ////

    Types.GenGroup[] LoopParentsToObject(string[] newParents)
    {
        Types.GenGroup[] parents = new Types.GenGroup[newParents.Length];

        for (int i = 0; i < newParents.Length; i++)
        {
            parents[i] = (Types.GenGroup)System.Enum.Parse(typeof(Types.GenGroup), newParents[i]);
        }

        return parents;
    }

    string[] LoopParentsToJson(Types.GenGroup[] newParents)
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
                    group = (Types.Group)System.Enum.Parse(typeof(Types.Group), splitString[0]),
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
    }
}
