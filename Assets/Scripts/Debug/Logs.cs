using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Merge
{
    public class Logs : MonoBehaviour
    {
        // Variables
        public bool logsEnabled = true;
        public bool shakingEnabled = true;
        public int titleHight = 20;
        public Color defaultLogColor;

        // Debug
        [Header("Debug")]
        public bool debug = false;
        [Condition("debug", true)]
        public bool toggle = false;

        [Header("Stats")]
        [ReadOnly]
        public bool logsOpen = false;

        private float lastToggleTime;
        private const float TOGGLE_THRESHOLD_SECONDS = 1f;
        private const float SHAKE_ACCELERATION = 4f;

        private Coroutine clearTimeout;

        [Serializable]
        public class LogData
        {
            public string message;
            public Color color;
        }

        private List<LogData> logsData = new();

        // References
        private UIDocument logsUI;

        // UI
        private VisualElement root;
        private VisualElement logsContainer;
        private VisualElement logsTop;
        private Label logsTitleLabel;
        private ScrollView logsScrollView;
        private Button clearButton;
        private Button closeButton;

        void OnDisable()
        {
            Application.logMessageReceivedThreaded -= HandleNewLog;
        }

        void OnEnable()
        {
            Application.logMessageReceivedThreaded += HandleNewLog;
        }

        void Start()
        {
            // Cache
            logsUI = GetComponent<UIDocument>();

            // UI
            root = logsUI.rootVisualElement;
            logsContainer = root.Q<VisualElement>("LogsContainer");

            logsTop = logsContainer.Q<VisualElement>("LogsTop");
            logsTitleLabel = logsContainer.Q<Label>("Title");
            logsScrollView = logsContainer.Q<ScrollView>("LogsScrollView");

            clearButton = logsContainer.Q<Button>("ClearButton");
            closeButton = logsContainer.Q<Button>("CloseButton");

            clearButton.clicked += () => ClearData();
            closeButton.clicked += () => Toggle();

            // Initialize
            logsContainer.style.display = DisplayStyle.None;
            logsContainer.style.opacity = 0;

            logsTitleLabel.style.height = titleHight;
            logsScrollView.style.top = titleHight;

            logsScrollView.Clear();

            root.RegisterCallback<GeometryChangedEvent>(CalcTopOffset);
        }

        void Update()
        {
            if (shakingEnabled && Time.realtimeSinceStartup - lastToggleTime >= TOGGLE_THRESHOLD_SECONDS && CheckShaking())
            {
                lastToggleTime = Time.realtimeSinceStartup;

                Toggle();
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (toggle)
            {
                toggle = false;

                Toggle();
            }
        }
#endif

        public void Toggle()
        {
            logsOpen = !logsOpen;

            if (logsOpen)
            {
                Glob.StopTimeout(clearTimeout);

                logsContainer.style.display = DisplayStyle.Flex;
                logsContainer.style.opacity = 1;

                for (int i = 0; i < logsData.Count; i++)
                {
                    AddNewLogToUI(logsData[i]);
                }

                ScrollToBottom();
            }
            else
            {
                logsContainer.style.display = DisplayStyle.None;
                logsContainer.style.opacity = 0;

                clearTimeout = Glob.SetTimeout(() =>
                {
                    logsScrollView.Clear();
                }, 0.3f);
            }
        }

        void CalcTopOffset(GeometryChangedEvent evt)
        {
            root.UnregisterCallback<GeometryChangedEvent>(CalcTopOffset);

            int topOffset = Mathf.RoundToInt((Screen.height - Screen.safeArea.height) / (Camera.main.pixelWidth / GameData.GAME_PIXEL_WIDTH));

            if (topOffset > 0)
            {
                logsTop.style.display = DisplayStyle.Flex;
                logsTop.style.height = topOffset;

                logsTitleLabel.style.top = topOffset;

                logsScrollView.style.top = titleHight + topOffset;
            }
        }

        void ScrollToBottom()
        {
            logsScrollView.scrollOffset = logsScrollView.contentContainer.layout.max - logsScrollView.contentViewport.layout.size;
        }

        void ClearData()
        {
            logsScrollView.Clear();

            logsData.Clear();
        }

        void HandleNewLog(string newMessage, string newStackTrace, LogType newType)
        {   
            LogData newLogData = new()
            {
                message = newMessage,
                color = GetColorFromType(newType)
            };

            logsData.Add(newLogData);

            if (logsOpen)
            {
                AddNewLogToUI(newLogData);
            }
        }

        void AddNewLogToUI(LogData newLogData)
        {
            Label newLogLabel = new() { text = newLogData.message };

            newLogLabel.AddToClassList("log");

            newLogLabel.style.color = newLogData.color;

            logsScrollView.Add(newLogLabel);
        }

        Color GetColorFromType(LogType newType)
        {
            switch (newType)
            {
                case LogType.Error:
                    return Glob.colorRed;
                case LogType.Warning:
                    return Glob.colorYellow;
                case LogType.Assert:
                    return Glob.colorRed;
                case LogType.Exception:
                    return Glob.colorRed;
                default: // LogType.Log
                    return defaultLogColor;
            }
        }

        bool CheckShaking()
        {
#if ENABLE_INPUT_SYSTEM
            Vector3 acceleration = Accelerometer.current?.acceleration.ReadValue() ?? Vector3.zero;
#else
            Vector3 acceleration = Input.acceleration;
#endif

            return acceleration.sqrMagnitude > SHAKE_ACCELERATION;
        }
    }
}
