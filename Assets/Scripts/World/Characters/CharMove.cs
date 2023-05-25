using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharMove : MonoBehaviour
{
    // Variables
    public bool debug = false;
    [Condition("debug", true)]
    public bool useMouse = false;

    [Header("States")]
    [ReadOnly]
    public Dir direction;
    [ReadOnly]
    public int directionOrder;

    private Vector2 destinationPos;

    // Enums
    public enum Dir
    {
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft,
        Up,
        UpRight,
        Right
    };

    // References
    private CharOrderer charOrderer;
    private NavMeshAgent agent;
    private Animator animator;
    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        // Cache
        charOrderer = GetComponent<CharOrderer>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        cam = Camera.main;

        // Initialize the character nav mesh agent
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        // Initialize direction
        direction = Dir.DownRight;
        directionOrder = (int)direction;
    }

    // Update is called once per frame
    void Update()
    {
        MoveChar();

        if (agent.velocity.magnitude != 0)
        {
            float direction = CalcDirection(agent.velocity.normalized);

            animator.SetFloat("Direction", direction);

            animator.SetBool("Walking", true);

            charOrderer.CheckArea();
        }
        else
        {
            animator.SetBool("Walking", false);
        }

        GetTouchPos();

        // Debug
#if UNITY_EDITOR
        GetMousePos();
#endif
    }

    void MoveChar()
    {
        agent.SetDestination(new Vector3(destinationPos.x, destinationPos.y, transform.position.z));

        agent.velocity = agent.desiredVelocity;
    }

    public void SetDestination(Vector2 newPos)
    {
        destinationPos = newPos;
    }

    // Calculate the direction/angle the character is facing
    int CalcDirection(Vector2 currentDir)
    {
        float x = currentDir.x;
        float y = currentDir.y;

        float angle = Vector3.Angle(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(x, y, 0.0f));

        if (x < 0.0f)
        {
            angle = 360 - angle;
        }

        if (Between(angle, 0f, 22.5f) || Between(angle, 337.5f, 360f))
        {
            // Up
            direction = Dir.Up;
        }

        if (Between(angle, 22.5f, 67.5f))
        {
            // Up Right
            direction = Dir.UpRight;
        }

        if (Between(angle, 67.5f, 112.5f))
        {
            // Right
            direction = Dir.Right;
        }

        if (Between(angle, 112.5f, 157.5f))
        {
            // Down Right
            direction = Dir.DownRight;
        }

        if (Between(angle, 157.5f, 202.5f))
        {
            // Down
            direction = Dir.Down;
        }

        if (Between(angle, 202.5f, 247.5f))
        {
            // Down Left
            direction = Dir.DownLeft;
        }

        if (Between(angle, 247.5f, 292.5f))
        {
            // Left
            direction = Dir.Left;
        }

        if (Between(angle, 292.5f, 337.5f))
        {
            // Up Left
            direction = Dir.UpLeft;
        }

        directionOrder = (int)direction;

        return directionOrder;
    }

    // Check if the value is between two variables
    // NOTE - The min value is included and the max value is excluded
    bool Between(float value, float min, float max)
    {
        if (value >= min && value < max)
        {
            return true;
        }

        return false;
    }

    void GetTouchPos()
    {
        if (debug && !useMouse && Input.touchCount == 1)
        {
            Vector2 touchPos = cam.ScreenToWorldPoint(Input.GetTouch(0).position);

            SetDestination(touchPos);
        }
    }

    //// DEBUG ////
#if UNITY_EDITOR
    void GetMousePos()
    {
        if (debug & useMouse && Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

            SetDestination(mousePos);
        }
    }
#endif
}
