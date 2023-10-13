using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class CameraMotion : MonoBehaviour
    {
        // Variables
        public float speed = 100f;
        [ReadOnly]
        public bool moving = false;

        private Vector3 desiredPos;
        private bool useAltSpeed = false;
        private float altSpeed = -1f;

        // References
        private HubUI hubUI;

        void Start()
        {
            // Cache
            hubUI = GameRefs.Instance.hubUI;

            enabled = false;
        }

        void Update()
        {
            if (moving)
            {
                speed = Mathf.SmoothStep(0, 100, speed);

                // TODO - Add easing
                transform.position = Vector3.MoveTowards(transform.position, desiredPos, (useAltSpeed ? altSpeed : speed) * Time.deltaTime);

                if (Vector2.Distance(transform.position, desiredPos) < 0.01f)
                {
                    transform.position = new Vector3(desiredPos.x, desiredPos.y, transform.position.z);

                    moving = false;
                    useAltSpeed = false;
                    
                    PlayerPrefs.SetFloat("lastCamPosX", transform.position.x);
                    PlayerPrefs.SetFloat("lastCamPosY", transform.position.y);

                    hubUI.SetUIButtons();

                    enabled = false;
                }
            }
        }

        public void MoveTo(Vector2 pos, float newMotionSpeed = -1)
        {
            desiredPos = new Vector3(pos.x, pos.y, transform.position.z);
            moving = true;

            if (newMotionSpeed > -1)
            {
                useAltSpeed = true;

                altSpeed = newMotionSpeed;
            }
            else
            {
                useAltSpeed = false;
            }

            enabled = true;
        }
    }
}