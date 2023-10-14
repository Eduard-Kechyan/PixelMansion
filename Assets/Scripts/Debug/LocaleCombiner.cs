using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.IO;

namespace Merge
{
    public class LocaleCombiner : MonoBehaviour
    {
        public bool combine = false;
        public bool log = false;

        private Object[] chunks;

        void Awake()
        {
            Combine();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (combine)
            {
                combine = false;

                Combine();
            }
        }
#endif

        public void Combine(bool isBuilding = false, bool isPlaying = false)
        {
            string[] locales = System.Enum.GetNames(typeof(Types.Locale));

            int count = 0;

            for (int i = 0; i < locales.Length; i++)
            {
                string combinedString = "";

                chunks = Resources.LoadAll("Locales/Chunks/" + locales[i], typeof(TextAsset));

                if (chunks.Length > 0)
                {
                    for (int j = 0; j < chunks.Length; j++)
                    {
                        string textString = chunks[j].ToString();

                        textString = textString.Remove(0, 1);

                        textString = textString.Remove(textString.Length - 1, 1);

                        combinedString += textString + ",";

                        count++;
                    }

                    if (combinedString != "")
                    {
                        combinedString = combinedString.Remove(combinedString.Length - 1, 1);

                        combinedString = "{" + combinedString + "}";

                        SaveCombinedJson(combinedString, locales[i]);
                    }
                }
            }

            if (log)
            {
                if (isBuilding)
                {
                    Debug.Log("Build combining!");
                }
                else if (isPlaying)
                {
                    Debug.Log("Play combining!");
                }
                else
                {
                    Debug.Log("Manual combining!");
                }
            }

            if (count > 0)
            {
                if (log)
                {
                    Debug.Log("Combined successful!");
                }
            }
            else
            {
                Debug.LogWarning("Combined failed!");
            }
        }

        void SaveCombinedJson(string combinedString, string locale)
        {
            string prePath = Directory.GetCurrentDirectory() + "/Assets/Resources/Locales/";
            string fullPath = prePath + locale + ".json";

            if (Directory.Exists(prePath))
            {
                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, true);
                }

                StreamWriter writer = new(fullPath);

                writer.WriteLine(combinedString);

                writer.Close();
            }
        }
    }
}