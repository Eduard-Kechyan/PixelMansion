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

        public static Color colorBlue = Color.blue;
        public static Color colorCyan = Color.cyan;
        public static Color colorGreen = Color.green;
        public static Color colorYellow = Color.yellow;
        public static Color colorRed = Color.red;

        public static bool selectableIsChanging = false;
        public static bool convoUILoading = true;

        public static Types.Scene lastScene = Types.Scene.None;

        // Temp data
        public static string taskToComplete = "";

        // Instance
        public static Glob Instance;

        private readonly static List<Coroutine> timeouts = new();
        private readonly static List<Coroutine> intervals = new();

        void Awake()
        {
            Instance = this;

            lastScene = Types.Scene.None;
            taskToComplete = "";

            colorBlue = FromHEX("71A0F6");
            colorCyan = FromHEX("55CBB3");
            colorGreen = FromHEX("3EC37B");
            colorYellow = FromHEX("FEDA75");
            colorRed = FromHEX("EC737F");
        }

        //// TIMEOUT ////
        public static Coroutine SetTimeout(Action function, float seconds = 1f)
        {
            if (seconds > 10f)
            {
                Debug.LogWarning("Set timeout function delay is too big!");
            }

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
            if (timeout != null)
            {
                timeouts.Remove(timeout);
                Instance.StopCoroutine(timeout);
            }
        }

        //// INTERVALS ////
        public static Coroutine SetInterval(Action function, float seconds = 1f, bool callOnce = true)
        {
            if (seconds > 10f)
            {
                Debug.LogWarning("Set interval function delay is too big!");
            }

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
            if (interval != null)
            {
                intervals.Remove(interval);
                Instance.StopCoroutine(interval);
            }
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

        //// WAITERS ////
        public static void WaitForSelectable(Action function)
        {
            Instance.StartCoroutine(WaitForSelectableTimeout(function));
        }

        private static IEnumerator WaitForSelectableTimeout(Action function)
        {
            while (selectableIsChanging)
            {
                yield return null;
            }

            function();
        }

        //// OTHER ////
        public static T ParseEnum<T>(string value)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch
            {
                // ERROR
                ErrorManager.Instance.Throw(
                    Types.ErrorType.Code,
                    "Glob",
                    "Couldn't parse enum: " + typeof(T) + ". Value: " + value
                );

                return default;
            }
        }

        public static Color FromHEX(string hex)
        {
            Color newColor = Color.white;

            if (hex.Length >= 6)
            {
                var r = hex.Substring(0, 2);
                var g = hex.Substring(2, 2);
                var b = hex.Substring(4, 2);
                var a = "FF";

                if (hex.Length == 8)
                {
                    a = hex.Substring(6, 2);
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

        public static string GetRandomWord(int min = 0, int max = 0, bool firstIsUppercase = false)
        {
            string randomWord = "";
            int wordLength = UnityEngine.Random.Range(min, max + 1);
            int startIndex = 0;

            if (firstIsUppercase)
            {
                startIndex = 1;

                randomWord += (char)UnityEngine.Random.Range('A', 'Z' + 1);
            }

            for (int i = startIndex; i < wordLength; i++)
            {
                randomWord += (char)UnityEngine.Random.Range('a', 'z' + 1);
            }

            return randomWord;
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
