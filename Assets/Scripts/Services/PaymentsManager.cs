using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Merge
{
    public class PaymentsManager : MonoBehaviour
    {
        public void Purchase(float price = 0f, Action callback = null, Action failCallback = null)
        {
            Debug.Log("Purchase made! Price: $" + price);

            callback?.Invoke();

            //failCallback?.Invoke();
        }

        public void OnProductFetched(Product product)
        {
            Debug.Log(product.metadata.localizedTitle);
            Debug.Log(product.metadata.localizedPrice);
            Debug.Log(product.metadata.isoCurrencyCode);
            Debug.Log(product.definition.id);
            Debug.Log(product.definition.payouts);
            Debug.Log(product.definition.payout.quantity);
            Debug.Log(product.definition.type);
        }
    }
}