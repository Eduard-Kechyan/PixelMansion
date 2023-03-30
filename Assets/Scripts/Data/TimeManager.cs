using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CI.QuickSave;

public class TimeManager : MonoBehaviour
{
    private DataManager dataManager;
    private GameData gameData;

    void Start()
    {
        dataManager = DataManager.Instance;

        gameData = GameData.Instance;
    }

    public void AddTimer(Types.TimerType type, string timerName = "")
    {
        DateTime newTimer = DateTime.Now;

        gameData.timers.Add(
            new Types.Timer
            {
                dateTime = newTimer,
                type = type,
                timerName = timerName
            }
        );

        dataManager.SaveTimers();
    }

    public void RemoveTimer(string timerName)
    {
        int index = 0;
        int count = 0;

        for (int i = 0; i < gameData.timers.Count; i++)
        {
            if (gameData.timers[i].timerName == timerName)
            {
                index = count;
            }

            count++;
        }

        gameData.timers.RemoveAt(index);

        dataManager.SaveTimers();
    }

    public bool CheckTimer(string timerName)
    {
        bool finished = false;

        for (int i = 0; i < gameData.timers.Count; i++)
        {
            if (gameData.timers[i].timerName == timerName)
            {
                DateTime dateTime = gameData.timers[i].dateTime;

                TimeSpan diff = DateTime.Now - dateTime;

                int seconds = diff.Seconds + (diff.Minutes * 60);

                if (seconds < 1)
                {
                    finished = true;
                }

                break;
            }
        }

        return finished;
    }
}
