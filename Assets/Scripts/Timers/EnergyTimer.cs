using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnergyTimer : MonoBehaviour
{
    // Variables
    public int energyTime = 120;
    private float energyTimeOut;
    //private float singleMinuteInSeconds = 60f;

    //private bool energyTimerOn = false;

    private DateTime dateStart;

    public bool devMode = false;
    [Condition("devMode", true)]
    public int energyTimeDevMode = 10;

    private Coroutine innerTimer = null;

    [SerializeField]
    private string energyTimerText = "00:00";

    // References
    private GameData gameData;
    private ValuesUI valuesUI;

    void Start()
    {
        // References
       /* gameData = GameData.Instance;
        valuesUI = GameRefs.Instance.valuesUI;

        energyTimerOn = PlayerPrefs.HasKey("energyTimerOn");

        if (PlayerPrefs.HasKey("energyTimerStart"))
        {
            dateStart = DateTime.Parse(PlayerPrefs.GetString("energyTimerStart"));
        }

#if UNITY_EDITOR
        if (devMode)
        {
            energyTime = energyTimeDevMode;
            singleMinuteInSeconds = 6f;
        }
#endif

        energyTimeOut = energyTime;

        CalcCurrentData();*/
    }

   /* void Update()
    {
        if (energyTimerOn)
        {
            if (energyTimeOut > 0)
            {
                energyTimeOut -= Time.deltaTime;
                UpdateEnergyTimer(energyTimeOut);
            }
            else
            {
                gameData.UpdateEnergy();

                CheckEnergy(true);
            }
        }
    }*/

    public void CheckEnergy(bool fromTimer = false)
    {
        /*if(energy < MAX_ENERGY){
            timeManager.AddEnergyTimer();
        }*/

       /* if (fromTimer)
        {
            energyTimeOut = energyTime;
        }
        else if (!energyTimerOn || energyTimeOut <= 0)
        {
            energyTimeOut = energyTime;
        }

        // Increase energy after set time if energy is less than 100
        if (gameData.energy < 100)
        {
            if (!energyTimerOn)
            {
                PlayerPrefs.SetInt("energyTimerOn", 1);
            }
            else
            {
                CalcCurrentData();
            }

            SetCounter();

            energyTimerOn = true;
        }
        else
        {
            // End the timer and notify the player that the energy is full
            energyTimerOn = false;

            if (fromTimer)
            {
                if (gameData.energy >= 100)
                {
                    Debug.Log("Energy full!");

                    ResetCurrentData();
                }
            }
            else
            {
                if (valuesUI.energyTimer != null)
                {
                    Glob.SetTimout(() =>
                    {
                        valuesUI.energyTimer.style.display = DisplayStyle.None;
                    }, 1f);
                }
            }

            PlayerPrefs.DeleteKey("energyTimerOn");
        }*/
    }

    void UpdateEnergyTimer(float currentTime)
    {
        currentTime++;

        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        string minutesText = minutes < 10 ? "0" + minutes : minutes.ToString();
        string secondsText = seconds < 10 ? "0" + seconds : seconds.ToString();

        energyTimerText = minutesText + ":" + secondsText;

        valuesUI.energyTimer.text = energyTimerText;
        valuesUI.energyTimer.style.display = DisplayStyle.Flex;
    }

    // Dates
    void SetCounter()
    {
        float energyToGetLeft = 100 - gameData.energy;

        if (innerTimer != null)
        {
            Glob.StopTimeout(innerTimer);
        }

        // TODO - Add notification for a full energy when the app is closed after the "timeLeft" in seconds

       /* Debug.Log(energyToGetLeft);
        Debug.Log(energyTime);
        Debug.Log(singleMinuteInSeconds);
        Debug.Log(energyToGetLeft * (energyTime / singleMinuteInSeconds));

        innerTimer = Glob.SetTimout(() =>
        {
            Debug.Log("Energy full B!");
        }, energyToGetLeft * (energyTime / singleMinuteInSeconds));*/

        /*dateStart = DateTime.UtcNow;

        PlayerPrefs.SetString("energyTimerStart", dateStart.ToString());*/
    }

    void CalcCurrentData()
    {
        DateTime dateNow = DateTime.UtcNow;

        //Debug.Log(dateNow - dateStart);
    }

    void ResetCurrentData()
    {
        PlayerPrefs.DeleteKey("energyTimerStart");
    }
}
