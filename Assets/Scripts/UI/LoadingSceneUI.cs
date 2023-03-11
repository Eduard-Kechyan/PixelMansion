using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LoadingSceneUI : MonoBehaviour
{
    public float backgroundDelay = 15f;
    public float skyUpDelay = 0.1f;
    public float skyDownDelay = 1f;
    private VisualElement background;
    private Label version;
    private VisualElement skyUp;
    private VisualElement skyDown;
    private int backgroundCount = 0;
    private float skyUpCount = 0;
    private float skyDownCount = 0;
    private Sprite[] backgroundSprites;

    private void Start()
    {
        // Load the sprites
        backgroundSprites = Resources.LoadAll<Sprite>("Scenes/Loading/Scene");

        // Get UI items
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        background = root.Q<VisualElement>("Background");
        version = root.Q<Label>("Version");
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
        version.text = "Version: " + Application.version;
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
