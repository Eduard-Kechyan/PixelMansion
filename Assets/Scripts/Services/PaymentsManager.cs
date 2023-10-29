using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Merge
{
    public class PaymentsManager : MonoBehaviour, IDetailedStoreListener
    {
        // Variables
        public ShopData shopData;

        private IStoreController controller;
        //private IExtensionProvider extension;

        private Action callback;
        private Action failCallback;

        // References
        //private ShopMenu shopMenu;
        private InitUnity initUnity;

        void Start()
        {
            // Cache
            //shopMenu = GameRefs.Instance.shopMenu;
            initUnity = GetComponent<InitUnity>();
        }

        //// Initialization ////

    /*    public PaymentsManager()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            for (int i = 0; i < shopData.goldContent.Length; i++)
            {
                builder.AddProduct(shopData.goldContent[i].id, ProductType.Consumable);
            }

            for (int i = 0; i < shopData.gemsContent.Length; i++)
            {
                builder.AddProduct(shopData.goldContent[i].id, ProductType.Consumable);
            }

            StartCoroutine(WaitForUnityServices(builder));
        }*/

        IEnumerator WaitForUnityServices(ConfigurationBuilder builder)
        {
            while (!initUnity.initialized)
            {
                yield return null;
            }

            UnityPurchasing.Initialize(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extension)
        {
            this.controller = controller;
            //this.extension = extension;

            /*    for (int i = 0; i < controller.products.all.Length; i++)
                {
                    shopMenu.SetPrice(controller.products.all[i]);
                }*/
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError(error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError(error);
            Debug.LogError(message);
        }

        //// Purchase ////

        public void Purchase(string id, Action newCallback = null, Action newFailCallback = null)
        {
            controller.InitiatePurchase(id);

            callback = newCallback;

            failCallback = newFailCallback;
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            callback?.Invoke();

            callback = null;
            failCallback = null;

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.LogError(reason);

            failCallback?.Invoke();

            callback = null;
            failCallback = null;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription description)
        {
            Debug.LogError(description.message);

            failCallback?.Invoke();

            callback = null;
            failCallback = null;
        }
    }
}