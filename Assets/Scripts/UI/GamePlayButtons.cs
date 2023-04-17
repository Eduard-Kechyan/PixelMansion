using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class GamePlayButtons : MonoBehaviour
{
    public SceneLoader sceneLoader;

    public VisualElement root;
    private Button homeButton;
    private Button inventoryButton;
    public Button bonusButton;
    private VisualElement bonusButtonImage;
    private Button shopButton;
    private Button taskButton;

    private InventoryMenu inventoryMenu;
    private ShopMenu shopMenu;
    private TaskMenu taskMenu;
    private GameData gameData;
    private BonusManager bonusManager;

    void Start()
    {
        // Cache
        gameData = GameData.Instance;
        bonusManager = GetComponent<BonusManager>();

        // UI
        root = GetComponent<UIDocument>().rootVisualElement;

        homeButton = root.Q<Button>("HomeButton");
        inventoryButton = root.Q<Button>("InventoryButton");
        bonusButton = root.Q<Button>("BonusButton");
        bonusButtonImage = bonusButton.Q<VisualElement>("Image");
        shopButton = root.Q<Button>("ShopButton");
        taskButton = root.Q<Button>("TaskButton");

        // Menus
        inventoryMenu = MenuManager.Instance.GetComponent<InventoryMenu>();
        shopMenu = MenuManager.Instance.GetComponent<ShopMenu>();
        taskMenu = MenuManager.Instance.GetComponent<TaskMenu>();

        // Button taps
        homeButton.clicked += () => sceneLoader.Load(1);
        inventoryButton.clicked += () => inventoryMenu.Open();
        bonusButton.clicked += () => bonusManager.GetBonus();
        shopButton.clicked += () => shopMenu.Open();
        taskButton.clicked += () => taskMenu.Open();

        root.RegisterCallback<GeometryChangedEvent>(CalcBonusButtonPositionChangeEvent);

        CheckBonusButton();
    }

    void CalcBonusButtonPositionChangeEvent(GeometryChangedEvent evt)
    {
        root.UnregisterCallback<GeometryChangedEvent>(CalcBonusButtonPositionChangeEvent);

        bonusManager.CalcBonusButtonPosition();
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
