using Lib.SimpleJSON;
using System;
using UnityEngine;

namespace Merge
{
    public sealed class I18n
    {
        private static JSONNode config = null;

        private static readonly I18n instance = new();

        private static Locale currentLocale = Locale.English;

        private static string localePath = "Locales/";

        private static bool isLoggingMissing = true;

        // Enums
        public enum Locale
        {
            English, // English (en-US)
            French, // Français (fr-FR)
            Spanish, // Español (es-ES)
            German, // Deutsch (de-DE)
            Italian, // Italiano (it-IT)
            Russian, // Русский (ru-RU)
            Armenian, // Հայերեն (hy-HY)
            Japanese, // 日本語 (ja-JP)
            Korean, // 한국어 (ko-KR)
            Chinese // 中文 (zh-CN)
        };

        public enum LocaleAlt
        {
            English, // English (en-US)
            Français, // French (fr-FR)
            Español, // Spanish (es-ES)
            Deutsch, // German (de-DE)
            Italiano, // Italian (it-IT)
            Русский, // Russian (ru-RU)
            Հայերեն, // Armenian (hy-HY)
            日本語, // Japanese (ja-JP)
            한국어, // Korean (ko-KR)
            中文 // Chinese (zh-CN)
        };

        static I18n() { }

        public I18n() { }

        public static I18n Instance
        {
            get { return instance; }
        }

        static void InitConfig()
        {
            // Read the file as one string.
            TextAsset configText = Resources.Load(localePath + currentLocale.ToString()) as TextAsset;
            config = JSON.Parse(configText.text);
        }

        public Locale GetLocale()
        {
            return currentLocale;
        }

        public static void SetLocale(Locale newLocale)
        {
            currentLocale = newLocale;
            InitConfig();
        }

        public static void Configure(
            string newLocalePath = null,
            Locale newLocale = Locale.English,
            bool newLogMissing = false
        )
        {
            if (newLocalePath != null)
            {
                localePath = newLocalePath;
            }

            currentLocale = newLocale;

            if (newLogMissing)
            {
                isLoggingMissing = newLogMissing;
            }

            InitConfig();
        }

        public string Get(string key, params object[] args)
        {
            if (config == null)
            {
                InitConfig();
            }

            string translation = key;

            if (config[key] != null)
            {
                // if this key is a direct string
                if (config[key].Count == 0)
                {
                    translation = config[key];
                }
                else
                {
                    translation = FindSingularOrPlural(key, args);
                }

                // check if we have embeddable data
                if (args.Length > 0)
                {
                    translation = string.Format(translation, args);
                }
            }
            else if (isLoggingMissing)
            {
                // ERROR
                ErrorManager.Instance.ThrowWarning(ErrorManager.ErrorType.Locale, GetType().Name, "Missing translation for: " + key);
            }

            return translation;
        }

        public bool CheckIfExists(string key)
        {
            return config[key] != null;
        }

        public bool TryCheckIfExists(string key, out string data)
        {
            data = "";

            if (config[key] != null)
            {
                data = Get(key);

                return data != "";
            }

            return false;
        }

        public int GetNestedLength(string key, bool zeroExpected = false)
        {
            int count = -1;

            if (config == null)
            {
                InitConfig();
            }

            if (config[key] != null)
            {
                count = config[key].Count;
            }
            else if (isLoggingMissing && !zeroExpected)
            {
                // ERROR
                ErrorManager.Instance.ThrowWarning(ErrorManager.ErrorType.Locale, GetType().Name, "Missing translation for: " + key);
            }

            return count;
        }

        public int GetLength(string key)
        {
            int count = 0;

            if (config == null)
            {
                InitConfig();
            }

            bool finding = true;

            while (finding)
            {
                string foundText = config[key + count];

                if (foundText == null || foundText == "")
                {
                    finding = false;
                }
                else
                {
                    count++;
                }
            }

            return count;
        }

        string FindSingularOrPlural(string key, object[] args)
        {
            JSONClass translationOptions = config[key].AsObject;

            string translation = key;

            string singPlurKey = GetCountAmount(args) switch
            {
                0 => "zero",
                1 => "one",
                _ => "other",
            };

            // try to use this plural/singular key
            if (translationOptions[singPlurKey] != null)
            {
                translation = translationOptions[singPlurKey];
            }
            else if (isLoggingMissing)
            {
                // ERROR
                ErrorManager.Instance.ThrowWarning(ErrorManager.ErrorType.Locale, GetType().Name, "Missing singPlurKey:" + singPlurKey + " for:" + key);
            }
            return translation;
        }

        int GetCountAmount(object[] args)
        {
            int argOne = 0;
            // if arguments passed, try to parse first one to use as count
            if (args.Length > 0 && IsNumeric(args[0]))
            {
                argOne = Math.Abs(Convert.ToInt32(args[0]));
                if (argOne == 1 && Math.Abs(Convert.ToDouble(args[0])) != 1)
                {
                    // check if arg actually equals one
                    argOne = 2;
                }
                else if (argOne == 0 && Math.Abs(Convert.ToDouble(args[0])) != 0)
                {
                    // check if arg actually equals one
                    argOne = 2;
                }
            }
            return argOne;
        }

        bool IsNumeric(System.Object Expression)
        {
            if (Expression == null || Expression is DateTime)
                return false;

            if (
                Expression is Int16
                || Expression is Int32
                || Expression is Int64
                || Expression is Decimal
                || Expression is Single
                || Expression is Double
                || Expression is Boolean
            )
                return true;

            return false;
        }
    }
}
