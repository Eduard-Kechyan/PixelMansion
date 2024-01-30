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
        public Color lockOverlayColor;
        public float overlayOffset = 5f;
        public float overlayTime = 2f;
        public float overlayTimeOffset = 0.5f;

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

        private LockedOverlayPH lockedOverlayPH;
        private NavPH navPH;
        private GameObject lockedOverlay;
        private GameObject lockedNavArea;
        private GameObject nav;
        private Vector3 roomCenter;

        // References
        private DoorManager doorManager;
        private DataManager dataManager;
        private SoundManager soundManager;
        private CharMove charMove;
        private CameraMotion cameraMotion;

        void Start()
        {
            doorManager = DoorManager.Instance;
            dataManager = DataManager.Instance;
            soundManager = SoundManager.Instance;
            charMove = CharMain.Instance.charMove;
            cameraMotion = Camera.main.GetComponent<CameraMotion>();

            lockedOverlayPH = transform.GetComponentInChildren<LockedOverlayPH>();
            navPH = transform.GetComponentInChildren<NavPH>();

            if (lockedOverlayPH != null)
            {
                lockedOverlay = lockedOverlayPH.gameObject;
            }

            if (navPH != null)
            {
                nav = navPH.gameObject;
            }

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
                    Unlock(null, true);
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
            /* if (lockedOverlay == null)
             {
                 lockedOverlayPH = transform.GetComponentInChildren<LockedOverlayPH>();
             }*/

            if (lockedOverlayPH != null)
            {
                lockedOverlay = lockedOverlayPH.gameObject;

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
                navPH = transform.GetComponentInChildren<NavPH>();

                if (navPH != null)
                {
                    nav = navPH.gameObject;
                }
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
                navPH = transform.GetComponentInChildren<NavPH>();

                if (navPH != null)
                {
                    nav = navPH.gameObject;
                }
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

        void Lock()
        {
            if (lockedOverlay != null)
            {
                for (int i = 0; i < lockedOverlay.transform.childCount; i++)
                {
                    if (lockedOverlay.transform.GetChild(i).TryGetComponent(out SpriteRenderer spriteRenderer))
                    {
                        //  spriteRenderer.color = lockOverlayColor;
                        spriteRenderer.sortingLayerName = overlaySortingLayer;
                    }
                }
            }

            DisableNav();

            locked = true;
        }

        public void Unlock(Action callback = null, bool alt = false)
        {
            locked = false;

            dataManager.UnlockRoom(gameObject.name);

            EnableNav(() =>
            {
                if (debugOn && alt)
                {
                    transform.parent.Find("NavMesh").GetComponent<NavMeshManager>().Bake();
                }

                doorManager.OpenDoor(roomSortingLayer, (Vector2 position) =>
                {
                    charMove.SetPosition(position, () =>
                    {
                        UnlockAfter();
                    });
                });
            });

            cameraMotion.MoveTo(roomCenter, -1f, () =>
            {
                soundManager.PlaySound(Types.SoundType.LevelUp); // TODO add proper unlocking sfx (RoomUnlocking)

                StartCoroutine(PlayParticles());

                RemoveClouds(roomCenter);
            });

            callback?.Invoke();
        }

        void RemoveClouds(Vector3 roomCenter)
        {
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

            float finalXPos = cloud.transform.position.x + (cloud.transform.position.x > roomCenterX ? overlayOffset : -overlayOffset);

            float elapsedTime = 0;

            float newOverlayTime = overlayTime + (UnityEngine.Random.value >= 0.5 ? overlayTimeOffset : -overlayTimeOffset);

            SpriteRenderer renderer = cloud.GetComponent<SpriteRenderer>();

            Color initialColor = renderer.color;

            while (elapsedTime < newOverlayTime)
            {
                float time = elapsedTime / newOverlayTime;

                cloud.transform.position = Vector3.Lerp(startingPos, new Vector3(finalXPos, cloud.transform.position.y, cloud.transform.position.z), time);

                renderer.color = Color.Lerp(initialColor, lockOverlayColor, time);

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            renderer.sortingLayerName = "Default";
        }

        IEnumerator PlayParticles()
        {
            float elapsedTime = 0;

            while (elapsedTime < overlayTime)
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime / overlayTime > overlayTime / 3)
                {
                    // TODO - Add a nice particle effect here

                    yield break;
                }
            }
        }

        public void UnlockAfter()
        {
            if (nav != null)
            {
                Transform walkingArea = nav.transform.GetChild(0);

                if (walkingArea != null)
                {
                    PolygonCollider2D navCollider = GetComponent<PolygonCollider2D>();

                    if (navCollider != null)
                    {
                        charMove.SetDestination(navCollider.bounds.center, false, false, null, "speech_room_unlocked_" + roomSortingLayer);
                    }
                }
            }
        }

        public void UnlockAlt(bool initial = false)
        {
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