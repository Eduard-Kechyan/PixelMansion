using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class DoorPH : MonoBehaviour
    {
        // Note - This is a placeholder script

        [SortingLayer]
        public string roomSortingLayer;

#if UNITY_EDITOR
        void OnValidate()
        {
            Glob.Validate(() =>
            {
                if (roomSortingLayer == "" || roomSortingLayer == "Default")
                {
                    Debug.LogWarning("The room sorting layer of this door placeholder ins't selected: " + gameObject.name);
                }
            });
        }
#endif
    }
}