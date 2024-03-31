using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class MergeUI : MonoBehaviour
    {
        // Variables
        public SceneLoader sceneLoader;
        public PointerHandler pointerHandler;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        public FeedbackManager feedbackManager;
#endif

        [HideInInspector]
        public bool loaded = false;

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
        private DataManager dataManager;
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
            dataManager = DataManager.Instance;
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
                if (pointerHandler != null && pointerHandler.buttonCallback != null)
                {
                    pointerHandler.ButtonPress(Types.Button.Home);
                }
                else
                {
                    soundManager.PlaySound(Types.SoundType.Transition);
                    sceneLoader.Load(Types.Scene.World);
                }
            };
            inventoryButton.clicked += () => inventoryMenu.Open();
            bonusButton.clicked += () => bonusManager.GetBonus();
            shopButton.clicked += () => shopMenu.Open();
            taskButton.clicked += () =>
            {
                if (pointerHandler != null && pointerHandler.buttonCallback != null)
                {
                    pointerHandler.ButtonPress(Types.Button.Task, false, () =>
                    {
                        taskMenu.Open();
                    });
                }
                else
                {
                    taskMenu.Open();
                }
            };

            // Calculate the button position on the screen and the world space
            singlePixelWidth = Camera.main.pixelWidth / GameData.GAME_PIXEL_WIDTH;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (Application.isEditor || Debug.isDebugBuild)
            {
                Button debugButton = root.Q<Button>("DebugButton");

                debugButton.style.display = DisplayStyle.Flex;

                debugButton.clicked += () => DebugManager.Instance.OpenMenu();

                if (feedbackManager != null)
                {
                    Button feedbackButton = root.Q<Button>("FeedbackButton");

                    feedbackButton.style.display = DisplayStyle.Flex;

                    feedbackButton.clicked += () => feedbackManager.Open();
                }
            }
#endif

            root.RegisterCallback<GeometryChangedEvent>(evt => SetUIButtons(evt, true));

            StartCoroutine(CheckBonusData());
        }

        //// Positions ////

        // Calc inventory button position in the world space
        Vector2 CalcButtonPosition(Button button)
        {
            return Camera.main.ScreenToWorldPoint(new(
                singlePixelWidth * (root.worldBound.width - (root.worldBound.width - button.worldBound.center.x)),
                singlePixelWidth * (root.worldBound.height - button.worldBound.center.y)
            ));
        }

        void SetUIButtons(GeometryChangedEvent evt, bool initial = false)
        {
            root.UnregisterCallback<GeometryChangedEvent>(evt => SetUIButtons(evt, true));

            loaded = true;

            bool setButtons = true;

            if (initial && uiButtons.mergeButtonsSet)
            {
                setButtons = false;
            }

            if (setButtons)
            {
                uiButtons.mergeButtonsSet = true;

                uiButtons.mergeHomeButtonPos = CalcButtonPosition(homeButton);
                uiButtons.mergeShopButtonPos = CalcButtonPosition(shopButton);
                uiButtons.mergeTaskButtonPos = CalcButtonPosition(taskButton);
                uiButtons.mergeBonusButtonPos = CalcButtonPosition(bonusButton);
            }

            HideButtons();
        }

        public void DisableButtons()
        {
            /*  homeButton.SetEnabled(false);
              inventoryButton.SetEnabled(false);
              shopButton.SetEnabled(false);
              taskButton.SetEnabled(false);*/
        }

        public void EnableButtons()
        {
            homeButton.SetEnabled(true);
            inventoryButton.SetEnabled(true);
            shopButton.SetEnabled(true);
            taskButton.SetEnabled(true);
        }

        public void ToggleButton(string name, bool enabled = false)
        {
            /*  switch (name)
              {
                  case "home":
                      homeButton.SetEnabled(enabled);
                      break;
                  case "inventory":
                      inventoryButton.SetEnabled(enabled);
                      break;
                  case "shop":
                      shopButton.SetEnabled(enabled);
                      break;
                  case "task":
                      taskButton.SetEnabled(enabled);
                      break;
              }*/
        }

        void HideButtons()
        {
            /*  if (!PlayerPrefs.HasKey("mergeHomeButtonShowing"))
              {
                  homeButton.style.display = DisplayStyle.None;
              }

              if (!PlayerPrefs.HasKey("mergeInventoryButtonShowing"))
              {
                  inventoryButton.style.display = DisplayStyle.None;
              }

              if (!PlayerPrefs.HasKey("mergeShopButtonShowing"))
              {
                  shopButton.style.display = DisplayStyle.None;
              }

              if (!PlayerPrefs.HasKey("mergeTaskButtonShowing"))
              {
                  taskButton.style.display = DisplayStyle.None;
              }*/
        }

        public void ShowButtons()
        {
            homeButton.style.display = DisplayStyle.Flex;
            inventoryButton.style.display = DisplayStyle.Flex;
            shopButton.style.display = DisplayStyle.Flex;
            taskButton.style.display = DisplayStyle.Flex;
        }

        public void ShowButton(Types.Button button)
        {
            switch (button)
            {
                case Types.Button.Home:
                    homeButton.style.display = DisplayStyle.Flex;
                    PlayerPrefs.SetInt("mergeHomeButtonShowing", 1);
                    break;
                case Types.Button.Settings:
                    inventoryButton.style.display = DisplayStyle.Flex;
                    PlayerPrefs.SetInt("mergeInventoryButtonShowing", 1);
                    break;
                case Types.Button.Shop:
                    shopButton.style.display = DisplayStyle.Flex;
                    PlayerPrefs.SetInt("mergeShopButtonShowing", 1);
                    break;
                case Types.Button.Task:
                    taskButton.style.display = DisplayStyle.Flex;
                    PlayerPrefs.SetInt("mergeTaskButtonShowing", 1);
                    break;
            }
        }

        public void HideButton(Types.Button button, bool alt = false)
        {
            /*     switch (button)
                 {
                     case Types.Button.Home:
                         if (alt)
                         {
                             homeButton.SetEnabled(false);
                         }
                         else
                         {
                             homeButton.style.display = DisplayStyle.None;
                             PlayerPrefs.DeleteKey("mergeHomeButtonShowing");
                         }
                         break;
                     case Types.Button.Inventory:
                         if (alt)
                         {
                             inventoryButton.SetEnabled(false);
                         }
                         else
                         {
                             inventoryButton.style.display = DisplayStyle.None;
                             PlayerPrefs.DeleteKey("mergeInventoryButtonShowing");
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
                             PlayerPrefs.DeleteKey("worldShopButtonShowing");
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
                             PlayerPrefs.DeleteKey("worldTaskButtonShowing");
                         }
                         break;
                 }*/
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

        //// Bonus ////
        IEnumerator CheckBonusData()
        {
            while (!dataManager.loaded)
            {
                yield return null;
            }

            CheckBonusButton();
        }

        // Check if there are any bonus items, 
        // if so, show the bonus button
        public void CheckBonusButton()
        {
            // Check if we should show bonus button
            if (gameData.bonusData.Count > 0)
            {
                int lastIndex = gameData.bonusData.Count - 1;

                bonusButton.style.visibility = Visibility.Visible;
                bonusButton.style.opacity = 1f;

                bonusButtonImage.style.backgroundImage = new StyleBackground(
                    gameData.bonusData[lastIndex].sprite
                );

                if (!blippingBonusIndicator)
                {
                    blippingBonusIndicator = true;

                    StartCoroutine(BonusIndicatorBlip());
                }
            }
            else
            {
                bonusButton.style.visibility = Visibility.Hidden;
                bonusButton.style.opacity = 0f;

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
