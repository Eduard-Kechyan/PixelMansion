using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class CameraMotion : MonoBehaviour
    {
        // Variables
        public float speed = 100f;

        private Vector3 desiredPos;
        private bool move = false;
        private bool useAltSpeed = false;
        private float altSpeed = -1f;

        // References
        private Camera cam;
        private CharMain charMain;

        void Start()
        {
            // Cache
            cam = Camera.main;
            charMain = CharMain.Instance;

            /*Glob.SetTimeout(() =>
            {
                MoveTo(charMain.transform.position);
            }, 2f);*/
        }

        void Update()
        {
            if (move)
            {
                speed = Mathf.SmoothStep(0, 100, speed);

                // TODO - Add easing
                transform.position = Vector3.MoveTowards(transform.position, desiredPos, (useAltSpeed ? altSpeed : speed) * Time.deltaTime);

                if (Vector2.Distance(transform.position, desiredPos) < 0.01f)
                {
                    transform.position = new Vector3(desiredPos.x, desiredPos.y, transform.position.z);

                    move = false;
                    useAltSpeed = false;
                }
            }
        }

        public void MoveTo(Vector2 pos, float newMotionSpeed = -1)
        {
            desiredPos = new Vector3(pos.x, pos.y, transform.position.z);
            move = true;

            if (newMotionSpeed > -1)
            {
                useAltSpeed = true;

                altSpeed = newMotionSpeed;
            }
            else
            {
                useAltSpeed = false;
            }
        }
    }
}