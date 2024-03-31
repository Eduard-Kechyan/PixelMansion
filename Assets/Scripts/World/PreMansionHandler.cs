using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class PreMansionHandler : MonoBehaviour
    {
        // Variables
        public RoomHandler firstRoom;
        public SpriteRenderer stairs;
        [SortingLayer]
        public string stairsSortingLayer;
        public float finishDelay = 0.2f;
        public float moveIntoViewSpeed = 100f;
        public float scaleToViewSpeed = 2f;
        public float scaleToViewSize = 290f;
        public Vector2 moveIntoViewOffset;

        [Header("Mansion Parts")]
        public Transform[] mansionParts;
        public float firstScaleSpeed = 3.5f;
        public float firstTargetScale = 1.2f;
        public float secondScaleSpeed = 8f;
        public float secondTargetScale = 0f;

        [Header("Clouds")]
        public Transform clouds;
        public Color cloudColor;
        public float cloudOffset = 40f;
        public float cloudTime = 1.7f;
        public float cloudTimeOffset = 0.5f;

        [Header("Debug")]
        public bool remove = false;
        public bool dontDestroyAtStart = false;

        private Vector3 roomCenter;

        private bool partsRemoved = false;
        private bool cloudsRemoved = false;

        void Start()
        {
            // Destroy the mansion at the start of the game if we have already unlocked it
            if (PlayerPrefs.HasKey("PreMansionRemoved"))
            {
                if (!dontDestroyAtStart || !Debug.isDebugBuild)
                {
                    Destroy(gameObject);
                }
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (remove)
            {
                remove = false;

                Remove();
            }
        }
#endif

        // Remove the pre mansion
        public void Remove(Action callback = null)
        {
            // Set mansion entrance stairs sorting layer
            stairs.sortingLayerName = stairsSortingLayer;

            firstRoom.MoveRoomIntoView(moveIntoViewSpeed, scaleToViewSpeed, scaleToViewSize, moveIntoViewOffset, () =>
            {
                HandlePartsRemoval(() =>
                {
                    // Unlock the first room that the player enters
                    firstRoom.Unlock((Vector3 newRoomCenter) =>
                    {
                        roomCenter = newRoomCenter;

                        // Save state
                        PlayerPrefs.SetInt("PreMansionRemoved", 1);

                        // Call the callback
                        callback?.Invoke();

                        // Destroy at the end
                        Destroy(gameObject);
                    });
                });
            });
        }

        // Remove all clouds and mansion parts
        void HandlePartsRemoval(Action callback)
        {
            int cloudsCount = clouds.childCount;
            int mansionPartsLength = mansionParts.Length;

            // Remove clouds
            for (int i = 0; i < cloudsCount; i++)
            {
                // Get the cloud child object
                Transform child = clouds.GetChild(i);

                if (child.name.Contains("Cloud"))
                {
                    // Determine if this is the last cloud
                    bool last = i == cloudsCount - 1;

                    StartCoroutine(MoveCloud(child, last));
                }
            }

            // Remove mansion parts
            for (int i = 0; i < mansionPartsLength; i++)
            {
                for (int j = 0; j < mansionParts[i].childCount; j++)
                {
                    // Determine if this is the last mansion part
                    bool last = i == mansionPartsLength - 1 && j == mansionParts[i].childCount - 1;

                    StartCoroutine(RemovePart(mansionParts[i].GetChild(j), last));
                }
            }

            StartCoroutine(WaitForPartsAndClouds(callback));
        }

        // Move the clouds to the left or the right depending on the center of the room.
        // Also make them transparent little by little until they are invisible
        IEnumerator MoveCloud(Transform cloud, bool last = false)
        {
            Vector3 startingPos = cloud.transform.position;

            float finalXPos = cloud.transform.position.x + (cloud.transform.position.x > roomCenter.x ? cloudOffset : -cloudOffset);

            float elapsedTime = 0;

            float newCloudTime = cloudTime + (UnityEngine.Random.value >= 0.5 ? cloudTimeOffset : -cloudTimeOffset);

            SpriteRenderer renderer = cloud.GetComponent<SpriteRenderer>();

            Color initialColor = renderer.color;

            while (elapsedTime < newCloudTime)
            {
                float time = elapsedTime / newCloudTime;

                cloud.transform.position = Vector3.Lerp(startingPos, new Vector3(finalXPos, cloud.transform.position.y, cloud.transform.position.z), time);

                renderer.color = Color.Lerp(initialColor, cloudColor, time);

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            renderer.sortingLayerName = "Default";

            // Check if this is the last cloud
            if (last)
            {
                cloudsRemoved = true;
            }
        }

        // Make the mansion part disappear
        IEnumerator RemovePart(Transform part, bool last = false)
        {
            // Add a random delay before starting the removal
            float randomDelay = UnityEngine.Random.Range(0f, 2f);

            yield return new WaitForSeconds(randomDelay);

            // Increase the part scale slightly
            Vector3 newFirstTargetScale = new Vector3(firstTargetScale, firstTargetScale, firstTargetScale);

            while (part.localScale != newFirstTargetScale)
            {
                part.localScale = Vector3.MoveTowards(
                    part.localScale,
                    newFirstTargetScale,
                    firstScaleSpeed * Time.deltaTime
                );

                yield return null;
            }

            // Increase the part scale to 0
            Vector3 newSecondTargetScale = new Vector3(secondTargetScale, secondTargetScale, secondTargetScale);

            while (part.localScale != newSecondTargetScale)
            {
                part.localScale = Vector3.MoveTowards(
                    part.localScale,
                    newSecondTargetScale,
                    firstScaleSpeed * Time.deltaTime
                );

                yield return null;
            }

            // Check if this is the last part
            if (last)
            {
                yield return new WaitForSeconds(finishDelay);

                partsRemoved = true;
            }
        }

        // Wait until all clouds and mansion parts have been removed
        IEnumerator WaitForPartsAndClouds(Action callback)
        {
            while (!partsRemoved || !cloudsRemoved)
            {
                yield return null;
            }

            callback();
        }
    }
}
