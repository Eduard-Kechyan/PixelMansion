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
        private VisualElement settingsButtonNoteDot;
        private VisualElement shopButtonNoteDot;
        private VisualElement taskButtonNoteDot;
        private VisualElement playButtonNoteDot;

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
            taskButtonNoteDot = taskButton.Q<VisualElement>("NoteDot");
            playButtonNoteDot = playButton.Q<VisualElement>("NoteDot");

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

            topBox.style.top = safeAreaHandler.GetTopOffset() + extraTopPadding;

            CalcPlayButtonPosition();
        }

        void CalcPlayButtonPosition()
        {
            // Calculate the button position on the screen and the world space
            float singlePixelWidth = Camera.main.pixelWidth / GameData.GAME_PIXEL_WIDTH;

            Vector2 playButtonScreenPosition = new Vector2(
                singlePixelWidth
                    * (
                        root.worldBound.width
                        - (
                            root.worldBound.width
                            - (
                                playButton.worldBound.center.x
                                - (playButton.resolvedStyle.width / 4)
                            )
                        )
                    ),
                singlePixelWidth
                    * (
                        root.worldBound.height
                        - (playButton.worldBound.center.y - (playButton.resolvedStyle.width / 4))
                    )
            );

            playButtonPosition = Camera.main.ScreenToWorldPoint(playButtonScreenPosition);
        }

        public void ToggleButtonNoteDot(string buttonName, bool show)
        {
            switch (buttonName)
            {
                case "settings":
                    settingsButtonNoteDot.style.visibility = show
                        ? Visibility.Visible
                        : Visibility.Hidden;
                    settingsButtonNoteDot.style.opacity = show ? 1 : 0;
                    break;
                case "shop":
                    shopButtonNoteDot.style.visibility = show
                        ? Visibility.Visible
                        : Visibility.Hidden;
                    shopButtonNoteDot.style.opacity = show ? 1 : 0;
                    break;
                case "task":
                    taskButtonNoteDot.style.visibility = show
                        ? Visibility.Visible
                        : Visibility.Hidden;
                    taskButtonNoteDot.style.opacity = show ? 1 : 0;
                    break;
                case "play":
                    playButtonNoteDot.style.visibility = show
                        ? Visibility.Visible
                        : Visibility.Hidden;
                    playButtonNoteDot.style.opacity = show ? 1 : 0;
                    break;
            }
        }
    }
}
