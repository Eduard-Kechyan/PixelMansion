using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldDataManager : MonoBehaviour
{
    Transform worldRoot;

    void Start()
    {
        if (worldRoot == null)
        {
            ErrorManager.Instance.FindUsed("the World Root");

            worldRoot = GameObject.Find("World").transform.Find("Root");
        }


        GetInitialData();
    }

    void GetInitialData()
    {
        
    }
}
