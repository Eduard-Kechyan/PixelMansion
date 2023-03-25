using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SafeAreaHandler : MonoBehaviour
{
    public float gamePixelWidth = 180f;
    public float manualSizes = 0f;
    public bool alternativeAspectRatio;
    public Camera cam;

    private float height;
    private float calculatedHeight;
    private float dividedHeight;
    private float topPadding;
    private float devicePixelWidth;
    private float singlePixelWidth;
    private VisualElement topBox;
    private VisualElement bottomBox;
    private VisualElement options;
    private VisualElement infoBox;
    private VisualElement board;

    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        topBox = root.Q<VisualElement>("TopBox");
        bottomBox = root.Q<VisualElement>("BottomBox");
        options = root.Q<VisualElement>("Options");
        infoBox = root.Q<VisualElement>("InfoBox");
        board = root.Q<VisualElement>("Board");

        devicePixelWidth = cam.pixelWidth;
        singlePixelWidth = devicePixelWidth / gamePixelWidth;

        height = Screen.height - Screen.safeArea.height;

        // Set top padding for top box
        topPadding = Mathf.RoundToInt(height / singlePixelWidth);

        topBox.style.paddingTop = topPadding;

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
