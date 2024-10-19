using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using GoogleMobileAds.Api;
using UnityEngine.SceneManagement;

namespace Merge
{
    public class DebugMenu : MonoBehaviour
    {
        // Variables
        public LoadingManager loadingManager;
        public PreMansionHandler preMansionHandler;
        public float transitionDuration = 0.1f;
        public SceneLoader sceneLoader;
        public CameraPan cameraPan;

        private MenuUI.Menu menuType = MenuUI.Menu.Debug;

        private bool controlJuliaChecked = false;

        // References
        private MenuUI menuUI;
        private Logs logs;
        private ValuesUI valuesUI;
        private ErrorManager errorManager;
        private UIData uiData;

        // UI
        private VisualElement content;

        private VisualElement menuBackground;

        private VisualElement sceneContainer;
        private Button skipSceneButton;
        private Button worldSceneButton;
        private Button mergeSceneButton;

        private VisualElement otherContainer;
        private Button adButton;
        private Button diagnosticsButton;
        private Button logsButton;
        private Button logsShakingButton;
        private Button unlockPreMansionButton;
        private Button controlJuliaButton;

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

                sceneContainer = content.Q<VisualElement>("SceneContainer");
                skipSceneButton = sceneContainer.Q<Button>("SkipSceneButton");
                worldSceneButton = sceneContainer.Q<Button>("WorldSceneButton");
                mergeSceneButton = sceneContainer.Q<Button>("MergeSceneButton");

                otherContainer = content.Q<VisualElement>("OtherContainer");
                adButton = otherContainer.Q<Button>("AdButton");
                diagnosticsButton = otherContainer.Q<Button>("DiagnosticsButton");
                logsButton = otherContainer.Q<Button>("LogsButton");
                logsShakingButton = otherContainer.Q<Button>("LogsShakingButton");
                unlockPreMansionButton = otherContainer.Q<Button>("UnlockPreMansionButton");
                controlJuliaButton = otherContainer.Q<Button>("ControlJuliaButton");

                // UI taps
                skipSceneButton.clicked += () => SoundManager.Tap(() =>
                {
                    loadingManager.LoadNextScene();
                    CloseMenu();
                });
                worldSceneButton.clicked += () => SoundManager.Tap(() =>
                {
                    sceneLoader.Load(SceneLoader.SceneType.World);
                    CloseMenu();
                });
                mergeSceneButton.clicked += () => SoundManager.Tap(() =>
                {
                    sceneLoader.Load(SceneLoader.SceneType.Merge);
                    CloseMenu();
                });

                menuBackground.AddManipulator(new Clickable(evt =>
                {
                    SoundManager.Tap(CloseMenu);
                }));

                adButton.clicked += () => SoundManager.Tap(() => MobileAds.OpenAdInspector(error =>
                {
                    if (error != null)
                    {
                        Debug.LogError("Couldn't open ad inspector!");
                        Debug.LogError(error.GetMessage());
                        Debug.LogError(error.GetCause());
                    }
                }));
                diagnosticsButton.clicked += () => SoundManager.Tap(() => ToggleDiagnostic());
                logsButton.clicked += () => SoundManager.Tap(logs.Toggle);
                logsShakingButton.clicked += () => SoundManager.Tap(() => ToggleLogsShaking());
                unlockPreMansionButton.clicked += () => SoundManager.Tap(RemovePreMansion);
                controlJuliaButton.clicked += () => SoundManager.Tap(() => ToggleControlJulia());

                Init();
            });
        }

        void Init()
        {
            // Checks
            CheckDiagnostic();

            CheckLogsShaking();

            // Handle scenes
            switch (Glob.ParseEnum<SceneLoader.SceneType>(SceneManager.GetActiveScene().name))
            {
                case SceneLoader.SceneType.Loading:
                    skipSceneButton.style.display = DisplayStyle.Flex; //
                    worldSceneButton.style.display = DisplayStyle.None;
                    mergeSceneButton.style.display = DisplayStyle.None;
                    unlockPreMansionButton.style.display = DisplayStyle.None;
                    controlJuliaButton.style.display = DisplayStyle.None;
                    break;
                case SceneLoader.SceneType.World:
                    skipSceneButton.style.display = DisplayStyle.None;
                    worldSceneButton.style.display = DisplayStyle.None;
                    mergeSceneButton.style.display = DisplayStyle.Flex; //
                    unlockPreMansionButton.style.display = DisplayStyle.Flex; //
                    controlJuliaButton.style.display = DisplayStyle.Flex; //

                    CheckJuliaControl();
                    break;
                case SceneLoader.SceneType.Merge:
                    skipSceneButton.style.display = DisplayStyle.None;
                    worldSceneButton.style.display = DisplayStyle.Flex; //
                    mergeSceneButton.style.display = DisplayStyle.None;
                    unlockPreMansionButton.style.display = DisplayStyle.None;
                    controlJuliaButton.style.display = DisplayStyle.None;
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

        //// CONTROL JULIA ////

        void ToggleControlJulia(bool toggle = true)
        {
            if (cameraPan != null)
            {
                Debug.Log("cj");

                if (toggle)
                {
                    cameraPan.debugCharacterMovement = !cameraPan.debugCharacterMovement;
                }

                if (cameraPan.debugCharacterMovement)
                {
                    controlJuliaButton.text = "Control Julia: On";
                    controlJuliaButton.RemoveFromClassList("debug_menu_button_red");
                }
                else
                {
                    controlJuliaButton.text = "Control Julia: Off";
                    controlJuliaButton.AddToClassList("debug_menu_button_red");
                }

                PlayerPrefs.SetInt("debugCharacterMovement", cameraPan.debugCharacterMovement ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        void CheckJuliaControl()
        {
            if (cameraPan != null && !controlJuliaChecked)
            {
                if (PlayerPrefs.HasKey("debugCharacterMovement"))
                {
                    cameraPan.debugCharacterMovement = PlayerPrefs.GetInt("debugCharacterMovement") == 1 ? true : false;
                }
                else
                {
                    cameraPan.debugCharacterMovement = false;
                }

                controlJuliaChecked = true;

                ToggleLogsShaking(false);
            }
        }
    }
}
