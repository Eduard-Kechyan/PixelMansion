using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Merge
{
    public class CharMove : MonoBehaviour
    {
        [Header("States")]
        [ReadOnly]
        public Dir direction;
        [ReadOnly]
        public int directionOrder;
        [ReadOnly]
        public bool isWalking = false;

        private Vector2 destinationPos;

        private bool canCheck = false;

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
        private CharOrderSetter charOrderSetter;
        private NavMeshAgent agent;
        private Animator animator;

        // Start is called before the first frame update
        void Start()
        {
            // Cache
            charOrderSetter = GetComponent<CharOrderSetter>();
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();

            // Initialize the character nav mesh agent
            agent.updateRotation = false;
            agent.updateUpAxis = false;

            // Initialize direction
            direction = Dir.DownRight;
            directionOrder = (int)direction;

            enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            MoveChar();

            CheckVelocity();
        }

        void MoveChar()
        {
            agent.SetDestination(new Vector3(destinationPos.x, destinationPos.y, transform.position.z));

            agent.velocity = agent.desiredVelocity;

            if (!canCheck)
            {
                Glob.SetTimeout(() =>
                {
                    canCheck = true;

                    enabled = true;
                }, 0.1f);
            }
        }

        public void SetDestination(Vector2 newPos, bool stayInRoom = false)
        {
            destinationPos = newPos;
            
            enabled = true;

            MoveChar();

            if (stayInRoom)
            {
                //Debug.Log("Staying in room!");
            }
        }

        void CheckVelocity()
        {
            if (canCheck)
            {
                if (agent.velocity.magnitude == 0)
                {
                    animator.SetBool("Walking", false);

                    enabled = false;

                    canCheck = false;

                    isWalking = false;

                    charOrderSetter.SetShadow();
                }
                else
                {
                    float direction = CalcDirection(agent.velocity.normalized);

                    animator.SetFloat("Direction", direction);

                    animator.SetBool("Walking", true);

                    charOrderSetter.CheckArea();

                    charOrderSetter.SetShadow(false);
                }
            }
        }

        public void SetPosition(Vector2 newPos)
        {
            agent.isStopped = true;

            transform.position = newPos;
        }

        public void StopMoving()
        {
            agent.isStopped = true;

            enabled = false;
        }

        public void ContinueMoving()
        {
            agent.isStopped = false;

            enabled = true;
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
    }
}