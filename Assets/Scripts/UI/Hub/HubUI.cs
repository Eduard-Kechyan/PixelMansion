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

        [HideInInspector]
        public Vector2 playButtonPosition;
        [HideInInspector]
        public Vector2 taskButtonPosition;

        // References
        private SafeAreaHandler safeAreaHandler;
        private SettingsMenu settingsMenu;
        private ShopMenu shopMenu;
        private TaskMenu taskMenu;
        private SoundManager soundManager;

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

            CalcPlayButtonPosition();

            CalcTaskButtonPosition();
        }

        //// Positions ////

        // Get play button position in the world space
        void CalcPlayButtonPosition()
        {
            // Calculate the button position on the screen and the world space
            float singlePixelWidth = Camera.main.pixelWidth / GameData.GAME_PIXEL_WIDTH;

            Vector2 playButtonScreenPosition = new(
                singlePixelWidth* (root.worldBound.width- (root.worldBound.width- playButton.worldBound.center.x)),
                singlePixelWidth* (root.worldBound.height- playButton.worldBound.center.y )
            );

            playButtonPosition = Camera.main.ScreenToWorldPoint(playButtonScreenPosition);
        }

        // Get Task button position in the world space
        void CalcTaskButtonPosition()
        {
            // Calculate the button position on the screen and the world space
            float singlePixelWidth = Camera.main.pixelWidth / GameData.GAME_PIXEL_WIDTH;

            Vector2 taskButtonScreenPosition = new(
                singlePixelWidth* (root.worldBound.width- (root.worldBound.width- taskButton.worldBound.center.x)),
                singlePixelWidth* (root.worldBound.height- taskButton.worldBound.center.y)
            );

            taskButtonPosition = Camera.main.ScreenToWorldPoint(taskButtonScreenPosition);
        }
    }
}
