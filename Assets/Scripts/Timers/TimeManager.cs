using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CI.QuickSave;
using Newtonsoft.Json;

namespace Merge
{
    public class TimeManager : MonoBehaviour
    {
        // Variables
        [HideInInspector]
        public ClockManager clockManager;
        [HideInInspector]
        public InfoBox infoBox;
        [HideInInspector]
        public BoardManager boardManager;

        private bool handlingTimers;

        // References
        private DataManager dataManager;
        private GameData gameData;
        private EnergyTimer energyTimer;

        [Serializable]
        public class EnergyTimerClass
        {
            public DateTime startTime;
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

            StartCoroutine(WaitForGameData());
        }

        IEnumerator WaitForGameData()
        {
            while (!dataManager.loaded)
            {
                yield return null;
            }

            CheckInitialTimers();
        }

        //// Timers ////
        public void AddTimer(Types.TimerType type, string id, Vector2 position = default, int seconds = 0)
        {
            DateTime startTime = DateTime.UtcNow;

            // TODO - Add notification

            gameData.timers.Add(
                new Types.Timer
                {
                    startTime = startTime,
                    type = type,
                    id = id,
                    seconds = seconds
                }
            );

            if (type == Types.TimerType.Item)
            {
                ToggleTimerOnBoardData(id, true);

                clockManager.AddClock(position, id, startTime, seconds);
            }

            if (!handlingTimers)
            {
                InvokeRepeating("HandleTimers", 0f, 0.5f);
            }

            dataManager.SaveTimers();
        }

        public void RemoveTimer(string id)
        {
            int index = 0;
            int count = 0;

            for (int i = 0; i < gameData.timers.Count; i++)
            {
                if (gameData.timers[i].id == id)
                {
                    index = count;
                }

                count++;
            }

            gameData.timers.RemoveAt(index);

            dataManager.SaveTimers();
        }

        public bool CheckTimer(string id)
        {
            bool finished = false;

            for (int i = 0; i < gameData.timers.Count; i++)
            {
                if (gameData.timers[i].id == id)
                {
                    DateTime startTime = gameData.timers[i].startTime;

                    TimeSpan diff = DateTime.Now - startTime;

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

        void CheckInitialTimers()
        {
            // Start time handler
            if (gameData.timers.Count > 0)
            {
                InvokeRepeating("HandleTimers", 0f, 0.5f);

                handlingTimers = true;
            }
        }

        void HandleTimers()
        {
            List<int> indexesToRemove = new();

            for (int i = 0; i < gameData.timers.Count; i++)
            {
                if (gameData.timers[i].type == Types.TimerType.Item)
                {
                    DateTime endTime = DateTime.UtcNow;

                    double timeDiffInSeconds = (endTime - gameData.timers[i].startTime).TotalSeconds;

                    if (timeDiffInSeconds >= gameData.timers[i].seconds)
                    {
                        // End
                        indexesToRemove.Add(i);

                        ToggleTimerOnBoardData(gameData.timers[i].id, false);
                    }
                    else
                    {
                        // Continue
                        if (clockManager != null)
                        {
                            clockManager.SetFillAmount(gameData.timers[i].id, timeDiffInSeconds);
                        }

                        if (infoBox != null)
                        {
                            infoBox.TryToSetTimer(gameData.timers[i].id, CalcTimerText(gameData.timers[i].seconds, timeDiffInSeconds));
                        }
                    }
                }
            }

            for (int i = 0; i < indexesToRemove.Count; i++)
            {
                if (clockManager != null)
                {
                    clockManager.RemoveClock(gameData.timers[indexesToRemove[i]].id);
                }

                gameData.timers.RemoveAt(indexesToRemove[i]);

                dataManager.SaveTimers();
            }

            if (gameData.timers.Count == 0)
            {
                CancelInvoke("HandleTimers");

                handlingTimers = false;
            }
        }

        void ToggleTimerOnBoardData(string id, bool enable)
        {
            bool found = false;

            int count = 0;

            for (int x = 0; x < gameData.boardData.GetLength(0); x++)
            {
                for (int y = 0; y < gameData.boardData.GetLength(1); y++)
                {
                    if (gameData.boardData[x, y].id == id)
                    {
                        gameData.boardData[x, y].timerOn = enable;

                        boardManager.ToggleTimerOnItem(count, enable);

                        found = true;

                        break;
                    }

                    count++;
                }
            }

            if (found)
            {
                dataManager.SaveBoard(false, false);

                infoBox.Refresh();
            }
        }

        string CalcTimerText(int totalSeconds, double passedSeconds)
        {
            string minutes;
            string seconds;

            float secondsDiff = totalSeconds - (float)passedSeconds;

            int minutesPre = Mathf.FloorToInt(secondsDiff / 60);

            int secondsPre = Mathf.FloorToInt(secondsDiff - (minutesPre * 60));

            if (minutesPre < 10)
            {
                minutes = "0" + minutesPre;
            }
            else
            {
                minutes = minutesPre.ToString(); ;
            }

            if (secondsPre < 10)
            {
                seconds = "0" + secondsPre;
            }
            else
            {
                seconds = secondsPre.ToString();
            }

            return minutes + ":" + seconds;
        }

        public void CheckCoolDown(Item item)
        {
            // Check if cool down already exists
            if (gameData.coolDowns.Count > 0)
            {
                for (int i = 0; i < gameData.coolDowns.Count; i++)
                {
                    if (gameData.coolDowns[i].id == item.id)
                    {
                        gameData.coolDowns[i].count++;

                        if (gameData.coolDowns[i].count == item.coolDown.maxCount)
                        {
                            item.timerOn = true;
                            item.timerSeconds = item.coolDown.seconds;

                            AddTimer(Types.TimerType.Item, item.id, item.transform.position, item.coolDown.seconds);

                            gameData.coolDowns.RemoveAt(i);
                        }

                        return;
                    }
                }
            }

            // If cool down doesn't exists, add one
            gameData.coolDowns.Add(new()
            {
                count = 1,
                id = item.id,
            });
        }

        //// Energy ////
        public void SetEnergyTimer(int newSeconds)
        {
            RemoveEnergyTimer(false);

            Types.Timer newEnergyTimer = new()
            {
                startTime = DateTime.UtcNow,
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
}