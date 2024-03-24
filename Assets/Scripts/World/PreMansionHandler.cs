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
            if (PlayerPrefs.HasKey("PreMansionRemoved"))
            {
                Remove();
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (remove)
            {
                remove = false;

                Remove(false);
            }
        }
#endif

        public void Remove(bool destroy = true)
        {
            // Set stairs sorting layer
            stairs.sortingLayerName = stairsSortingLayer;

            if (destroy)
            {
                if (!dontDestroyAtStart || !Application.isEditor)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
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

                            // Destroy at the end
                            Destroy(gameObject);
                        });
                    });
                });
            }
        }

        void HandlePartsRemoval(Action callback)
        {
            // Remove clouds
            for (int i = 0; i < clouds.childCount; i++)
            {
                Transform child = clouds.GetChild(i);

                if (child.name.Contains("Cloud"))
                {
                    bool last = i == clouds.childCount - 1;

                    StartCoroutine(MoveCloud(child, last));
                }
            }

            // Remove parts
            for (int i = 0; i < mansionParts.Length; i++)
            {
                for (int j = 0; j < mansionParts[i].childCount; j++)
                {
                    bool last = i == mansionParts.Length - 1 && j == mansionParts[i].childCount - 1;

                    if (last)
                    {
                        Debug.Log(mansionParts[i].GetChild(j).name);
                    }

                    StartCoroutine(RemovePart(mansionParts[i].GetChild(j), last));
                }
            }

            // Wait for removal
            StartCoroutine(WaitForPartsAndClouds(callback));
        }

        IEnumerator RemovePart(Transform part, bool last = false)
        {
            //// START

            float randomDelay = UnityEngine.Random.Range(0f, 2f);

            yield return new WaitForSeconds(randomDelay);

            //// FIRST

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

            //// SECOND

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

            //// END

            if (last)
            {
                yield return new WaitForSeconds(finishDelay);

                partsRemoved = true;
            }
        }

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

            if (last)
            {
                cloudsRemoved = true;
            }
        }

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
