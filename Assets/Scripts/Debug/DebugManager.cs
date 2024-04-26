using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using GoogleMobileAds.Api;
using UnityEngine.SceneManagement;

namespace Merge
{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    public class DebugManager : MonoBehaviour
    {
        // Variables
        public BoardInteractions boardInteractions;
        public LoadingManager loadingManager;
        public PreMansionHandler preMansionHandler;
        public float transitionDuration = 0.1f;

        private bool valuesShown;

        // References
        private UIDocument debugUI;
        private Logs logs;
        private ValuesUI valuesUI;
        private ErrorManager errorManager;

        // UI
        private VisualElement root;

        private VisualElement debugMenu;
        private VisualElement menuBackground;

        private VisualElement otherContainer;
        private Button adButton;
        private Button diagnosticsButton;
        private Button logsButton;
        private Button logsShakingButton;
        private Button unlockPreMansionButton;

        private VisualElement sceneContainer;
        private Button skipSceneButton;
        private Button worldSceneButton;
        private Button mergeSceneButton;

        // Instance
        public static DebugManager Instance;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            // Cache
            debugUI = GetComponent<UIDocument>();
            logs = Logs.Instance;
            valuesUI = GameRefs.Instance.valuesUI;
            errorManager = ErrorManager.Instance;

            // UI
            root = debugUI.rootVisualElement;

            debugMenu = root.Q<VisualElement>("DebugMenu");
            menuBackground = debugMenu.Q<VisualElement>("Background");

            otherContainer = debugMenu.Q<VisualElement>("OtherContainer");
            adButton = otherContainer.Q<Button>("AdButton");
            diagnosticsButton = otherContainer.Q<Button>("DiagnosticsButton");
            logsButton = otherContainer.Q<Button>("LogsButton");
            logsShakingButton = otherContainer.Q<Button>("LogsShakingButton");
            unlockPreMansionButton = otherContainer.Q<Button>("UnlockPreMansionButton");

            sceneContainer = debugMenu.Q<VisualElement>("SceneContainer");
            skipSceneButton = sceneContainer.Q<Button>("SkipSceneButton");
            worldSceneButton = sceneContainer.Q<Button>("WorldSceneButton");
            mergeSceneButton = sceneContainer.Q<Button>("MergeSceneButton");

            // Button taps
            menuBackground.AddManipulator(new Clickable(evt =>
            {
                CloseMenu();
            }));
            adButton.clicked += () => MobileAds.OpenAdInspector(error =>
            {
                if (error != null)
                {
                    Debug.LogError("Couldn't open ad inspector!");
                    Debug.LogError(error.GetMessage());
                    Debug.LogError(error.GetCause());
                }
            });
            diagnosticsButton.clicked += () => ToggleDiagnostic();
            logsButton.clicked += () => logs.Toggle();
            logsShakingButton.clicked += () => ToggleLogsShaking();
            unlockPreMansionButton.clicked += () => RemovePreMansion();

            skipSceneButton.clicked += () =>
            {
                loadingManager.LoadNextScene();
                CloseMenu();
            };
            worldSceneButton.clicked += () =>
            {
                SceneManager.LoadScene((int)Types.Scene.World);
                CloseMenu();
            };
            mergeSceneButton.clicked += () =>
            {
                SceneManager.LoadScene((int)Types.Scene.Merge);
                CloseMenu();
            };

            // Init
            Init();
        }

        void Init()
        {
            // Hide menu
            debugMenu.style.display = DisplayStyle.None;
            debugMenu.style.opacity = 0;

            // Diagnostics
            CheckDiagnostic();

            // Log shaking
            CheckLogsShaking();

            // Handle scenes
            switch (Glob.ParseEnum<Types.Scene>(SceneManager.GetActiveScene().name))
            {
                case Types.Scene.Loading:
                    skipSceneButton.style.display = DisplayStyle.Flex; //
                    worldSceneButton.style.display = DisplayStyle.None;
                    mergeSceneButton.style.display = DisplayStyle.None;
                    break;
                case Types.Scene.World:
                    skipSceneButton.style.display = DisplayStyle.None;
                    worldSceneButton.style.display = DisplayStyle.None;
                    mergeSceneButton.style.display = DisplayStyle.Flex;//
                    break;
                case Types.Scene.Merge:
                    skipSceneButton.style.display = DisplayStyle.None;
                    worldSceneButton.style.display = DisplayStyle.Flex;//
                    mergeSceneButton.style.display = DisplayStyle.None;
                    break;
            }
        }

        //// DIAGNOSTICS ////

        void ToggleDiagnostic(bool toggle = true)
        {
            if (toggle)
            {
                errorManager.ToggleDiagnostic();
            }

            if (errorManager.diagnosticsEnabled)
            {
                diagnosticsButton.text = "Diagnostics: On";
                diagnosticsButton.RemoveFromClassList("debug_menu_button_red");
            }
            else
            {
                diagnosticsButton.text = "Diagnostics: Off";
                diagnosticsButton.AddToClassList("debug_menu_button_red");
            }

            PlayerPrefs.SetInt("diagnosticsEnabled", errorManager.diagnosticsEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        void CheckDiagnostic()
        {
            if (PlayerPrefs.HasKey("diagnosticsEnabled"))
            {
                errorManager.diagnosticsEnabled = PlayerPrefs.GetInt("diagnosticsEnabled") == 1 ? true : false;
            }
            else
            {
                errorManager.diagnosticsEnabled = false;
            }

            ToggleDiagnostic(false);
        }

        //// SHAKING ////

        void ToggleLogsShaking(bool toggle = true)
        {
            if (toggle)
            {
                logs.shakingEnabled = !logs.shakingEnabled;
            }

            if (logs.shakingEnabled)
            {
                logsShakingButton.text = "Shaking: On";
                logsShakingButton.RemoveFromClassList("debug_menu_button_red");
            }
            else
            {
                logsShakingButton.text = "Shaking: Off";
                logsShakingButton.AddToClassList("debug_menu_button_red");
            }

            PlayerPrefs.SetInt("shakingEnabled", logs.shakingEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        void CheckLogsShaking()
        {
            if (PlayerPrefs.HasKey("shakingEnabled"))
            {
                logs.shakingEnabled = PlayerPrefs.GetInt("shakingEnabled") == 1 ? true : false;
            }
            else
            {
                logs.shakingEnabled = true;
            }

            ToggleLogsShaking(false);
        }

        //// MENU ////

        public void OpenMenu()
        {
            // Show the menu
            debugMenu.style.display = DisplayStyle.Flex;
            debugMenu.style.opacity = 1;

            if (preMansionHandler == null || (!preMansionHandler.dontDestroyAtStart && PlayerPrefs.HasKey("preMansionRemoved")))
            {
                unlockPreMansionButton.style.display = DisplayStyle.None;
            }
            else
            {
                unlockPreMansionButton.style.display = DisplayStyle.Flex;
            }

            ShowValues();

            // Disable the board
            if (boardInteractions != null)
            {
                boardInteractions.DisableInteractions();
            }
        }

        void CloseMenu()
        {
            // Hide the menu
            debugMenu.style.opacity = 0;

            // Enable the board
            if (boardInteractions != null)
            {
                boardInteractions.EnableInteractions();
            }

            if (valuesShown)
            {
                HideValues();
            }

            StartCoroutine(HideMenuAfter());
        }

        IEnumerator HideMenuAfter()
        {
            yield return new WaitForSeconds(transitionDuration);

            debugMenu.style.display = DisplayStyle.None;
        }

        void ShowValues()
        {
            if (valuesUI != null)
            {
                // Show the values over the menu and disable the buttons
                valuesShown = true;

                valuesUI.SetSortingOrder(12);

                valuesUI.DisableButtons();
            }
        }

        void HideValues()
        {
            if (valuesUI != null)
            {
                // Reset values order in hierarchy and enable the buttons
                valuesShown = false;

                valuesUI.SetSortingOrder(10);

                valuesUI.EnableButtons();
            }
        }

        void RemovePreMansion()
        {
            if (preMansionHandler != null)
            {
                preMansionHandler.Remove();

                CloseMenu();
            }
        }
    }
#endif
}
