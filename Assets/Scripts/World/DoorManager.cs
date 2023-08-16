using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
public class DoorManager : MonoBehaviour
{
    // Variables
    [HideInInspector]
    public DoorPH[] doors;

    // Instance
    public static DoorManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        doors = new DoorPH[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            doors[i] = transform.GetChild(i).GetComponent<DoorPH>();
        }
    }
}
}