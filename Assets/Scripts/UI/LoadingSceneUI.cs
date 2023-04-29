using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LoadingSceneUI : MonoBehaviour
{
    // Variables
    public float backgroundDelay = 15f;
    public float skyUpDelay = 0.1f;
    public float skyDownDelay = 1f;

    private int backgroundCount = 0;
    private float skyUpCount = 0;
    private float skyDownCount = 0;
    private Sprite[] backgroundSprites;

// UI
    private VisualElement root;
    private VisualElement background;
    private Label versionLabel;
    private VisualElement skyUp;
    private VisualElement skyDown;

    private void Start()
    {
        // Load the sprites
        backgroundSprites = Resources.LoadAll<Sprite>("Scenes/Loading/Scene");

        // Cache UI
        root = GetComponent<UIDocument>().rootVisualElement;

        background = root.Q<VisualElement>("Background");
        versionLabel = root.Q<Label>("Version");
        skyUp = root.Q<VisualElement>("SkyUp");
        skyDown = root.Q<VisualElement>("SkyDown");

        SetVersion();

        // Start the animation
        InvokeRepeating("ChangeBackgroundSprite", 0.0f, backgroundDelay * Time.fixedDeltaTime);
        InvokeRepeating("MoveSkyUpSprite", 0.0f, skyUpDelay * Time.fixedDeltaTime);
        InvokeRepeating("MoveSkyDownSprite", 0.0f, skyDownDelay * Time.fixedDeltaTime);
    }

    void SetVersion()
    {
        versionLabel.text = "v." + Application.version;
    }

    void ChangeBackgroundSprite()
    {
        background.style.backgroundImage = new StyleBackground(backgroundSprites[backgroundCount]);

        if (backgroundCount == backgroundSprites.Length - 1)
        {
            backgroundCount = 0;
        }
        else
        {
            backgroundCount++;
        }
    }

    void MoveSkyUpSprite()
    {
        skyUp.style.right = new Length(skyUpCount, LengthUnit.Pixel);

        if (skyUpCount <= -500)
        {
            skyUpCount = 0;
        }
        else
        {
            skyUpCount -= 0.1f;
        }
    }

    void MoveSkyDownSprite()
    {
        skyDown.style.right = new Length(skyDownCount, LengthUnit.Pixel);

        if (skyDownCount <= -500)
        {
            skyDownCount = 0;
        }
        else
        {
            skyDownCount -= 0.1f;
        }
    }
}
