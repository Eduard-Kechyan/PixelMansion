using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Merge
{
    public class Glob : MonoBehaviour
    {
        // Variables
        public static AnimationCurve defaultChanceCurve;

        // Instance
        public static Glob Instance;

        private static List<Coroutine> timeouts = new List<Coroutine>();
        private static List<Coroutine> intervals = new List<Coroutine>();

        void Awake()
        {
            Instance = this;
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
        public static Coroutine SetInterval(
            Action function,
            float seconds = 1f,
            bool callOnce = true
        )
        {
            Coroutine newInterval = Instance.StartCoroutine(SetTimeoutCoroutine(function, seconds));

            intervals.Add(newInterval);

            return Instance.StartCoroutine(SetIntervalCoroutine(function, seconds, callOnce));
        }

        private static IEnumerator SetIntervalCoroutine(
            Action function,
            float seconds,
            bool callOnce
        )
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

        public static Color FromHEX(string hex)
        {
            Color newColor = Color.white;

            if (hex.Length >= 6)
            {
                var r = hex.Substring(0, 2);
                var g = hex.Substring(2, 4);
                var b = hex.Substring(4, 6);
                var a = "FF";

                if (hex.Length == 8)
                {
                    a = hex.Substring(6, 8);
                }

                newColor = new Color(
                    (int.Parse(r, NumberStyles.HexNumber) / 255f),
                    (int.Parse(g, NumberStyles.HexNumber) / 255f),
                    (int.Parse(b, NumberStyles.HexNumber) / 255f),
                    (int.Parse(a, NumberStyles.HexNumber) / 255f)
                );
            }

            return newColor;
        }

#if UNITY_EDITOR
        public static void Validate(Action callback, params UnityEngine.Object[] newObjects)
        {
            void NextUpdate()
            {
                EditorApplication.update -= NextUpdate;

                if (newObjects.Any(c => !c))
                {
                    return;
                }

                if (newObjects.All(c => !EditorUtility.IsDirty(c)))
                {
                    return;
                }

                callback?.Invoke();

                foreach (UnityEngine.Object component in newObjects)
                {
                    EditorUtility.SetDirty(component);
                }
            }

            EditorApplication.update += NextUpdate;
        }
#endif
    }
}
