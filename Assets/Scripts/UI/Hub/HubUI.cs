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
        public float extraTopPadding = 15;

        private float singlePixelWidth;

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

            taskButton.clicked += () => taskMenu.Open();
            playButton.clicked += () =>
            {
                soundManager.PlaySound("Transition");
                sceneLoader.Load(2);
            };

            root.RegisterCallback<GeometryChangedEvent>(Init);
        }

        void Init(GeometryChangedEvent evt)
        {
            root.UnregisterCallback<GeometryChangedEvent>(Init);

            // Set top box top padding based on the values position on the screen
            topBox.style.top = safeAreaHandler.GetTopOffset() + extraTopPadding;

            // Calculate the button position on the screen and the world space
            singlePixelWidth = Camera.main.pixelWidth / GameData.GAME_PIXEL_WIDTH;

            SetUIButtons();
        }

        //// Positions ////

        // Calc inventory button position in the world space
        Vector2 CalcButtonPosition(Button button, bool checkForSelector = false)
        {
            if (checkForSelector && selectorUIHandler.isSelectorOpen)
            {
                return Camera.main.ScreenToWorldPoint(new(
                    singlePixelWidth * (root.worldBound.width - (root.worldBound.width - button.worldBound.center.x)),
                    singlePixelWidth * (root.worldBound.height - button.worldBound.center.y + (selectorUIHandler.bottomOffset - (button.worldBound.width / 2)))
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

        public void SetUIButtons()
        {
            uiButtons.hubButtonsSet = true;

            uiButtons.hubTaskButtonPos = CalcButtonPosition(taskButton, true);
            uiButtons.hubPlayButtonPos = CalcButtonPosition(playButton, true);
        }
    }
}
