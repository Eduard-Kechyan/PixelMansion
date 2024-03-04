using System;
using System.Collections;
using System.Text.RegularExpressions;
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
        public bool showStack = false;
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
        private const float TOGGLE_THRESHOLD_SECONDS = 2f;
        private const float SHAKE_ACCELERATION = 5f;

        private Coroutine clearTimeout;

        [Serializable]
        public class LogData
        {
            public string message;
            public List<string> stackTrace;
            public Color color;
        }

        private readonly List<LogData> logsData = new();

        // References
        private UIDocument debugUI;

        // UI
        private VisualElement root;
        private VisualElement logsContainer;
        private VisualElement logsTop;
        private Label logsTitleLabel;
        private ScrollView logsScrollView;
        private Button clearButton;
        private Button stackButton;
        private Button closeButton;

        // Instance
        public static Logs Instance;

        void Awake()
        {
            if ((Instance != null && Instance != this) || (!Debug.isDebugBuild && !Application.isEditor))
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        void Start()
        {
            // Cache
            debugUI = GetComponent<UIDocument>();

            // UI
            root = debugUI.rootVisualElement;
            logsContainer = root.Q<VisualElement>("LogsContainer");

            logsTop = logsContainer.Q<VisualElement>("LogsTop");
            logsTitleLabel = logsContainer.Q<Label>("Title");
            logsScrollView = logsContainer.Q<ScrollView>("LogsScrollView");

            clearButton = logsContainer.Q<Button>("ClearButton");
            stackButton = logsContainer.Q<Button>("StackButton");
            closeButton = logsContainer.Q<Button>("CloseButton");

            clearButton.clicked += () => ClearData();
            stackButton.clicked += () => ToggleStack();
            closeButton.clicked += () => Toggle();

            // Init
            Init();
        }

        void OnEnable()
        {
            Application.logMessageReceivedThreaded += HandleNewLog;
        }

        void OnDisable()
        {
            Application.logMessageReceivedThreaded -= HandleNewLog;
        }

        void Init()
        {
            logsContainer.style.display = DisplayStyle.None;
            logsContainer.style.opacity = 0;

            logsTitleLabel.style.height = titleHight;
            logsScrollView.style.top = titleHight;

            logsScrollView.Clear();

            ToggleStack(false);

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
            List<string> stackList = new();

            if (newStackTrace != null && newStackTrace != "")
            {
                Regex regexPattern = new Regex("\\(at(.*Assets/Scripts.*)\\)");

                int count = 0;

                foreach (Match matchItem in regexPattern.Matches(newStackTrace))
                {
                    string countString = count.ToString();

                    if (count < 10)
                    {
                        countString = "0" + count;
                    }

                    stackList.Add(matchItem.ToString().Replace("(at", "[" + countString + "] ").Replace(")", ""));

                    count++;
                }
            }

            LogData newLogData = new()
            {
                message = newMessage,
                stackTrace = stackList,
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
            Label newLogMessageLabel = new() { text = newLogData.message };

            newLogMessageLabel.AddToClassList("log");

            newLogMessageLabel.style.color = newLogData.color;

            logsScrollView.Add(newLogMessageLabel);

            if (newLogData.stackTrace.Count > 0)
            {
                for (int i = 0; i < newLogData.stackTrace.Count; i++)
                {
                    Label newLogStackTraceLabel = new() { text = newLogData.stackTrace[i] };

                    newLogStackTraceLabel.AddToClassList("log");
                    newLogStackTraceLabel.AddToClassList("stack");

                    if (showStack)
                    {
                        newLogStackTraceLabel.AddToClassList("show_stack");
                    }

                    newLogStackTraceLabel.style.color = newLogData.color;

                    logsScrollView.Add(newLogStackTraceLabel);
                }
            }
        }

        void ToggleStack(bool toggle = true)
        {
            if (toggle)
            {
                showStack = !showStack;

                for (int i = 0; i < logsScrollView.childCount; i++)
                {
                    if (showStack)
                    {
                        logsScrollView.ElementAt(i).AddToClassList("show_stack");
                    }
                    else
                    {
                        logsScrollView.ElementAt(i).RemoveFromClassList("show_stack");
                    }
                }
            }

            if (showStack)
            {
                stackButton.text = "Hide Stack";
            }
            else
            {
                stackButton.text = "Show Stack";
            }
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
