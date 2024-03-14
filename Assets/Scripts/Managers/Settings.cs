using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class Settings : MonoBehaviour
    {
        // Variables
        public bool soundOn = true;
        public bool musicOn = true;
        public bool vibrationOn = true;
        public bool notificationsOn = true;

        public Types.Locale currentLocale;

        // References
        private SoundManager soundManager;
        private SettingsMenu settingsMenu;
        private Services services;
        private NotificsManager notificsManager;
        private CloudSave cloudSave;

        // Instance
        public static Settings Instance;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void Start()
        {
            // Cache
            soundManager = SoundManager.Instance;
            services = Services.Instance;
            notificsManager = services.GetComponent<NotificsManager>();
            cloudSave = services.GetComponent<CloudSave>();

            GetSound();
            GetMusic();
            GetVibration();
            GetNotifications();

            SetLocalePre();

            StartCoroutine(WaitForCloudSave());
        }

        IEnumerator WaitForCloudSave()
        {
            while (!services.cloudSaveAvailable)
            {
                yield return null;
            }

            SetLocale(Types.Locale.English, true);
        }

        public void Init()
        {
            settingsMenu = GameRefs.Instance.settingsMenu;
        }

        public void ToggleSound()
        {
            soundOn = !soundOn;

            soundManager.sourceSound.enabled = soundOn;

            PlayerPrefs.SetInt("sound", soundOn ? 1 : 0);
            PlayerPrefs.Save();

            cloudSave.SaveDataAsync("sound", soundOn ? 1 : 0);

            settingsMenu.SetUIOptionsButtons();
        }

        public void ToggleMusic()
        {
            musicOn = !musicOn;

            soundManager.sourceMusic.enabled = musicOn;

            if (musicOn)
            {
                soundManager.sourceMusic.Play();
            }

            PlayerPrefs.SetInt("music", musicOn ? 1 : 0);
            PlayerPrefs.Save();

            cloudSave.SaveDataAsync("music", musicOn ? 1 : 0);

            settingsMenu.SetUIOptionsButtons();
        }

        public void ToggleVibration()
        {
            vibrationOn = !vibrationOn;

            PlayerPrefs.SetInt("vibration", vibrationOn ? 1 : 0);
            PlayerPrefs.Save();

            cloudSave.SaveDataAsync("vibration", vibrationOn ? 1 : 0);

            settingsMenu.SetUIOptionsButtons();
        }

        public void ToggleNotifications()
        {
            notificationsOn = !notificationsOn;

            if (notificationsOn)
            {
                notificsManager.CheckPermission(null, (bool granted) =>
                {
                    if (granted)
                    {
                        PlayerPrefs.SetInt("notifications", 1);
                        PlayerPrefs.Save();

                        cloudSave.SaveDataAsync("notifications", 1);

                        settingsMenu.SetUIOptionsButtons();
                        notificsManager.EnableNotifications();
                    }
                    else
                    {
                        // FIX - Notify the play that notifications are disabled in the settings 
                        notificationsOn = false;
                    }
                });
            }
            else
            {
                notificsManager.DisableNotifications();

                PlayerPrefs.SetInt("notifications", 0);
                PlayerPrefs.Save();

                cloudSave.SaveDataAsync("notifications", 0);

                settingsMenu.SetUIOptionsButtons();
            }
        }

        public void SetNotifications(bool enable)
        {
            notificationsOn = enable;

            PlayerPrefs.SetInt("notifications", enable ? 1 : 0);
            PlayerPrefs.Save();

            cloudSave.SaveDataAsync("notifications", enable ? 1 : 0);
        }

        public void SetLocalePre()
        {
            if (PlayerPrefs.HasKey("locale"))
            {
                Types.Locale locale = Glob.ParseEnum<Types.Locale>(PlayerPrefs.GetString("locale"));

                currentLocale = locale;

                I18n.SetLocale(locale);
            }
        }

        public void SetLocale(Types.Locale newLocale, bool initial = false)
        {
            if (initial)
            {
                if (PlayerPrefs.HasKey("locale"))
                {
                    Types.Locale locale = Glob.ParseEnum<Types.Locale>(PlayerPrefs.GetString("locale"));

                    currentLocale = locale;

                    I18n.SetLocale(locale);
                }
                else
                {
                    List<string> locales = new();

                    foreach (Types.Locale locale in System.Enum.GetValues(typeof(Types.Locale)))
                    {
                        locales.Add(locale.ToString());
                    }

                    if (locales.Contains(Application.systemLanguage.ToString()))
                    {
                        Types.Locale locale = Glob.ParseEnum<Types.Locale>(Application.systemLanguage.ToString());

                        currentLocale = locale;

                        I18n.SetLocale(locale);

                        PlayerPrefs.SetString("locale", locale.ToString());

                        cloudSave.SaveDataAsync("locale", locale.ToString());
                    }
                    else
                    {
                        I18n.SetLocale(Types.Locale.English);

                        PlayerPrefs.SetString("locale", "English");

                        cloudSave.SaveDataAsync("locale", "English");
                    }

                    PlayerPrefs.Save();
                }
            }
            else
            {
                PlayerPrefs.SetString("locale", newLocale.ToString());

                PlayerPrefs.Save();

                cloudSave.SaveDataAsync("locale", newLocale.ToString());
            }
        }

        //// GET ////

        public void GetSound()
        {
            if (PlayerPrefs.HasKey("sound"))
            {
                soundOn = PlayerPrefs.GetInt("sound") == 1;

                soundManager.SetVolumeSound(soundOn ? 1 : 0);

                soundManager.sourceSound.enabled = soundOn;
            }
        }

        public void GetMusic()
        {
            if (PlayerPrefs.HasKey("music"))
            {
                musicOn = PlayerPrefs.GetInt("music") == 1;

                soundManager.SetVolumeMusic(musicOn ? 1 : 0);

                soundManager.sourceMusic.enabled = musicOn;
            }
        }

        public void GetVibration()
        {
            if (PlayerPrefs.HasKey("vibration"))
            {
                vibrationOn = PlayerPrefs.GetInt("vibration") == 1;
            }
        }

        public void GetNotifications()
        {
            if (PlayerPrefs.HasKey("notifications"))
            {
                notificationsOn = PlayerPrefs.GetInt("notifications") == 1 ? true : false;
            }
        }
    }
}