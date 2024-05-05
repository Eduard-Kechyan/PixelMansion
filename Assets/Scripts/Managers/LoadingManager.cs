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
        public bool logSystemAndDeviceInfo = false;
        public float fillSpeed = 30f;
        public TutorialData tutorialData;
        public SceneLoader sceneLoader;
        public GameObject uiDocument;
        public MenuUI menuUI;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
        private AnalyticsManager analyticsManager;
        private TermsMenu termsMenu;
        private ConflictMenu conflictMenu;
        private UpdateMenu updateMenu;

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

        // TODO   Make this a coroutine! 

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
            // analyticsManager = services.GetComponent<AnalyticsManager>();
            analyticsManager = AnalyticsManager.Instance;
            termsMenu = menuUI.GetComponent<TermsMenu>();
            conflictMenu = menuUI.GetComponent<ConflictMenu>();
            updateMenu = menuUI.GetComponent<UpdateMenu>();

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

            if (logSystemAndDeviceInfo)
            {
                Testing();
            }

            StartLoading();
        }

        async void Update()
        {
            if (loading && fillCount < 100f && CheckForLoading())
            {
                fillCount = Mathf.MoveTowards(fillCount, 100, fillSpeed * Time.deltaTime);

                // Check and get player cloud save data
                // This phase isn't being checked every time
                if (fillCount >= singlePhasePercent * 1 && phase == 1)
                {
                    loading = false;

                    cloudSave.CheckUserData(() =>
                    {
                        cloudSave.checkedForUserData = true;
                        callback();
                    });

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

                    dataManager.CheckForLoadedData(() =>
                    {
                        menuUI.CheckLoadingSceneMenus(callback);
                    });

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
                        analyticsManager.StartDataCollection();

                        ContinueLoading();
                    }
                    else
                    {
                        if (acceptTermsAuto)
                        {
                            termsMenu.AcceptTerms(() =>
                            {
                                analyticsManager.StartDataCollection();

                                ContinueLoading();
                            });
                        }
                        else
                        {
                            termsMenu.Open(() =>
                            {
                                analyticsManager.StartDataCollection();

                                ContinueLoading();
                            });
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
                        //updateMenu.Open(callback);

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
                        conflictMenu.Open(callbackConflict);
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

                // Resolve account linking conflict
                if (fillCount >= singlePhasePercent * 8 && phase == 8)
                {
                    loading = false;

                    await authManager.HandleDSANotifications(callback);

                    if (logPhases)
                    {
                        Debug.Log("Phase 8");
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
            soundManager.PlayMusic(SoundManager.MusicType.Loading);

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
                        if (tutorialData.steps[i].scene == SceneLoader.SceneType.Merge)
                        {
                            // Merge scene
                            sceneLoader.Load(SceneLoader.SceneType.Merge);

                            return;
                        }

                        break;
                    }
                }
            }

            // World scene
            sceneLoader.Load(SceneLoader.SceneType.World);
        }

        bool CheckForLoading()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (
                // Feedback manager
                !feedbackManager.feedbackOpen &&
                !feedbackManager.thanksOpen &&
                !feedbackManager.failureOpen &&
                // Debug Menu
                !menuUI.menuOpen &&
                // Logs
                !logs.logsOpen
                // Other
                )
            {
                return true;
            }
#endif

            return false;
        }

        void Testing()
        {
            Debug.Log("//// APPLICATION ////");
            Debug.Log("Installer Name: " + Application.installerName);
            Debug.Log("Install Mode: " + Application.installMode);
            Debug.Log("System Language: " + Application.systemLanguage);
            Debug.Log("Platform: " + Application.platform);
            Debug.Log("Genuine Check Available: " + Application.genuineCheckAvailable);
            if (Application.genuineCheckAvailable)
            {
                Debug.Log("Genuine: " + Application.genuine);
            }
            Debug.Log("Build GUID: " + Application.buildGUID);
            Debug.Log("Cloud Project Id: " + Application.cloudProjectId);
            Debug.Log("Product Name: " + Application.productName);
            Debug.Log("Identifier: " + Application.identifier);
            Debug.Log("Version: " + Application.version);
            Debug.Log("Unity Version: " + Application.unityVersion);

            Debug.Log("//// SYSTEM INFO ////");
            Debug.Log("Unique Identifier: " + SystemInfo.deviceUniqueIdentifier);
            Debug.Log("Model: " + SystemInfo.deviceModel);
            Debug.Log("Name: " + SystemInfo.deviceName);
            Debug.Log("Type: " + SystemInfo.deviceType);
            Debug.Log("OS: " + SystemInfo.operatingSystem);
            Debug.Log("OS Family: " + SystemInfo.operatingSystemFamily);
            Debug.Log("Memory Size: " + SystemInfo.systemMemorySize);

            Debug.Log("//// GRAPHICS DEVICE ////");
            Debug.Log("ID: " + SystemInfo.graphicsDeviceID);
            Debug.Log("Name: " + SystemInfo.graphicsDeviceName);
            Debug.Log("Vendor: " + SystemInfo.graphicsDeviceVendor);
            Debug.Log("Vendor ID: " + SystemInfo.graphicsDeviceVendorID);
            Debug.Log("Version: " + SystemInfo.graphicsDeviceVersion);
            Debug.Log("Memory Size: " + SystemInfo.graphicsMemorySize);

            Debug.Log("//// PROCESSOR ////");
            Debug.Log("Core Count: " + SystemInfo.processorCount);
            Debug.Log("Frequency: " + SystemInfo.processorFrequency);
            Debug.Log("Type: " + SystemInfo.processorType);
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