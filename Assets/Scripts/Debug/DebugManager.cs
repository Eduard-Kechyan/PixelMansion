using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using GoogleMobileAds.Api;

namespace Merge
{
    public class DebugManager : MonoBehaviour
    {
        // Variables
        public BoardInteractions boardInteractions;
        public LoadingManager loadingManager;
        public float transitionDuration = 0.1f;

        // References
        private UIDocument logsUI;
        private Logs logs;
        private ValuesUI valuesUI;

        // UI
        private VisualElement root;

        private VisualElement debugMenu;
        private VisualElement menuBackground;

        private VisualElement otherContainer;
        private Button adButton;
        private Button logsButton;

        private VisualElement loadingContainer;
        private Button skipButton;

        // Instance
        public static DebugManager Instance;

        void Awake()
        {
            if (!Debug.isDebugBuild && !Application.isEditor)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        void Start()
        {
            // Cache
            logsUI = GetComponent<UIDocument>();
            logs = GetComponent<Logs>();
            valuesUI = GameRefs.Instance.valuesUI;

            // UI
            root = logsUI.rootVisualElement;

            debugMenu = root.Q<VisualElement>("DebugMenu");
            menuBackground = debugMenu.Q<VisualElement>("Background");

            otherContainer = debugMenu.Q<VisualElement>("OtherContainer");
            adButton = otherContainer.Q<Button>("AdButton");
            logsButton = otherContainer.Q<Button>("LogsButton");

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
            logsButton.clicked += () => logs.Toggle();

            // Init
            debugMenu.style.display = DisplayStyle.None;
            debugMenu.style.opacity = 0;

            // Loading scene
            if (loadingManager != null)
            {
                // UI
                loadingContainer = debugMenu.Q<VisualElement>("LoadingContainer");

                skipButton = loadingContainer.Q<Button>("SkipButton");

                // Button taps
                skipButton.clicked += () =>
                {
                    loadingManager.LoadNextScene();
                    CloseMenu();
                };

                // Init
                loadingContainer.style.display = DisplayStyle.Flex;
            }
        }

        public void OpenMenu()
        {
            // Show the menu
            debugMenu.style.display = DisplayStyle.Flex;
            debugMenu.style.opacity = 1;

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

            HideValues();

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
                valuesUI.SetSortingOrder(12);

                valuesUI.DisableButtons();
            }
        }

        void HideValues()
        {
            if (valuesUI != null)
            {
                valuesUI.SetSortingOrder(10);

                valuesUI.EnableButtons();
            }
        }
    }
}
