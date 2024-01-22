using System;
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
        private bool useAltMoveSpeed = false;
        private float altMoveSpeed = -1f;

        private Action callBack;

        // References
        private HubUI hubUI;
        private CharMain charMain;

        void Start()
        {
            // Cache
            hubUI = GameRefs.Instance.hubUI;
            charMain= CharMain.Instance;

            enabled = false;
        }

        void Update()
        {
            if (moving)
            {
                speed = Mathf.SmoothStep(0, 100, speed);

                // TODO - Add easing
                transform.position = Vector3.MoveTowards(transform.position, desiredPos, (useAltMoveSpeed ? altMoveSpeed : speed) * Time.deltaTime);

                if (Vector2.Distance(transform.position, desiredPos) < 0.01f)
                {
                    transform.position = new Vector3(desiredPos.x, desiredPos.y, transform.position.z);

                    moving = false;
                    useAltMoveSpeed = false;

                    PlayerPrefs.SetFloat("lastCamPosX", transform.position.x);
                    PlayerPrefs.SetFloat("lastCamPosY", transform.position.y);

                    hubUI.SetUIButtons();

                    enabled = false;

                    callBack?.Invoke();
                }
            }
        }

        public void MoveTo(Vector2 pos, float newMotionSpeed = -1, Action newCallBack = null)
        {
            desiredPos = new Vector3(pos.x, pos.y, transform.position.z);
            moving = true;

            callBack = newCallBack;

            if (newMotionSpeed > -1)
            {
                useAltMoveSpeed = true;

                altMoveSpeed = newMotionSpeed;
            }
            else
            {
                useAltMoveSpeed = false;
            }

            enabled = true;
        }
    
        public void MoveToChar(float newMotionSpeed = 250, Action newCallBack = null)
        {
            MoveTo(charMain.transform.position, newMotionSpeed, newCallBack);
        }
    }
}