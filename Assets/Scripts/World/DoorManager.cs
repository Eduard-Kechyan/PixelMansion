using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class DoorManager : MonoBehaviour
    {
        // Variables
        public float doorOffset = 40;
        [HideInInspector]
        public DoorPH[] doors;

        private List<string> unlockedDoors = new();

        // References
        public WorldDataManager worldDataManager;

        // Instance
        public static DoorManager Instance;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            // Cache
            worldDataManager = GameRefs.Instance.worldDataManager;

            // Init
            StartCoroutine(WaitForWorldData());
        }

        IEnumerator WaitForWorldData()
        {
            while (!worldDataManager.loaded)
            {
                yield return null;
            }

            if (PlayerPrefs.HasKey("doorSet"))
            {
                unlockedDoors = worldDataManager.LoadDoors();

                GetDoors();
            }
            else
            {
                GetDoors();
            }
        }

        public void GetDoors()
        {
            doors = new DoorPH[transform.childCount];

            for (int i = 0; i < transform.childCount; i++)
            {
                doors[i] = transform.GetChild(i).GetComponent<DoorPH>();

                for (int j = 0; j < unlockedDoors.Count; j++)
                {
                    if (unlockedDoors[j] == doors[i].roomSortingLayer)
                    {
                        doors[i].gameObject.SetActive(false);

                        break;
                    }
                }
            }
        }

        public void OpenDoor(string roomSortingLayer, Action<Vector2> callback = null)
        {
            Vector2 foundDoorPosition = Vector2.zero;

            bool foundDoor = false;
            bool foundRoomSortingLayer = false;

            for (int i = 0; i < doors.Length; i++)
            {
                if (doors[i].roomSortingLayer == roomSortingLayer)
                {
                    foundDoorPosition = doors[i].transform.position;

                    doors[i].gameObject.SetActive(false);

                    foundDoor = true;

                    break;
                }
            }

            if (foundDoor)
            {
                for (int i = 0; i < unlockedDoors.Count; i++)
                {
                    if (unlockedDoors[i] == roomSortingLayer)
                    {
                        foundRoomSortingLayer = true;

                        break;
                    }
                }

                if (!foundRoomSortingLayer)
                {
                    unlockedDoors.Add(roomSortingLayer);

                    worldDataManager.SaveDoors(unlockedDoors);
                }
            }

            callback?.Invoke(new Vector2(foundDoorPosition.x, foundDoorPosition.y - doorOffset));
        }

        public void GetPosition(string roomSortingLayer, Action<Vector2> callback)
        {
            Vector2 foundDoorPosition = Vector2.zero;

            for (int i = 0; i < doors.Length; i++)
            {
                if (doors[i].roomSortingLayer == roomSortingLayer)
                {
                    foundDoorPosition = doors[i].transform.position;

                    break;
                }
            }

            callback?.Invoke(new Vector2(foundDoorPosition.x, foundDoorPosition.y - doorOffset));
        }
    }
}