using Lib.SimpleJSON;
using System;
using UnityEngine;
using System.Linq;

namespace Locale
{
    public sealed class I18n
    {
        private static JSONNode config = null;

        private static readonly I18n instance = new I18n();

        private static readonly string[] locales = new string[]
        {
            "en-US", // English - English
            "fr-FR", // French - Français
            "es-ES", // Spanish - Español
            "de-DE", // German - Deutsch
            "it-IT", // Italian - Utaliano
            "ru-RU", // Russian - Русский
            "ja-JP", // Japanese - 日本語
            "ko-KR", // Korean - 한국어
            "zh-CN" // Chinese - 中文
        };

        private static string currentLocale = "en-US";

        private static string localePath = "Locales/";

        private static bool isLoggingMissing = true;

        static I18n() { }

        public I18n() { }

        public static I18n Instance
        {
            get { return instance; }
        }

        static void InitConfig()
        {
            if (locales.Contains(currentLocale))
            {
                // Read the file as one string.
                TextAsset configText = Resources.Load(localePath + currentLocale) as TextAsset;
                config = JSON.Parse(configText.text);
            }
            else if (isLoggingMissing)
            {
                Debug.Log("Missing: locale [" + currentLocale + "] not found in supported list");
            }
        }

        public static string GetLocale()
        {
            return currentLocale;
        }

        public static void SetLocale(string newLocale = null)
        {
            if (newLocale != null)
            {
                currentLocale = newLocale;
                InitConfig();
            }
        }

        public static void Configure(
            string newLocalePath = null,
            string newLocale = null,
            bool newLogMissing = false
        )
        {
            if (newLocalePath != null)
            {
                localePath = newLocalePath;
            }
            if (newLocale != null)
            {
                currentLocale = newLocale;
            }
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
                Debug.Log("Missing translation for:" + key);
            }
            return translation;
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
                Debug.Log("Missing singPlurKey:" + singPlurKey + " for:" + key);
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
