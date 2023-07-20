using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

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
    private Action termsCallback;
    private Action<int> ageCallback;

    // References
    private I18n LOCALE;

    // UI
    private VisualElement root;

    private VisualElement background;
    private Label versionLabel;
    private VisualElement skyUp;
    private VisualElement skyDown;

    private VisualElement overlayBackground;

    // Terms
    private VisualElement termsMenu;
    private Label termsTitleLabel;
    private Label termsLabel;
    private Button termsAcceptButton;
    private Button termsTermsButton;
    private Button termsPrivacyButton;

    // Age
    private VisualElement ageMenu;
    private Label ageTitleLabel;
    private Label ageLabel;
    private SliderInt ageSlider;
    private Button ageAcceptButton;

    private void Start()
    {
        // Instances
        LOCALE = I18n.Instance;

        // Load the sprites
        backgroundSprites = Resources.LoadAll<Sprite>("Scenes/Loading/Scene");

        // Cache UI
        root = GetComponent<UIDocument>().rootVisualElement;

        background = root.Q<VisualElement>("Background");
        versionLabel = root.Q<Label>("Version");
        skyUp = root.Q<VisualElement>("SkyUp");
        skyDown = root.Q<VisualElement>("SkyDown");

        overlayBackground = root.Q<VisualElement>("OverlayBackground");

        termsMenu = root.Q<VisualElement>("TermsMenu");
        termsTitleLabel = termsMenu.Q<Label>("TitleLabel");
        termsLabel = termsMenu.Q<Label>("TermsLabel");
        termsAcceptButton = termsMenu.Q<Button>("AcceptButton");
        termsTermsButton = termsMenu.Q<Button>("TermsButton");
        termsPrivacyButton = termsMenu.Q<Button>("PrivacyButton");

        ageMenu = root.Q<VisualElement>("AgeMenu");
        ageTitleLabel = ageMenu.Q<Label>("TitleLabel");
        ageLabel = ageMenu.Q<Label>("AgeLabel");
        ageSlider = ageMenu.Q<SliderInt>("AgeSlider");
        ageAcceptButton = ageMenu.Q<Button>("AcceptButton");

        termsAcceptButton.clicked += () => AcceptTerms();
        termsTermsButton.clicked += () =>
        {
            Application.OpenURL(GameData.WEB_ADDRESS + "/terms");
        };
        termsPrivacyButton.clicked += () =>
        {
            Application.OpenURL(GameData.WEB_ADDRESS + "/privacy");
        };

        ageAcceptButton.clicked += () => AcceptAge();
        ageSlider.RegisterValueChangedCallback(AgeSliderHandle);

        Init();

        // Start the animation
        InvokeRepeating("ChangeBackgroundSprite", 0.0f, backgroundDelay * Time.fixedDeltaTime);
        InvokeRepeating("MoveSkyUpSprite", 0.0f, skyUpDelay * Time.fixedDeltaTime);
        InvokeRepeating("MoveSkyDownSprite", 0.0f, skyDownDelay * Time.fixedDeltaTime);
    }

    void Init()
    {
        versionLabel.text = "v." + Application.version;

        HideTermsMenu();
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

    //// TERMS ////
    public void CheckTerms(Action callback = null)
    {
        termsCallback = callback;

        // Show the overlay
        overlayBackground.style.display = DisplayStyle.Flex;
        overlayBackground.style.opacity = 1;

        // Show the menu
        termsMenu.style.display = DisplayStyle.Flex;
        termsMenu.style.opacity = 1f;

        termsTitleLabel.text = LOCALE.Get("terms_menu_title");
        termsLabel.text = LOCALE.Get("terms_menu_label");

        termsAcceptButton.text = LOCALE.Get("terms_accept");
        termsTermsButton.text = LOCALE.Get("terms_terms_button");
        termsPrivacyButton.text = LOCALE.Get("terms_privacy_button");
    }

    void AcceptTerms()
    {
        HideTermsMenu();

        PlayerPrefs.SetInt("termsAccepted", 1);

        if (termsCallback != null)
        {
            termsCallback();
        }
    }

    void HideTermsMenu()
    {
        // Hide the overlay
        overlayBackground.style.display = DisplayStyle.None;
        overlayBackground.style.opacity = 0;

        // Hide the menu
        termsMenu.style.display = DisplayStyle.None;
        termsMenu.style.opacity = 0;
    }

    //// AGE ////
    public void CheckAge(Action<int> callback = null)
    {
        ageCallback = callback;

        // Show the overlay
        overlayBackground.style.display = DisplayStyle.Flex;
        overlayBackground.style.opacity = 1;

        // Show the menu
        ageMenu.style.display = DisplayStyle.Flex;
        ageMenu.style.opacity = 1f;

        ageTitleLabel.text = LOCALE.Get("age_menu_title");
        ageLabel.text = LOCALE.Get("age_menu_label", 12); // TODO - Change 12 to the propper age

        ageAcceptButton.text = LOCALE.Get("age_accept");
        ageAcceptButton.SetEnabled(false);
    }

    void AcceptAge()
    {
        HideAgeMenu();

        PlayerPrefs.SetInt("ageAccepted", 1);

        if (ageCallback != null)
        {
            ageCallback(ageSlider.value);
        }
    }

    void HideAgeMenu()
    {
        // Hide the overlay
        overlayBackground.style.display = DisplayStyle.None;
        overlayBackground.style.opacity = 0;

        // Hide the menu
        ageMenu.style.display = DisplayStyle.None;
        ageMenu.style.opacity = 0;
    }

    void AgeSliderHandle(ChangeEvent<int> newData)
    {
        ageSlider.label = newData.newValue.ToString();

        if (newData.newValue > 0f)
        {
            ageAcceptButton.SetEnabled(true);
        }
        else
        {
            ageAcceptButton.SetEnabled(false);
        }
    }
}
