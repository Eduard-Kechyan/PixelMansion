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

        [HideInInspector]
        public Vector2 bonusButtonPosition;
        [HideInInspector]
        public Vector2 inventoryButtonPosition;

        // References
        private BonusManager bonusManager;
        private GameData gameData;
        private InventoryMenu inventoryMenu;
        private ShopMenu shopMenu;
        private TaskMenu taskMenu;
        private SoundManager soundManager;

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
                sceneLoader.Load(2);
            };
            inventoryButton.clicked += () => inventoryMenu.Open();
            bonusButton.clicked += () => bonusManager.GetBonus();
            shopButton.clicked += () => shopMenu.Open();
            taskButton.clicked += () => taskMenu.Open();

            root.RegisterCallback<GeometryChangedEvent>(CalcBonusButtonPosition);
            root.RegisterCallback<GeometryChangedEvent>(CalcInventoryButtonPosition);

            CheckBonusButton();
        }

        //// Positions ////

        // Get bonus button position in the world space
        public void CalcBonusButtonPosition(GeometryChangedEvent evt)
        {
            root.UnregisterCallback<GeometryChangedEvent>(CalcBonusButtonPosition);

            // Calculate the button position on the screen and the world space
            float singlePixelWidth = Camera.main.pixelWidth / GameData.GAME_PIXEL_WIDTH;

            Vector2 bonusButtonScreenPosition = new(
                singlePixelWidth * (root.worldBound.width - (root.worldBound.width - bonusButton.worldBound.center.x)),
                singlePixelWidth * (root.worldBound.height - bonusButton.worldBound.center.y)
            );

            bonusButtonPosition = Camera.main.ScreenToWorldPoint(bonusButtonScreenPosition);
        }

        // Get inventory button position in the world space
        public void CalcInventoryButtonPosition(GeometryChangedEvent evt)
        {
            root.UnregisterCallback<GeometryChangedEvent>(CalcInventoryButtonPosition);

            // Calculate the button position on the screen and the world space
            float singlePixelWidth = Camera.main.pixelWidth / GameData.GAME_PIXEL_WIDTH;

            Vector2 inventoryButtonScreenPosition = new(
                singlePixelWidth * (root.worldBound.width - (root.worldBound.width - inventoryButton.worldBound.center.x)),
                singlePixelWidth * (root.worldBound.height - inventoryButton.worldBound.center.y)
            );

            inventoryButtonPosition = Camera.main.ScreenToWorldPoint(inventoryButtonScreenPosition);
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
