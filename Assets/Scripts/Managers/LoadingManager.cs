using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;

namespace Merge
{
    public class LoadingManager : MonoBehaviour
    {
        // Variables
        public bool stayOnScene = false;
        public bool logPhases = false;
        public float fillSpeed = 30f;
        public SceneLoader sceneLoader;
        public GameObject uiDocument;
        public LoadingSceneUI loadingSceneUI;
        public int phase = 1;
        public float maxPhase = 6;

        [SerializeField]
        private bool loading = false;

        [ReadOnly]
        [SerializeField]
        private float fillCount = 0;
        private VisualElement fill;
        private Action callback;
        private Action<int> callbackAge;
        private float singlePhasePercent = 10f;
        private int tempAge = 0;
        private bool initial = true;

        // References
        private DataManager dataManager;
        private UserDataHandler userDataHandler;
        private SoundManager soundManager;
        //private Notifics notifics;

        void Start()
        {
            // Cache
            dataManager = DataManager.Instance;
            userDataHandler = GetComponent<UserDataHandler>();
            soundManager = SoundManager.Instance;
            //notifics = Services.Instance.GetComponent<Notifics>();

            // UI
            VisualElement root = uiDocument.GetComponent<UIDocument>().rootVisualElement;
            fill = root.Q<VisualElement>("Fill");

            // Get Ready
            singlePhasePercent = 100f / (maxPhase + 1);

            callback += ContinueLoading;
            callbackAge += HandleAge;

            tempAge = PlayerPrefs.GetInt("tempAge");

            initial = !PlayerPrefs.HasKey("InitialLoaded"); // Note the "!"
        }

        void Update()
        {
            if (loading && fillCount < 100f)
            {
                fillCount = Mathf.MoveTowards(fillCount, 100, fillSpeed * Time.deltaTime);

                fill.style.width = new Length(fillCount, LengthUnit.Pixel);

                // Terms of Service and Privacy Policy notice
                // This phase is being checked once
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

                // Age notice
                // This phase is being checked once
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

                // Check and get game data
                // This phase is being checked every time
                if (fillCount >= singlePhasePercent * 3 && phase == 3)
                {
                    loading = false;
                    dataManager.CheckInitialData(callback);

                    if (logPhases)
                    {
                        Debug.Log("Phase 3");
                    }
                }

                /*  // Check and create user
                // This phase is being checked once
                  if (fillCount >= singlePhasePercent * 4 && phase == 4)
                  {
                      loading = false;
                      ContinueLoading();
                      // TODO - Fix this function
                     // userDataHandler.CheckUser(callback, tempAge);

                      if (logPhases)
                      {
                          Debug.Log("Phase 4");
                      }
                  }

                  // Check for updates
                // This phase is being checked every time
                  if (fillCount >= singlePhasePercent * 5 && phase == 5)
                  {
                      if (!initial)
                      {
                          loading = false;
                          ContinueLoading();
                          // TODO - Fix this function
                          // loadingSceneUI.CheckForUpdates(callback);

                          if (logPhases)
                          {
                              Debug.Log("Phase 5");
                          }
                      }
                  }*/

                // Get notification permission
                // This phase is being checked once
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

                // Load next scene
                if (fillCount >= 100f)
                {
                    PlayerPrefs.SetInt("InitialLoaded", 1);
                    PlayerPrefs.Save();

                    if (stayOnScene && Application.isEditor)
                    {
                        Debug.Log("Skipping Next scene!");
                    }
                    else
                    {
                        sceneLoader.Load(1);
                    }
                }
            }
        }

        public void StartLoading()
        {
// PLay background music
            soundManager.PlayMusic("Loading");

            loading = true;
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
}