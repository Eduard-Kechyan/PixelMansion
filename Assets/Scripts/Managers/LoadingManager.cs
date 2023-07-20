using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;

public class LoadingManager : MonoBehaviour
{
    // Variables
    public bool stayOnScene = false;
    public bool logPhases = false;
    public float fillSpeed = 3f;
    public SceneLoader sceneLoader;
    public GameObject uiDocument;
    public LoadingSceneUI loadingSceneUI;
    public bool loading = false;
    public int phase = 1;
    public float maxPhase = 5;

    private VisualElement fill;
    private float fillCount = 0;
    private Action callback;
    private Action<int> callbackAge;
    private UserDataHandler userDataHandler;
    private float singlePhasePercent = 10f;
    private int tempAge = 0;

    // References
    private DataManager dataManager;
    //private Notifics notifics;

    void Start()
    {
        // Cache
        dataManager = DataManager.Instance;
        //notifics = Notifics.Instance;
        userDataHandler = GetComponent<UserDataHandler>();

        // Cache UI
        VisualElement root = uiDocument.GetComponent<UIDocument>().rootVisualElement;

        fill = root.Q<VisualElement>("Fill");

        loading = true;

        singlePhasePercent = 100f / (maxPhase + 1);

        callback += ContinueLoading;
        callbackAge += HandleAge;

        tempAge = PlayerPrefs.GetInt("tempAge");
    }

    void Update()
    {
        if (loading && fillCount < 100f)
        {
            fillCount = Mathf.MoveTowards(fillCount, 100, fillSpeed * Time.deltaTime);

            fill.style.width = new Length(fillCount, LengthUnit.Pixel);

            if (fillCount >= singlePhasePercent * 1 && phase == 1)
            {
                loading = false;
                if (PlayerPrefs.HasKey("termsAccepted"))
                {
                    ContinueLoading();
                }
                else
                {
                    loadingSceneUI.CheckTerms(callback);
                }

                if (logPhases)
                {
                    Debug.Log("Phase 1");
                }
            }

            if (fillCount >= singlePhasePercent * 2 && phase == 2)
            {
                loading = false;
                if (PlayerPrefs.HasKey("ageAccepted"))
                {
                    ContinueLoading();
                }
                else
                {
                    loadingSceneUI.CheckAge(callbackAge);
                }

                if (logPhases)
                {
                    Debug.Log("Phase 2");
                }
            }

            if (fillCount >= singlePhasePercent * 3 && phase == 3)
            {
                loading = false;
                dataManager.CheckInitialData(callback);

                if (logPhases)
                {
                    Debug.Log("Phase 3");
                }
            }

            if (fillCount >= singlePhasePercent * 4 && phase == 4)
            {
                loading = false;
                userDataHandler.CheckUser(callback, tempAge);

                if (logPhases)
                {
                    Debug.Log("Phase 4");
                }
            }

            /*if (fillCount >= singlePhasePercent*5 && phase==5)
            {
                loading = false;

                // Check notifications
                if (SystemInfo.operatingSystem.Contains("13") && SystemInfo.operatingSystem.Contains("33"))
                {
                    loading = false;
                    //StartCoroutine(notifics.RequestPermission()); await Task.Delay(750);
                    notifics.CheckNotifications();
                    loading = true;
                }

                if (logPhases)
                {
                    Debug.Log("Phase 5");
                }
            }*/

            if (fillCount >= 100f)
            {
                if (!stayOnScene)
                {
                    sceneLoader.Load(1);
                }
                else
                {
                    Debug.Log("Skipping Next scene!");
                }
            }
        }
    }

    void HandleAge(int newValue)
    {
        tempAge = newValue;

        PlayerPrefs.SetInt("tempAge", newValue);

        ContinueLoading();
    }

    void ContinueLoading()
    {
        loading = true;
        phase++;
    }
}
