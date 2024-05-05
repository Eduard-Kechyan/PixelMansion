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
        public int logsTopHight = 24;
        public int defaultFontSize = 6;
        public Color defaultLogColor;
        [Tooltip("Use the black list to exclude some logs.")]
        public bool useBlackList = true;
        [Condition("useBlackList", true)]
        public string[] blackList;

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

        private int fontSize = 0;

        private Coroutine clearTimeout;

        public List<LogData> tempLogsData = new();

        // Classes
        [Serializable]
        public class LogData
        {
            public string message;
            public List<string> stackTrace;
            public Color color;
        }

        // References
        private UIDocument debugUI;
        private GameData gameData;

        // UI
        private VisualElement root;
        private VisualElement logsContainer;

        private VisualElement logsTopSafeArea;
        private VisualElement logsTop;
        private Button closeButton;

        private ScrollView logsScrollView;

        private Button clearButton;
        private Button smallerButton;
        private Label fontSizeLabel;
        private Button biggerButton;
        private Button stackButton;

        // Instance
        public static Logs Instance;

        void Awake()
        {
            if (Instance != null && Instance != this)
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
            gameData = GameData.Instance;

            // UI
            root = debugUI.rootVisualElement;
            logsContainer = root.Q<VisualElement>("LogsContainer");

            logsTopSafeArea = logsContainer.Q<VisualElement>("LogsTopSafeArea");
            logsTop = logsContainer.Q<VisualElement>("LogsTop");
            closeButton = logsTop.Q<Button>("CloseButton");

            logsScrollView = logsContainer.Q<ScrollView>("LogsScrollView");

            clearButton = logsContainer.Q<Button>("ClearButton");
            smallerButton = logsContainer.Q<Button>("SmallerButton");
            fontSizeLabel = logsContainer.Q<Label>("FontSizeLabel");
            biggerButton = logsContainer.Q<Button>("BiggerButton");
            stackButton = logsContainer.Q<Button>("StackButton");

            // UI taps
            closeButton.clicked += () => Toggle();

            clearButton.clicked += () => ClearData();
            smallerButton.clicked += () => SetFontSize(false);
            biggerButton.clicked += () => SetFontSize();
            stackButton.clicked += () => ToggleStack();

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

            logsTopSafeArea.style.height = 0;
            logsScrollView.style.top = logsTopHight;

            logsScrollView.Clear();

            fontSize = defaultFontSize;

            fontSizeLabel.text = fontSize.ToString();

            if (tempLogsData.Count > 0)
            {
                for (int i = 0; i < tempLogsData.Count; i++)
                {
                    gameData.logsData.Add(tempLogsData[i]);
                }
            }

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

            if (useBlackList && (blackList == null || blackList.Length == 0))
            {
                Debug.LogWarning("The logs black list is enabled, but there are no entries in the list!");
            }
        }
#endif

        void CalcTopOffset(GeometryChangedEvent evt)
        {
            root.UnregisterCallback<GeometryChangedEvent>(CalcTopOffset);

            int topOffset = Mathf.RoundToInt((Screen.height - Screen.safeArea.height) / (Camera.main.pixelWidth / GameData.GAME_PIXEL_WIDTH));

            if (topOffset > 0)
            {
                logsTopSafeArea.style.display = DisplayStyle.Flex;
                logsTopSafeArea.style.height = topOffset;

                logsTop.style.top = topOffset;

                logsScrollView.style.top = logsTopHight + topOffset;
            }
        }

        public void Toggle()
        {
            logsOpen = !logsOpen;

            if (logsOpen)
            {
                Glob.StopTimeout(clearTimeout);

                logsContainer.style.display = DisplayStyle.Flex;
                logsContainer.style.opacity = 1;

                for (int i = 0; i < gameData.logsData.Count; i++)
                {
                    AddNewLogToUI(gameData.logsData[i]);
                }

                ScrollToBottom();

                SetButtons();
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

        void ScrollToBottom()
        {
            logsScrollView.scrollOffset = logsScrollView.contentContainer.layout.max - logsScrollView.contentViewport.layout.size;
        }

        void ClearData()
        {
            logsScrollView.Clear();

            gameData.logsData.Clear();

            SetButtons();
        }

        void HandleNewLog(string newMessage, string newStackTrace, LogType newType)
        {
            bool ignore = false;

            if (useBlackList && blackList.Length > 0)
            {
                for (int i = 0; i < blackList.Length; i++)
                {
                    if (newMessage.Contains(blackList[i]))
                    {
                        ignore = true;

                        break;
                    }
                }
            }

            if (!ignore)
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

                if (gameData == null)
                {
                    gameData = GameData.Instance;

                    tempLogsData.Add(newLogData);
                }
                else
                {
                    gameData.logsData.Add(newLogData);
                }

                if (logsOpen)
                {
                    AddNewLogToUI(newLogData);
                }
            }
        }

        void AddNewLogToUI(LogData newLogData)
        {
            Label newLogMessageLabel = new() { text = "• " + newLogData.message };

            newLogMessageLabel.AddToClassList("log");

            newLogMessageLabel.style.color = newLogData.color;
            newLogMessageLabel.style.fontSize = fontSize;

            logsScrollView.Add(newLogMessageLabel);

            if (newLogData.stackTrace.Count > 0)
            {
                for (int i = 0; i < newLogData.stackTrace.Count; i++)
                {
                    Label newLogStackTraceLabel = new() { text = newLogData.stackTrace[i] };

                    newLogStackTraceLabel.AddToClassList("log");
                    newLogStackTraceLabel.AddToClassList("stack");

                    newLogStackTraceLabel.style.fontSize = fontSize;

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
                    if (logsScrollView.ElementAt(i).ClassListContains("stack"))
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

        void SetFontSize(bool increase = true)
        {
            // Update font size
            if (increase)
            {
                fontSize++;
            }
            else
            {
                fontSize--;
            }

            // Update the font size of the logs
            for (int i = 0; i < logsScrollView.childCount; i++)
            {
                logsScrollView.ElementAt(i).style.fontSize = fontSize;
            }

            // Show the current font size
            fontSizeLabel.text = fontSize.ToString();

            SetButtons();
        }

        void SetButtons()
        {
            // Has logs data
            if (gameData.logsData.Count > 0)
            {
                clearButton.SetEnabled(true);

                // Font size
                smallerButton.SetEnabled(fontSize > 3);
                biggerButton.SetEnabled(fontSize < 12);

                // Has stack
                bool hasStack = false;

                for (int i = 0; i < gameData.logsData.Count; i++)
                {
                    if (gameData.logsData[i].stackTrace.Count > 0)
                    {
                        hasStack = true;

                        break;
                    }
                }

                stackButton.SetEnabled(hasStack);
            }
            else
            {
                clearButton.SetEnabled(false);

                smallerButton.SetEnabled(false);
                biggerButton.SetEnabled(false);

                stackButton.SetEnabled(false);
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
