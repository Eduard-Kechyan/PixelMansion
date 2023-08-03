using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CI.QuickSave;
using Newtonsoft.Json;

public class TimeManager : MonoBehaviour
{
    private DataManager dataManager;
    private GameData gameData;
    private EnergyTimer energyTimer;

    [Serializable]
    public class EnergyTimerClass
    {
        public DateTime startDate;
        public int seconds;
    }

    // Instance
    public static TimeManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

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
                startDate = newTimer,
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
                DateTime startDate = gameData.timers[i].startDate;

                TimeSpan diff = DateTime.Now - startDate;

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

    public void SetEnergyTimer(int newSeconds)
    {
        RemoveEnergyTimer(false);

        Types.Timer newEnergyTimer = new()
        {
            startDate = DateTime.UtcNow,
            seconds = newSeconds,
            on = true,
            type = Types.TimerType.Energy
        };

        gameData.timers.Add(newEnergyTimer);

        dataManager.SaveTimers();
    }

    public Types.Timer GetEnergyTimer()
    {
        Types.Timer newEnergyTimer = new()
        {
            on = false,
        };

        if (gameData.timers.Count > 0)
        {
            int index = -1;

            for (int i = 0; i < gameData.timers.Count; i++)
            {
                if (gameData.timers[i].type == Types.TimerType.Energy)
                {
                    index = i;
                }
            }

            if (index >= 0)
            {
                return gameData.timers[index];
            }
            else
            {
                return newEnergyTimer;
            }
        }
        else
        {
            return newEnergyTimer;
        }
    }

    public void RemoveEnergyTimer(bool save = true)
    {
        if (gameData.timers.Count > 0)
        {
            int index = -1;

            for (int i = 0; i < gameData.timers.Count; i++)
            {
                if (gameData.timers[i].type == Types.TimerType.Energy)
                {
                    index = i;
                }
            }

            if (index >= 0)
            {
                gameData.timers.RemoveAt(index);

                if (save)
                {
                    dataManager.SaveTimers();
                }
            }
        }
    }
}
