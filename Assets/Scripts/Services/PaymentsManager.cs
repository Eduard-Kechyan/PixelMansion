using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class PaymentsManager : MonoBehaviour
{
    void Start()
    {

    }

    public void Purchase(float price = 0f, Action callback = null, Action failCallback = null)
    {
        Debug.Log("Purchase made! Price: $" + price);

        callback?.Invoke();

        //failCallback?.Invoke();
    }
}
}