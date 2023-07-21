using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

        settingsButton.clicked += () => settingsMenu.Open();
        shopButton.clicked += () => shopMenu.Open();

        taskButton.clicked += () => taskMenu.Open();
        playButton.clicked += () => {
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
                    root.worldBound.width - (root.worldBound.width - (playButton.worldBound.center.x - (playButton.resolvedStyle.width / 4)))
                ),
            singlePixelWidth * (root.worldBound.height - (playButton.worldBound.center.y - (playButton.resolvedStyle.width / 4)))
        );

        playButtonPosition = Camera.main.ScreenToWorldPoint(playButtonScreenPosition);
    }
}
