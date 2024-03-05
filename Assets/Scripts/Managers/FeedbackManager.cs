using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Services.UserReporting;
using Unity.Services.UserReporting.Client;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class FeedbackManager : MonoBehaviour
    {
        // Variables
        public float postCloseDelay = 2f;
        public float loadingOverlayCloseDelay = 5f;
        public int titleHeight = 30;

        [Header("Spinner")]
        public float spinnerSpeed = 0.07f;
        public Sprite[] spinnerSprites;

        [Header("Stats")]
        [ReadOnly]
        public bool feedbackOpen = false;
        [ReadOnly]
        public bool thanksOpen = false;
        [ReadOnly]
        public bool failureOpen = false;

        private bool loadingOverlayOpen = false;

        // Enums
        public enum ReportCategory
        {
            Suggestion,
            Bug,
            Performance
        }

        public enum ReportBugType
        {
            General,
            UI,
            Bug,
            Other
        }

        public enum ReportBugFrequency
        {
            Rarely,
            Sometimes,
            Often,
            Always
        }

        // References
        private UIDocument feedbackUI;
        private I18n LOCALE;

        // UI
        private VisualElement root;

        private VisualElement feedbackContainer;
        private VisualElement feedbackScrollView;
        private Label titleLabel;
        private Label feedbackLabelA;
        private Label feedbackLabelB;
        private Label feedbackLabelC;

        private Label reportTitleLabel;
        private TextField reportTitleTextField;
        private Label reportCategoryLabel;
        private EnumField reportCategoryEnumField;
        private Label reportBugTypeLabel;
        private EnumField reportBugTypeEnumField;
        private Label reportBugFrequencyLabel;
        private EnumField reportBugFrequencyEnumField;
        private Label reportDescriptionLabel;
        private TextField reportDescriptionTextField;

        private Button cancelButton;
        private Button sendButton;

        private VisualElement loadingOverlay;
        private VisualElement loadingOverlaySpinner;

        private VisualElement thanksContainer;
        private Label thanksLabel;

        private VisualElement failureContainer;
        private Label failureLabel;

        // Instance
        public static FeedbackManager Instance;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(Instance);
            }
        }

        void Start()
        {
            // Cache
            feedbackUI = GetComponent<UIDocument>();
            LOCALE = I18n.Instance;

            // UI
            root = feedbackUI.rootVisualElement;

            feedbackContainer = root.Q<VisualElement>("FeedbackContainer");
            feedbackScrollView = feedbackContainer.Q<VisualElement>("FeedbackScrollView");
            titleLabel = feedbackContainer.Q<Label>("Title");
            feedbackLabelA = feedbackContainer.Q<Label>("LabelA");
            feedbackLabelB = feedbackContainer.Q<Label>("LabelB");
            feedbackLabelC = feedbackContainer.Q<Label>("LabelC");
            cancelButton = feedbackContainer.Q<Button>("CancelButton");
            sendButton = feedbackContainer.Q<Button>("SendButton");

            reportTitleLabel = feedbackContainer.Q<Label>("ReportTitleLabel");
            reportTitleTextField = feedbackContainer.Q<TextField>("ReportTitleTextField");
            reportCategoryLabel = feedbackContainer.Q<Label>("ReportCategoryLabel");
            reportCategoryEnumField = feedbackContainer.Q<EnumField>("ReportCategoryEnumField");
            reportBugTypeLabel = feedbackContainer.Q<Label>("ReportBugTypeLabel");
            reportBugTypeEnumField = feedbackContainer.Q<EnumField>("ReportBugTypeEnumField");
            reportBugFrequencyLabel = feedbackContainer.Q<Label>("ReportBugFrequencyLabel");
            reportBugFrequencyEnumField = feedbackContainer.Q<EnumField>("ReportBugFrequencyEnumField");
            reportDescriptionLabel = feedbackContainer.Q<Label>("ReportDescriptionLabel");
            reportDescriptionTextField = feedbackContainer.Q<TextField>("ReportDescriptionTextField");

            loadingOverlay = root.Q<VisualElement>("LoadingOverlay");
            loadingOverlaySpinner = loadingOverlay.Q<VisualElement>("Spinner");

            thanksContainer = root.Q<VisualElement>("ThanksContainer");
            thanksLabel = thanksContainer.Q<Label>("Label");

            failureContainer = root.Q<VisualElement>("FailureContainer");
            failureLabel = failureContainer.Q<Label>("Label");

            // Button taps
            cancelButton.clicked += () => Close();
            sendButton.clicked += () => Send();

            thanksContainer.AddManipulator(new Clickable(evt =>
            {
                CloseThanks();
            }));

            failureContainer.AddManipulator(new Clickable(evt =>
            {
                CloseFailure();
            }));

            reportCategoryEnumField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                HandleReportCategoryChanged(evt);
            });

            // Init
            Init();

            root.RegisterCallback<GeometryChangedEvent>(evt => SetSafeAreaTop(evt));
        }

        void Update()
        {
            CheckSendButton();
        }

        void Init()
        {
            // Set texts
            titleLabel.text = LOCALE.Get("feedback_title");
            feedbackLabelA.text = LOCALE.Get("feedback_label_a");
            feedbackLabelB.text = LOCALE.Get("feedback_label_b");
            feedbackLabelC.text = LOCALE.Get("feedback_label_c");

            reportTitleLabel.text = LOCALE.Get("feedback_report_title");
            reportCategoryLabel.text = LOCALE.Get("feedback_report_category");
            reportBugTypeLabel.text = LOCALE.Get("feedback_report_bug_type");
            reportBugFrequencyLabel.text = LOCALE.Get("feedback_report_bug_frequency");
            reportDescriptionLabel.text = LOCALE.Get("feedback_report_description");

            cancelButton.text = LOCALE.Get("feedback_cancel");
            sendButton.text = LOCALE.Get("feedback_send");

            thanksLabel.text = LOCALE.Get("feedback_label_thanks");

            failureLabel.text = LOCALE.Get("feedback_label_failure");

            // TODO - Enable this lines after updating the engine to 2023 or higher
            // reportTitleTextField.textEdition.placeholder = LOCALE.Get("feedback_report_title_placeholder");
            // reportDescriptionLabel.textEdition.placeholder = LOCALE.Get("feedback_report_description_placeholder_Suggestion");

            reportTitleTextField.multiline = true;
            reportDescriptionTextField.multiline = true;

            reportBugTypeLabel.style.display = DisplayStyle.None;
            reportBugTypeEnumField.style.display = DisplayStyle.None;
            reportBugFrequencyLabel.style.display = DisplayStyle.None;
            reportBugFrequencyEnumField.style.display = DisplayStyle.None;

            // Hide Container
            feedbackContainer.style.display = DisplayStyle.None;
            feedbackContainer.style.opacity = 0;

            loadingOverlay.style.display = DisplayStyle.None;
            loadingOverlay.style.opacity = 0;

            thanksContainer.style.display = DisplayStyle.None;
            thanksContainer.style.opacity = 0;

            failureContainer.style.display = DisplayStyle.None;
            failureContainer.style.opacity = 0;

            // Config user reporting
            var userReportingConfiguration = new UserReportingClientConfiguration(100, 300, 60, 10, MetricsGatheringMode.Automatic);
            UserReportingService.Instance.Configure(userReportingConfiguration);
            UserReportingService.Instance.SendInternalMetrics = true;

            Clear();
        }

        void SetSafeAreaTop(GeometryChangedEvent evt)
        {
            root.UnregisterCallback<GeometryChangedEvent>(evt => SetSafeAreaTop(evt));

            // Set safe area at the top
            float tempHeight = Screen.height - Screen.safeArea.height;

            float safeAreaTopHeight = Mathf.RoundToInt(tempHeight / (Camera.main.pixelWidth / GameData.GAME_PIXEL_WIDTH));

            titleLabel.style.top = safeAreaTopHeight;
            feedbackScrollView.style.top = titleHeight + safeAreaTopHeight;
        }

        //// CONTAINER ////

        public void Open()
        {
            feedbackOpen = true;

            ToggleContainer();
        }

        void Close()
        {
            feedbackOpen = false;

            ToggleContainer();
        }

        void ToggleContainer()
        {
            if (feedbackOpen)
            {
                feedbackContainer.style.display = DisplayStyle.Flex;
                feedbackContainer.style.opacity = 1;
            }
            else
            {
                feedbackContainer.style.opacity = 0;

                StartCoroutine(HideContainer());
            }
        }

        IEnumerator HideContainer()
        {
            if (!feedbackOpen)
            {
                yield return new WaitForSeconds(0.3f);
            }

            if (!feedbackOpen)
            {
                feedbackContainer.style.display = DisplayStyle.None;

                Clear();
            }
        }

        //// DATA ////

        void Send()
        {
            // TODO - Handle mobile input enter button tap

            OpenLoadingOverlay();

            UserReportingService.Instance.TakeScreenshot(Screen.width, Screen.height);

            UserReportingService.Instance.SetReportSummary(reportTitleTextField.text);
            UserReportingService.Instance.SetReportDescription(reportDescriptionTextField.text);

            UserReportingService.Instance.AddDimensionValue("Category", reportCategoryEnumField.value.ToString());

            if (Glob.ParseEnum<ReportCategory>(reportCategoryEnumField.value.ToString()) == ReportCategory.Bug)
            {
                UserReportingService.Instance.AddDimensionValue("Bug Type", reportBugTypeEnumField.value.ToString());
                UserReportingService.Instance.AddDimensionValue("Bug Frequency", reportBugFrequencyEnumField.value.ToString());
            }

            UserReportingService.Instance.SendUserReport(null, (success) =>
            {
                CloseLoadingOverlay();

                if (success)
                {
                    Close();

                    OpenThanks();
                }
                else
                {
                    OpenFailed();
                }
            });
        }

        void HandleReportCategoryChanged(ChangeEvent<string> evt)
        {
            // TODO - Enable this lines after updating the engine to 2023 or higher
            // reportDescriptionLabel.textEdition.placeholder = LOCALE.Get("feedback_report_description_placeholder_" + evt.newValue);

            if (Glob.ParseEnum<ReportCategory>(evt.newValue) == ReportCategory.Bug)
            {
                reportBugTypeLabel.style.display = DisplayStyle.Flex;
                reportBugTypeEnumField.style.display = DisplayStyle.Flex;
                reportBugFrequencyLabel.style.display = DisplayStyle.Flex;
                reportBugFrequencyEnumField.style.display = DisplayStyle.Flex;
            }
            else
            {
                reportBugTypeLabel.style.display = DisplayStyle.None;
                reportBugTypeEnumField.style.display = DisplayStyle.None;
                reportBugFrequencyLabel.style.display = DisplayStyle.None;
                reportBugFrequencyEnumField.style.display = DisplayStyle.None;
            }
        }

        void CheckSendButton()
        {
            sendButton.SetEnabled(reportTitleTextField.value != "" && reportDescriptionTextField.value != "");
        }

        void Clear()
        {
            UserReportingService.Instance.ClearOngoingReport();

            reportTitleTextField.value = "";
            reportCategoryEnumField.value = ReportCategory.Suggestion;
            reportBugTypeEnumField.value = ReportBugType.General;
            reportBugFrequencyEnumField.value = ReportBugFrequency.Sometimes;
            reportDescriptionTextField.value = "";
        }

        //// THANKS CONTAINER ////

        void OpenThanks()
        {
            thanksOpen = true;

            thanksContainer.style.display = DisplayStyle.Flex;
            thanksContainer.style.opacity = 1;

            StartCoroutine(CloseThanksAfterDelay());
        }

        void CloseThanks()
        {
            thanksOpen = false;

            thanksContainer.style.opacity = 0;

            StartCoroutine(HideThanksContainer());
        }

        IEnumerator CloseThanksAfterDelay()
        {
            if (thanksOpen)
            {
                yield return new WaitForSeconds(postCloseDelay);
            }

            if (thanksOpen)
            {
                thanksContainer.style.opacity = 0;

                HideThanksContainer();
            }

        }

        IEnumerator HideThanksContainer()
        {
            if (!feedbackOpen)
            {
                yield return new WaitForSeconds(0.3f);
            }

            if (!feedbackOpen)
            {
                thanksContainer.style.display = DisplayStyle.None;
            }
        }

        //// FAILED CONTAINER ////

        void OpenFailed()
        {
            failureOpen = true;

            failureContainer.style.display = DisplayStyle.Flex;
            failureContainer.style.opacity = 1;

            StartCoroutine(CloseFailureAfterDelay());
        }

        void CloseFailure()
        {
            failureOpen = false;

            failureContainer.style.opacity = 0;

            StartCoroutine(HideFailureContainer());
        }

        IEnumerator CloseFailureAfterDelay()
        {
            if (failureOpen)
            {
                yield return new WaitForSeconds(postCloseDelay);
            }

            if (failureOpen)
            {
                failureContainer.style.opacity = 0;

                HideFailureContainer();
            }

        }

        IEnumerator HideFailureContainer()
        {
            if (!feedbackOpen)
            {
                yield return new WaitForSeconds(0.3f);
            }

            if (!feedbackOpen)
            {
                failureContainer.style.display = DisplayStyle.None;
            }
        }

        //// LOADING OVERLAY////

        void OpenLoadingOverlay()
        {
            loadingOverlayOpen = true;

            loadingOverlay.style.display = DisplayStyle.Flex;
            loadingOverlay.style.opacity = 1;

            StartCoroutine(SpinTheSpinner());
            StartCoroutine(HideLoadingOverlay());
        }

        void CloseLoadingOverlay()
        {
            loadingOverlayOpen = false;

            loadingOverlay.style.display = DisplayStyle.None;
            loadingOverlay.style.opacity = 0;

            StopCoroutine(SpinTheSpinner());
        }

        IEnumerator SpinTheSpinner()
        {
            WaitForSeconds wait = new(spinnerSpeed);

            int count = 0;

            while (loadingOverlayOpen)
            {
                loadingOverlaySpinner.style.backgroundImage = new StyleBackground(spinnerSprites[count]);

                if (count == spinnerSprites.Length - 1)
                {
                    count = 0;
                }
                else
                {
                    count++;
                }

                yield return wait;
            }
        }

        IEnumerator HideLoadingOverlay()
        {
            yield return new WaitForSeconds(loadingOverlayCloseDelay);

            CloseLoadingOverlay();
        }
    }
}
