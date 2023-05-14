using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharMain : MonoBehaviour
{
    // References
    [HideInInspector]
    public CharSpeech charSpeech;
    [HideInInspector]
    public CharMove charMove;

    // Instance
    public static CharMain Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        charSpeech = GetComponent<CharSpeech>();
        charMove = GetComponent<CharMove>();
    }
}
