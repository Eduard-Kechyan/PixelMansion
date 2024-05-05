using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class WorldUI : MonoBehaviour
    {
        // Variables
        public float extraTopPadding = 15;
        public float visibilityBottomOffset = 50f;

        [HideInInspector]
        public bool loaded = false;

        private float singlePixelWidth;

        private bool buttonsHidden = false;

        private bool textBoxOpen = false;

        // References
        private GameRefs gameRefs;
        private SafeAreaHandler safeAreaHandler;
        private SettingsMenu settingsMenu;
        private ShopMenu shopMenu;
        private TaskMenu taskMenu;
        private DebugMenu debugMenu;
        private SoundManager soundManager;
        private UIButtons uiButtons;
        private SelectorUIHandler selectorUIHandler;
        private ErrorManager errorManager;
        private SceneLoader sceneLoader;
        private PointerHandler pointerHandler;
        private TutorialManager tutorialManager;
        private I18n LOCALE;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private FeedbackManager feedbackManager;
#endif

        // UI
        private VisualElement root;
        private VisualElement topBox;
        private Button settingsButton;
        private Button shopButton;
        private VisualElement bottomBox;
        private Button taskButton;
        private Button playButton;

        public VisualElement settingsButtonNoteDot;
        public VisualElement shopButtonNoteDot;
        public VisualElement playButtonNoteDot;
        public VisualElement taskButtonNoteDot;
        public Label taskButtonNoteDotLabel;

        private VisualElement textBox;
        private Label textBoxText;

        void Start()
        {
            // Cache
            gameRefs = GameRefs.Instance;
            safeAreaHandler = gameRefs.safeAreaHandler;
            settingsMenu = gameRefs.settingsMenu;
            shopMenu = gameRefs.shopMenu;
            taskMenu = gameRefs.taskMenu;
            debugMenu = gameRefs.debugMenu;
            soundManager = SoundManager.Instance;
            uiButtons = GameData.Instance.GetComponent<UIButtons>();
            selectorUIHandler = gameRefs.worldGameUIDoc.GetComponent<SelectorUIHandler>();
            errorManager = ErrorManager.Instance;
            sceneLoader = gameRefs.sceneLoader;
            pointerHandler = gameRefs.pointerHandler;
            tutorialManager = gameRefs.tutorialManager;
            LOCALE = I18n.Instance;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            feedbackManager = gameRefs.feedbackManager;
#endif

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            topBox = root.Q<VisualElement>("TopBox");
            settingsButton = topBox.Q<Button>("SettingsButton");
            shopButton = topBox.Q<Button>("ShopButton");

            bottomBox = root.Q<VisualElement>("BottomBox");

            taskButton = root.Q<Button>("TaskButton");
            playButton = root.Q<Button>("PlayButton");

            settingsButtonNoteDot = settingsButton.Q<VisualElement>("NoteDot");
            shopButtonNoteDot = shopButton.Q<VisualElement>("NoteDot");
            playButtonNoteDot = playButton.Q<VisualElement>("NoteDot");
            taskButtonNoteDot = taskButton.Q<VisualElement>("NoteDot");
            taskButtonNoteDotLabel = taskButtonNoteDot.Q<Label>("Value");

            textBox = root.Q<VisualElement>("TextBox");
            textBoxText = textBox.Q<Label>("Text");

            // UI taps
            settingsButton.clicked += () => settingsMenu.Open();
            shopButton.clicked += () => shopMenu.Open();

            taskButton.clicked += () =>
            {
                if (pointerHandler != null && pointerHandler.buttonCallback != null)
                {
                    pointerHandler.ButtonPress(UIButtons.Button.Task, false, () =>
                    {
                        taskMenu.Open();
                    });
                }
                else
                {
                    taskMenu.Open();
                }
            };
            playButton.clicked += () =>
            {
                if (pointerHandler != null && pointerHandler.buttonCallback != null)
                {
                    pointerHandler.ButtonPress(UIButtons.Button.Play);
                }
                else
                {
                    soundManager.PlaySound(SoundManager.SoundType.Transition);
                    sceneLoader.Load(SceneLoader.SceneType.Merge);
                }
            };

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Application.isEditor || Debug.isDebugBuild)
            {
                Button debugButton = root.Q<Button>("DebugButton");

                debugButton.style.display = DisplayStyle.Flex;

                debugButton.clicked += () => debugMenu.OpenMenu();

                if (feedbackManager != null)
                {
                    Button feedbackButton = root.Q<Button>("FeedbackButton");

                    feedbackButton.style.display = DisplayStyle.Flex;

                    feedbackButton.clicked += () => feedbackManager.Open();
                }
            }
#endif

            root.RegisterCallback<GeometryChangedEvent>(Init);
        }

        void OnEnable()
        {
            // Subscribe to events
            TaskManager.OnTaskHandling += ToggleButtons;
        }

        void OnDestroy()
        {
            // Unsubscribe from events
            TaskManager.OnTaskHandling -= ToggleButtons;
        }

        void Init(GeometryChangedEvent evt)
        {
            root.UnregisterCallback<GeometryChangedEvent>(Init);

            loaded = true;

            // Set top box top padding based on the values position on the screen
            topBox.style.top = safeAreaHandler.GetTopOffset() + extraTopPadding;

            // Calculate the button position on the screen and the world space
            singlePixelWidth = Camera.main.pixelWidth / GameData.GAME_PIXEL_WIDTH;

            topBox.RegisterCallback<GeometryChangedEvent>(SetUIButtons);
        }

        //// Positions ////

        // Calc inventory button position in the world space
        Vector2 CalcButtonPosition(Button button, bool checkForSelector = false)
        {
            if (checkForSelector && selectorUIHandler.isSelectorOpen)
            {
                return Camera.main.ScreenToWorldPoint(new(
                    singlePixelWidth * (root.worldBound.width - (root.worldBound.width - button.worldBound.center.x)),
                    singlePixelWidth * (root.worldBound.height - button.worldBound.center.y + selectorUIHandler.bottomOffset)
                ));
            }
            else
            {
                return Camera.main.ScreenToWorldPoint(new(
                    singlePixelWidth * (root.worldBound.width - (root.worldBound.width - button.worldBound.center.x)),
                    singlePixelWidth * (root.worldBound.height - button.worldBound.center.y)
                ));
            }
        }

        public void SetUIButtons(GeometryChangedEvent evt = null)
        {
            root.UnregisterCallback<GeometryChangedEvent>(SetUIButtons);

            uiButtons.worldButtonsSet = true;

            uiButtons.worldShopButtonPos = CalcButtonPosition(shopButton);
            uiButtons.worldTaskButtonPos = CalcButtonPosition(taskButton, true);
            uiButtons.worldPlayButtonPos = CalcButtonPosition(playButton, true);

            shopButton.style.display = DisplayStyle.None;
        }

        public void HideButtons()
        {
            playButton.style.display = DisplayStyle.None;
            settingsButton.style.display = DisplayStyle.None;
            shopButton.style.display = DisplayStyle.None;
            taskButton.style.display = DisplayStyle.None;
        }

        public void ShowButtons(bool fromTutorial = false)
        {
            if (!buttonsHidden)
            {
                Debug.Log("C");
                playButton.SetEnabled(true);
                taskButton.SetEnabled(true);

                if (fromTutorial)
                {
                    buttonsHidden = true;
                }
            }

            playButton.SetEnabled(true);
            settingsButton.SetEnabled(true);
            shopButton.SetEnabled(true);
            taskButton.SetEnabled(true);

            playButton.style.display = DisplayStyle.Flex;
            settingsButton.style.display = DisplayStyle.Flex;
            // shopButton.style.display = DisplayStyle.Flex;
            taskButton.style.display = DisplayStyle.Flex;

            PlayerPrefs.DeleteKey("worldPlayButtonShowing");
            PlayerPrefs.DeleteKey("worldSettingsButtonShowing");
            PlayerPrefs.DeleteKey("worldShopButtonShowing");
            PlayerPrefs.DeleteKey("worldTaskButtonShowing");
        }

        public void CheckButtons()
        {
            if (PlayerPrefs.HasKey("worldPlayButtonShowing"))
            {
                playButton.style.display = DisplayStyle.Flex;
            }

            if (PlayerPrefs.HasKey("worldSettingsButtonShowing"))
            {
                settingsButton.style.display = DisplayStyle.Flex;
            }

            if (PlayerPrefs.HasKey("worldShopButtonShowing"))
            {
                shopButton.style.display = DisplayStyle.Flex;
            }

            if (PlayerPrefs.HasKey("worldTaskButtonShowing"))
            {
                taskButton.style.display = DisplayStyle.Flex;
            }
        }

        public void ShowButton(UIButtons.Button button)
        {
            switch (button)
            {
                case UIButtons.Button.Play:
                    playButton.SetEnabled(true);
                    playButton.style.display = DisplayStyle.Flex;
                    PlayerPrefs.SetInt("worldPlayButtonShowing", 1);
                    break;
                case UIButtons.Button.Settings:
                    settingsButton.SetEnabled(true);
                    settingsButton.style.display = DisplayStyle.Flex;
                    PlayerPrefs.SetInt("worldSettingsButtonShowing", 1);
                    break;
                case UIButtons.Button.Shop:
                    shopButton.SetEnabled(true);
                    shopButton.style.display = DisplayStyle.Flex;
                    PlayerPrefs.SetInt("worldShopButtonShowing", 1);
                    break;
                case UIButtons.Button.Task:
                    taskButton.SetEnabled(true);
                    taskButton.style.display = DisplayStyle.Flex;
                    PlayerPrefs.SetInt("worldTaskButtonShowing", 1);
                    break;
            }
        }

        public void HideButton(UIButtons.Button button, bool alt = false)
        {
            switch (button)
            {
                case UIButtons.Button.Play:
                    if (alt)
                    {
                        playButton.SetEnabled(false);
                    }
                    else
                    {
                        playButton.style.display = DisplayStyle.None;
                        PlayerPrefs.DeleteKey("worldPlayButtonShowing");
                    }
                    break;
                case UIButtons.Button.Settings:
                    if (alt)
                    {
                        settingsButton.SetEnabled(false);
                    }
                    else
                    {
                        settingsButton.style.display = DisplayStyle.None;
                        PlayerPrefs.DeleteKey("worldSettingsButtonShowing");
                    }
                    break;
                case UIButtons.Button.Shop:
                    if (alt)
                    {
                        shopButton.SetEnabled(false);
                    }
                    else
                    {
                        shopButton.style.display = DisplayStyle.None;
                        PlayerPrefs.DeleteKey("worldShopButtonShowing");
                    }
                    break;
                case UIButtons.Button.Task:
                    if (alt)
                    {
                        taskButton.SetEnabled(false);
                    }
                    else
                    {
                        taskButton.style.display = DisplayStyle.None;
                        PlayerPrefs.DeleteKey("worldTaskButtonShowing");
                    }
                    break;
                default:
                    errorManager.ThrowWarning(ErrorManager.ErrorType.Code, GetType().ToString(), "UIButtons.Button " + button + " is not implemented!");
                    break;
            }
        }

        public void ToggleButtons()
        {
            if (PlayerPrefs.HasKey("tutorialFinished"))
            {
                if (buttonsHidden)
                {
                    ShowButton(UIButtons.Button.Play);
                    ShowButton(UIButtons.Button.Task);

                    buttonsHidden = false;
                }
                else
                {
                    HideButton(UIButtons.Button.Play, true);
                    HideButton(UIButtons.Button.Task, true);

                    buttonsHidden = true;
                }
            }
        }

        public void CloseUI()
        {
            List<TimeValue> fullDelay = new() { new TimeValue(0.6f) };

            topBox.style.left = -50f;
            topBox.style.right = -50f;
            topBox.style.transitionDelay = fullDelay;
            bottomBox.style.bottom = -visibilityBottomOffset; // Note the -
            bottomBox.style.transitionDelay = fullDelay;
        }

        public void OpenUI()
        {
            List<TimeValue> nullDelay = new() { new TimeValue(0.0f) };

            topBox.style.left = 0;
            topBox.style.right = 0;
            topBox.style.transitionDelay = nullDelay;
            bottomBox.style.bottom = 0;
            bottomBox.style.transitionDelay = nullDelay;
        }

        public void OpenTextBox(string stepId)
        {
            if (tutorialManager != null && LOCALE.TryCheckIfExists("tutorial_info_world_" + stepId, out string foundText))
            {
                textBoxOpen = true;

                textBox.style.display = DisplayStyle.Flex;
                textBox.style.opacity = 1;

                if (Random.value > 0.5f)
                {
                    textBox.AddToClassList("text_box_right");
                }
                else
                {
                    textBox.RemoveFromClassList("text_box_right");
                }

                textBoxText.text = foundText;
            }
        }

        public void CloseTextBox()
        {
            if (textBoxOpen)
            {
                textBox.style.display = DisplayStyle.None;
                textBox.style.opacity = 0;

                Glob.SetTimeout(() =>
                {
                    textBoxOpen = false;
                }, 0.3f);
            }
        }
    }
}
