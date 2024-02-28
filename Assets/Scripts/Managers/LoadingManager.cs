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
        public int phase = 1;
        public float maxPhase = 6;

        [SerializeField]
        private bool loading = false;

        [ReadOnly]
        [SerializeField]
        private float fillCount = 0;
        private Action callback;
        private Action<int> callbackAge;
        private Action<bool> callbackConflict;
        private float singlePhasePercent = 10f;
        private int tempAge = 0;
        private bool initial = true;

        // References
        private DataManager dataManager;
        private GameData gameData;
        private UserDataHandler userDataHandler;
        private NotificsManager notificsManager;
        private SoundManager soundManager;
        private AuthManager authManager;

        void Start()
        {
            // Cache
            dataManager = DataManager.Instance;
            gameData = GameData.Instance;
            userDataHandler = GetComponent<UserDataHandler>();
            notificsManager = Services.Instance.GetComponent<NotificsManager>();
            soundManager = SoundManager.Instance;
            authManager = Services.Instance.GetComponent<AuthManager>();

            // Get Ready
            singlePhasePercent = 100f / (maxPhase + 1);

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
            if (loading && fillCount < 100f)
            {
                fillCount = Mathf.MoveTowards(fillCount, 100, fillSpeed * Time.deltaTime);

                // Check and get game data
                // This phase is being checked every time
                if (fillCount >= singlePhasePercent * 1 && phase == 1)
                {
                    loading = false;
                    dataManager.CheckForLoadedResources(callback);

                    if (logPhases)
                    {
                        Debug.Log("Phase 1");
                    }
                }

                // Terms of Service and Privacy Policy notice
                // This phase is being checked once
                if (fillCount >= singlePhasePercent * 2 && phase == 2)
                {
                    loading = false;
                    if (PlayerPrefs.HasKey("termsAccepted") || acceptTermsAuto)
                    {
                        ContinueLoading();
                    }
                    else
                    {
                        loadingSceneUI.CheckTerms(callback);
                    }

                    if (logPhases)
                    {
                        Debug.Log("Phase 2");
                    }
                }

                // Age notice
                // This phase is being checked once
                if (fillCount >= singlePhasePercent * 3 && phase == 3)
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
                        Debug.Log("Phase 3");
                    }
                }

                // Check and create user
                // This phase is being checked once
                if (fillCount >= singlePhasePercent * 4 && phase == 4)
                {
                    loading = false;
                    userDataHandler.CheckUser(callback, tempAge);

                    if (logPhases)
                    {
                        Debug.Log("Phase 4");
                    }
                }

                // Check for updates
                // This phase is being checked every time
                /*  if (fillCount >= singlePhasePercent * 5 && phase == 5)
                 {
                     if (!initial)
                     {
                         loading = false;
                         ContinueLoading();
                         // FIX - Fix this function
                         // loadingSceneUI.CheckForUpdates(callback);

                         if (logPhases)
                         {
                             Debug.Log("Phase 5");
                         }
                     }
                 }*/

                // Get notification permission
                // This phase is being checked once
                if (fillCount >= singlePhasePercent * 5 && phase == 5)
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
                        Debug.Log("Phase 5");
                    }
                }

                // Resolve account linking conflict
                if (fillCount >= singlePhasePercent * 6 && phase == 6)
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
                        Debug.Log("Phase 6");
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
                        if (tutorialData.steps[i].scene == Types.Scene.Gameplay)
                        {
                            // Gameplay scene
                            sceneLoader.Load(Types.Scene.Gameplay);

                            return;
                        }

                        break;
                    }
                }
            }

            // Hub scene
            sceneLoader.Load(Types.Scene.Hub);
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
            loading = true;
            phase++;
        }
    }
}