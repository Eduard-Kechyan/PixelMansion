using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class EarthMaster : MonoBehaviour
    {
        //  Variables
        [Header("Earth")]
        public GameObject earth;
        public float earthRotatingSpeed = 0.01f;
        public Gradient earthGradient;
        public bool rotateCamera = true;

        [Header("Sky")]
        public GameObject skyDay;
        public GameObject skyNight;
        public GameObject skyTop;
        public GameObject skyBottom;
        public Gradient skyGradientDay;
        public Gradient skyGradientNight;
        public Gradient skyGradientTop;
        public Gradient skyGradientBottom;
        public float scrollSpeed = 0.2f;

        private SpriteRenderer earthRenderer;

        private SpriteRenderer skyDayARenderer;
        private SpriteRenderer skyDayBRenderer;
        private SpriteRenderer skyNightARenderer;
        private SpriteRenderer skyNightBRenderer;
        private SpriteRenderer skyTopRenderer;
        private SpriteRenderer skyBottomRenderer;

        private float skyDayOffsetLimit;
        private float skyNightOffsetLimit;

        [HideInInspector]
        public float mainRotation = 0f;
        private float singleClick;
        private float rotationCurrent;

        // References
        private EarthOptions earthOptions;
        private Camera cam;

        void Start()
        {
            // Cache
            earthOptions = GetComponent<EarthOptions>();
            cam = Camera.main;

            // Initialize
            earthRenderer = earth.GetComponent<SpriteRenderer>();

            skyDayARenderer = skyDay.transform.GetChild(0).GetComponent<SpriteRenderer>();
            skyDayBRenderer = skyDay.transform.GetChild(1).GetComponent<SpriteRenderer>();
            skyNightARenderer = skyNight.transform.GetChild(0).GetComponent<SpriteRenderer>();
            skyNightBRenderer = skyNight.transform.GetChild(1).GetComponent<SpriteRenderer>();
            skyTopRenderer = skyTop.GetComponent<SpriteRenderer>();
            skyBottomRenderer = skyBottom.GetComponent<SpriteRenderer>();

            singleClick = 1f / 360;

            skyDayOffsetLimit = -Mathf.Abs(skyDay.transform.GetChild(1).transform.position.x);
            skyNightOffsetLimit = -Mathf.Abs(skyNight.transform.GetChild(1).transform.position.x);
        }

        void Update()
        {
            CalcRotationAndTimeOfDay();
        }

        void CalcRotationAndTimeOfDay()
        {
            float speed;

            // Calc rotation
            if (earthOptions.rotationSpeed == EarthOptions.RotationSpeed.Normal)
            {
                speed = earthRotatingSpeed;
            }
            else if (earthOptions.rotationSpeed == EarthOptions.RotationSpeed.Slow)
            {
                speed = earthRotatingSpeed / 4;
            }
            else // Fast
            {
                speed = earthRotatingSpeed * 4;
            }

            if (mainRotation >= 360)
            {
                mainRotation = 0;
            }
            else
            {
                mainRotation += speed * Time.deltaTime;
            }

            rotationCurrent = singleClick * mainRotation;

            // Calc time of day
            if (mainRotation >= 299 && mainRotation <= 305)
            {
                earthOptions.ShowTimeOfDay("Day");
            }
            else if (mainRotation >= 47 && mainRotation <= 53)
            {
                earthOptions.ShowTimeOfDay("Dusk");
            }
            else if (mainRotation >= 133 && mainRotation <= 139)
            {
                earthOptions.ShowTimeOfDay("Night");
            }
            else if (mainRotation >= 211 && mainRotation <= 217)
            {
                earthOptions.ShowTimeOfDay("Dawn");
            }

            SetSky();

            SetEarth();
        }

        void SetSky()
        {
            skyDayARenderer.color = skyGradientDay.Evaluate(rotationCurrent);
            skyDayBRenderer.color = skyGradientDay.Evaluate(rotationCurrent);
            skyNightARenderer.color = skyGradientNight.Evaluate(rotationCurrent);
            skyNightBRenderer.color = skyGradientNight.Evaluate(rotationCurrent);
            skyTopRenderer.color = skyGradientTop.Evaluate(rotationCurrent);
            skyBottomRenderer.color = skyGradientBottom.Evaluate(rotationCurrent);

            float speed;

            // Calc rotation
            if (earthOptions.rotationSpeed == EarthOptions.RotationSpeed.Normal)
            {
                speed = scrollSpeed;
            }
            else if (earthOptions.rotationSpeed == EarthOptions.RotationSpeed.Slow)
            {
                speed = scrollSpeed / 4;
            }
            else // Fast
            {
                speed = scrollSpeed * 4;
            }

            if (skyDay.transform.position.x <= skyDayOffsetLimit)
            {
                skyDay.transform.position = new Vector3(0, skyDay.transform.position.y, skyDay.transform.position.z);
            }
            else
            {
                skyDay.transform.position = new Vector3(skyDay.transform.position.x - speed * Time.deltaTime, skyDay.transform.position.y, skyDay.transform.position.z);
            }

            if (skyNight.transform.position.x <= skyNightOffsetLimit)
            {
                skyNight.transform.position = new Vector3(0, skyNight.transform.position.y, skyNight.transform.position.z);
            }
            else
            {
                skyNight.transform.position = new Vector3(skyNight.transform.position.x - speed * Time.deltaTime, skyNight.transform.position.y, skyNight.transform.position.z);
            }
        }

        void SetEarth()
        {
            if (rotateCamera)
            {
                cam.transform.eulerAngles = Vector3.forward * mainRotation;
            }

            earth.transform.eulerAngles = Vector3.forward * (360 - mainRotation);

            earthRenderer.color = earthGradient.Evaluate(rotationCurrent);
        }
    }
}
