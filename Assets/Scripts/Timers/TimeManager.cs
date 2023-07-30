using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CI.QuickSave;

public class TimeManager : MonoBehaviour
{
    private DataManager dataManager;
    private GameData gameData;
    private EnergyTimer energyTimer;


    void Start()
    {
        dataManager = DataManager.Instance;
        gameData = GameData.Instance;
        energyTimer = GetComponent<EnergyTimer>();
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

    public void AddEnergyTimer()
    {
        int energyNeeded = GameData.MAX_ENERGY - gameData.energy;

        int totalSeconds = (int)Mathf.Floor(energyNeeded * energyTimer.energyTime);

        Debug.Log(totalSeconds);

        AddTimer(Types.TimerType.Energy, "Energy");

        // gameData.UpdateEnergy(1);
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

    public void CheckTimers()
    {
        for (int i = 0; i < gameData.timers.Count; i++)
        {
            if (gameData.timers[i].type == Types.TimerType.Energy)
            {
                CheckEnergyTimer(gameData.timers[i]);
            }
            else if (gameData.timers[i].type == Types.TimerType.Item)
            {
                CheckItemTimer(gameData.timers[i]);
            }
        }
    }

    void CheckEnergyTimer(Types.Timer timer)
    {
        //RemoveTimer(timer.timerName);
    }

    void CheckItemTimer(Types.Timer timer)
    {
        //RemoveTimer(timer.timerName);
    }
}
