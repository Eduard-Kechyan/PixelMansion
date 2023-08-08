using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorPH : MonoBehaviour
{
    [SortingLayer]
    public string roomSortingLayer;

    private void OnValidate()
    {
        if (roomSortingLayer == "")
        {
            Debug.LogWarning("The room sorting layer of this door placeholder ins't selected: " + gameObject.name);
        }
    }
}
