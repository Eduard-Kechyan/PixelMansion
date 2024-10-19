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

        private int loadedCount = 0;

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
                int loadingCount = 0;

                WallLeftPH wallLeftPH = transform.GetComponentInChildren<WallLeftPH>();
                WallRightPH wallRightPH = transform.GetComponentInChildren<WallRightPH>();
                FloorPH floorPH = transform.GetComponentInChildren<FloorPH>();
                FurniturePH furniturePH = transform.GetComponentInChildren<FurniturePH>();
                PropsPH propsPH = transform.GetComponentInChildren<PropsPH>();
                FilthPH filthPH = transform.GetComponentInChildren<FilthPH>();

                if (wallLeftPH != null)
                {
                    wallLeft = wallLeftPH.transform;

                    WaitForWallToLoad(wallLeftPH.GetComponent<ChangeWall>());

                    loadingCount++;
                }

                if (wallRightPH != null)
                {
                    wallRight = wallRightPH.transform;

                    WaitForWallToLoad(wallRightPH.GetComponent<ChangeWall>());

                    loadingCount++;
                }

                if (floorPH != null)
                {
                    floor = floorPH.transform;

                    WaitForFloorToLoad(floorPH.GetComponent<ChangeFloor>());

                    loadingCount++;
                }

                if (furniturePH != null)
                {
                    furniture = furniturePH.transform;

                    WaitForFurnitureToLoad(furniturePH.GetComponent<ChangeFurniture>());

                    loadingCount++;
                }

                if (propsPH != null)
                {
                    props = propsPH.transform;

                    WaitForPropToLoad(furniturePH.GetComponent<ChangeProp>());

                    loadingCount++;
                }

                if (filthPH != null)
                {
                    filth = filthPH.transform;

                    WaitForFilthToLoad(filthPH.GetComponent<RemoveFilth>());

                    loadingCount++;
                }

                referencesSet = true;

                callback?.Invoke();

                WaitForPartsToLoad(loadingCount, callback);
            }
            else
            {
                callback?.Invoke();
            }
        }

        IEnumerator WaitForPartsToLoad(int targetCount, Action callback)
        {
            while (targetCount < loadedCount)
            {
                yield return null;
            }

            loadedCount = 0;

            callback?.Invoke();
        }

        IEnumerator WaitForWallToLoad(ChangeWall changer)
        {
            while (changer.loaded)
            {
                yield return null;
            }

            loadedCount++;
        }

        IEnumerator WaitForFloorToLoad(ChangeFloor changer)
        {
            while (changer.loaded)
            {
                yield return null;
            }

            loadedCount++;
        }

        IEnumerator WaitForFurnitureToLoad(ChangeFurniture changer)
        {
            while (changer.loaded)
            {
                yield return null;
            }

            loadedCount++;
        }

        IEnumerator WaitForPropToLoad(ChangeProp changer)
        {
            while (changer.loaded)
            {
                yield return null;
            }

            loadedCount++;
        }

        IEnumerator WaitForFilthToLoad(RemoveFilth changer)
        {
            while (changer.loaded)
            {
                yield return null;
            }

            loadedCount++;
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
