using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Merge
{
    public class Settings : MonoBehaviour
{
    // References
    private SoundManager soundManager;
    private I18n LOCALE;
    private SettingsMenu settingsMenu;

    public bool soundOn = true;
    public bool musicOn = true;
    public bool vibrationOn = true;
    public bool notificationsOn = false;

    public bool googleSignedIn = false;
    public bool facebookSignedIn = false;
    public bool appleSignedIn = false;

    public Types.Locale currentLocale;

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
        soundManager = SoundManager.Instance;
        LOCALE = I18n.Instance;

        GetSound();
        GetMusic();
        GetVibration();
        GetNotifications();
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

        settingsMenu.SetUIOptionsButtons();
    }

    public void ToggleMusic()
    {
        musicOn = !musicOn;

        soundManager.sourceMusic.enabled = musicOn;

        PlayerPrefs.SetInt("music", musicOn ? 1 : 0);
        PlayerPrefs.Save();

        settingsMenu.SetUIOptionsButtons();
    }

    public void ToggleVibration()
    {
        vibrationOn = !vibrationOn;

        PlayerPrefs.SetInt("vibration", vibrationOn ? 1 : 0);
        PlayerPrefs.Save();

        settingsMenu.SetUIOptionsButtons();
    }

    public void ToggleNotifications()
    {
        notificationsOn = !notificationsOn;

        PlayerPrefs.SetInt("notifications", notificationsOn ? 1 : 0);
        PlayerPrefs.Save();

        settingsMenu.SetUIOptionsButtons();
    }

    public void SetLocale(Types.Locale newLocale, bool initial = false)
    {
        string localeCode = "en-US";

        if (initial)
        {
            if (PlayerPrefs.HasKey("locale"))
            {
                localeCode = PlayerPrefs.GetString("locale");

                currentLocale = LOCALE.ConvertToLocale(localeCode);

                I18n.SetLocale(localeCode);
            }
            else
            {
                List<string> locales = new List<string>();

                foreach (Types.Locale locale in System.Enum.GetValues(typeof(Types.Locale)))
                {
                    locales.Add(locale.ToString());
                }

                if (locales.Contains(Application.systemLanguage.ToString()))
                {
                    Types.Locale locale =Glob.ParseEnum<Types.Locale>(Application.systemLanguage.ToString());

                    localeCode = LOCALE.ConvertToCode(locale);
                    I18n.SetLocale(localeCode);
                    PlayerPrefs.SetString("locale", localeCode);
                }
                else
                {
                    I18n.SetLocale(localeCode);
                    PlayerPrefs.SetString("locale", localeCode);
                }

                PlayerPrefs.Save();
            }
        }
        else
        {
            currentLocale = newLocale;

            localeCode = LOCALE.ConvertToCode(newLocale);

            PlayerPrefs.SetString("locale", localeCode);

            PlayerPrefs.Save();
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