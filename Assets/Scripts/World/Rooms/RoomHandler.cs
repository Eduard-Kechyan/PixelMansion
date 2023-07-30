using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomHandler : MonoBehaviour
{
    public bool locked = true;
    public float unlockSpeed = 3f;
    public Color lockOverlayColor;

    public bool showNav = false;
    [SortingLayer]
    public string navSortingLayer;

    [Header("Debug")]
    public bool debugOn = false;
    [Condition("debugOn", true)]
    public bool unlock = false;

    private LockedOverlayPH lockedOverlayPH;
    private NavPH tempNavPH;
    private GameObject lockedOverlay;
    private GameObject nav;

    // References
    private NavMeshManager navMeshManager;

    void Start()
    {
        navMeshManager = NavMeshManager.Instance;

        lockedOverlayPH = transform.GetComponentInChildren<LockedOverlayPH>();

        tempNavPH = transform.GetComponentInChildren<NavPH>();
        tempNavPH = transform.GetComponentInChildren<NavPH>();

        if (lockedOverlayPH != null)
        {
            lockedOverlay = lockedOverlayPH.gameObject;
        }

        if (tempNavPH != null)
        {
            nav = tempNavPH.gameObject;
        }

        showNav = false;

        ToggleNav();

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

        ToggleNav();
    }

    void ToggleNav()
    {
        if (nav == null)
        {
            tempNavPH = transform.GetComponentInChildren<NavPH>();

            if (tempNavPH != null)
            {
                nav = tempNavPH.gameObject;

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
    }

    void Lock()
    {
        if (lockedOverlay != null)
        {
            lockedOverlay.GetComponent<SpriteRenderer>().color = lockOverlayColor;
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

            locked = false;
        }
    }
}
