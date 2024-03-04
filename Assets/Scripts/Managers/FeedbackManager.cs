using System.Collections;
using System.Collections.Generic;
using Unity.Services.UserReporting;
using Unity.Services.UserReporting.Client;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class FeedbackManager : MonoBehaviour
    {
        // Variables
        public float closeThanksDelay = 2f;
        [Space(10)]

        [ReadOnly]
        public bool feedbackOpen = false;
        [ReadOnly]
        public bool thanksOpen = false;

        // Enums
        public enum ReportCategory
        {
            Suggestion,
            Bug,
            PerformanceIssue
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
        private Label titleLabel;
        private Label feedbackLabelA;
        private Label feedbackLabelB;

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

        private VisualElement thanksContainer;
        private Label thanksLabel;

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
            titleLabel = feedbackContainer.Q<Label>("Title");
            feedbackLabelA = feedbackContainer.Q<Label>("LabelA");
            feedbackLabelB = feedbackContainer.Q<Label>("LabelB");
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

            thanksContainer = root.Q<VisualElement>("ThanksContainer");
            thanksLabel = thanksContainer.Q<Label>("ThanksLabel");

            // Button taps
            cancelButton.clicked += () => Close();
            sendButton.clicked += () => Send();

            thanksContainer.AddManipulator(new Clickable(evt =>
            {
                CloseThanks();
            }));

            reportCategoryEnumField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                HandleReportCategoryChanged(evt);
            });

            // Init
            Init();
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

            reportTitleLabel.text = LOCALE.Get("feedback_report_title");
            reportCategoryLabel.text = LOCALE.Get("feedback_report_category");
            reportBugTypeLabel.text = LOCALE.Get("feedback_report_bug_type");
            reportBugFrequencyLabel.text = LOCALE.Get("feedback_report_bug_frequency");
            reportDescriptionLabel.text = LOCALE.Get("feedback_report_description");

            cancelButton.text = LOCALE.Get("feedback_cancel");
            sendButton.text = LOCALE.Get("feedback_send");

            thanksLabel.text = LOCALE.Get("feedback_label_thanks");

            // TODO - Enable this lines after updating the engine to 2023
            // reportTitleTextField.textEdition.placeholder = LOCALE.Get("feedback_report_title_placeholder");
            // reportDescriptionLabel.textEdition.placeholder = LOCALE.Get("feedback_report_description_placeholder");

            reportTitleTextField.multiline = true;
            reportDescriptionTextField.multiline = true;

            reportBugTypeLabel.style.display = DisplayStyle.None;
            reportBugTypeEnumField.style.display = DisplayStyle.None;
            reportBugFrequencyLabel.style.display = DisplayStyle.None;
            reportBugFrequencyEnumField.style.display = DisplayStyle.None;

            // Hide Container
            feedbackContainer.style.display = DisplayStyle.None;
            feedbackContainer.style.opacity = 0;

            // Config user reporting
            var userReportingConfiguration = new UserReportingClientConfiguration(100, 300, 60, 10, MetricsGatheringMode.Automatic);
            UserReportingService.Instance.Configure(userReportingConfiguration);
            UserReportingService.Instance.SendInternalMetrics = true;

            Clear();
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
                yield return new WaitForSeconds(closeThanksDelay);
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
            // TODO - Implement this function
            Debug.Log("OpenError is not implemented yet!");
        }

        //// DATA ////

        void Send()
        {
            // TODO - Handle mobile input enter button tap
            // TODO - Send feedback to the servers
            UserReportingService.Instance.SetReportSummary(reportTitleTextField.text);
            UserReportingService.Instance.AddDimensionValue("Category", reportCategoryEnumField.value.ToString());
            UserReportingService.Instance.SetReportDescription(reportDescriptionTextField.text);

            if (Glob.ParseEnum<ReportCategory>(reportCategoryEnumField.value.ToString()) == ReportCategory.Bug)
            {
                UserReportingService.Instance.AddDimensionValue("Bug Type", reportBugTypeEnumField.value.ToString());
                UserReportingService.Instance.AddDimensionValue("Bug Frequency", reportBugFrequencyEnumField.value.ToString());
            }
            else
            {
                UserReportingService.Instance.AddDimensionValue("Bug Type", "n/a");
                UserReportingService.Instance.AddDimensionValue("Bug Frequency", "n/a");
            }

            UserReportingService.Instance.CreateNewUserReport();

            UserReportingService.Instance.SendUserReport((uploadProgress) =>
            {
                // TODO - Implement loading
                Debug.Log(uploadProgress);
            }, (success) =>
            {
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
    }
}
