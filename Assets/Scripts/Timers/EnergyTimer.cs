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
        public int timeoutSeconds = 120;
        [ReadOnly]
        public int timeoutMinutes = 2;

        [HideInInspector]
        public float timeout;

        [ReadOnly]
        public bool timerOn = false;

        [Header("Debug")]
        public bool devMode = false;
        [Condition("devMode", true)]
        public int timeoutSecondsDev = 10;

        [HideInInspector]
        public bool timerChecked = false;
        private bool running = false;

        private Coroutine counterCoroutine;

        // References
        private TimeManager timeManager;
        private GameData gameData;
        private DataManager dataManager;
        private ValuesUI valuesUI;

        void Start()
        {
            // Cache
            timeManager = GetComponent<TimeManager>();
            gameData = GameData.Instance;
            dataManager = DataManager.Instance;
            valuesUI = GameRefs.Instance.valuesUI;

            valuesUI.energyTimer = this;

            // Change the time if we are in dev mode
#if UNITY_EDITOR
            if (devMode)
            {
                timeoutSeconds = timeoutSecondsDev;
            }
#endif

            timeout = timeoutSeconds;

            StartCoroutine(WaitForGameData());
        }

        void OnEnable()
        {
            // Subscribe to events
            GameData.EnergyUpdatedEventAction += CheckEnergy;
        }

        void OnDisable()
        {
            // Unsubscribe to events
            GameData.EnergyUpdatedEventAction += CheckEnergy;
        }

        void OnValidate()
        {
            timeoutMinutes = timeoutSeconds / 60;
        }

        IEnumerator WaitForGameData()
        {
            while (!dataManager.loaded || !timeManager.energyTimerChecked)
            {
                yield return null;
            }

            // Start time handler
            bool found = false;

            for (int i = 0; i < gameData.timers.Count; i++)
            {
                if (gameData.timers[i].type == Types.TimerType.Energy)
                {
                    found = true;

                    break;
                }
            }

            if (!found)
            {
                timerChecked = true;

                CheckEnergy();
            }
        }

        public void CheckEnergy(bool addTimer = true)
        {
            if (timerChecked)
            {
                if (gameData.energyTemp >= GameData.MAX_ENERGY)
                {
                    timerOn = false;

                    if (addTimer)
                    {
                        timeManager.RemoveEnergyTimer();
                    }

                    Glob.StopTimeout(counterCoroutine);

                    if (this != null)
                    {
                        StopCoroutine(HandleTimer());
                    }
                }
                else
                {
                    int neededEnergy = 100 - gameData.energyTemp;

                    int neededSeconds = neededEnergy * timeoutSeconds;

                    if (addTimer)
                    {
                        timeManager.AddEnergyTimer(neededSeconds);
                    }

                    if (!running)
                    {
                        counterCoroutine = Glob.SetTimeout(() =>
                        {
                            if (!running)
                            {
                                timerOn = true;

                                StartCoroutine(HandleTimer());
                            }
                        }, 0.3f);
                    }
                }
            }
        }

        public void TimerEnd()
        {
            if (gameData.energyTemp < GameData.MAX_ENERGY)
            {
                gameData.SetEnergy(GameData.MAX_ENERGY);
            }

            timerChecked = true;
        }

        public void TimerContinue(float passedSeconds)
        {
            int left = Mathf.CeilToInt(passedSeconds / timeoutSeconds);

            int leftoverSeconds = (left * timeoutSeconds) - Mathf.RoundToInt(passedSeconds);

            timerOn = true;

            StartCoroutine(HandleTimer(leftoverSeconds));
        }

        IEnumerator HandleTimer(int leftoverSeconds = 0)
        {
            running = true;

            int currentTimeout = timeoutSeconds;

            if (leftoverSeconds > 0)
            {
                currentTimeout = leftoverSeconds;

                timerChecked = true;
            }

            WaitForSeconds waitA = new(currentTimeout);
            WaitForSeconds waitB = new(1);

            if (timerOn)
            {
                valuesUI.ToggleEnergyTimer(true);

                while (gameData.energyTemp < GameData.MAX_ENERGY)
                {
                    timeout = currentTimeout;

                    yield return waitA;

                    gameData.UpdateValue(1, Types.CollGroup.Energy, false, true);

                    yield return waitB;
                }

                valuesUI.ToggleEnergyTimer(false);

                running = false;

                CheckEnergy();
            }
        }
    }
}