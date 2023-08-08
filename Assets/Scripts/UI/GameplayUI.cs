using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameplayUI : MonoBehaviour
{
    // Variables
    public SceneLoader sceneLoader;

    [Header("Indicators")]
    public Sprite[] inventoryIndicatorSprites;
    public Sprite[] bonusIndicatorSprites;
    public float inventoryIndicatorSpeed = 5f;
    public float bonusIndicatorSpeed = 0.05f;

    [HideInInspector]
    public Vector2 bonusButtonPosition;
    [HideInInspector]
    public Vector2 inventoryButtonPosition;

    private bool blippingInventoryIndicator = false;
    private bool blippingBonusIndicator = false;

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

        // Disable inventory button border
        inventoryButton.Q<VisualElement>("Border").pickingMode = PickingMode.Ignore;

        inventoryIndicator = inventoryButton.Q<VisualElement>("Indicator");
        bonusIndicator = bonusButton.Q<VisualElement>("Indicator");

        // Button taps
        homeButton.clicked += () =>
        {
            soundManager.PlaySound("Transition");
            sceneLoader.Load(1);
        };
        inventoryButton.clicked += () => inventoryMenu.Open();
        bonusButton.clicked += () => bonusManager.GetBonus();
        shopButton.clicked += () => shopMenu.Open();
        taskButton.clicked += () => taskMenu.Open();

        root.RegisterCallback<GeometryChangedEvent>(CalcBonusButtonPosition);
        root.RegisterCallback<GeometryChangedEvent>(CalcInventoryButtonPosition);

        CheckBonusButton();
    }

    public void CalcBonusButtonPosition(GeometryChangedEvent evt)
    {
        root.UnregisterCallback<GeometryChangedEvent>(CalcBonusButtonPosition);

        // Calculate the button position on the screen and the world space
        float singlePixelWidth = Camera.main.pixelWidth / GameData.GAME_PIXEL_WIDTH;

        Vector2 bonusButtonScreenPosition = new Vector2(
            singlePixelWidth
                * (
                    root.worldBound.width
                    - (root.worldBound.width - (bonusButton.worldBound.center.x - (bonusButton.resolvedStyle.width / 4)))
                ),
            singlePixelWidth
                * (root.worldBound.height - (bonusButton.worldBound.center.y - (bonusButton.resolvedStyle.width / 4)))
        );

        bonusButtonPosition = Camera.main.ScreenToWorldPoint(bonusButtonScreenPosition);
    }

    public void CalcInventoryButtonPosition(GeometryChangedEvent evt)
    {
        root.UnregisterCallback<GeometryChangedEvent>(CalcInventoryButtonPosition);

        // Calculate the button position on the screen and the world space
        float singlePixelWidth = Camera.main.pixelWidth / GameData.GAME_PIXEL_WIDTH;

        Vector2 inventoryButtonScreenPosition = new Vector2(
            singlePixelWidth
                * (
                    root.worldBound.width
                    - (root.worldBound.width - (inventoryButton.worldBound.center.x - (inventoryButton.resolvedStyle.width / 4)))
                ),
            singlePixelWidth
                * (root.worldBound.height - (inventoryButton.worldBound.center.y - (inventoryButton.resolvedStyle.width / 4)))
        );

        inventoryButtonPosition = Camera.main.ScreenToWorldPoint(inventoryButtonScreenPosition);
    }

    public void BlipInventoryIndicator()
    {
        if (!blippingInventoryIndicator)
        {
            blippingInventoryIndicator = true;

            StartCoroutine(InventoryIndicatorBlip());
        }
    }

    IEnumerator InventoryIndicatorBlip()
    {
        int count = 0;

        inventoryIndicator.style.display = DisplayStyle.Flex;
        inventoryIndicator.style.backgroundImage = new StyleBackground(inventoryIndicatorSprites[0]);

        while (blippingInventoryIndicator)
        {
            yield return new WaitForSeconds(inventoryIndicatorSpeed);

            inventoryIndicator.style.backgroundImage = new StyleBackground(inventoryIndicatorSprites[count]);

            count++;

            if (count == inventoryIndicatorSprites.Length - 1)
            {
                blippingInventoryIndicator = false;

                inventoryIndicator.style.display = DisplayStyle.None;

                yield return null;
            }
        }
    }

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

            bonusIndicator.style.backgroundImage = new StyleBackground(bonusIndicatorSprites[count]);

            count++;
        }
    }

}
