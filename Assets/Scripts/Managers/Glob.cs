using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glob : MonoBehaviour
{
    // Instance
    public static Glob Instance;

    private static List<Coroutine> timeouts = new List<Coroutine>();
    private static List<Coroutine> intervals = new List<Coroutine>();

    void Awake()
    {
        Instance = this;
    }

    //// TIMEOUT ////
    public static Coroutine SetTimout(Action function, float seconds = 1f)
    {
        Coroutine newTimeout = Instance.StartCoroutine(SetTimoutCoroutine(function, seconds));

        timeouts.Add(newTimeout);

        return newTimeout;
    }

    private static IEnumerator SetTimoutCoroutine(Action function, float seconds = 1f)
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
        Coroutine newInterval = Instance.StartCoroutine(SetTimoutCoroutine(function, seconds));

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
}
