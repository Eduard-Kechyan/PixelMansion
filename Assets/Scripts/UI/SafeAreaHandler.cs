using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SafeAreaHandler : MonoBehaviour
{
    public float manualSizes = 0f;
    public bool alternativeAspectRatio;
    public Camera cam;
    public UIDocument valuesUI;

    private float height;
    private float calculatedHeight;
    private float dividedHeight;
    [HideInInspector]
    public float topPadding;
    private float devicePixelWidth;
    private float singlePixelWidth;
    private VisualElement valuesBox;
    private VisualElement bottomBox;
    private VisualElement options;
    private VisualElement infoBox;
    private VisualElement board;

    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        valuesBox = valuesUI.rootVisualElement.Q<VisualElement>("ValuesBox");
        bottomBox = root.Q<VisualElement>("BottomBox");
        options = root.Q<VisualElement>("Options");
        infoBox = root.Q<VisualElement>("InfoBox");
        board = root.Q<VisualElement>("Board");

        devicePixelWidth = cam.pixelWidth;
        singlePixelWidth = devicePixelWidth / GameData.GAME_PIXEL_WIDTH;

        height = Screen.height - Screen.safeArea.height;

        // Set top padding for top box
        topPadding = Mathf.RoundToInt(height / singlePixelWidth);

        valuesBox.style.top = topPadding;

        // Calculated sizes
        calculatedHeight = ((Screen.height - height) / singlePixelWidth) - manualSizes;

        if (alternativeAspectRatio)
        {
            dividedHeight = Mathf.FloorToInt(calculatedHeight / 5);
        }
        else
        {
            dividedHeight = Mathf.FloorToInt(calculatedHeight / 4);
        }

        bottomBox.style.paddingBottom = dividedHeight;
        infoBox.style.marginBottom = dividedHeight;
        board.style.marginBottom = dividedHeight;
    }

    public float GetBottomOffset()
    {
        float bottomOffset = options.resolvedStyle.height + (dividedHeight * 2);

        return bottomOffset;
    }
}
