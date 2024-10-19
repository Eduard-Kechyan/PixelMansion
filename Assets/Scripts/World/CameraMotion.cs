using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class CameraMotion : MonoBehaviour
    {
        // Variables
        public float moveSpeed = 100f;
        public float scaleSpeed = 100f;

        [Header("Stats")]
        [ReadOnly]
        public bool moving = false;
        [ReadOnly]
        public bool scaling = false;
        [ReadOnly]
        public bool movingAndScaling = false;

        private Vector3 desiredPos;
        private float desiredScale;
        private bool useAltMoveSpeed = false;
        private float altMoveSpeed = -1f;
        private bool useAltScaleSpeed = false;
        private float altScaleSpeed = -1f;

        private Action callBack;

        // References
        private WorldUI worldUI;
        private CharMain charMain;
        private Camera cam;

        void Start()
        {
            // Cache
            worldUI = GameRefs.Instance.worldUI;
            charMain = CharMain.Instance;
            cam = Camera.main;

            enabled = false;
        }

        void Update()
        {
            if (moving)
            {
                moveSpeed = Mathf.SmoothStep(0, 100, moveSpeed);

                transform.position = Vector3.MoveTowards(transform.position, desiredPos, (useAltMoveSpeed ? altMoveSpeed : moveSpeed) * Time.deltaTime);

                if (Vector2.Distance(transform.position, desiredPos) < 0.01f)
                {
                    transform.position = new Vector3(desiredPos.x, desiredPos.y, transform.position.z);

                    moving = false;
                    useAltMoveSpeed = false;

                    PlayerPrefs.SetFloat("lastCamPosX", transform.position.x);
                    PlayerPrefs.SetFloat("lastCamPosY", transform.position.y);

                    worldUI.SetUIButtons();

                    enabled = false;

                    callBack?.Invoke();
                }
            }

            if (scaling)
            {
                scaleSpeed = Mathf.SmoothStep(0, 100, scaleSpeed);

                cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, desiredScale, (useAltScaleSpeed ? altScaleSpeed : scaleSpeed) * Time.deltaTime);

                if (Mathf.Abs(cam.orthographicSize - desiredScale) < 0.01f)
                {
                    cam.orthographicSize = desiredScale;

                    scaling = false;
                    useAltScaleSpeed = false;

                    enabled = false;

                    callBack?.Invoke();
                }
            }

            if (movingAndScaling)
            {
                moveSpeed = Mathf.SmoothStep(0, 100, moveSpeed);
                scaleSpeed = Mathf.SmoothStep(0, 100, scaleSpeed);

                transform.position = Vector3.MoveTowards(transform.position, desiredPos, (useAltMoveSpeed ? altMoveSpeed : moveSpeed) * Time.deltaTime);
                cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, desiredScale, (useAltScaleSpeed ? altScaleSpeed : scaleSpeed) * Time.deltaTime);

                if (Vector2.Distance(transform.position, desiredPos) < 0.01f && Mathf.Abs(cam.orthographicSize - desiredScale) < 0.01f)
                {
                    transform.position = new Vector3(desiredPos.x, desiredPos.y, transform.position.z);
                    cam.orthographicSize = desiredScale;

                    movingAndScaling = false;
                    useAltMoveSpeed = false;
                    useAltScaleSpeed = false;

                    PlayerPrefs.SetFloat("lastCamPosX", transform.position.x);
                    PlayerPrefs.SetFloat("lastCamPosY", transform.position.y);

                    worldUI.SetUIButtons();

                    enabled = false;

                    callBack?.Invoke();
                }
            }
        }

        public void MoveTo(Vector2 pos, float newMotionSpeed = -1, Action newCallBack = null)
        {
            callBack = newCallBack;

            desiredPos = new Vector3(pos.x, pos.y, transform.position.z);

            if (newMotionSpeed > -1)
            {
                useAltMoveSpeed = true;

                altMoveSpeed = newMotionSpeed;
            }
            else
            {
                useAltMoveSpeed = false;
            }

            moving = true;
            enabled = true;
        }

        public void ScaleTo(float newOrthographicSize = 0f, float newScaleSpeed = -1, Action newCallBack = null)
        {
            callBack = newCallBack;

            desiredScale = newOrthographicSize;

            if (newScaleSpeed > -1)
            {
                useAltScaleSpeed = true;

                altScaleSpeed = newScaleSpeed;
            }
            else
            {
                useAltScaleSpeed = false;
            }

            scaling = true;
            enabled = true;
        }

        public void MoveToAndScaleTo(Vector2 pos, float newMotionSpeed = -1, float newOrthographicSize = 0f, float newScaleSpeed = -1, Action newCallBack = null)
        {
            callBack = newCallBack;

            desiredPos = new Vector3(pos.x, pos.y, transform.position.z);
            desiredScale = newOrthographicSize;

            if (newMotionSpeed > -1)
            {
                useAltMoveSpeed = true;

                altMoveSpeed = newMotionSpeed;
            }
            else
            {
                useAltMoveSpeed = false;
            }

            if (newScaleSpeed > -1)
            {
                useAltScaleSpeed = true;

                altScaleSpeed = newScaleSpeed;
            }
            else
            {
                useAltScaleSpeed = false;
            }

            movingAndScaling = true;
            enabled = true;
        }

        public void MoveToChar(float newMotionSpeed = 250, Action newCallBack = null)
        {
            MoveTo(charMain.transform.position, newMotionSpeed, newCallBack);
        }
    }
}