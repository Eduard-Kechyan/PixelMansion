using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class CameraPinch : MonoBehaviour
    {
        // Variables
        public bool canPinch = true;

        [Header("Clamp")]
        public bool shouldClamp = true;
        public float minClamp = 130f;
        public float maxClamp = 420f;

        [Header("Rebound")]
        public bool shouldRebound = true;
        public float rebound = 20f;
        public float reboundSpeed = 100f;

        [Header("Reset")]
        public bool shouldReset = true;
        public float resetMinSpeed = 300f;
        public float resetMaxSpeed = 500f;

        [Header("Mouse debug")]
        public float scrollSpeed = 1.1f;
        public float clickDelay = 0.5f;
        [ReadOnly]
        [SerializeField]
        private bool canUseMouse = false;

        [Header("States")]
        [ReadOnly]
        public bool isPinching = false;
        [ReadOnly]
        public bool isResetting = false;
        [ReadOnly]
        public bool isRebounding = false;

        private float initialCamSize = 0;
        private float initialCamSizeRebound = 0;
        private bool lastCanPinch = true;
        private bool beganOutOfUI = true;

        private float clicked = 0;
        private float clickTime = 0;

        // References
        private Camera cam;
        private MenuUI menuUI;
        private CameraPan cameraPan;

        // UI
        private VisualElement root;

        void Start()
        {
            // Cache
            cam = Camera.main;
            menuUI = GameRefs.Instance.menuUI;
            cameraPan = GetComponent<CameraPan>();

            // UI
            root = GameRefs.Instance.hubUIDoc.rootVisualElement;

            // Get the camera's initial orthographic size
            initialCamSize = CalcInitialCamSize();

            cameraPan.initialCamSize = initialCamSize;

            // Check if we even can use the mouse/scroll wheel
            canUseMouse = Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer && Input.mousePresent;
        }

        void Update()
        {
            // Check if pinching is enabled
            if (canPinch && !menuUI.menuOpen)
            {
                // Get two finger pinch
                if (canUseMouse)
                {
                    // Get mouse wheel scroll
                    if (Input.mouseScrollDelta.y != 0)
                    {
                        Pinch(Input.mousePosition, 1, Input.mouseScrollDelta.y < 0 ? (1 / scrollSpeed) : scrollSpeed);

                        isPinching = true;
                    }
                    else
                    {
                        if (isPinching)
                        {
                            if (shouldClamp && shouldRebound)
                            {
                                CalcReboundSize();
                            }
                        }

                        isPinching = false;
                    }

                    // Check for double click
                    if (DoubleClicked() && shouldReset)
                    {
                        isResetting = true;
                    }
                }
                else
                {
                    if (Input.touchCount == 2)
                    {
                        Touch touch1 = Input.touches[0];
                        Touch touch2 = Input.touches[1];

                        // Check if we are touching out of the UI
                        if (touch1.phase == TouchPhase.Began && touch2.phase == TouchPhase.Began)
                        {
                            beganOutOfUI = CheckOutOfUITouch(touch1.position, touch2.position);
                        }

                        if (beganOutOfUI)
                        {
                            float previousDistance = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);

                            float currentDistance = Vector2.Distance(touch1.position, touch2.position);

                            if (previousDistance != currentDistance)
                            {
                                Pinch((touch1.position + touch2.position) / 2, previousDistance, currentDistance);

                                isPinching = true;
                            }
                            else
                            {
                                isPinching = false;
                            }

                            if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended)
                            {
                                if (shouldClamp && shouldRebound)
                                {
                                    CalcReboundSize();
                                }
                            }
                        }
                    }
                    else
                    {
                        isPinching = false;
                    }

                    // Check for double tap
                    if (Input.touchCount == 1 && Input.GetTouch(0).tapCount == 2 && shouldReset)
                    {
                        isResetting = true;
                    }
                }
            }

            // Check for rebounding
            if (isRebounding && !isPinching)
            {
                Vector2 prevSize = new(cam.orthographicSize, cam.orthographicSize);
                Vector2 initialSize = new(initialCamSizeRebound, initialCamSizeRebound);

                Vector2 newSize = Vector2.MoveTowards(prevSize, initialSize, reboundSpeed * Time.deltaTime);

                cam.orthographicSize = newSize.x;

                if (cam.orthographicSize == initialSize.x)
                {
                    isRebounding = false;
                }
            }

            // Check for resetting
            if (isResetting && !isPinching)
            {
                float speed = resetMinSpeed;
                Vector2 prevSize = new(cam.orthographicSize, cam.orthographicSize);
                Vector2 initialSize = new(initialCamSize, initialCamSize);

                if (cam.orthographicSize > initialCamSize)
                {
                    speed = resetMaxSpeed;
                }

                Vector2 newSize = Vector2.MoveTowards(prevSize, initialSize, speed * Time.deltaTime);

                cam.orthographicSize = newSize.x;

                if (cam.orthographicSize == initialCamSize)
                {
                    isResetting = false;
                }
            }
        }

        float CalcInitialCamSize()
        {
            float singlePixelWidth = Screen.width / GameData.GAME_PIXEL_WIDTH;

            float halfScreenHeight = (Screen.height / singlePixelWidth) / 2;

            cam.orthographicSize = halfScreenHeight;

            return halfScreenHeight;
        }

        void Pinch(Vector2 center, float oldDistance, float newDistance)
        {
            var currentPinchPosition = cam.ScreenToWorldPoint(center);

            cam.orthographicSize = Mathf.Max(0.1f, cam.orthographicSize * oldDistance / newDistance);

            if (shouldClamp)
            {
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minClamp, maxClamp);
            }

            var newPinchPosition = cam.ScreenToWorldPoint(center);

            transform.position -= newPinchPosition - currentPinchPosition;
        }

        void CalcReboundSize()
        {
            if (cam.orthographicSize > (maxClamp - rebound))
            {
                initialCamSizeRebound = maxClamp - rebound;

                isRebounding = true;
            }
            else if (cam.orthographicSize < (minClamp + rebound))
            {
                initialCamSizeRebound = minClamp + rebound;

                isRebounding = true;
            }
        }

        bool DoubleClicked()
        {
            if (Input.GetMouseButtonDown(0))
            {
                clicked++;

                if (clicked == 1)
                {
                    clickTime = Time.time;
                }
            }

            if (clicked > 1 && Time.time - clickTime < clickDelay)
            {
                clicked = 0;
                clickTime = 0;
                return true;
            }
            else if (clicked > 2 || Time.time - clickTime > 1)
            {
                clicked = 0;
            }

            return false;
        }

        public void EnablePinch()
        {
            if (!canPinch && lastCanPinch)
            {
                canPinch = true;
                lastCanPinch = false;
            }
        }

        public void DisablePinch()
        {
            if (canPinch)
            {
                canPinch = false;
                lastCanPinch = true;
            }
        }

        bool CheckOutOfUITouch(Vector2 touchPos1, Vector2 touchPos2)
        {
            Vector2 newUIPos1 = RuntimePanelUtils.CameraTransformWorldToPanel(
                root.panel,
                cam.ScreenToWorldPoint(touchPos1),
                cam
            );

            Vector2 newUIPos2 = RuntimePanelUtils.CameraTransformWorldToPanel(
                root.panel,
                cam.ScreenToWorldPoint(touchPos2),
                cam
            );

            var pickedElement1 = root.panel.Pick(newUIPos1);
            var pickedElement2 = root.panel.Pick(newUIPos2);

            if (pickedElement1 == null && pickedElement2 == null)
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