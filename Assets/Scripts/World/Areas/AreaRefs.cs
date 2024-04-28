using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class AreaRefs : MonoBehaviour
    {
        // Variables
        [ReadOnly]
        public bool referencesSet = false;

        // References
        [HideInInspector]
        public LockedOverlayPH lockedOverlayPH;
        [HideInInspector]
        public NavPH navPH;

        [HideInInspector]
        public Transform wallLeft;
        [HideInInspector]
        public Transform wallRight;
        [HideInInspector]
        public Transform floor;
        [HideInInspector]
        public Transform furniture;
        [HideInInspector]
        public Transform props;
        [HideInInspector]
        public Transform filth;

        void Awake()
        {
            GetReferences();
        }

        public void GetReferences(Action callback = null)
        {
            if (!referencesSet)
            {
                WallLeftPH wallLeftPH = transform.GetComponentInChildren<WallLeftPH>();
                WallRightPH wallRightPH = transform.GetComponentInChildren<WallRightPH>();
                FloorPH floorPH = transform.GetComponentInChildren<FloorPH>();
                FurniturePH furniturePH = transform.GetComponentInChildren<FurniturePH>();
                PropsPH propsPH = transform.GetComponentInChildren<PropsPH>();
                FilthPH filthPH = transform.GetComponentInChildren<FilthPH>();

                if (wallLeftPH != null)
                {
                    wallLeft = wallLeftPH.transform;
                }

                if (wallRightPH != null)
                {
                    wallRight = wallRightPH.transform;
                }

                if (floorPH != null)
                {
                    floor = floorPH.transform;
                }

                if (furniturePH != null)
                {
                    furniture = furniturePH.transform;
                }

                if (propsPH != null)
                {
                    props = propsPH.transform;
                }

                if (filthPH != null)
                {
                    filth = filthPH.transform;
                }

                referencesSet = true;

                callback?.Invoke();
            }
            else
            {
                callback?.Invoke();
            }
        }

        public GameObject GetLockedOverlay()
        {
            if (lockedOverlayPH == null)
            {
                lockedOverlayPH = transform.GetComponentInChildren<LockedOverlayPH>();
            }

            if (lockedOverlayPH != null)
            {
                return lockedOverlayPH.gameObject;
            }

            return null;
        }

        public GameObject GetNav()
        {
            if (navPH == null)
            {
                navPH = transform.GetComponentInChildren<NavPH>();
            }

            if (navPH != null)
            {
                return navPH.gameObject;
            }

            return null;
        }
    }
}
