using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class GameplayUI : MonoBehaviour
    {
        // Variables
        public SceneLoader sceneLoader;

        [Header("Indicators")]
        public Sprite[] inventoryIndicatorSprites;
        public Sprite[] bonusIndicatorSprites;
        public float inventoryIndicatorSpeed = 5f;
        public float bonusIndicatorSpeed = 0.05f;
        private bool blippingInventoryIndicator = false;
        private bool blippingBonusIndicator = false;

        private float singlePixelWidth;

        // References
        private BonusManager bonusManager;
        private GameData gameData;
        private InventoryMenu inventoryMenu;
        private ShopMenu shopMenu;
        private TaskMenu taskMenu;
        private SoundManager soundManager;
        private UIButtons uiButtons;

        // UI
        private VisualElement root;
        private Button homeButton;
        private Button inventoryButton;
        public Button bonusButton;
        private VisualElement bonusButtonImage;
        private Button shopButton;
        private Button taskButton;
        private VisualElement inventoryIndicator;
        private VisualElement bonusIndicator;

        public VisualElement homeButtonNoteDot;
        public VisualElement inventoryButtonNoteDot;
        public VisualElement shopButtonNoteDot;
        public VisualElement taskButtonNoteDot;
        public Label taskButtonNoteDotLabel;

        void Start()
        {
            // Cache
            bonusManager = GetComponent<BonusManager>();
            gameData = GameData.Instance;
            inventoryMenu = GameRefs.Instance.inventoryMenu;
            shopMenu = GameRefs.Instance.shopMenu;
            taskMenu = GameRefs.Instance.taskMenu;
            soundManager = SoundManager.Instance;
            uiButtons = gameData.GetComponent<UIButtons>();

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            homeButton = root.Q<Button>("HomeButton");
            inventoryButton = root.Q<Button>("InventoryButton");
            bonusButton = root.Q<Button>("BonusButton");
            bonusButtonImage = bonusButton.Q<VisualElement>("Image");
            shopButton = root.Q<Button>("ShopButton");
            taskButton = root.Q<Button>("TaskButton");

            homeButtonNoteDot = homeButton.Q<VisualElement>("NoteDot");
            inventoryButtonNoteDot = inventoryButton.Q<VisualElement>("NoteDot");
            shopButtonNoteDot = shopButton.Q<VisualElement>("NoteDot");
            taskButtonNoteDot = taskButton.Q<VisualElement>("NoteDot");
            taskButtonNoteDotLabel = taskButtonNoteDot.Q<Label>("Value");

            // Disable inventory button border
            inventoryButton.Q<VisualElement>("Border").pickingMode = PickingMode.Ignore;

            inventoryIndicator = inventoryButton.Q<VisualElement>("Indicator");
            bonusIndicator = bonusButton.Q<VisualElement>("Indicator");

            // UI taps
            homeButton.clicked += () =>
            {
                soundManager.PlaySound("Transition");
                sceneLoader.Load(1);
            };
            inventoryButton.clicked += () => inventoryMenu.Open();
            bonusButton.clicked += () => bonusManager.GetBonus();
            shopButton.clicked += () => shopMenu.Open();
            taskButton.clicked += () => taskMenu.Open();

            // Calculate the button position on the screen and the world space
            singlePixelWidth = Camera.main.pixelWidth / GameData.GAME_PIXEL_WIDTH;

            if (Application.isEditor || Debug.isDebugBuild)
            {
                Button debugButton = root.Q<Button>("DebugButton");

                debugButton.style.display = DisplayStyle.Flex;

                debugButton.clicked += () => DebugManager.Instance.OpenMenu();
            }

            root.RegisterCallback<GeometryChangedEvent>(evt => SetUIButtons(evt, true));

            CheckBonusButton();
        }

        //// Positions ////

        // Calc inventory button position in the world space
        Vector2 CalcButtonPosition(Button button)
        {
            return new(
                singlePixelWidth * (root.worldBound.width - (root.worldBound.width - button.worldBound.center.x)),
                singlePixelWidth * (root.worldBound.height - button.worldBound.center.y)
            );
        }

        void SetUIButtons(GeometryChangedEvent evt, bool initial = false)
        {
            root.UnregisterCallback<GeometryChangedEvent>(evt => SetUIButtons(evt, true));

            bool setButtons = true;

            if (initial && uiButtons.gameplayButtonsSet)
            {
                setButtons = false;
            }

            if (setButtons)
            {
                uiButtons.gameplayButtonsSet = true;

                uiButtons.gameplayBonusButtonScreenPos = CalcButtonPosition(bonusButton);
                uiButtons.gameplayBonusButtonPos = Camera.main.ScreenToWorldPoint(uiButtons.gameplayBonusButtonScreenPos);
            }
        }

        //// Indicators ////

        // Stop inventory indication when a new item is dropped on the inventory button
        // and start again
        public void BlipInventoryIndicator()
        {
            if (!blippingInventoryIndicator)
            {
                blippingInventoryIndicator = true;

                StartCoroutine(InventoryIndicatorBlip());
            }
        }

        // Indicate when an item is dropped on the inventory button
        IEnumerator InventoryIndicatorBlip()
        {
            int count = 0;

            inventoryIndicator.style.display = DisplayStyle.Flex;
            inventoryIndicator.style.backgroundImage = new StyleBackground(
                inventoryIndicatorSprites[0]
            );

            while (blippingInventoryIndicator)
            {
                yield return new WaitForSeconds(inventoryIndicatorSpeed);

                inventoryIndicator.style.backgroundImage = new StyleBackground(
                    inventoryIndicatorSprites[count]
                );

                count++;

                if (count == inventoryIndicatorSprites.Length - 1)
                {
                    blippingInventoryIndicator = false;

                    inventoryIndicator.style.display = DisplayStyle.None;

                    yield return null;
                }
            }
        }

        // Check if there are any bonus items, 
        // if so, show the bonus button
        public void CheckBonusButton()
        {
            // Check if we should show bonus button
            if (gameData.bonusData.Count > 0)
            {
                int lastIndex = gameData.bonusData.Count - 1;

                // Show bonus button if it's hidden
                if (bonusButton.resolvedStyle.opacity == 0f)
                {
                    bonusButton.style.visibility = Visibility.Visible;
                    bonusButton.style.opacity = 1f;
                }

                // Change the bonus button image if it's different
                if (
                    bonusButtonImage.resolvedStyle.backgroundImage
                    != new StyleBackground(gameData.bonusData[lastIndex].sprite)
                )
                {
                    bonusButtonImage.style.backgroundImage = new StyleBackground(
                        gameData.bonusData[lastIndex].sprite
                    );
                }

                if (!blippingBonusIndicator)
                {
                    blippingBonusIndicator = true;

                    StartCoroutine(BonusIndicatorBlip());
                }
            }
            else
            {
                // Hide bonus button if it's visible
                if (bonusButton.resolvedStyle.opacity == 1f)
                {
                    bonusButton.style.visibility = Visibility.Hidden;
                    bonusButton.style.opacity = 0f;
                }

                if (blippingBonusIndicator)
                {
                    blippingBonusIndicator = false;

                    StopCoroutine(BonusIndicatorBlip());
                }
            }
        }

        // Indicate that there are bonus items
        IEnumerator BonusIndicatorBlip()
        {
            int count = 0;

            bonusIndicator.style.display = DisplayStyle.Flex;
            bonusIndicator.style.backgroundImage = new StyleBackground(bonusIndicatorSprites[0]);

            while (blippingBonusIndicator)
            {
                yield return new WaitForSeconds(bonusIndicatorSpeed);

                if (count == bonusIndicatorSprites.Length)
                {
                    count = 0;
                }

                bonusIndicator.style.backgroundImage = new StyleBackground(
                    bonusIndicatorSprites[count]
                );

                count++;
            }
        }
    }
}
