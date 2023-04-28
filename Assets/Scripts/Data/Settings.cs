using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Locale;

public class Settings : MonoBehaviour
{
    // References
    private SoundManager soundManager;
    private DataManager dataManager;
    private I18n LOCALE;

    private void Start()
    {
        soundManager = SoundManager.Instance;
        dataManager = DataManager.Instance;
        LOCALE = I18n.Instance;
    }

    //// SET ////
    public void SetSound(float volume)
    {
        soundManager.SetVolumeSFX(volume);

        PlayerPrefs.SetFloat("soundVolume", volume);
    }

    public void SetMusic(float volume)
    {
        soundManager.SetVolumeBG(volume);

        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSave(bool save)
    {
        PlayerPrefs.SetInt("saveData", save ? 1 : 0);

        dataManager.ignoreInitialCheck = !save;
    }

    public void SetLocale(Types.Locale newLocale, bool fromDataManager = false)
    {
        string locale = "en-US";

        if (fromDataManager)
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

    public float GetSound()
    {
        if (PlayerPrefs.HasKey("soundVolume"))
        {
            float volume = PlayerPrefs.GetFloat("soundVolume");

            soundManager.SetVolumeSFX(volume);

            return volume;
        }
        else
        {
            soundManager.SetVolumeSFX(1f);

            return 1f;
        }
    }

    public float GetMusic()
    {
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            float volume = PlayerPrefs.GetFloat("musicVolume");

            soundManager.SetVolumeBG(volume);

            return volume;
        }
        else
        {
            soundManager.SetVolumeBG(1f);

            return 1f;
        }
    }

    public bool GetSave()
    {
        if (PlayerPrefs.HasKey("saveData"))
        {
            bool save = PlayerPrefs.GetInt("saveData") == 1 ? true : false;

            dataManager.ignoreInitialCheck = !save;

            return save;
        }
        else
        {
            dataManager.ignoreInitialCheck = false;

            return true;
        }
    }

    public Types.Locale GetLocale()
    {
        if (PlayerPrefs.HasKey("locale"))
        {
            return LOCALE.ConvertToLocale(PlayerPrefs.GetString("locale"));
        }
        else
        {
            return Types.Locale.English;
        }
    }
}
