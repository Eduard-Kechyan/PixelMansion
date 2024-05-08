using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class LoadingSceneUI : MonoBehaviour
    {
        // Variables
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public FeedbackManager feedbackManager;
#endif
        public MenuUI menuUI;

        [Header("Age")]
        public int ageHeight = 24;

        [Header("Spinner")]
        public float spinnerSpeed = 0.05f;
        [SerializeField]
        private Sprite[] spinnerSprites;

        private Action<int> ageCallback;
        private bool isAgeScrolling;
        private int currentAge = 0;

        // References
        private I18n LOCALE;
        private DebugMenu debugMenu;
        private SoundManager soundManager;

        // UI
        private VisualElement root;

        private VisualElement mainTitle;
        private Label subtitle;
        private VisualElement spinner;
        private Label versionLabel;

        private VisualElement overlayBackground;

        // Age
        private VisualElement ageMenu;
        private Label ageTitleLabel;
        private Label ageLabel;
        private Button ageAcceptButton;
        private ScrollView ageScrollView;

        void Start()
        {
            // Cache
            LOCALE = I18n.Instance;
            debugMenu = menuUI.GetComponent<DebugMenu>();
            soundManager = SoundManager.Instance;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            mainTitle = root.Q<VisualElement>("MainTitle");
            subtitle = root.Q<Label>("SubtitleLabel");
            spinner = root.Q<VisualElement>("Spinner");
            versionLabel = root.Q<Label>("Version");

            overlayBackground = root.Q<VisualElement>("OverlayBackground");

            ageMenu = root.Q<VisualElement>("AgeMenu");
            ageTitleLabel = ageMenu.Q<Label>("TitleLabel");
            ageLabel = ageMenu.Q<Label>("AgeLabel");
            ageAcceptButton = ageMenu.Q<Button>("AcceptButton");
            ageScrollView = ageMenu.Q<ScrollView>("AgeScrollView");

            // UI taps
            ageAcceptButton.clicked += () => soundManager.Tap(SetAge);

            ageScrollView.verticalScroller.valueChanged += newValue => AgeScrollerHandle(newValue);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Application.isEditor || Debug.isDebugBuild)
            {
                Button debugButton = root.Q<Button>("DebugButton");

                debugButton.style.display = DisplayStyle.Flex;

                debugButton.clicked += () => soundManager.Tap(debugMenu.OpenMenu);

                if (feedbackManager != null)
                {
                    Button feedbackButton = root.Q<Button>("FeedbackButton");

                    feedbackButton.style.display = DisplayStyle.Flex;

                    feedbackButton.clicked += () => soundManager.Tap(feedbackManager.Open);
                }
            }
#endif

            Init();

            // Start the animation
            StartCoroutine(SpinTheSpinner());
        }

        void Init()
        {
            versionLabel.text = "v." + Application.version;

            SetTitle();

            HideAgeMenu();
        }

        // Dynamically set the game's title
        void SetTitle()
        {
            string currentLocaleString = LOCALE.GetLocale().ToString();

            string[] gameTitleChunks = LOCALE.Get("game_title").Split(" "[0]);

            mainTitle.Clear();

            // Set the title
            for (int i = 0; i < gameTitleChunks.Length; i++)
            {
                // Create title chunk with name and title
                Label titleChunk = new() { name = "TitleChunk" + i, text = gameTitleChunks[i] };

                // Set the styles
                titleChunk.AddToClassList("chunk");
                titleChunk.AddToClassList("chunk_" + currentLocaleString);

                // Add to the title container
                mainTitle.Add(titleChunk);
            }

            // Set the subtitle
            subtitle.text = LOCALE.Get("game_subtitle");
            subtitle.AddToClassList("subtitle_" + currentLocaleString);
        }

        //// Animations ////

        // Animate the spinner
        IEnumerator SpinTheSpinner()
        {
            WaitForSeconds wait = new(spinnerSpeed);
            int count = 0;

            while (true)
            {
                spinner.style.backgroundImage = new StyleBackground(spinnerSprites[count]);

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

        //// AGE ////

        // Show the age menu 
        public void CheckAge(Action<int> callback = null)
        {
            ageCallback = callback;

            SetAgeScroller();

            // Show the overlay
            overlayBackground.style.display = DisplayStyle.Flex;
            overlayBackground.style.opacity = 1;

            // Show the menu
            ageMenu.style.display = DisplayStyle.Flex;
            ageMenu.style.opacity = 1f;

            ageTitleLabel.text = LOCALE.Get("menu_Age_title");
            ageLabel.text = LOCALE.Get("menu_Age_label", 12); // TODO - Change 12 to the proper age

            ageAcceptButton.text = LOCALE.Get("menu_Age_accept");
            ageAcceptButton.SetEnabled(false);
        }

        // Handle the player setting the age
        void SetAge()
        {
            HideAgeMenu();

            PlayerPrefs.SetInt("ageAccepted", 1);
            PlayerPrefs.Save();

            ageCallback?.Invoke(currentAge);
        }

        void HideAgeMenu()
        {
            // Hide the overlay
            overlayBackground.style.display = DisplayStyle.None;
            overlayBackground.style.opacity = 0;

            // Hide the menu
            ageMenu.style.display = DisplayStyle.None;
            ageMenu.style.opacity = 0;
        }

        // These three functions handle the age's scroller
        void SetAgeScroller()
        {
            for (int i = 0; i < 100; i++)
            {
                Label ageLabel = new() { name = "AgeLabel" + i, text = i.ToString() };

                ageLabel.style.width = 50;
                ageLabel.style.height = ageHeight;
                ageLabel.style.borderTopWidth = 1;
                ageLabel.style.borderBottomWidth = 1;
                ageLabel.style.borderTopColor = new Color(0, 0, 0, 0.1f);
                ageLabel.style.borderBottomColor = new Color(0, 0, 0, 0.1f);
                ageLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                ageLabel.style.fontSize = 14;

                ageLabel.style.marginLeft = 10;
                ageLabel.style.marginTop = 0;
                ageLabel.style.marginRight = 10;
                ageLabel.style.marginBottom = 0;
                ageLabel.style.paddingLeft = 0;
                ageLabel.style.paddingTop = 0;
                ageLabel.style.paddingRight = 0;
                ageLabel.style.paddingBottom = 0;

                if (i == 0)
                {
                    ageLabel.style.borderTopWidth = 0;

                    ageLabel.style.marginTop = 5;
                }

                if (i == 100)
                {
                    ageLabel.style.borderBottomWidth = 0;
                }

                ageScrollView.Add(ageLabel);
            }
        }

        void AgeScrollerHandle(float newValue)
        {
            if (Input.touchCount == 0 && isAgeScrolling)
            {
                ageScrollView.scrollDecelerationRate = 0f;
                isAgeScrolling = false;

                int rounded = (int)Mathf.Round(newValue / ageHeight);

                ageScrollView.verticalScroller.value = rounded * ageHeight;

                currentAge = int.Parse(
                    ageScrollView.contentContainer.Q<Label>("AgeLabel" + rounded).text
                );

                ageScrollView.scrollDecelerationRate = 0.135f;
            }
            else
            {
                isAgeScrolling = true;
            }

            CheckAgeButton();
        }

        void CheckAgeButton()
        {
            if (currentAge > 0)
            {
                ageAcceptButton.SetEnabled(true);
            }
            else
            {
                ageAcceptButton.SetEnabled(false);
            }
        }
    }
}
