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

    public bool soundOn = true;
    public bool musicOn = true;
    public bool vibrationOn = true;
    public bool notificationsOn = true;

    public bool googleSignedIn = false;
    public bool facebookSignedIn = false;
    public bool appleSignedIn = false;

    private bool initialized = false;

    public void InitPre()
    {
        soundManager = SoundManager.Instance;
        LOCALE = I18n.Instance;

        GetSound();
        GetMusic();

        initialized = true;
    }

    public void Init()
    {
        if (!initialized)
        {
            soundManager = SoundManager.Instance;
            LOCALE = I18n.Instance;

            GetSound();
            GetMusic();
        }

        settingsMenu = GameRefs.Instance.settingsMenu;
        GetVibration();
        GetNotifications();
        SetLocale(Types.Locale.English, true);
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

    public void SetLocale(Types.Locale newLocale, bool initial = false)
    {
        string locale = "en-US";

        if (initial)
        {
            if (PlayerPrefs.HasKey("locale"))
            {
                I18n.SetLocale(PlayerPrefs.GetString("locale"));
            }
            else
            {
                switch (Application.systemLanguage)
                {
                    case SystemLanguage.English:
                        locale = "en-US";
                        I18n.SetLocale(locale);
                        PlayerPrefs.SetString("locale", locale);
                        break;
                    default:
                        I18n.SetLocale(locale);
                        PlayerPrefs.SetString("locale", locale);
                        break;
                }
            }
        }
        else
        {
            locale = LOCALE.ConvertToCode(newLocale);

            I18n.SetLocale(locale);
            PlayerPrefs.SetString("locale", locale);
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

            soundManager.SetVolumeSound(musicOn ? 1 : 0);
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
