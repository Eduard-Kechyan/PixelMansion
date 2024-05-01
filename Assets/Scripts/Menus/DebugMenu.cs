using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using GoogleMobileAds.Api;
using UnityEngine.SceneManagement;

namespace Merge
{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    public class DebugMenu : MonoBehaviour
    {
        // Variables
        public LoadingManager loadingManager;
        public PreMansionHandler preMansionHandler;
        public float transitionDuration = 0.1f;
        public SceneLoader sceneLoader;

        private bool valuesShown;

        private Types.Menu menuType = Types.Menu.Debug;

        // References
        private MenuUI menuUI;
        private Logs logs;
        private ValuesUI valuesUI;
        private ErrorManager errorManager;
        private UIData uiData;

        // UI
        private VisualElement content;

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
        public static DebugMenu Instance;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            // Cache
            menuUI = GetComponent<MenuUI>();
            logs = Logs.Instance;
            valuesUI = GameRefs.Instance.valuesUI;
            errorManager = ErrorManager.Instance;
            uiData = GameData.Instance.GetComponent<UIData>();

            DataManager.Instance.CheckLoaded(() =>
            {
                // UI
                content = uiData.GetMenuAsset(menuType);

                menuBackground = content.Q<VisualElement>("Background");

                otherContainer = content.Q<VisualElement>("OtherContainer");
                adButton = otherContainer.Q<Button>("AdButton");
                diagnosticsButton = otherContainer.Q<Button>("DiagnosticsButton");
                logsButton = otherContainer.Q<Button>("LogsButton");
                logsShakingButton = otherContainer.Q<Button>("LogsShakingButton");
                unlockPreMansionButton = otherContainer.Q<Button>("UnlockPreMansionButton");

                sceneContainer = content.Q<VisualElement>("SceneContainer");
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
                    sceneLoader.Load(Types.Scene.World);
                    CloseMenu();
                };
                mergeSceneButton.clicked += () =>
                {
                    sceneLoader.Load(Types.Scene.Merge);
                    CloseMenu();
                };

                Init();
            });
        }

        void Init()
        {
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

        public void OpenMenu()
        {
            // Check menu
            if (menuUI.IsMenuOpen(menuType))
            {
                return;
            }

            // Set menu content
            if (preMansionHandler == null || (!preMansionHandler.dontDestroyAtStart && PlayerPrefs.HasKey("preMansionRemoved")))
            {
                unlockPreMansionButton.style.display = DisplayStyle.None;
            }
            else
            {
                unlockPreMansionButton.style.display = DisplayStyle.Flex;
            }

            // Open menu
            menuUI.OpenMenu(content, menuType, "Debug");
        }

        void CloseMenu()
        {
            menuUI.CloseMenu(menuType);
        }

        void RemovePreMansion()
        {
            if (preMansionHandler != null)
            {
                preMansionHandler.Remove();

                CloseMenu();
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
    }
#endif
}
