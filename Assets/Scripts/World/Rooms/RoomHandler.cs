using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomHandler : MonoBehaviour
{
    // Variables
    public bool locked = true;
    public float unlockSpeed = 3f;
    public bool hasDoor = false;
    public Color lockOverlayColor;

    public bool showOverlay = false;
    public bool showNav = false;

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
    private NavMeshManager navMeshManager;
    private DoorManager doorManager;

    void Start()
    {
        navMeshManager = NavMeshManager.Instance;
        doorManager = DoorManager.Instance;

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

        showNav = false;

        ToggleNav();

        ToggleOverlay();

        if (locked)
        {
            Lock();
        }
    }

    void OnValidate()
    {
        if (debugOn && unlock)
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

        ToggleNav();

        ToggleOverlay();
    }

    void ToggleNav()
    {
        if (nav == null)
        {
            navPH = transform.GetComponentInChildren<NavPH>();
        }

        if (navPH != null)
        {
            nav = navPH.gameObject;

            Transform walkable = nav.transform.GetChild(0);
            Transform notWalkable = nav.transform.GetChild(1);

            for (int i = 0; i < walkable.childCount; i++)
            {
                walkable.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = 0;
                walkable.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = showNav ? navSortingLayer : "Default";
            }

            for (int i = 0; i < notWalkable.childCount; i++)
            {
                notWalkable.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = 1;
                walkable.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = showNav ? navSortingLayer : "Default";
            }
        }
    }

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
                lockedOverlay.transform.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = showOverlay ? overlaySortingLayer : "Default";
            }
        }
    }

    void Lock()
    {
        if (lockedOverlay != null)
        {
            for (int i = 0; i < lockedOverlay.transform.childCount; i++)
            {
                lockedOverlay.transform.GetChild(i).GetComponent<SpriteRenderer>().color = lockOverlayColor;
                lockedOverlay.transform.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = overlaySortingLayer;
            }
        }

        if (nav != null)
        {
            nav.SetActive(false);
        }

        locked = true;
    }

    public void Unlock()
    {
        if (locked)
        {
            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(false);
            }

            if (nav != null)
            {
                nav.SetActive(true);

                navMeshManager.Bake();
            }

            bool found = false;

            for (int i = 0; i < doorManager.doors.Length; i++)
            {
                if (doorManager.doors[i].roomSortingLayer == roomSortingLayer)
                {
                    doorManager.doors[i].gameObject.SetActive(false);

                    found = true;

                    break;
                }
            }

            if (!found && hasDoor)
            {
                Debug.LogWarning("This room with a door couldn't find the door: " + gameObject.name);
            }

            locked = false;
        }
    }
}
