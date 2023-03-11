using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class LoadingManager : MonoBehaviour
{
    public bool stayOnScene = false;
    public float fillSpeed = 3f;
    public SceneLoader sceneLoader;
    public GameObject uiDocument;

    private DataManager dataManager;

    private Camera cam;
    private VisualElement fill;
    private float fillCount = 0;
    private Action callback;
    private bool loaded = false;

    private I18n LOCALE = I18n.Instance;

    // Data
    public float gamePixelWidth = 180f;

    void Start()
    {
        // Cache the camera
        cam = Camera.main;

        dataManager = DataManager.Instance;

        // Cache UI elements
        VisualElement root = uiDocument.GetComponent<UIDocument>().rootVisualElement;

        fill = root.Q<VisualElement>("Fill");

        // Check if data has already been loaded
        if (PlayerPrefs.HasKey("Loaded") && PlayerPrefs.GetInt("Loaded") == 1)
        {
            loaded = true;
            callback = LoadContentPre;
            StartCoroutine(SetFill(50f, callback));
        }
        else
        {
            FirstLoading();
        }
    }

    //////// LOADING ////////

    void FirstLoading()
    {
        SetLanguage(true);

        SetPrefs(); // This goes last

        // Next step
        callback = LoadContentPre;
        StartCoroutine(SetFill(50f, callback));
    }

    async void LoadContentPre()
    {
        await dataManager.CheckInitialData();

        // Next step
        HalfFinishedLoading();
    }

    void HalfFinishedLoading()
    {
        if (!loaded)
        {
            SetLanguage();

            // Save the prefs to disk
            PlayerPrefs.Save();
        }

        // Next step
        FinishedLoading();
    }

    void FinishedLoading()
    {
        if (fill.resolvedStyle.width >= 99f)
        {
            if (!stayOnScene)
            {
                sceneLoader.Load(1);
            }
        }
        else
        {
            // Fill the loader to 100%
            callback = FinishedLoading;
            StartCoroutine(SetFill(100f, callback));
        }
    }

    IEnumerator SetFill(float amount, Action callback)
    {
        while (fillCount < amount)
        {
            fillCount++;

            fill.style.width = new Length(fillCount, LengthUnit.Pixel);

            yield return new WaitForSeconds(fillSpeed * Time.fixedDeltaTime);

            if (fillCount == amount)
            {
                callback();
                yield break;
            }
        }
    }

    //////// STEPS ////////

    void SetLanguage(bool first = false)
    {
        string locale = "en-US";

        if (first)
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
        else
        {
            I18n.SetLocale(PlayerPrefs.GetString("locale"));
        }
    }

    void SetPrefs()
    {
        // Set the prefs
        PlayerPrefs.SetFloat("gamePixelWidth", gamePixelWidth);

        // Set Loaded to 1 to make sure this callculations aren't run again
        PlayerPrefs.SetInt("Loaded", 1);

        // Save the prefs to disk
        PlayerPrefs.Save();
    }
}
