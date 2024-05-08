using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class SpeechBubble : MonoBehaviour
    {
        // Variables
        public Vector2 posOffset;
        public float closeDelay = 0.3f;
        public float speechLength = 3f;
        public float borderClamp;
        public float tailDiffOffset = 4f;
        public float tailRightOffset = 15f;
        public float charTopOffset = 20f;

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
        private Translate speechBubbleTranslate;
        private string[] speechChunks;
        private Coroutine speechCoroutine;

        //References
        private UIDocument worldUIDoc;
        private UIDocument valuesUIDoc;
        private Camera cam;

        // UI
        private VisualElement root;
        private Button speechBubble;
        private Label speechLabel;
        private VisualElement tail;

        void Start()
        {
            // Cache
            worldUIDoc = GameRefs.Instance.worldUIDoc;
            valuesUIDoc = GameRefs.Instance.valuesUIDoc;
            cam = Camera.main;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            speechBubble = root.Q<Button>("SpeechBubble");
            speechLabel = root.Q<Label>("SpeechLabel");
            tail = root.Q<VisualElement>("Tail");

            // UI taps
            speechBubble.clicked += () => SoundManager.Tap(() => StartCoroutine(CloseBubble()));

            enabled = false;
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
            speechBubble.style.scale = new Vector2(0f, 0f);
            speechBubble.style.top = 0f;
            speechBubble.style.left = 0f;
            speechBubble.style.translate = speechBubbleTranslate;

            // Calc the top clamp
            VisualElement worldRoot = worldUIDoc.rootVisualElement;

            VisualElement topBox = worldRoot.Q<VisualElement>("TopBox");
            Button settingsButton = topBox.Q<Button>("SettingsButton");

            VisualElement valuesBox = valuesUIDoc.rootVisualElement.Q<VisualElement>("ValuesBox");

            topBorderClamp = valuesBox.resolvedStyle.top + valuesBox.resolvedStyle.marginTop + valuesBox.resolvedStyle.height;

            topClamp = new Vector2(
                topBox.resolvedStyle.paddingLeft + settingsButton.resolvedStyle.width + settingsButton.resolvedStyle.marginRight + borderClamp,
                topBox.resolvedStyle.top + topBox.resolvedStyle.height + borderClamp - settingsButton.resolvedStyle.marginBottom
            );

            // Calc the bottom clamp
            VisualElement bottomBox = worldRoot.Q<VisualElement>("BottomBox");
            Button playButton = bottomBox.Q<Button>("PlayButton");

            bottomClamp = new Vector2(
                bottomBox.resolvedStyle.paddingLeft + playButton.resolvedStyle.height + borderClamp,
                bottomBox.resolvedStyle.paddingBottom + playButton.resolvedStyle.height + borderClamp
            );
        }

        //// Handle Movement ////

        // Make the bubble follow the character around
        // Clamp it inside the game/scene/hud view
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

                // Set the position to the clamps
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

        // Move and flip the tail as necessary
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
                tail.style.top = -8;
                tail.style.bottom = new StyleLength(StyleKeyword.Auto);

                if (isOnTheLeft) // Left
                {
                    tail.style.backgroundImage = new StyleBackground(tailTopLeftSprite);

                    tail.style.translate = new Translate(0, 0);
                }
                else // Right
                {
                    tail.style.backgroundImage = new StyleBackground(tailTopRightSprite);

                    tail.style.translate = new Translate(-tailRightOffset, 0);
                }
            }
            else // Bottom
            {
                tail.style.top = new StyleLength(StyleKeyword.Auto);
                tail.style.bottom = -7;

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

        //// Open ////

        // Get ready to open the bubble
        public void Open(string newContent, Vector2 newPos, CharSpeech newCharSpeech)
        {
            if (!first)
            {
                Initialize();

                first = true;
            }

            Glob.StopTimeout(speechCoroutine);

            if (isBubbleShowing)
            {
                charSpeech?.Closed();

                HideSpeechBubble(() =>
                {
                    /*if (newContent.Contains("(-)"))
                    {
                        OpenMulti(newContent, newPos, newCharSpeech);
                    }
                    else
                    {
                        OpenSingle(newContent, newPos, newCharSpeech);
                    }*/
                    OpenSingle(newContent, newPos, newCharSpeech);
                });
            }
            else
            {
                /* if (newContent.Contains("(-)"))
                 {
                     OpenMulti(newContent, newPos, newCharSpeech);
                 }
                 else
                 {
                     OpenSingle(newContent, newPos, newCharSpeech);
                 }*/
                OpenSingle(newContent, newPos, newCharSpeech);
            }
        }

        // Open the bubble once
        void OpenSingle(string newContent, Vector2 newPos, CharSpeech newCharSpeech)
        {
            charSpeech = newCharSpeech;

            SetPos(newPos);

            ShowSpeechBubble(newContent);

            speechCoroutine = Glob.SetTimeout(() =>
            {
                //StartCoroutine(CloseBubble());
            }, speechLength);
        }

        // Open the bubble multiple times until the character finishes talking
        void OpenMulti(string newContent, Vector2 newPos, CharSpeech newCharSpeech)
        {
            // TODO - Properly handle multiline speech
            charSpeech = newCharSpeech;

            SetPos(newPos);

            speechChunks = newContent.Split(" (-) ");

            if (speechChunks.Length > 0)
            {
                ShowSpeechBubble(speechChunks[0]);

                speechCoroutine = Glob.SetTimeout(() =>
                {
                    //StartCoroutine(CloseBubble());
                }, speechLength);
            }
            else
            {
                // ERROR
                ErrorManager.Instance.Throw(ErrorManager.ErrorType.Code, "SpeechBubble.cs -> SetOpenData()", "speechChunks length is 0");

                charSpeech?.Closed();
            }
        }

        //// Show/Hide ////

        void ShowSpeechBubble(string newContent)
        {
            speechLabel.text = newContent;
            speechBubble.style.display = DisplayStyle.Flex;
            speechBubble.style.scale = new Vector2(1f, 1f);
            speechBubble.style.opacity = 1;

            isBubbleShowing = true;
            enabled = true;
        }

        void HideSpeechBubble(Action callback = null, bool immediately = false)
        {
            speechBubble.style.opacity = 0f;
            speechBubble.style.scale = new Vector2(0f, 0f);

            isBubbleShowing = false;
            enabled = false;

            if (immediately)
            {
                speechBubble.style.display = DisplayStyle.None;
            }
            else
            {
                StartCoroutine(CloseSpeechBubble(callback));
            }
        }

        IEnumerator CloseSpeechBubble(Action callback = null)
        {
            yield return new WaitForSeconds(0.3f);

        }

        public void Close(bool immediately = false)
        {
            HideSpeechBubble(null, immediately);
        }

        public void SetPos(Vector2 newPos)
        {
            charPos = RuntimePanelUtils.CameraTransformWorldToPanel(
                root.panel,
                newPos,
                cam
            );

            Vector2 calculatedPos = new(newPos.x + posOffset.x, newPos.y + posOffset.y);

            newUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
                root.panel,
                calculatedPos,
                cam
            );
        }

        IEnumerator CloseBubble()
        {
            yield return new WaitForSeconds(closeDelay);

            HideSpeechBubble();

            charSpeech?.Closed();
        }
    }
}