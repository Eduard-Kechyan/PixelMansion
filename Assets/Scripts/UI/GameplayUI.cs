using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameplayUI : MonoBehaviour
{
    // Variables
    public SceneLoader sceneLoader;

    [HideInInspector]
    public Vector2 bonusButtonPosition;

    // References
    private BonusManager bonusManager;
    private GameData gameData;
    private InventoryMenu inventoryMenu;
    private ShopMenu shopMenu;
    private TaskMenu taskMenu;

    // UI
    private VisualElement root;
    private Button homeButton;
    private Button inventoryButton;
    public Button bonusButton;
    private VisualElement bonusButtonImage;
    private Button shopButton;
    private Button taskButton;

    void Start()
    {
        // Cache
        bonusManager = GetComponent<BonusManager>();
        gameData = GameData.Instance;
        inventoryMenu = GameRefs.Instance.inventoryMenu;
        shopMenu = GameRefs.Instance.shopMenu;
        taskMenu = GameRefs.Instance.taskMenu;

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

        // Button taps
        homeButton.clicked += () => sceneLoader.Load(1);
        inventoryButton.clicked += () => inventoryMenu.Open();
        bonusButton.clicked += () => bonusManager.GetBonus();
        shopButton.clicked += () => shopMenu.Open();
        taskButton.clicked += () => taskMenu.Open();

        root.RegisterCallback<GeometryChangedEvent>(CalcBonusButtonPosition);

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
        }
        else
        {
            // Hide bonus button if it's visible
            if (bonusButton.resolvedStyle.opacity == 1f)
            {
                bonusButton.style.visibility = Visibility.Hidden;
                bonusButton.style.opacity = 0f;
            }
        }
    }
}
