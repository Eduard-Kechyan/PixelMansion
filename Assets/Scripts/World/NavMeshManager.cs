using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavMeshPlus.Components;

public class NavMeshManager : MonoBehaviour
{
    // References
    private NavMeshSurface navMeshSurface;

    // Instance
    public static NavMeshManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start() {
        // References
        navMeshSurface = GetComponent<NavMeshSurface>();
    }

    public void Bake()
    {
        Debug.Log("Started Baking!");

        navMeshSurface.BuildNavMeshAsync();

        Debug.Log("Finished Baking!");
    }
}
