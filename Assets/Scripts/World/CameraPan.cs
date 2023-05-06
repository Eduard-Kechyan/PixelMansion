using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPan : MonoBehaviour
{
    // Variables
    public Selector selector;
    
    public bool canPan = true;
    public float touchThreshold = 20f;

    public float slideSpeed = 10f;
    public bool isSliding = false;
    private Vector2 slideDirection;
    public float slideDistance;

    [Header("Clamp")]
    public bool shouldClamp = true;
    public float clampMinX = -150;
    public float clampMaxX = 150;
    public float clampMinY = -150;
    public float clampMaxY = 150;

    [Header("Rebound")]
    public bool shouldRebound = true;
    public float rebound = 10f;
    public float reboundSpeed = 1f;

    [Header("Reset debug")]
    public bool shouldReset = true;
    public float resetSpeed = 10f;

    [Header("States")]
    [ReadOnly]
    public bool isPanning = false;
    [ReadOnly]
    public bool isRebounding = false;
    [ReadOnly]
    public bool isReseting = false;

    private Vector2 touchPosition { get { return lastTouchPos; } }
    private Vector3 initialPos;
    private Vector2 initialTouchPos;
    private Vector2 lastTouchPos;
    private float touchStartTime;
    private Vector2 reboundPos;
    private Vector2 deltaPos;

    // References
    private Camera cam;
    private Rigidbody2D rigidBody;
    private MenuUI menuUI;

    void Start()
    {
        // Cache
        cam = Camera.main;
        rigidBody = GetComponent<Rigidbody2D>();
        menuUI = GameRefs.Instance.menuUI;

        // Set the camera's initial position
        initialPos = new Vector3(
            transform.position.x,
            transform.position.y,
            transform.position.z
        );

        clampMinX = -clampMinX;
        clampMinY = -clampMinY;
    }

    void Update()
    {
        // Check if panning is enabled
        if (canPan && !menuUI.menuOpen)
        {
            if (Input.touchCount == 1) // Pan
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        initialTouchPos = touch.position;
                        touchStartTime = Time.time;
                        lastTouchPos = initialTouchPos;

                        isPanning = false;
                        isReseting = false;
                        isRebounding = false;

                        rigidBody.velocity = Vector2.zero;
                        rigidBody.Sleep();

                        break;

                    case TouchPhase.Moved:
                        lastTouchPos = touch.position;

                        // Compare the current position to the initial position
                        Vector2 diffMoved = touch.position - initialTouchPos;

                        if (
                            diffMoved.x > touchThreshold
                            || diffMoved.x < -touchThreshold
                            || diffMoved.y > touchThreshold
                            || diffMoved.y < -touchThreshold
                        )
                        {
                            isPanning = true;

                            if (touch.deltaPosition != Vector2.zero)
                            {
                                deltaPos = touch.deltaPosition;
                                Pan(deltaPos);
                            }
                        }

                        break;

                    case TouchPhase.Ended:
                        isPanning = false;

                        Slide();

                        ResetPan(true);

                        if (shouldRebound && !isPanning)
                        {
                            isRebounding = true;
                        }

                        break;

                    case TouchPhase.Stationary:
                        // Compare the current position to the initial position
                        Vector2 diffStationary = touch.position - initialTouchPos;

                        if (diffStationary.x < touchThreshold
                            || diffStationary.x > -touchThreshold
                            || diffStationary.y < touchThreshold
                            || diffStationary.y > -touchThreshold
                        )
                        {
                            if (Time.time - touchStartTime >= selector.tapDuration && !isPanning)
                            {
                                selector.Select(touch.position);
                            }
                        }

                        break;
                }
            }
        }

        // Check for rebounding
        if (isRebounding && !isPanning)
        {
            Vector3 clampedPos = new Vector3(
                Mathf.Clamp(transform.position.x, clampMinX + rebound, clampMaxX - rebound),
                Mathf.Clamp(transform.position.y, clampMinY + rebound, clampMaxY - rebound),
                transform.position.z);

            transform.position = Vector3.MoveTowards(transform.position, clampedPos, reboundSpeed * Time.deltaTime);

            if (transform.position.x == clampedPos.x && transform.position.y == clampedPos.y)
            {
                isRebounding = false;
            }
        }

        // Check for reseting
        if (isReseting && !isPanning)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                initialPos,
                resetSpeed * Time.deltaTime
            );

            if (transform.position.x == initialPos.x && transform.position.y == initialPos.y)
            {
                isReseting = false;
            }
        }

        if (isSliding && !isPanning)
        {
            /*transform.position = Vector3.MoveTowards(
                transform.position,
                initialPos,
                resetSpeed * Time.deltaTime
            );*/

            //rigidBody.AddForce(slideDirection);
            /* if (transform.position.x == initialPos.x && transform.position.y == initialPos.y)
             {
                 isReseting = false;
             }*/
        }
    }

    void Slide()
    {
        slideDirection = (cam.ScreenToWorldPoint(initialTouchPos) - cam.ScreenToWorldPoint(deltaPos));
        slideDistance = Vector2.Distance(cam.ScreenToWorldPoint(initialTouchPos), cam.ScreenToWorldPoint(deltaPos));

        isSliding = true;
    }

    void LateUpdate()
    {
        ClampCamera();
    }

    public void Pan(Vector2 deltaPosition)
    {
        transform.position -= (cam.ScreenToWorldPoint(deltaPosition) - cam.ScreenToWorldPoint(Vector2.zero));
    }

    void ClampCamera()
    {
        if (shouldClamp)
        {
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, clampMinX, clampMaxX),
                Mathf.Clamp(transform.position.y, clampMinY, clampMaxY),
                transform.position.z
            );
        }
    }

    void ResetPan(bool overtime = false)
    {
        if (shouldReset)
        {
            rigidBody.velocity = Vector2.zero;
            rigidBody.Sleep();

            if (overtime)
            {
                isReseting = true;
            }
            else
            {
                transform.position = new Vector3(
                    initialPos.x,
                    initialPos.y,
                    initialPos.z
                );
            }
        }
    }
}
