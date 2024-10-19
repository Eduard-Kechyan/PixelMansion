using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    At the start of the game the exterior of the mansion is initially visible to the player.
    Only after completing the Garden area, will the mansion open up.
    This function is responsible for unlocking the mansion and the extra clouds next to.
    It also unlocks the living room and moves the main character inside.
    After unlocking the mansion and entering the World scene, the mansion will be automatically destroyed.
*/

namespace Merge
{
    public class PreMansionHandler : MonoBehaviour
    {
        // Variables
        [Header("Rooms")]
        public RoomHandler firstRoom;
        public PolygonCollider2D gardenPolygonCollider2D;
        [SortingLayer]
        public string gardenSortingLayer;

        [Header("Stairs")]
        public OrderSetter stairsOrderSetter;
        [SortingLayer]
        public string stairsSortingLayer;
        public int stairsSortingOrder = 12;

        [Header("Movement and Scale")]
        public float finishDelay = 0.2f;
        public float moveIntoViewSpeed = 100f;
        public float scaleToViewSpeed = 2f;
        public float scaleToViewSize = 290f;
        public Vector2 moveIntoViewOffset;

        [Header("Mansion Parts")]
        public float firstScaleSpeed = 5f;
        public float firstTargetScale = 1.2f;
        public float secondScaleSpeed = 9f;
        public float secondTargetScale = 0f;

        [Header("Clouds")]
        public Transform clouds;
        public Color cloudColor;
        public float cloudOffset = 40f;
        public float cloudTime = 1.7f;
        public float cloudTimeOffset = 0.5f;

        [Header("Debug")]
        public bool togglePreMansionSceneVisibility = false;
        public bool remove = false;
        public bool dontDestroyAtStart = false;

        private Vector3 roomCenter;
        private List<Transform> mansionParts = new();

        private bool partsRemoved = false;
        private bool cloudsRemoved = false;

        [Header("Stats")]
        [SerializeField]
        [ReadOnly]
        public bool preMansionHidden = false;

        // References
        private SpriteRenderer stairsSpriteRenderer;
        private CharMain charMain;
        private CloudSave cloudSave;
        private SoundManager soundManager;

        void Start()
        {
            // Cache
            stairsSpriteRenderer = stairsOrderSetter.transform.GetChild(0).GetComponent<SpriteRenderer>();
            charMain = CharMain.Instance;
            cloudSave = Services.Instance.GetComponent<CloudSave>();
            soundManager = SoundManager.Instance;

            if (!Debug.isDebugBuild)
            {
                dontDestroyAtStart = false;
            }

            // Destroy the mansion at the start of the game if we have already unlocked it
            if (PlayerPrefs.HasKey("preMansionRemoved"))
            {
                if (!dontDestroyAtStart)
                {
                    // Set the main character's post position
                    charMain.SetRoom(firstRoom.GetRoomCenter(), true);

                    // Destroy the pre mansion
                    Destroy(gameObject);
                }
            }
            else
            {
                // Set the main character's pre position
                charMain.SetRoom(gardenPolygonCollider2D.bounds.center);
            }

            GetMansionParts();

            if (preMansionHidden)
            {
                TogglePreMansionSceneVisibility();
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

            if (togglePreMansionSceneVisibility)
            {
                togglePreMansionSceneVisibility = false;

                Glob.Validate(() =>
                {
                    TogglePreMansionSceneVisibility();
                }, this);
            }
        }
#endif

        // Remove the pre mansion
        public void Remove(Action callback = null)
        {
            Glob.taskLoading = true;

            // Set mansion entrance stairs sorting layer
            stairsSpriteRenderer.sortingLayerName = stairsSortingLayer;
            stairsSpriteRenderer.sortingOrder = stairsSortingOrder;
            stairsOrderSetter.sortingLayer = stairsSortingLayer;

            firstRoom.MoveRoomIntoView(moveIntoViewSpeed, scaleToViewSpeed, scaleToViewSize, moveIntoViewOffset, (Vector3 newRoomCenter) =>
            {
                roomCenter = newRoomCenter;

                HandlePartsRemoval(() =>
                {
                    // Unlock the first room that the player enters
                    firstRoom.Unlock((Vector3 newRoomCenter) =>
                    {
                        // Save state
                        cloudSave.SaveDataAsync("preMansionRemoved", 1);

                        PlayerPrefs.SetInt("preMansionRemoved", 1);

                        // Call the callback
                        callback?.Invoke();

                        Glob.taskLoading = false;

                        // Destroy at the end
                        Destroy(gameObject);
                    }, 1f);
                });
            });
        }

        // Remove all clouds and mansion parts
        void HandlePartsRemoval(Action callback)
        {
            int cloudsCount = clouds.childCount;
            int mansionPartsCount = mansionParts.Count;

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
            for (int i = 0; i < mansionPartsCount; i++)
            {
                for (int j = 0; j < mansionParts[i].childCount; j++)
                {
                    // Determine if this is the last mansion part
                    bool last = i == mansionPartsCount - 1 && j == mansionParts[i].childCount - 1;

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

            if (finishDelay < randomDelay)
            {
                finishDelay = randomDelay + 0.1f;
            }

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

            soundManager.PlaySound(SoundManager.SoundType.Generate);

            // Decrease the part scale to 0
            Vector3 newSecondTargetScale = new Vector3(secondTargetScale, secondTargetScale, secondTargetScale);

            while (part.localScale != newSecondTargetScale)
            {
                part.localScale = Vector3.MoveTowards(
                    part.localScale,
                    newSecondTargetScale,
                    secondScaleSpeed * Time.deltaTime
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
        IEnumerator WaitForPartsAndClouds(Action callback = null)
        {
            while (!partsRemoved || !cloudsRemoved)
            {
                yield return null;
            }

            callback?.Invoke();
        }

        // Toggle the pre mansion's visibility in the scene view
        void TogglePreMansionSceneVisibility()
        {
            GetMansionParts();

            int cloudsCount = clouds.childCount;
            int mansionPartsCount = mansionParts.Count;

            // Toggle clouds
            for (int i = 0; i < cloudsCount; i++)
            {
                clouds.GetChild(i).gameObject.SetActive(preMansionHidden);
            }

            // Toggle mansion parts
            for (int i = 0; i < mansionPartsCount; i++)
            {
                mansionParts[i].gameObject.SetActive(preMansionHidden);
            }

            preMansionHidden = !preMansionHidden;
        }

        // Get the mansion parts
        void GetMansionParts()
        {
            if (mansionParts.Count == 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform part = transform.GetChild(i);

                    if (part != null && !part.name.Contains("Cloud"))
                    {
                        mansionParts.Add(part);
                    }
                }
            }
        }
    }
}
