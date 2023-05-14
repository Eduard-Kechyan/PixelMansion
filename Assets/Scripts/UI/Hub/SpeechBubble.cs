using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class SpeechBubble : MonoBehaviour
{
    public Vector2 posOffset;
    public float closeDelay = 0.5f;
    public float borderClamp;
    public float tailDiffOffset = 4f;
    public float tailTopOffset = 40f;
    public float tailRightOffset = 15f;
    public float charTopOffset = 10f;

    [Header("Sprites")]
    public Sprite tailTopLeftSprite;
    public Sprite tailTopRightSprite;
    public Sprite tailBottomLeftSprite;
    public Sprite tailBottomRightSprite;

    [Header("States")]
    [ReadOnly]
    public bool isBubbleShowing = false;
    [ReadOnly]
    public float topBorderClamp;
    [ReadOnly]
    public Vector2 topClamp;
    [ReadOnly]
    public Vector2 bottomClamp;

    private CharSpeech charSpeech;
    private Vector2 newUIPos;
    private Vector2 charPos;
    private bool first;

    //References
    private UIDocument hubUIDoc;
    private UIDocument valuesUIDoc;
    private I18n LOCALE;
    private Camera cam;

    // UI
    private VisualElement root;
    private Button speechBubble;
    private Label speechLabel;
    private VisualElement tail;

    void Start()
    {
        // Cache
        hubUIDoc = GameRefs.Instance.hubUIDoc;
        valuesUIDoc = GameRefs.Instance.valuesUIDoc;
        LOCALE = I18n.Instance;
        cam = Camera.main;

        // UI
        root = GetComponent<UIDocument>().rootVisualElement;

        speechBubble = root.Q<Button>("SpeechBubble");
        speechLabel = root.Q<Label>("SpeechLabel");
        tail = root.Q<VisualElement>("Tail");

        speechBubble.clicked += () => StartCoroutine(CloseBubble());
    }

    void Update()
    {
        MoveBubble();
    }

    void Initialize()
    {
        // Reset the bubble
        speechBubble.style.display = DisplayStyle.None;
        speechBubble.style.opacity = 0f;
        speechBubble.style.scale = new Vector2(0.5f, 0.5f);
        speechBubble.style.top = 0f;
        speechBubble.style.left = 0f;

        // Calc the top clamp
        VisualElement hubRoot = hubUIDoc.rootVisualElement;

        VisualElement topBox = hubRoot.Q<VisualElement>("TopBox");
        Button settingsButton = topBox.Q<Button>("SettingsButton");

        VisualElement valuesBox = valuesUIDoc.rootVisualElement.Q<VisualElement>("ValuesBox");

        topBorderClamp = valuesBox.resolvedStyle.top + valuesBox.resolvedStyle.marginTop + valuesBox.resolvedStyle.height;

        topClamp = new Vector2(
            topBox.resolvedStyle.paddingLeft + settingsButton.resolvedStyle.width + settingsButton.resolvedStyle.marginRight + borderClamp,
            topBox.resolvedStyle.top + topBox.resolvedStyle.height + borderClamp - settingsButton.resolvedStyle.marginBottom
        );

        // Calc the bottom clamp
        VisualElement bottomBox = hubRoot.Q<VisualElement>("BottomBox");
        Button playButton = bottomBox.Q<Button>("PlayButton");

        bottomClamp = new Vector2(
            bottomBox.resolvedStyle.paddingLeft + playButton.resolvedStyle.height + borderClamp,
            bottomBox.resolvedStyle.paddingBottom + playButton.resolvedStyle.height + borderClamp
        );
    }

    void MoveBubble()
    {
        if (isBubbleShowing)
        {
            float width = speechBubble.resolvedStyle.width;
            float height = speechBubble.resolvedStyle.height;

            float maxWidth = GameData.GAME_PIXEL_WIDTH - borderClamp - width;
            float maxHeight = GameData.GAME_PIXEL_HEIGHT - borderClamp - height;

            float maxWidthTop = maxWidth - topClamp.x + borderClamp;
            bool useTop = false;

            float clampX = Mathf.Clamp(newUIPos.x, borderClamp, maxWidth);
            float clampY = Mathf.Clamp(newUIPos.y, topBorderClamp, maxHeight);

            // Check for the top clamp
            if (clampY < topClamp.y)
            {
                if (clampX > GameData.GAME_PIXEL_WIDTH - topClamp.x - width)
                {
                    clampX = Mathf.Clamp(newUIPos.x, borderClamp, GameData.GAME_PIXEL_WIDTH - topClamp.x - width);

                    useTop = true;
                }
            }

            // Check for the bottom clamp
            if (clampY > GameData.GAME_PIXEL_HEIGHT - bottomClamp.y - height)
            {
                // Clamp by the left bottom button
                if (clampX < bottomClamp.x)
                {
                    clampX = Mathf.Clamp(newUIPos.x, bottomClamp.x, maxWidth);
                }

                // Clamp by the right bottom button
                if (clampX > GameData.GAME_PIXEL_WIDTH - bottomClamp.x - width)
                {
                    clampX = Mathf.Clamp(newUIPos.x, borderClamp, GameData.GAME_PIXEL_WIDTH - bottomClamp.x - width);
                }
            }

            // Set the posotion to the clamps
            speechBubble.transform.position = new Vector2(
                clampX,
                clampY
            );

            // Check if the tail needs to be moved
            MoveTail
            (
                maxWidth,
                maxWidthTop,
                useTop
            );
        }
    }

    void MoveTail(float maxXDefault, float maxXTop, bool useTop)
    {
        bool isOnTheTop = false;
        bool isOnTheLeft = true;

        float maxX;

        if (useTop)
        {
            maxX = maxXTop;
        }
        else
        {
            maxX = maxXDefault;
        }

        // Check if the bubble is touching the top wall
        if (speechBubble.transform.position.y == topBorderClamp && (charPos.y - charTopOffset) < topBorderClamp)
        {
            isOnTheTop = true;
        }

        // Check if the bubble is touching the right wall
        if (speechBubble.transform.position.x == maxX)
        {
            float diff = charPos.x - (speechBubble.worldBound.x - tailDiffOffset);

            // Check if the character is further than the bubble
            if (charPos.x > (speechBubble.worldBound.x - tailDiffOffset))
            {
                // Check if the tail hasn't reached the end of the bubble
                if ((tail.resolvedStyle.left + tail.resolvedStyle.width) < speechBubble.resolvedStyle.width + tailRightOffset)
                {
                    tail.style.left = Mathf.Clamp(diff, 0, speechBubble.resolvedStyle.width + tailRightOffset - (tailDiffOffset * 2));

                    // Check if the tail is halfway the bubble
                    if ((tail.resolvedStyle.left + (tail.resolvedStyle.width / 2)) > ((speechBubble.resolvedStyle.width / 2) + tailRightOffset))
                    {
                        isOnTheLeft = false;
                    }
                }
                else
                {
                    tail.style.left = Mathf.Clamp(diff, 0, speechBubble.resolvedStyle.width + tailRightOffset - (tailDiffOffset * 2));

                    isOnTheLeft = false;
                }
            }
            else
            {
                tail.style.left = Mathf.Clamp(diff, 0, speechBubble.resolvedStyle.width + tailRightOffset - (tailDiffOffset * 2));
            }
        }
        else
        {
            tail.style.left = 0;
        }

        // Set the tail's sprite and position
        if (isOnTheTop) // Top
        {
            if (isOnTheLeft) // Left
            {
                tail.style.backgroundImage = new StyleBackground(tailTopLeftSprite);

                tail.style.translate = new Translate(0, -tailTopOffset);
            }
            else // Right
            {
                tail.style.backgroundImage = new StyleBackground(tailTopRightSprite);

                tail.style.translate = new Translate(-tailRightOffset, -tailTopOffset);
            }
        }
        else // Bottom
        {
            if (isOnTheLeft) // Left
            {
                tail.style.backgroundImage = new StyleBackground(tailBottomLeftSprite);

                tail.style.translate = new Translate(0, 0);
            }
            else // Right
            {
                tail.style.backgroundImage = new StyleBackground(tailBottomRightSprite);

                tail.style.translate = new Translate(-tailRightOffset, 0);
            }
        }
    }

    public void Open(string newContent, Vector2 newPos, CharSpeech newCharSpeech)
    {
        if (!first)
        {
            Initialize();

            first = true;
        }

        charSpeech = newCharSpeech;

        SetPos(newPos);

        //speechLabel.text = LOCALE.Get(newContent);
        speechLabel.text = newContent;

        speechBubble.style.display = DisplayStyle.Flex;

        isBubbleShowing = true;

        StartCoroutine(CloseOpen());
    }

    IEnumerator CloseOpen()
    {
        yield return new WaitForSeconds(0.2f);

        speechBubble.style.scale = Vector2.one;
        speechBubble.style.opacity = 1f;
    }

    public void Close()
    {
        speechBubble.style.opacity = 0f;
        speechBubble.style.scale = new Vector2(0.5f, 0.5f);

        isBubbleShowing = false;

        StartCoroutine(CloseAfter());
    }

    IEnumerator CloseAfter()
    {
        yield return new WaitForSeconds(0.3f);

        speechBubble.style.display = DisplayStyle.None;
    }

    public void SetPos(Vector2 newPos)
    {
        charPos = RuntimePanelUtils.CameraTransformWorldToPanel(
            root.panel,
            newPos,
            cam
        );

        Vector2 calculatedPos = new Vector2(newPos.x + posOffset.x, newPos.y + posOffset.y);

        newUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
            root.panel,
            calculatedPos,
            cam
        );
    }

    IEnumerator CloseBubble()
    {
        yield return new WaitForSeconds(closeDelay);

        Close();

        if (charSpeech != null)
        {
            charSpeech.Closed();
        }
    }
}
