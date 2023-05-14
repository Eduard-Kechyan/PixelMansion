using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Locale;

public class Settings : MonoBehaviour
{
    // References
    private SoundManager soundManager;
    private I18n LOCALE;
    private SettingsMenu settingsMenu;
    private ResetHandler resetHandler;

    public bool soundOn = true;
    public bool musicOn = true;
    public bool vibrationOn = true;
    public bool notificationsOn = true;

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
        resetHandler = GameRefs.Instance.menuUI.GetComponent<ResetHandler>();
        settingsMenu = GameRefs.Instance.settingsMenu;
    }

    public void ToggleSound()
    {
        soundOn = !soundOn;

        soundManager.SetVolumeSound(soundOn ? 1 : 0);

        PlayerPrefs.SetInt("sound", soundOn ? 1 : 0);

        settingsMenu.SetUIOptionsButtons();
    }

    public void ToggleMusic()
    {
        musicOn = !musicOn;

        soundManager.SetVolumeMusic(musicOn ? 1 : 0);

        PlayerPrefs.SetInt("music", musicOn ? 1 : 0);

        settingsMenu.SetUIOptionsButtons();
    }

    public void ToggleVibration()
    {
        vibrationOn = !vibrationOn;

        PlayerPrefs.SetInt("vibration", vibrationOn ? 1 : 0);

        settingsMenu.SetUIOptionsButtons();
    }

    public void ToggleNotifications()
    {
        notificationsOn = !notificationsOn;

        PlayerPrefs.SetInt("notifications", notificationsOn ? 1 : 0);

        settingsMenu.SetUIOptionsButtons();
    }

    public void SetLocale(Types.Locale newLocale, bool initial = false, bool restart = false)
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
                    Types.Locale locale = (Types.Locale)
                        System.Enum.Parse(typeof(Types.Locale), Application.systemLanguage.ToString());

                    localeCode = LOCALE.ConvertToCode(locale);
                    I18n.SetLocale(localeCode);
                    PlayerPrefs.SetString("locale", localeCode);
                }
                else
                {
                    I18n.SetLocale(localeCode);
                    PlayerPrefs.SetString("locale", localeCode);
                }
            }
        }
        else
        {
            currentLocale = newLocale;

            localeCode = LOCALE.ConvertToCode(newLocale);

            I18n.SetLocale(localeCode);
            PlayerPrefs.SetString("locale", localeCode);

            if (restart && I18n.GetLocale() == localeCode)
            {
                resetHandler.RestartApp();
            }
        }
    }

    //// GET ////

    public void GetSound()
    {
        if (PlayerPrefs.HasKey("sound"))
        {
            soundOn = PlayerPrefs.GetInt("sound") == 1 ? true : false;

            soundManager.SetVolumeSound(soundOn ? 1 : 0);
        }
    }

    public void GetMusic()
    {
        if (PlayerPrefs.HasKey("music"))
        {
            musicOn = PlayerPrefs.GetInt("music") == 1 ? true : false;

            soundManager.SetVolumeMusic(musicOn ? 1 : 0);
        }
    }

    public void GetVibration()
    {
        if (PlayerPrefs.HasKey("vibration"))
        {
            vibrationOn = PlayerPrefs.GetInt("vibration") == 1 ? true : false;
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
