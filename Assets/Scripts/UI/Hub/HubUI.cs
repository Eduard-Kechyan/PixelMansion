using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class HubUI : MonoBehaviour
    {
        // Variables
        public SceneLoader sceneLoader;
        public PointerHandler pointerHandler;
        public float extraTopPadding = 15;
        public float visibilityBottomOffset = 50f;

        [HideInInspector]
        public bool loaded = false;

        private float singlePixelWidth;

        private bool uiButtonHidden = false;

        // References
        private SafeAreaHandler safeAreaHandler;
        private SettingsMenu settingsMenu;
        private ShopMenu shopMenu;
        private TaskMenu taskMenu;
        private SoundManager soundManager;
        private UIButtons uiButtons;
        private SelectorUIHandler selectorUIHandler;

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

        void Start()
        {
            // Cache
            safeAreaHandler = GameRefs.Instance.safeAreaHandler;
            settingsMenu = GameRefs.Instance.settingsMenu;
            shopMenu = GameRefs.Instance.shopMenu;
            taskMenu = GameRefs.Instance.taskMenu;
            soundManager = SoundManager.Instance;
            uiButtons = GameData.Instance.GetComponent<UIButtons>();
            selectorUIHandler = GameRefs.Instance.hubGameUIDoc.GetComponent<SelectorUIHandler>();

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

            // UI taps
            settingsButton.clicked += () => settingsMenu.Open();
            shopButton.clicked += () => shopMenu.Open();

            taskButton.clicked += () =>
            {
                if (pointerHandler != null)
                {
                    pointerHandler.ButtonPress(Types.Button.Task, () =>
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
                if (pointerHandler != null)
                {
                    pointerHandler.ButtonPress(Types.Button.Play);
                }
                else
                {
                    soundManager.PlaySound(Types.SoundType.Transition);
                    sceneLoader.Load(Types.Scene.Gameplay);
                }
            };

            if (Application.isEditor || Debug.isDebugBuild)
            {
                Button debugButton = topBox.Q<Button>("DebugButton");

                debugButton.style.display = DisplayStyle.Flex;

                debugButton.clicked += () => DebugManager.Instance.OpenMenu();
            }

            root.RegisterCallback<GeometryChangedEvent>(Init);
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

            uiButtons.hubButtonsSet = true;

            uiButtons.hubShopButtonPos = CalcButtonPosition(shopButton);
            uiButtons.hubTaskButtonPos = CalcButtonPosition(taskButton, true);
            uiButtons.hubPlayButtonPos = CalcButtonPosition(playButton, true);

            if (!uiButtonHidden)
            {
                HideButtons();
                uiButtonHidden = true;
            }
        }

        public void DisableButtons()
        {
            playButton.SetEnabled(false);
            settingsButton.SetEnabled(false);
            shopButton.SetEnabled(false);
            taskButton.SetEnabled(false);
        }

        public void EnableButtons()
        {
            playButton.SetEnabled(true);
            settingsButton.SetEnabled(true);
            shopButton.SetEnabled(true);
            taskButton.SetEnabled(true);
        }

        public void ToggleButton(string name, bool enabled = false)
        {
            switch (name)
            {
                case "play":
                    playButton.SetEnabled(enabled);
                    break;
                case "settings":
                    settingsButton.SetEnabled(enabled);
                    break;
                case "shop":
                    shopButton.SetEnabled(enabled);
                    break;
                case "task":
                    taskButton.SetEnabled(enabled);
                    break;
            }
        }

        void HideButtons()
        {
            if (!PlayerPrefs.HasKey("hubPlayButtonShowing"))
            {
                playButton.style.display = DisplayStyle.None;
            }

            if (!PlayerPrefs.HasKey("hubSettingsButtonShowing"))
            {
                settingsButton.style.display = DisplayStyle.None;
            }

            if (!PlayerPrefs.HasKey("hubShopButtonShowing"))
            {
                shopButton.style.display = DisplayStyle.None;
            }

            if (!PlayerPrefs.HasKey("hubTaskButtonShowing"))
            {
                taskButton.style.display = DisplayStyle.None;
            }
        }

        public void ShowButtons()
        {
            playButton.style.display = DisplayStyle.Flex;
            settingsButton.style.display = DisplayStyle.Flex;
            shopButton.style.display = DisplayStyle.Flex;
            taskButton.style.display = DisplayStyle.Flex;
        }

        public void ShowButton(Types.Button button)
        {
            switch (button)
            {
                case Types.Button.Play:
                    playButton.style.display = DisplayStyle.Flex;
                    PlayerPrefs.SetInt("hubPlayButtonShowing", 1);
                    break;
                case Types.Button.Settings:
                    settingsButton.style.display = DisplayStyle.Flex;
                    PlayerPrefs.SetInt("hubSettingsButtonShowing", 1);
                    break;
                case Types.Button.Shop:
                    shopButton.style.display = DisplayStyle.Flex;
                    PlayerPrefs.SetInt("hubShopButtonShowing", 1);
                    break;
                case Types.Button.Task:
                    taskButton.style.display = DisplayStyle.Flex;
                    PlayerPrefs.SetInt("hubTaskButtonShowing", 1);
                    break;
            }
        }

        public void HideButton(Types.Button button, bool alt = false)
        {
            switch (button)
            {
                case Types.Button.Play:
                    if (alt)
                    {
                        playButton.SetEnabled(false);
                    }
                    else
                    {
                        playButton.style.display = DisplayStyle.None;
                        PlayerPrefs.DeleteKey("hubPlayButtonShowing");
                    }
                    break;
                case Types.Button.Settings:
                    if (alt)
                    {
                        settingsButton.SetEnabled(false);
                    }
                    else
                    {
                        settingsButton.style.display = DisplayStyle.None;
                        PlayerPrefs.DeleteKey("hubSettingsButtonShowing");
                    }
                    break;
                case Types.Button.Shop:
                    if (alt)
                    {
                        shopButton.SetEnabled(false);
                    }
                    else
                    {
                        shopButton.style.display = DisplayStyle.None;
                        PlayerPrefs.DeleteKey("hubShopButtonShowing");
                    }
                    break;
                case Types.Button.Task:
                    if (alt)
                    {
                        taskButton.SetEnabled(false);
                    }
                    else
                    {
                        taskButton.style.display = DisplayStyle.None;
                        PlayerPrefs.DeleteKey("hubTaskButtonShowing");
                    }
                    break;
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
    }
}
