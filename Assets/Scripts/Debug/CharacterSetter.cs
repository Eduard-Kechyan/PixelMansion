using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Lib.SimpleJSON;

namespace Merge
{
    public class CharacterSetter : MonoBehaviour
    {
        // Variables
        public bool set = false;
        public bool log = false;

        public I18n.Locale[] localesToCreate;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (set)
            {
                set = false;

                SetCharacters();
            }
        }
#endif

        public void SetCharacters()
        {
            for (int i = 0; i < localesToCreate.Length; i++)
            {
                Object localeJsonObject = Resources.Load("Locales/" + localesToCreate[i], typeof(TextAsset));

                if (localeJsonObject != null)
                {
                    string localeJsonString = localeJsonObject.ToString();

                    if (localeJsonString != "" && localeJsonString != "{}")
                    {
                        JSONNode localeJsonNode = JSON.Parse(localeJsonString);

                        string localeCharacters = "";

                        foreach (var childNode in localeJsonNode.Children)
                        {
                            localeCharacters = HandleTextSnippet(localeCharacters, childNode.Value);
                        }

                        if (localeCharacters != "")
                        {
                            if (log)
                            {
                                Debug.Log("//////// " + localesToCreate[i] + " ////////");

                                Debug.Log("Found " + localeCharacters.Length + " characters!");

                                Debug.Log(localeCharacters);
                            }

                            SaveCharacterSetToDisk(localeCharacters, localesToCreate[i]);
                        }
                    }
                }
            }
        }

        string HandleTextSnippet(string foundCharacters, string snippet)
        {
            string newCharacters = "";

            foreach (char c in snippet)
            {
                if (!foundCharacters.Contains(c) && !newCharacters.Contains(c))
                {
                    newCharacters += c;
                }
            }

            return foundCharacters + newCharacters;
        }

        void SaveCharacterSetToDisk(string characters, I18n.Locale locale)
        {
            string prePath = Directory.GetCurrentDirectory() + "/Assets/Fonts/Character Sets/";
            string fullPath = prePath + "Character Set " + locale + ".txt";

            if (Directory.Exists(prePath))
            {
                bool writeFile = true;

                if (Directory.Exists(fullPath))
                {
                    string previousCharacters = File.ReadAllText(fullPath);

                    if (previousCharacters.Equals(characters))
                    {
                        if (log)
                        {
                            Debug.LogWarning("Locale character setting halted because there were changes!");
                        }

                        writeFile = false;
                    }
                    else
                    {
                        Directory.Delete(fullPath, true);
                    }
                }

                if (writeFile)
                {
                    StreamWriter writer = new(fullPath);

                    writer.WriteLine(characters);

                    writer.Close();

                    CreateFontAsset(characters);

                    if (log)
                    {
                        Debug.Log("Locale character setting was successful!");
                    }
                }
            }
        }

        void CreateFontAsset(string characters)
        {

        }
    }
}
