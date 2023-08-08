using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    void Start()
    {
        
    }

    public void WatchAd(Action callback = null, Action failCallback = null)
    {
        Debug.Log("Ad watched!");

        callback?.Invoke();

        //failCallback?.Invoke();
    }
}
