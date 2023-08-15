using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavMeshPlus.Components;

public class NavMeshManager : MonoBehaviour
{
    // Variables
    public bool bake;
    public Transform worldRoot;

    // References
    private NavMeshSurface navMeshSurface;

    // Instance
    public static NavMeshManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // References
        navMeshSurface = GetComponent<NavMeshSurface>();

        CheckRooms();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (bake)
        {
            bake = false;

            Glob.Validate(() =>
            {
                CheckRooms(() =>
                {
                    Bake();
                });
            }, this);
        }
    }
#endif

    public void CheckRooms(Action callback = null)
    {
        for (int i = 0; i < worldRoot.childCount; i++)
        {
            RoomHandler room = worldRoot.GetChild(i).GetComponent<RoomHandler>();

            if (room != null)
            {
                if (room.locked)
                {
                    room.DisableNav();
                }
                else
                {
                    room.EnableNav();
                }
            }
        }

        callback?.Invoke();
    }

    public void Bake()
    {
        if (navMeshSurface == null)
        {
            navMeshSurface = GetComponent<NavMeshSurface>();
        }

        navMeshSurface.BuildNavMesh();
    }
}
