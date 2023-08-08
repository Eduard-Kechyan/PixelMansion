using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glob : MonoBehaviour
{
    // Variables
    public static AnimationCurve defaultChanceCurve;
    public static PopupManager popupManager;

    // Instance
    public static Glob Instance;

    private static List<Coroutine> timeouts = new List<Coroutine>();
    private static List<Coroutine> intervals = new List<Coroutine>();

    void Awake()
    {
        Instance = this;

        popupManager = GetComponent<PopupManager>();
    }

    //// TIMEOUT ////
    public static Coroutine SetTimeout(Action function, float seconds = 1f)
    {
        Coroutine newTimeout = Instance.StartCoroutine(SetTimeoutCoroutine(function, seconds));

        timeouts.Add(newTimeout);

        return newTimeout;
    }

    private static IEnumerator SetTimeoutCoroutine(Action function, float seconds = 1f)
    {
        yield return new WaitForSeconds(seconds);

        function();
    }

    public static void StopTimeout(Coroutine timeout)
    {
        timeouts.Remove(timeout);
    }

    //// INTERVALS ////
    public static Coroutine SetInterval(Action function, float seconds = 1f, bool callOnce = true)
    {
        Coroutine newInterval = Instance.StartCoroutine(SetTimeoutCoroutine(function, seconds));

        intervals.Add(newInterval);

        return Instance.StartCoroutine(SetIntervalCoroutine(function, seconds, callOnce));
    }

    private static IEnumerator SetIntervalCoroutine(Action function, float seconds, bool callOnce)
    {
        while (true)
        {
            if (callOnce)
            {
                function();

                yield return new WaitForSeconds(seconds);
            }
            else
            {
                yield return new WaitForSeconds(seconds);

                function();
            }
        }
    }

    public static void StopInterval(Coroutine interval)
    {
        intervals.Remove(interval);
    }

    //// CHANCES ////
    public static float CalcChances(float[] chances)
    {
        float randomPoint = UnityEngine.Random.value * chances.Length;

        for (int i = 0; i < chances.Length; i++)
        {
            if (randomPoint < chances[i])
            {
                return i;
            }
            else
            {
                randomPoint -= chances[i];
            }
        }

        return chances.Length - 1;
    }

    public static float CalcCurvedChances(AnimationCurve curve = null)
    {
        if (curve == null)
        {
            curve = defaultChanceCurve;
        }

        return curve.Evaluate(UnityEngine.Random.value);
    }

    //// OTHER ////
    public static T ParseEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }
}
