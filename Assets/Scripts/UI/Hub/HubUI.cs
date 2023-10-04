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
        public int taskNoteDotAmount = 0;

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
        private VisualElement playButtonNoteDot;
        private VisualElement taskButtonNoteDot;
        private Label taskButtonNoteDotLabel;

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

        public void ToggleButtonNoteDot(string buttonName, bool show, int amount = 0, bool useBloop = false)
        {
            VisualElement buttonNoteDot = new();

            if (useBloop)
            {
                StopCoroutine(BloopNoteDot(buttonNoteDot));
            }

            switch (buttonName)
            {
                case "settings":
                    buttonNoteDot = settingsButtonNoteDot;
                    break;
                case "shop":
                    buttonNoteDot = shopButtonNoteDot;
                    break;
                case "play":
                    buttonNoteDot = playButtonNoteDot;
                    break;
                case "task":
                    buttonNoteDot = taskButtonNoteDot;

                    if (amount > 0)
                    {
                        taskNoteDotAmount = amount;

                        if (amount > 0)
                        {
                            taskButtonNoteDotLabel.text = amount.ToString();
                        }
                    }
                    break;
            }

            buttonNoteDot.RemoveFromClassList("note_dot_bloop");

            buttonNoteDot.style.visibility = show
                ? Visibility.Visible
                : Visibility.Hidden;
            buttonNoteDot.style.opacity = show ? 1 : 0;

            if (useBloop)
            {
                StartCoroutine(BloopNoteDot(buttonNoteDot));
            }
        }

        IEnumerator BloopNoteDot(VisualElement buttonNoteDot)
        {
            yield return new WaitForSeconds(0.2f);

            buttonNoteDot.AddToClassList("note_dot_bloop");

            yield return new WaitForSeconds(0.2f);

            buttonNoteDot.RemoveFromClassList("note_dot_bloop");
        }
    }
}
