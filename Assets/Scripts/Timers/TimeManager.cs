using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Merge
{
    public class TimeManager : MonoBehaviour
    {
        // Variables
        private bool handlingTimers;

        [HideInInspector]
        public bool energyTimerChecked = false;

        // References
        private DataManager dataManager;
        private GameData gameData;
        private EnergyTimer energyTimer;
        private ClockManager clockManager;
        private InfoBox infoBox;
        private BoardManager boardManager;
        private NotificsManager notificsManager;

        void Start()
        {
            dataManager = DataManager.Instance;
            gameData = GameData.Instance;
            energyTimer = GetComponent<EnergyTimer>();
            notificsManager = Services.Instance.GetComponent<NotificsManager>();

            Init();
        }

        void Init()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            if (sceneName == "Gameplay")
            {
                clockManager = GameRefs.Instance.clockManager;
                infoBox = GameRefs.Instance.infoBox;
                boardManager = GameRefs.Instance.boardManager;
            }

            StartCoroutine(WaitForGameData());
        }

        //// Timers ////

        void HandleTimers()
        {
            for (int i = gameData.timers.Count - 1; i >= 0; i--)
            {
                if (gameData.timers[i].running)
                {
                    DateTime endTime = DateTime.UtcNow;

                    double timeDiffInSeconds = (endTime - gameData.timers[i].startTime).TotalSeconds;

                    if (timeDiffInSeconds >= gameData.timers[i].seconds)
                    {
                        // End
                        ToggleTimerOnBoardData(gameData.timers[i].id, false);

                        if (clockManager != null && gameData.timers[i].type == Types.TimerType.Item)
                        {
                            clockManager.RemoveClock(gameData.timers[i].id);
                        }

                        if (!energyTimerChecked && gameData.timers[i].type == Types.TimerType.Energy)
                        {
                            energyTimerChecked = true;

                            energyTimer.TimerEnd();
                        }

                        gameData.timers.RemoveAt(i);

                        dataManager.SaveTimers();
                    }
                    else
                    {
                        // Continue
                        if (gameData.timers[i].type == Types.TimerType.Energy)
                        {
                            if (!energyTimerChecked)
                            {
                                energyTimerChecked = true;

                                energyTimer.TimerContinue((float)timeDiffInSeconds);
                            }
                        }
                        else
                        {
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
            }

            energyTimerChecked = true;

            if (gameData.timers.Count == 0)
            {
                CancelInvoke("HandleTimers");

                handlingTimers = false;
            }
        }

        public string GetTimerText(string id)
        {
            for (int i = 0; i < gameData.timers.Count; i++)
            {
                if (gameData.timers[i].id == id)
                {
                    DateTime endTime = DateTime.UtcNow;

                    double timeDiffInSeconds = (endTime - gameData.timers[i].startTime).TotalSeconds;

                    return CalcTimerText(gameData.timers[i].seconds, timeDiffInSeconds);
                }
            }

            return "00:00";
        }

        public void AddTimer(Types.TimerType type, Types.NotificationType notificationType, string itemName, string id, Vector2 position = default, int seconds = 0)
        {
            if (type == Types.TimerType.Energy)
            {
                Debug.LogWarning("You gave TimerType Energy to the AddTimer method. Use AddEnergyTimer instead!");
            }

            DateTime startTime = DateTime.UtcNow;

            int notificationId = notificsManager.Add(notificationType, startTime.AddSeconds(seconds), itemName);

            gameData.timers.Add(
                new Types.Timer
                {
                    startTime = startTime,
                    type = type,
                    id = id,
                    seconds = seconds,
                    running = true,
                    notificationId = notificationId
                }
            );

            if (type == Types.TimerType.Item)
            {
                ToggleTimerOnBoardData(id, true);

                clockManager.AddClock(position, id, seconds);
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
                    notificsManager.Remove(gameData.timers[i].notificationId);

                    index = count;

                    break;
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

        IEnumerator WaitForGameData()
        {
            while (!dataManager.loaded)
            {
                yield return null;
            }

            // Start time handler
            if (gameData.timers.Count > 0)
            {
                InvokeRepeating("HandleTimers", 0f, 0.5f);

                handlingTimers = true;
            }
            else
            {
                energyTimerChecked = true; 
            }
        }

        //// Items ////

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

                        if (boardManager != null)
                        {
                            boardManager.ToggleTimerOnItem(count, enable);
                        }

                        found = true;

                        break;
                    }

                    count++;
                }
            }

            if (found)
            {
                dataManager.SaveBoard(false, false);

                if (infoBox != null)
                {
                    infoBox.Refresh();
                }
            }
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

                            Types.NotificationType notificationType = item.type == Types.Type.Gen ? Types.NotificationType.Gen : Types.NotificationType.Chest;

                            // Note: use item.itemName not item.name 
                            AddTimer(Types.TimerType.Item, notificationType, item.itemName, item.id, item.transform.position, item.coolDown.seconds);

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

        public void ItemPutIntoInventory(string id, DateTime altTime)
        {
            for (int i = 0; i < gameData.timers.Count; i++)
            {
                if (gameData.timers[i].id == id)
                {
                    int leftOverSeconds = Mathf.FloorToInt(gameData.timers[i].seconds - (float)(altTime - gameData.timers[i].startTime).TotalSeconds);

                    gameData.timers[i].seconds = leftOverSeconds;
                    gameData.timers[i].startTime = altTime;
                    gameData.timers[i].running = false;

                    break;
                }
            }

            dataManager.SaveTimers();

            clockManager.RemoveClock(id);
        }

        public void ItemTakenOutOfInventory(string id)
        {
            for (int i = 0; i < gameData.timers.Count; i++)
            {
                if (gameData.timers[i].id == id)
                {
                    gameData.timers[i].running = true;

                    break;
                }
            }

            dataManager.SaveTimers();
        }

        public void ItemTakenOutOfInventoryAfter(Vector2 position, string id)
        {
            for (int i = 0; i < gameData.timers.Count; i++)
            {
                if (gameData.timers[i].id == id)
                {
                    clockManager.AddClock(position, id, gameData.timers[i].seconds);

                    break;
                }
            }
        }

        //// Energy ////

        public void AddEnergyTimer(int seconds)
        {
            RemoveEnergyTimer(false);
            DateTime startTime = DateTime.UtcNow;

            int notificationId = notificsManager.Add(Types.NotificationType.Energy, startTime.AddSeconds(seconds));

            Types.Timer newEnergyTimer = new()
            {
                startTime = DateTime.UtcNow,
                seconds = seconds,
                id = "energy_timer",
                type = Types.TimerType.Energy,
                notificationId = notificationId
            };

            gameData.timers.Add(newEnergyTimer);

            dataManager.SaveTimers();
        }

        public void RemoveEnergyTimer(bool save = true)
        {
            for (int i = gameData.timers.Count - 1; i >= 0; i--)
            {
                if (gameData.timers[i].type == Types.TimerType.Energy)
                {
                    notificsManager.Remove(gameData.timers[i].notificationId);

                    gameData.timers.Remove(gameData.timers[i]);
                }
            }

            if (save)
            {
                dataManager.SaveTimers();
            }
        }
    }
}