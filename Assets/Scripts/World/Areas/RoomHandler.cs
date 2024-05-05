using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavMeshPlus.Components;

namespace Merge
{
    public class RoomHandler : MonoBehaviour
    {
        // Variables
        public bool locked = true;
        public float unlockSpeed = 3f;
        public bool hasDoor = false;
        public bool showOverlay = false;
        public Color cloudColor;
        public float cloudOffset = 40f;
        public float cloudTime = 1.7f;
        public float cloudTimeOffset = 0.5f;

        [Header("Sorting Layers")]
        [SortingLayer]
        public string roomSortingLayer;
        [SortingLayer]
        public string overlaySortingLayer = "Overlay";
        [SortingLayer]
        public string navSortingLayer = "NavMeshArea";

        [Header("Debug")]
        public bool debugOn = false;
        [Condition("debugOn", true)]
        public bool unlock = false;

        private GameObject lockedNavArea;
        public GameObject lockedOverlay;
        public GameObject nav;
        private Vector3 roomCenter;

        // References
        private AreaRefs areaRefs;
        private DoorManager doorManager;
        private DataManager dataManager;
        private SoundManager soundManager;
        private CharMove charMove;
        private CharSpeech charSpeech;
        private CameraMotion cameraMotion;
        private NavMeshManager navMeshManager;

        void Awake()
        {
            areaRefs = GetComponent<AreaRefs>();

            lockedOverlay = areaRefs.GetLockedOverlay();
            nav = areaRefs.GetNav();
        }

        void Start()
        {
            doorManager = DoorManager.Instance;
            dataManager = DataManager.Instance;
            soundManager = SoundManager.Instance;
            charMove = CharMain.Instance.charMove;
            charSpeech = CharMain.Instance.charSpeech;
            cameraMotion = Camera.main.GetComponent<CameraMotion>();
            navMeshManager = NavMeshManager.Instance;

            ToggleOverlay();

            if (locked)
            {
                Lock();
            }
            else
            {
                UnlockAlt(true);
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (unlock)
            {
                unlock = false;

                // Debug
                if (locked)
                {
                    Unlock(null);
                }
                else
                {
                    Lock();
                }
            }

            if (roomSortingLayer == "")
            {
                Debug.LogWarning("The room sorting layer of this room handler ins't selected: " + gameObject.name);
            }
        }
#endif

        void ToggleOverlay()
        {
            if (lockedOverlay == null)
            {
                lockedOverlay = areaRefs.GetLockedOverlay();
            }

            if (lockedOverlay != null)
            {
                if (lockedOverlay.transform.GetChild(0).GetComponent<NavMeshModifier>() != null)
                {
                    lockedNavArea = lockedOverlay.transform.GetChild(0).gameObject;

                    roomCenter = lockedNavArea.GetComponent<PolygonCollider2D>().bounds.center;
                }

                for (int i = 0; i < lockedOverlay.transform.childCount; i++)
                {
                    if (lockedOverlay.transform.GetChild(i).TryGetComponent(out SpriteRenderer spriteRenderer))
                    {
                        spriteRenderer.sortingLayerName = showOverlay ? overlaySortingLayer : "Default";
                    }
                }
            }
        }

        public void EnableNav(Action callback = null)
        {
            if (nav == null)
            {
                nav = areaRefs.GetNav();
            }

            if (nav != null)
            {
                for (int i = 0; i < nav.transform.childCount; i++)
                {
                    nav.transform.GetChild(i).gameObject.SetActive(true);
                }
            }

            if (lockedNavArea != null)
            {
                lockedNavArea.SetActive(false);
            }

            callback?.Invoke();
        }

        public void DisableNav()
        {
            if (nav == null)
            {
                nav = areaRefs.GetNav();
            }

            if (nav != null)
            {
                for (int i = 0; i < nav.transform.childCount; i++)
                {
                    nav.transform.GetChild(i).gameObject.SetActive(false);
                }
            }

            if (lockedNavArea != null)
            {
                lockedNavArea.SetActive(true);
            }
        }

        public Vector2 GetRoomCenter()
        {
            if (roomCenter == Vector3.zero && areaRefs.lockedOverlayPH != null)
            {
                lockedOverlay = areaRefs.lockedOverlayPH.gameObject;

                if (lockedOverlay.transform.GetChild(0).GetComponent<NavMeshModifier>() != null)
                {
                    lockedNavArea = lockedOverlay.transform.GetChild(0).gameObject;

                    roomCenter = lockedNavArea.GetComponent<PolygonCollider2D>().bounds.center;
                }
            }

            return roomCenter;
        }

        public void MoveRoomIntoView(float cameraMoveSpeed, float cameraScaleSpeed, float scaleSize, Vector2 positionOffset, Action<Vector3> callback = null)
        {
            doorManager.GetPosition(roomSortingLayer, (Vector2 position) =>
            {
                charMove.SetPosition(position, () =>
                {
                    cameraMotion.MoveToAndScaleTo(position + positionOffset, cameraMoveSpeed, scaleSize, cameraScaleSpeed, () =>
                    {
                        callback?.Invoke(roomCenter);
                    });
                });
            });
        }

        void Lock()
        {
            if (lockedOverlay == null)
            {
                lockedOverlay = areaRefs.GetLockedOverlay();
            }

            if (lockedOverlay != null)
            {
                for (int i = 0; i < lockedOverlay.transform.childCount; i++)
                {
                    if (lockedOverlay.transform.GetChild(i).TryGetComponent(out SpriteRenderer spriteRenderer))
                    {
                        //  spriteRenderer.color = cloudColor;
                        spriteRenderer.sortingLayerName = overlaySortingLayer;
                    }
                }
            }

            DisableNav();

            locked = true;
        }

        public void Unlock(Action<Vector3> callback = null, float speechDelay = 0.3f)
        {
            Glob.taskLoading = true;

            locked = false;

            dataManager.UnlockRoom(gameObject.name);

            EnableNav(() =>
            {
                navMeshManager.Bake(() =>
                {
                    doorManager.OpenDoor(roomSortingLayer, (Vector2 position) =>
                    {
                        charMove.SetPosition(position, () =>
                        {
                            UnlockAfter(speechDelay);
                        });
                    });
                });
            });

            cameraMotion.MoveToAndScaleTo(roomCenter, -1f, 195f, -1f, () =>
            {
                soundManager.PlaySound(SoundManager.SoundType.LevelUp); // TODO set proper unlocking sfx (RoomUnlocking)

                StartCoroutine(PlayParticles());

                RemoveClouds(roomCenter);
            });

            Glob.taskLoading = false;

            callback?.Invoke(roomCenter);
        }

        void RemoveClouds(Vector3 roomCenter)
        {
            if (lockedOverlay == null)
            {
                lockedOverlay = areaRefs.GetLockedOverlay();
            }

            if (lockedOverlay != null)
            {
                for (int i = 0; i < lockedOverlay.transform.childCount; i++)
                {
                    Transform child = lockedOverlay.transform.GetChild(i);

                    if (child.name.Contains("Cloud"))
                    {
                        StartCoroutine(MoveCloud(child, roomCenter.x));
                    }
                }
            }
        }

        IEnumerator MoveCloud(Transform cloud, float roomCenterX)
        {
            Vector3 startingPos = cloud.transform.position;

            float finalXPos = cloud.transform.position.x + (cloud.transform.position.x > roomCenterX ? cloudOffset : -cloudOffset);

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
        }

        IEnumerator PlayParticles()
        {
            float elapsedTime = 0;

            while (elapsedTime < cloudTime)
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime / cloudTime > cloudTime / 3)
                {
                    // TODO - Add a nice particle effect here

                    yield break;
                }
            }
        }

        public void UnlockAfter(float speechDelay = 0.3f)
        {
            if (nav == null)
            {
                nav = areaRefs.GetNav();
            }

            if (nav != null)
            {
                Transform walkingArea = nav.transform.GetChild(0);

                if (walkingArea != null)
                {
                    PolygonCollider2D navCollider = GetComponent<PolygonCollider2D>();

                    if (navCollider != null)
                    {
                        Glob.SetTimeout(() =>
                        {
                            charSpeech.StopAndSpeak("speech_room_unlocked_" + roomSortingLayer, true, true);
                        }, speechDelay);

                        charMove.SetDestination(navCollider.bounds.center, false, false, null);
                    }
                }
            }
        }

        public void UnlockAlt(bool initial = false)
        {
            if (lockedOverlay == null)
            {
                lockedOverlay = areaRefs.GetLockedOverlay();
            }

            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(false);
            }

            if (!initial)
            {
                EnableNav();

                locked = false;
            }
        }
    }
}