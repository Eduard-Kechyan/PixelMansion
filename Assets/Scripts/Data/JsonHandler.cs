using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class JsonHandler : MonoBehaviour
{
    private GameData gameData;
    private ItemHandler itemHandler;

    void Start()
    {
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
            Types.Type newType = (Types.Type)
                System.Enum.Parse(typeof(Types.Type), boardJson[i].type);

            Types.Board newBoardData = new Types.Board
            {
                sprite = gameData.GetSprite(boardJson[i].sprite, newType),
                state = (Types.State)System.Enum.Parse(typeof(Types.State), boardJson[i].state),
                type = newType,
                group = (Types.Group)System.Enum.Parse(typeof(Types.Group), boardJson[i].group),
                genGroup = (Types.GenGroup)
                    System.Enum.Parse(typeof(Types.GenGroup), boardJson[i].genGroup),
                collGroup = (Types.CollGroup)
                    System.Enum.Parse(typeof(Types.CollGroup), boardJson[i].collGroup),
                crate = boardJson[i].crate,
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
            Types.BoardJson newBoardJson = new Types.BoardJson
            {
                sprite = boardData[i].sprite == null ? "" : boardData[i].sprite.name,
                state = boardData[i].state.ToString(),
                type = boardData[i].type.ToString(),
                group = boardData[i].group.ToString(),
                genGroup = boardData[i].genGroup.ToString(),
                collGroup = boardData[i].collGroup.ToString(),
                crate = initialLoop ? RandomCrateInt() : boardData[i].crate,
            };

            boardJson[i] = newBoardJson;
        }

        return JsonConvert.SerializeObject(boardJson);
    }

    //// BONUS ////

    public List<Types.Bonus> ConvertBonusFromJson(string bonusString)
    {
        Types.BonusJson[] bonusJson = JsonConvert.DeserializeObject<Types.BonusJson[]>(bonusString);

        List<Types.Bonus> bonusData =new List<Types.Bonus>();

        for (int i = 0; i < bonusJson.Length; i++)
        {
            Types.Type newType = (Types.Type)
                System.Enum.Parse(typeof(Types.Type), bonusJson[i].type);

            Types.Bonus newBonusData = new Types.Bonus
            {
                sprite = gameData.GetSprite(bonusJson[i].sprite, newType),
                type = newType,
                group = (Types.Group)System.Enum.Parse(typeof(Types.Group), bonusJson[i].group),
                genGroup = (Types.GenGroup)
                    System.Enum.Parse(typeof(Types.GenGroup), bonusJson[i].genGroup),
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
            Types.BonusJson newBonusJson = new Types.BonusJson
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

    //// TIMERS ////

    public List<Types.Timer> ConvertTimersFromJson(string timersString)
    {
        List<Types.Timer> timers = new List<Types.Timer>();

        Types.TimerJson[] timerJson = JsonConvert.DeserializeObject<Types.TimerJson[]>(
            timersString
        );

        for (int i = 0; i < timerJson.Length; i++)
        {
            timers.Add(
                new Types.Timer
                {
                    timerName = timerJson[i].timerName,
                    type = (Types.TimerType)
                        System.Enum.Parse(typeof(Types.TimerType), timerJson[i].type),
                    dateTime = System.DateTime.Parse(timerJson[i].dateTime),
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
            Types.TimerJson newTimerJson = new Types.TimerJson
            {
                timerName = timers[i].timerName,
                type = timers[i].type.ToString(),
                dateTime = timers[i].dateTime.ToString(),
            };

            timerJson[i] = newTimerJson;
        }

        return JsonConvert.SerializeObject(timerJson);
    }

    //// OTHER ////

    int RandomCrateInt()
    {
        int randomInt = Random.Range(0, itemHandler.crateSprites.Length);

        return randomInt;
    }

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
