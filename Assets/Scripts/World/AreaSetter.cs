using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class AreaSetter : MonoBehaviour
    {
        // Variables
        public bool set = false;

#if UNITY_EDITOR
        void Awake()
        {
            SetPositionZOrder();
        }
#endif

        void OnValidate()
        {
            if (set)
            {
                set = false;
            }

            SetPositionZOrder();
        }

        void SetPositionZOrder()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name.Contains("Area"))
                {
                    OrderSetter orderSetter = transform.GetChild(i).GetComponent<OrderSetter>();

                    if (orderSetter != null)
                    {
                        orderSetter.SetSpriteOrders();
                    }
                }
            }
        }
    }
}
