using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class RoomHandler : MonoBehaviour
    {
        // Variables
        public bool locked = true;
        public float unlockSpeed = 3f;
        public bool hasDoor = false;
        public Color lockOverlayColor;

        public bool showOverlay = false;

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
        private GameObject nav;

        // References
        private DoorManager doorManager;
        private DataManager dataManager;
        private SoundManager soundManager;
        private CharMove charMove;

        void Start()
        {
            doorManager = DoorManager.Instance;
            dataManager = DataManager.Instance;
            soundManager = SoundManager.Instance;
            charMove = CharMain.Instance.charMove;

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
                    Unlock();
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

            Glob.Validate(() =>
            {
                ToggleOverlay();
            }, this);
        }
#endif

        void ToggleOverlay()
        {
            if (lockedOverlay == null)
            {
                lockedOverlayPH = transform.GetComponentInChildren<LockedOverlayPH>();
            }

            if (lockedOverlayPH != null)
            {
                lockedOverlay = lockedOverlayPH.gameObject;

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
        }

        void Lock()
        {
            if (lockedOverlay != null)
            {
                for (int i = 0; i < lockedOverlay.transform.childCount; i++)
                {
                    if (lockedOverlay.transform.GetChild(i).TryGetComponent(out SpriteRenderer spriteRenderer))
                    {
                        spriteRenderer.color = lockOverlayColor;
                        spriteRenderer.sortingLayerName = overlaySortingLayer;
                    }
                }
            }

            DisableNav();

            locked = true;
        }

        public void Unlock(Action callback = null)
        {
            dataManager.UnlockRoom(gameObject.name);

            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(false);
            }

            EnableNav();

            doorManager.OpenDoor(roomSortingLayer, (Vector2 position) =>
            {
                charMove.SetPosition(position,()=>
                {
                    UnlockAfter();
                });
            });

            locked = false;

            callback?.Invoke();
        }

        public void UnlockAfter()
        {
            charMove.SetDestination(transform.localPosition);

            soundManager.PlaySound(Types.SoundType.LevelUp); // TODO add proper unlocking sfx (RoomUnlocking)

            // TODO - Add a nice particle effect here
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