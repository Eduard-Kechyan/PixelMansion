using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class TestBoardInteractions : MonoBehaviour
    {
        // Variables
        private Vector2 initialTouchPos = Vector2.zero;
        public float placeholderTransitionDelay = 0.3f;
        public float scaleTransitionDelay = 0.1f;
        public float touchThreshold = 20f;

        public Scale fullScale = new(new Vector2(1f, 1f));
        public Scale smallScale = new(new Vector2(0.8f, 0.8f));
        public Scale bigScale = new(new Vector2(1.2f, 1.2f));

        private BoardManager.Tile currentItem;

        private Vector2 initialPos;

        private bool isDragging = false;
        private bool isSelected = false;

        private readonly List<TimeValue> nullTransition = new();
        private readonly List<TimeValue> fullTransition = new();

        // References
        private Camera cam;
        private TestBoardInit testBoardInit;
        private TestBoardManager testBoardManager;

        // UI
        private VisualElement root;
        private VisualElement currentIUItem;
        private VisualElement placeholderSprite;

        void Start()
        {
            // Cache
            cam = Camera.main;
            testBoardInit = GetComponent<TestBoardInit>();
            testBoardManager = GetComponent<TestBoardManager>();

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;
            placeholderSprite = root.Q<VisualElement>("PlaceholderSprite");

            root.RegisterCallback<GeometryChangedEvent>(Init);

            nullTransition.Add(new TimeValue(0f, TimeUnit.Second));
            fullTransition.Add(new TimeValue(placeholderTransitionDelay, TimeUnit.Second));
        }

        void Init(GeometryChangedEvent evt)
        {
            root.UnregisterCallback<GeometryChangedEvent>(Init);

            StartCoroutine(WaitForBoardManager());
        }

        IEnumerator WaitForBoardManager()
        {
            while (!testBoardManager.ready)
            {
                yield return null;
            }
        }

        void Update()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    // Get the initial touch position for comparing
                    initialTouchPos = touch.position;
                }

                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    // Compare the current position to the initial position
                    Vector2 diff = touch.position - initialTouchPos;

                    if (
                        diff.x > touchThreshold
                        || diff.x < -touchThreshold
                        || diff.y > touchThreshold
                        || diff.y < -touchThreshold
                    )
                    {
                        if (isDragging)
                        {
                            MoveTile(touch.position);
                        }
                        else
                        {
                            DragTile();
                        }
                    }
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    if (isDragging)
                    {
                        DropTile();
                    }
                    else
                    {
                        StartCoroutine(SelectTile(touch.position));
                    }
                }
            }
        }

        void DragTile()
        {
            if (GetTileAtPos(initialTouchPos, out VisualElement tempUIItem))
            {
                int order = int.Parse(tempUIItem.name.Replace("Tile", ""));

                if (testBoardInit.boardData[order].sprite != null && (testBoardInit.boardData[order].state == Item.State.Default || testBoardInit.boardData[order].state == Item.State.Bubble))
                {
                    currentIUItem = tempUIItem;

                    initialPos = new Vector2(currentIUItem.worldBound.x, currentIUItem.worldBound.y);

                    currentItem = testBoardInit.boardData[order];

                    placeholderSprite.style.transitionDuration = new StyleList<TimeValue>(nullTransition);
                    placeholderSprite.style.display = DisplayStyle.Flex;
                    placeholderSprite.style.backgroundImage = new StyleBackground(currentItem.sprite);

                    placeholderSprite.style.left = initialPos.x;
                    placeholderSprite.style.top = initialPos.y;

                    isDragging = true;
                    isSelected = false;
                }
            }
        }

        void MoveTile(Vector2 pos)
        {
            Vector2 worldPos = cam.ScreenToWorldPoint(pos);

            Vector2 uiPos = RuntimePanelUtils.CameraTransformWorldToPanel(root.panel, worldPos, cam);

            currentIUItem.style.visibility = Visibility.Hidden;

            placeholderSprite.style.left = uiPos.x - 12;
            placeholderSprite.style.top = uiPos.y - 12;
        }

        void DropTile()
        {
            if (isDragging)
            {
                isDragging = false;
                isSelected = false;

                placeholderSprite.style.transitionDuration = new StyleList<TimeValue>(fullTransition);
                placeholderSprite.style.left = initialPos.x;
                placeholderSprite.style.top = initialPos.y;

                StartCoroutine(DropAfter(currentIUItem));
            }
        }

        IEnumerator DropAfter(VisualElement tempUIItem)
        {
            yield return new WaitForSeconds(placeholderTransitionDelay);

            tempUIItem.style.visibility = Visibility.Visible;

            if (!isDragging)
            {
                placeholderSprite.style.display = DisplayStyle.None;
            }
        }

        IEnumerator SelectTile(Vector2 pos)
        {
            if (!isDragging)
            {
                Debug.Log(pos);
                if (GetTileAtPos(pos, out VisualElement tempUIItem))
                {
                    Debug.Log(tempUIItem.name);
                    int order = int.Parse(tempUIItem.name.Replace("Tile", ""));

                    if (testBoardInit.boardData[order].sprite != null)
                    {
                        currentIUItem = tempUIItem;

                        currentItem = testBoardInit.boardData[order];

                        isSelected = true;

                        tempUIItem.style.scale = new StyleScale(smallScale);

                        yield return new WaitForSeconds(scaleTransitionDelay);

                        tempUIItem.style.scale = new StyleScale(bigScale);

                        yield return new WaitForSeconds(scaleTransitionDelay * 2);

                        tempUIItem.style.scale = new StyleScale(fullScale);

                        yield return new WaitForSeconds(scaleTransitionDelay);
                    }
                }
            }
        }

        bool GetTileAtPos(Vector2 pos, out VisualElement uiItem)
        {
            uiItem = null;

            Vector2 worldPos = cam.ScreenToWorldPoint(pos);

            Vector2 uiPos = RuntimePanelUtils.CameraTransformWorldToPanel(root.panel, worldPos, cam);

            VisualElement tempUIItem = root.panel.Pick(uiPos);

            if (tempUIItem != null && tempUIItem.name.Contains("Tile"))
            {
                uiItem = tempUIItem;

                return uiItem != null;
            }

            return false;
        }
    }
}
