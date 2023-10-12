using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class EnergyTimer : MonoBehaviour
    {
        // Variables
        public int time = 120;

        [Header("Debug")]
        public bool devMode = false;
        [Condition("devMode", true)]
        public int energyTimeDevMode = 10;

        [Header("Stats")]
        [ReadOnly]
        public bool timerOn = false;
        [ReadOnly]
        public bool waiting = false;
        [ReadOnly]
        public float timeOut;

        private DateTime startDate;
        private int startSeconds = 0;

        private Coroutine energyCoroutine;
        private bool gottenData = false;

        // References
        private TimeManager timeManager;
        private GameData gameData;

        void Start()
        {
            // References
            timeManager = GetComponent<TimeManager>();
            gameData = GameData.Instance;

            // Change the time if we are in dev mode
#if UNITY_EDITOR
            if (devMode)
            {
                time = energyTimeDevMode;
            }
#endif

            // Set the time
            timeOut = time;

            // GetTimerDate();
        }

        void Update()
        {
            if (timerOn)
            {
                if (timeOut > 0)
                {
                    timeOut -= Time.deltaTime;
                }
                else
                {
                    // Stop the timer here is important
                    timerOn = false;
                    waiting = true;

                    energyCoroutine = Glob.SetTimeout(() =>
                    {
                        waiting = false;
                        gameData.UpdateValue( 1, Types.CollGroup.Energy, false, true);
                    }, 0.5f);
                }
            }
        }

        // Check if there is a need for the timer to be on
        public void Check(int newTimeOut = -1)
        {
            // Reset the time out
            if (newTimeOut == -1)
            {
                if (!timerOn || timeOut <= 0)
                {
                    timeOut = time;
                }
            }
            else
            {
                timeOut = newTimeOut;
            }

            // Check if energy is less than the maximum amount
            if (gameData.energy < GameData.MAX_ENERGY)
            {
                // Enable the timer
                timerOn = true;

                Glob.StopTimeout(energyCoroutine);

                SetTimerDate();
            }
            else
            {
                // Disable the timer
                timerOn = false;

                // TODO - Notifiy the player that energy is full, and remove this line after that
                Debug.Log("Energy full!");

                ClearTimerDate();
            }

        }

        void SetTimerDate()
        {
            startDate = DateTime.UtcNow;
            startSeconds = ((time * (GameData.MAX_ENERGY - gameData.energy)) - time) + (int)timeOut;

            Debug.Log("AAAAAAAAAA");

            timeManager.SetEnergyTimer(startSeconds);
        }

        void GetTimerDate(GeometryChangedEvent evt = null)
        {
            if (!gottenData)
            {
                gottenData = true;

                DateTime endDate = DateTime.UtcNow;

                Types.Timer timerData = timeManager.GetEnergyTimer();

                if (timerData.on)
                {
                    startDate = timerData.startDate;
                    startSeconds = timerData.seconds;

                    TimeSpan difference = endDate - startDate;

                    if (difference.Seconds >= startSeconds)
                    {
                        timeManager.RemoveEnergyTimer();
                    }
                    else
                    {
                        int test1 = startSeconds / time;
                        int test2 = startSeconds - ((time - 1) * test1);

                        Check(test2);
                    }
                }
            }
        }

        void ClearTimerDate()
        {
            timeManager.RemoveEnergyTimer();
        }
    }
}