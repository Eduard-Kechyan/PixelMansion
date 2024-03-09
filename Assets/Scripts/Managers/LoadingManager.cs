using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class LoadingManager : MonoBehaviour
    {
        // Variables
        public bool stayOnScene = false;
        public bool acceptTermsAuto = false;
        public bool logPhases = false;
        public float fillSpeed = 30f;
        public TutorialData tutorialData;
        public SceneLoader sceneLoader;
        public GameObject uiDocument;
        public LoadingSceneUI loadingSceneUI;

#if DEVELOPER_BUILD || UNITY_EDITOR
        public FeedbackManager feedbackManager;
        public Logs logs;
#endif

        public float maxPhase = 8;
        [ReadOnly]
        public int phase = 1;

        [ReadOnly]
        [SerializeField]
        private float fillCount = 0;
        private Action callback;
        private Action<int> callbackAge;
        private Action<bool> callbackConflict;
        private float singlePhasePercent = 10f;
        private int tempAge = 0;
        private bool initial = true;

        [ReadOnly]
        [SerializeField]
        private bool loading = false;

        // References
        private DataManager dataManager;
        private GameData gameData;
        private NotificsManager notificsManager;
        private SoundManager soundManager;
        private Services services;
        private AuthManager authManager;
        private CloudSave cloudSave;

        void Awake()
        {
            // Toggle debug logs
            if (!Debug.isDebugBuild && !Application.isEditor)
            {
                Debug.unityLogger.logEnabled = false;
            }
            else
            {
                Debug.unityLogger.logEnabled = true;
            }
        }

        void Start()
        {
            // Cache
            dataManager = DataManager.Instance;
            gameData = GameData.Instance;
            notificsManager = Services.Instance.GetComponent<NotificsManager>();
            soundManager = SoundManager.Instance;
            services = Services.Instance;
            authManager = services.GetComponent<AuthManager>();
            cloudSave = services.GetComponent<CloudSave>();

            // Get Ready
            singlePhasePercent = 100f / maxPhase;

            callback += ContinueLoading;
            callbackAge += HandleAge;
            callbackConflict += HandleConflict;

            tempAge = PlayerPrefs.GetInt("tempAge");

            initial = !PlayerPrefs.HasKey("InitialLoaded"); // Note the "!"

            if (gameData.buildData.isBundling)
            {
                acceptTermsAuto = false;
            }

            StartLoading();
        }

        void Update()
        {
            if (loading && fillCount < 100f && CheckForLoading())
            {
                fillCount = Mathf.MoveTowards(fillCount, 100, fillSpeed * Time.deltaTime);

                // Check and get player cloud save data
                // This phase isn't being checked every time
                if (fillCount >= singlePhasePercent * 1 && phase == 1)
                {
                    loading = false;

                    cloudSave.CheckUserData(callback);

                    if (logPhases)
                    {
                        Debug.Log("Phase 1");
                    }
                }

                // Check and get game data
                // This phase is being checked every time
                if (fillCount >= singlePhasePercent * 2 && phase == 2)
                {
                    loading = false;

                    dataManager.CheckForLoadedSprites(callback);

                    if (logPhases)
                    {
                        Debug.Log("Phase 2");
                    }
                }

                // Terms of Service and Privacy Policy notice
                // This phase is being checked once
                if (fillCount >= singlePhasePercent * 3 && phase == 3)
                {
                    loading = false;

                    if (PlayerPrefs.HasKey("termsAccepted"))
                    {
                        ContinueLoading();
                    }
                    else
                    {
                        if (acceptTermsAuto)
                        {
                            PlayerPrefs.SetInt("termsAccepted", 1);
                            PlayerPrefs.Save();

                            services.termsAccepted = true;

                            dataManager.SaveValue("termsAccepted", true, false);

                            ContinueLoading();
                        }
                        else
                        {
                            loadingSceneUI.CheckTerms(callback);
                        }
                    }

                    if (logPhases)
                    {
                        Debug.Log("Phase 3");
                    }
                }

                // Age notice
                // This phase is being checked once
                if (fillCount >= singlePhasePercent * 4 && phase == 4)
                {
                    loading = false;

                    ContinueLoading();
                    /* loading = false;
                     if (PlayerPrefs.HasKey("ageAccepted"))
                     {
                         ContinueLoading();
                     }
                     else
                     {
                         loadingSceneUI.CheckAge(callbackAge);
                     }*/

                    if (logPhases)
                    {
                        Debug.Log("Phase 4");
                    }
                }

                // Check for updates
                // This phase is being checked every time
                if (fillCount >= singlePhasePercent * 5 && phase == 5)
                {
                    loading = false;

                    if (initial)
                    {
                        ContinueLoading();

                        if (logPhases)
                        {
                            Debug.Log("Phase 5");
                        }
                    }
                    else
                    {
                        ContinueLoading();
                        // FIX - Fix this function
                        // loadingSceneUI.CheckForUpdates(callback);

                        if (logPhases)
                        {
                            Debug.Log("Phase 5");
                        }
                    }
                }

                // Get notification permission
                // This phase is being checked once
                if (fillCount >= singlePhasePercent * 6 && phase == 6)
                {
                    loading = false;

                    // Check notifications
                    if (SystemInfo.operatingSystem.Contains("13") && SystemInfo.operatingSystem.Contains("33") || Application.isEditor)
                    {
                        loading = false;
                        notificsManager.CheckPermission(callback);
                    }

                    if (logPhases)
                    {
                        Debug.Log("Phase 6");
                    }
                }

                // Resolve account linking conflict
                if (fillCount >= singlePhasePercent * 7 && phase == 7)
                {
                    loading = false;

                    if (authManager.hasLinkingConflict)
                    {
                        loadingSceneUI.CheckConflict(callbackConflict);
                    }
                    else
                    {
                        ContinueLoading();
                    }

                    if (logPhases)
                    {
                        Debug.Log("Phase 7");
                    }
                }

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
                        LoadNextScene();
                    }
                }
            }
        }

        public void StartLoading()
        {
            // PLay background music
            soundManager.PlayMusic(Types.MusicType.Loading);

            loading = true;
        }

        public void LoadNextScene()
        {
            // Check tutorial scene here
            if (!PlayerPrefs.HasKey("tutorialFinished") && PlayerPrefs.HasKey("tutorialStep"))
            {
                string tutorialStep = PlayerPrefs.GetString("tutorialStep");

                for (int i = 0; i < tutorialData.steps.Length; i++)
                {
                    if (tutorialData.steps[i].id == tutorialStep)
                    {
                        if (tutorialData.steps[i].scene == Types.Scene.GamePlay)
                        {
                            // GamePlay scene
                            sceneLoader.Load(Types.Scene.GamePlay);

                            return;
                        }

                        break;
                    }
                }
            }

            // Hub scene
            sceneLoader.Load(Types.Scene.Hub);
        }

        bool CheckForLoading()
        {
#if DEVELOPER_BUILD || UNITY_EDITOR
            if (
                // Feedback manager
                feedbackManager != null &&
                feedbackManager.feedbackOpen &&
                !feedbackManager.thanksOpen &&
                !feedbackManager.failureOpen &&
                // Logs
                logs != null &&
                !logs.logsOpen
                // Other
                )
            {
                return true;
            }
#endif

            return false;
        }

        void HandleAge(int newValue)
        {
            tempAge = newValue;

            PlayerPrefs.SetInt("tempAge", newValue);

            ContinueLoading();
        }

        void HandleConflict(bool forceLinking)
        {
            authManager.ResolveLinkingConflict(forceLinking);

            ContinueLoading();
        }

        void ContinueLoading()
        {
            phase++;

            loading = true;
        }
    }
}