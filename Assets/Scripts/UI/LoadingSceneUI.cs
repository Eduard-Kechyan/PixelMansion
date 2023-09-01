using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class LoadingSceneUI : MonoBehaviour
    {
        // Variables
        [Header("Background")]
        public float backgroundDelay = 15f;
        public float skyUpDelay = 0.1f;
        public float skyDownDelay = 1f;

        [Header("Title")]
        public LocaleManager localeManager;

        [Header("Age")]
        public int ageHeight = 24;

        private int backgroundCount = 0;
        private float skyUpCount = 0;
        private float skyDownCount = 0;
        private Sprite[] backgroundSprites;
        private Action termsCallback;
        private Action<int> ageCallback;
        private Action updateCallback;
        private bool isAgeScrolling;
        private int currentAge = 0;

        // References
        private I18n LOCALE;

        // UI
        private VisualElement root;

        private VisualElement background;
        private VisualElement mainTitle;
        private Label versionLabel;
        private VisualElement skyUp;
        private VisualElement skyDown;

        private VisualElement overlayBackground;

        // Terms
        private VisualElement termsMenu;
        private Label termsTitleLabel;
        private Label termsLabel;
        private Button termsAcceptButton;
        private Button termsTermsButton;
        private Button termsPrivacyButton;

        // Age
        private VisualElement ageMenu;
        private Label ageTitleLabel;
        private Label ageLabel;
        private Button ageAcceptButton;
        private ScrollView ageScrollView;

        // Update
        private VisualElement updateMenu;
        private Label updateTitleLabel;
        private Label updateLabel;
        private Button updateButton;
        private Button updateExitButton;

        void Start()
        {
            // Instances
            LOCALE = I18n.Instance;

            // Load the sprites
            backgroundSprites = Resources.LoadAll<Sprite>("Scenes/Loading/Scene");

            // Cache UI
            root = GetComponent<UIDocument>().rootVisualElement;

            background = root.Q<VisualElement>("Background");
            mainTitle = root.Q<VisualElement>("MainTitle");
            versionLabel = root.Q<Label>("Version");
            skyUp = root.Q<VisualElement>("SkyUp");
            skyDown = root.Q<VisualElement>("SkyDown");

            overlayBackground = root.Q<VisualElement>("OverlayBackground");

            termsMenu = root.Q<VisualElement>("TermsMenu");
            termsTitleLabel = termsMenu.Q<Label>("TitleLabel");
            termsLabel = termsMenu.Q<Label>("TermsLabel");
            termsAcceptButton = termsMenu.Q<Button>("AcceptButton");
            termsTermsButton = termsMenu.Q<Button>("TermsButton");
            termsPrivacyButton = termsMenu.Q<Button>("PrivacyButton");

            ageMenu = root.Q<VisualElement>("AgeMenu");
            ageTitleLabel = ageMenu.Q<Label>("TitleLabel");
            ageLabel = ageMenu.Q<Label>("AgeLabel");
            ageAcceptButton = ageMenu.Q<Button>("AcceptButton");
            ageScrollView = ageMenu.Q<ScrollView>("AgeScrollView");

            updateMenu = root.Q<VisualElement>("UpdateMenu");
            updateTitleLabel = updateMenu.Q<Label>("TitleLabel");
            updateLabel = updateMenu.Q<Label>("UpdateLabel");
            updateButton = updateMenu.Q<Button>("UpdateButton");
            updateExitButton = updateMenu.Q<Button>("ExitButton");

            termsAcceptButton.clicked += () => AcceptTerms();
            termsTermsButton.clicked += () =>
            {
                Application.OpenURL(GameData.WEB_ADDRESS + "/terms");
            };
            termsPrivacyButton.clicked += () =>
            {
                Application.OpenURL(GameData.WEB_ADDRESS + "/privacy");
            };

            ageAcceptButton.clicked += () => AcceptAge();

            updateButton.clicked += () => UpdateGame();
            updateExitButton.clicked += () => Application.Quit();

            ageScrollView.verticalScroller.valueChanged += newValue => AgeScrollerHandle(newValue);

            Init();

            // Start the animation
            InvokeRepeating("ChangeBackgroundSprite", 0.0f, backgroundDelay * Time.fixedDeltaTime);
            InvokeRepeating("MoveSkyUpSprite", 0.0f, skyUpDelay * Time.fixedDeltaTime);
            InvokeRepeating("MoveSkyDownSprite", 0.0f, skyDownDelay * Time.fixedDeltaTime);
        }

        void Init()
        {
            versionLabel.text = "v." + Application.version;

            SetTitle();

            HideTermsMenu();

            HideAgeMenu();
        }

        void SetTitle()
        {
            string[] gameTitleChunks = GameData.GAME_TITLE.Split(" "[0]);

            mainTitle.Clear();

            for (int i = 0; i < gameTitleChunks.Length; i++)
            {
                // Create title chunk with name and title
                Label titleChunk = new() { name = "TitleChunk" + i, text = gameTitleChunks[i] };

                // Set the styles
                titleChunk.AddToClassList("chunk");

                // Add to the title container
                mainTitle.Add(titleChunk);
            }
        }

        void ChangeBackgroundSprite()
        {
            background.style.backgroundImage = new StyleBackground(
                backgroundSprites[backgroundCount]
            );

            if (backgroundCount == backgroundSprites.Length - 1)
            {
                backgroundCount = 0;
            }
            else
            {
                backgroundCount++;
            }
        }

        void MoveSkyUpSprite()
        {
            skyUp.style.right = new Length(skyUpCount, LengthUnit.Pixel);

            if (skyUpCount <= -500)
            {
                skyUpCount = 0;
            }
            else
            {
                skyUpCount -= 0.1f;
            }
        }

        void MoveSkyDownSprite()
        {
            skyDown.style.right = new Length(skyDownCount, LengthUnit.Pixel);

            if (skyDownCount <= -500)
            {
                skyDownCount = 0;
            }
            else
            {
                skyDownCount -= 0.1f;
            }
        }

        //// TERMS ////
        public void CheckTerms(Action callback = null)
        {
            termsCallback = callback;

            // Show the overlay
            overlayBackground.style.display = DisplayStyle.Flex;
            overlayBackground.style.opacity = 1;

            // Show the menu
            termsMenu.style.display = DisplayStyle.Flex;
            termsMenu.style.opacity = 1f;

            termsTitleLabel.text = LOCALE.Get("terms_menu_title");
            termsLabel.text = LOCALE.Get("terms_menu_label");

            termsAcceptButton.text = LOCALE.Get("terms_accept");
            termsTermsButton.text = LOCALE.Get("terms_terms_button");
            termsPrivacyButton.text = LOCALE.Get("terms_privacy_button");
        }

        void AcceptTerms()
        {
            HideTermsMenu();

            PlayerPrefs.SetInt("termsAccepted", 1);
            PlayerPrefs.Save();

            if (termsCallback != null)
            {
                termsCallback();
            }
        }

        void HideTermsMenu()
        {
            // Hide the overlay
            overlayBackground.style.display = DisplayStyle.None;
            overlayBackground.style.opacity = 0;

            // Hide the menu
            termsMenu.style.display = DisplayStyle.None;
            termsMenu.style.opacity = 0;
        }

        //// AGE ////
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

            ageTitleLabel.text = LOCALE.Get("age_menu_title");
            ageLabel.text = LOCALE.Get("age_menu_label", 12); // TODO - Change 12 to the propper age

            ageAcceptButton.text = LOCALE.Get("age_accept");
            ageAcceptButton.SetEnabled(false);
        }

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

        void AcceptAge()
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

        //// UPDATE ////
        public void CheckForUpdates(Action callback = null)
        {
            updateCallback = callback;

            // TODO - Check here if the game version matches the latest available version on the app store
            if (true)
            {
                if (updateCallback != null)
                {
                    updateCallback();
                }
            }
            else
            {
                // Show the overlay
                //overlayBackground.style.display = DisplayStyle.Flex;
                // overlayBackground.style.opacity = 1;

                // Show the menu
                /* updateMenu.style.display = DisplayStyle.Flex;
                 updateMenu.style.opacity = 1f;
    
                 updateTitleLabel.text = LOCALE.Get("update_menu_title");
                 updateLabel.text = LOCALE.Get("update_menu_label");
    
                 updateButton.text = LOCALE.Get("update_button");
                 updateExitButton.text = LOCALE.Get("update_exit_button");*/
            }
        }

        void UpdateGame()
        {
            // TODO - Open the app store here, or update in game
            Debug.Log("Updating!");
        }
    }
}
