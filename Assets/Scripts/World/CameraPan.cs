using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class CameraPan : MonoBehaviour
    {
        // Variables
        public Sprite rootSprite;
        public Selector selector;

        [Header("Pan")]
        public bool canPan = true;
        public float touchThreshold = 20f;
        public float velocityStep = 0.35f;
        public List<string> uiToAccept;

        [Header("Clamp")]
        public bool shouldClamp = true;

        [ReadOnly]
        public Vector2 clamp;

        [Header("Rebound")]
        public bool shouldRebound = true;
        public float rebound = 10f;
        public float reboundSpeed = 1f;

        [Header("Reset debug")]
        public bool shouldReset = true;
        public float resetSpeed = 10f;

        [Header("Gizmos debug")]
        public bool showClamp = false;
        public bool showRebound = false;
        public bool showRealClamp = false;
        public bool showRealRebound = false;

        [Header("Character Debug")]
        public bool debugCharacterMovement = false;

        [Header("States")]
        [ReadOnly]
        public bool isPanning = false;

        [ReadOnly]
        public bool isRebounding = false;

        [ReadOnly]
        public bool isResetting = false;

        private Vector3 initialPos;
        private Vector2 initialTouchPos;
        private Vector2 lastTouchPos;
        private float touchStartTime;
        private Vector2 deltaPos;
        private Vector2 panVelocity;
        private float camDiffX;
        private float camDiffY;
        private bool beganOutOfUI = true;

        [HideInInspector]
        public float initialCamSize = 0;
        private bool moved = false;

        // References
        private Camera cam;
        private MenuUI menuUI;
        private PopupManager popupManager;
        private CharMove charMove;
        private HubGameUI hubGameUI;

        // UI
        private VisualElement root;

        void Start()
        {
            // Cache
            cam = Camera.main;
            menuUI = GameRefs.Instance.menuUI;
            hubGameUI = GameRefs.Instance.hubGameUI;
            popupManager = GameRefs.Instance.popupManager;
            charMove = CharMain.Instance.charMove;

            // Cache UI
            root = GameRefs.Instance.hubUIDoc.rootVisualElement;

            if (selector == null)
            {
                selector = GameObject.Find("World").GetComponent<Selector>();
            }

            // Set the camera's initial position
            initialPos = new Vector3(
                transform.position.x,
                transform.position.y,
                transform.position.z
            );

            // Initialize clamp
            clamp = CalcClamp();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                if (showClamp)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(
                        new Vector3(-clamp.x, -clamp.y),
                        new Vector3(-clamp.x, clamp.y)
                    );
                    Gizmos.DrawLine(new Vector3(-clamp.x, clamp.y), new Vector3(clamp.x, clamp.y));
                    Gizmos.DrawLine(new Vector3(clamp.x, clamp.y), new Vector3(clamp.x, -clamp.y));
                    Gizmos.DrawLine(
                        new Vector3(clamp.x, -clamp.y),
                        new Vector3(-clamp.x, -clamp.y)
                    );
                }

                if (showRebound)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(
                        new Vector3(-clamp.x + rebound, -clamp.y + rebound),
                        new Vector3(-clamp.x + rebound, clamp.y - rebound)
                    );
                    Gizmos.DrawLine(
                        new Vector3(-clamp.x + rebound, clamp.y - rebound),
                        new Vector3(clamp.x - rebound, clamp.y - rebound)
                    );
                    Gizmos.DrawLine(
                        new Vector3(clamp.x - rebound, clamp.y - rebound),
                        new Vector3(clamp.x - rebound, -clamp.y + rebound)
                    );
                    Gizmos.DrawLine(
                        new Vector3(clamp.x - rebound, -clamp.y + rebound),
                        new Vector3(-clamp.x + rebound, -clamp.y + rebound)
                    );
                }

                if (showRealClamp)
                {
                    Vector2 newClamp = new(rootSprite.rect.width / 2, rootSprite.rect.height / 2);

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(
                        new Vector3(-newClamp.x, -newClamp.y),
                        new Vector3(-newClamp.x, newClamp.y)
                    );
                    Gizmos.DrawLine(
                        new Vector3(-newClamp.x, newClamp.y),
                        new Vector3(newClamp.x, newClamp.y)
                    );
                    Gizmos.DrawLine(
                        new Vector3(newClamp.x, newClamp.y),
                        new Vector3(newClamp.x, -newClamp.y)
                    );
                    Gizmos.DrawLine(
                        new Vector3(newClamp.x, -newClamp.y),
                        new Vector3(-newClamp.x, -newClamp.y)
                    );
                }

                if (showRealRebound)
                {
                    Vector2 newClamp = new(rootSprite.rect.width / 2, rootSprite.rect.height / 2);

                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(
                        new Vector3(-newClamp.x + rebound, -newClamp.y + rebound),
                        new Vector3(-newClamp.x + rebound, newClamp.y - rebound)
                    );
                    Gizmos.DrawLine(
                        new Vector3(-newClamp.x + rebound, newClamp.y - rebound),
                        new Vector3(newClamp.x - rebound, newClamp.y - rebound)
                    );
                    Gizmos.DrawLine(
                        new Vector3(newClamp.x - rebound, newClamp.y - rebound),
                        new Vector3(newClamp.x - rebound, -newClamp.y + rebound)
                    );
                    Gizmos.DrawLine(
                        new Vector3(newClamp.x - rebound, -newClamp.y + rebound),
                        new Vector3(-newClamp.x + rebound, -newClamp.y + rebound)
                    );
                }
            }
        }
#endif

        void Update()
        {
            // Check if panning is enabled
            if (canPan && !menuUI.menuOpen)
            {
                if (Input.touchCount == 1) // Pan
                {
                    Touch touch = Input.GetTouch(0);

                    // Check if we are touching out of the UI
                    if (touch.phase == TouchPhase.Began)
                    {
                        beganOutOfUI = CheckOutOfUITouch(touch.position);
                    }

                    if (beganOutOfUI)
                    {
                        switch (touch.phase)
                        {
                            case TouchPhase.Began:
                                initialTouchPos = touch.position;
                                touchStartTime = Time.time;
                                lastTouchPos = initialTouchPos;

                                panVelocity = Vector2.zero;

                                isPanning = false;
                                isResetting = false;
                                isRebounding = false;

                                selector.triedToSelectUnselectable = false;

                                moved = false;

                                break;

                            case TouchPhase.Moved:
                                // Compare the current position to the initial position
                                Vector2 diffMoved = touch.position - initialTouchPos;

                                if (
                                    diffMoved.x > touchThreshold
                                    || diffMoved.x < -touchThreshold
                                    || diffMoved.y > touchThreshold
                                    || diffMoved.y < -touchThreshold
                                )
                                {
                                    if (!selector.isSelecting)
                                    {
                                        isPanning = true;

                                        if (touch.deltaPosition != Vector2.zero)
                                        {
                                            deltaPos = touch.deltaPosition;

                                            panVelocity = deltaPos;

                                            Pan(deltaPos);

                                            if (selector.isSelecting && !selector.isSelected)
                                            {
                                                selector.CancelSelecting();
                                            }
                                        }
                                    }

                                    moved = true;
                                }

                                selector.triedToSelectUnselectable = false;

                                break;

                            case TouchPhase.Ended:
                                isPanning = false;

                                ResetPan(true);

                                if (selector.isSelecting && !selector.isSelected)
                                {
                                    selector.CancelSelecting();
                                }

                                if (!moved)
                                {
                                    if (debugCharacterMovement && !selector.isSelected)
                                    {
                                        charMove.SetDestination(
                                            cam.ScreenToWorldPoint(touch.position)
                                        );
                                    }

                                    if (Time.time - touchStartTime >= selector.secondTapDuration)
                                    {
                                        // Tapped
                                        if (
                                            !debugCharacterMovement
                                            && !selector.isSelecting
                                            && !selector.isSelected
                                        )
                                        {
                                            if (selector.triedToSelectUnselectable)
                                            {
                                                selector.triedToSelectUnselectable = false;
                                            }
                                            else
                                            {
                                                selector.StartSelecting(touch.position, true);
                                            }
                                        }

                                        // Select the next one
                                        if (!selector.isSelecting && selector.isSelected)
                                        {
                                            selector.StartSelecting(touch.position);
                                        }
                                    }
                                }

                                break;

                            case TouchPhase.Stationary:
                                // Compare the current position to the initial position
                                Vector2 diffStationary = touch.position - initialTouchPos;

                                panVelocity = Vector2.zero;

                                if (
                                    diffStationary.x < touchThreshold
                                    || diffStationary.x > -touchThreshold
                                    || diffStationary.y < touchThreshold
                                    || diffStationary.y > -touchThreshold
                                )
                                {
                                    // Start selecting
                                    if (
                                        Time.time - touchStartTime >= selector.tapDuration
                                        && !isPanning
                                        && !selector.isSelecting
                                        && !popupManager.isSelectorPopup
                                    )
                                    {
                                        selector.StartSelecting(
                                            touch.position,
                                            false,
                                            lastTouchPos == touch.position
                                                && selector.triedToSelectUnselectable
                                        );
                                    }
                                }

                                break;
                        }
                    }
                }
            }

            // Check for inertia
            if (!isPanning)
            {
                AddInertia();
            }

            // Check for rebounding
            if (isRebounding && !isPanning)
            {
                if (
                    transform.position.x < -clamp.x - camDiffX + rebound
                    || transform.position.x > clamp.x + camDiffX - rebound
                    || transform.position.y < -clamp.y - camDiffY + rebound
                    || transform.position.y > clamp.y + camDiffY - rebound
                )
                {
                    panVelocity = Vector2.zero;
                }

                Vector3 clampedPos = new Vector3(
                    Mathf.Clamp(
                        transform.position.x,
                        -clamp.x - camDiffX + rebound,
                        clamp.x + camDiffX - rebound
                    ),
                    Mathf.Clamp(
                        transform.position.y,
                        -clamp.y - camDiffY + rebound,
                        clamp.y + camDiffY - rebound
                    ),
                    transform.position.z
                );

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    clampedPos,
                    reboundSpeed * Time.deltaTime
                );

                if (transform.position.x == clampedPos.x && transform.position.y == clampedPos.y)
                {
                    isRebounding = false;
                }
            }

            // Check for reseting
            if (isResetting && !isPanning)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    initialPos,
                    resetSpeed * Time.deltaTime
                );

                if (transform.position.x == initialPos.x && transform.position.y == initialPos.y)
                {
                    isResetting = false;
                }
            }
        }

        void LateUpdate()
        {
            ClampCamera();
        }

        Vector2 CalcClamp()
        {
            float singlePixelWidth = Screen.width / GameData.GAME_PIXEL_WIDTH;

            float halfScreenWidth = GameData.GAME_PIXEL_WIDTH / 2;
            float halfScreenHeight = (Screen.height / singlePixelWidth) / 2;

            Vector2 newClamp =
                new(
                    (rootSprite.rect.width / 2) - halfScreenWidth,
                    (rootSprite.rect.height / 2) - halfScreenHeight
                );

            return newClamp;
        }

        void Pan(Vector2 deltaPosition)
        {
            transform.position -= (
                cam.ScreenToWorldPoint(deltaPosition) - cam.ScreenToWorldPoint(Vector2.zero)
            );
        }

        void AddInertia()
        {
            if (panVelocity.magnitude < 0.02f)
            {
                panVelocity = Vector2.zero;
            }

            if (panVelocity != Vector2.zero)
            {
                panVelocity = Vector2.Lerp(panVelocity, Vector2.zero, velocityStep);
                transform.position += new Vector3(
                    -panVelocity.x / (500 * (1 / cam.orthographicSize)),
                    -panVelocity.y / (500 * (1 / cam.orthographicSize)),
                    0
                );
            }
        }

        void ClampCamera()
        {
            camDiffY = initialCamSize - cam.orthographicSize;

            float screenDiff = ((float)Screen.height) / ((float)Screen.width);

            camDiffX = camDiffY / screenDiff;

            if (shouldClamp)
            {
                transform.position = new Vector3(
                    Mathf.Clamp(transform.position.x, -clamp.x - camDiffX, clamp.x + camDiffX),
                    Mathf.Clamp(transform.position.y, -clamp.y - camDiffY, clamp.y + camDiffY),
                    transform.position.z
                );
            }

            if (shouldRebound && !isPanning)
            {
                isRebounding = true;
            }
        }

        void ResetPan(bool overtime = false)
        {
            if (shouldReset)
            {
                panVelocity = Vector2.zero;

                if (overtime)
                {
                    isResetting = true;
                }
                else
                {
                    transform.position = new Vector3(initialPos.x, initialPos.y, initialPos.z);
                }
            }
        }

        bool CheckOutOfUITouch(Vector2 touchPos)
        {
            Vector2 newUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
                root.panel,
                cam.ScreenToWorldPoint(touchPos),
                cam
            );

            var pickedElement = root.panel.Pick(newUIPos);

            if (pickedElement == null || uiToAccept.Contains(pickedElement.name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
