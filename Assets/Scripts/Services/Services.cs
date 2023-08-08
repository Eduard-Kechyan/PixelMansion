using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Services : MonoBehaviour
{
    // Instance
    public static Services Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
