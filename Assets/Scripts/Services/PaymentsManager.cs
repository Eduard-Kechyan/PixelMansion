using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using System.Globalization;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Merge
{
    public class PaymentsManager : MonoBehaviour, IDetailedStoreListener
    {
        // Variables
        public bool canPurchase = true;

        [ReadOnly]
        public bool loaded = false;
        public bool unityServicesLoaded = false;

        public ProductCatalog catalog;

        private IStoreController controller;
        private IExtensionProvider provider;

        private Action callback;
        private Action<string> failCallback;

        async void Awake()
        {
            // Load IAP Catalog
            ResourceRequest operation = Resources.LoadAsync<TextAsset>("IAPProductCatalog");
            operation.completed += HandleIAPCatalog;

            // Initialize Unity Services
            InitializationOptions options = new InitializationOptions();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            options.SetEnvironmentName("development");
#else
            options.SetEnvironmentName("production");
#endif

            await UnityServices.InitializeAsync(options);

            unityServicesLoaded = true;
        }

        //// Initialization ////

        void HandleIAPCatalog(AsyncOperation operation)
        {
            // Handle the resource request
            ResourceRequest request = operation as ResourceRequest;

            catalog = JsonUtility.FromJson<ProductCatalog>((request.asset as TextAsset).text);

            StartCoroutine(WaitForUnityServices());
        }

        IEnumerator WaitForUnityServices()
        {
            while (!unityServicesLoaded)
            {
                yield return null;
            }

            BuildConfig();
        }

        void BuildConfig()
        {
            if (canPurchase)
            {
                // Set the config builder
#if UNITY_ANDROID
                ConfigurationBuilder builder = ConfigurationBuilder.Instance(
                StandardPurchasingModule.Instance(AppStore.GooglePlay)
                );
#elif UNITY_IOS
ConfigurationBuilder builder = ConfigurationBuilder.Instance(
StandardPurchasingModule.Instance(AppStore.AppleAppStore)
);
#endif

                // Add the catalog items to the config builder
                foreach (ProductCatalogItem item in catalog.allProducts)
                {
                    builder.AddProduct(item.id, item.type);
                }

                UnityPurchasing.Initialize(this, builder);
            }
        }

        public void OnInitialized(IStoreController newController, IExtensionProvider newProvider)
        {
            controller = newController;
            provider = newProvider;

            loaded = true;
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            // TODO - Add proper error handling
            Debug.Log(error);
            Debug.Log(message);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            // TODO - Add proper error handling
            Debug.Log(error);
        }

        //// Purchase ////

        public string GetPrice(string id)
        {
            for (int i = 0; i < controller.products.all.Length; i++)
            {
                if (controller.products.all[i].definition.id == id)
                {
                    return RegionInfo.CurrentRegion.CurrencySymbol + controller.products.all[i].metadata.localizedPriceString;
                }
            }

            return "";
        }

        public void Purchase(string productId, Action newCallback = null, Action<string> newFailCallback = null)
        {
            controller.InitiatePurchase(productId);

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
            HandlePurchaseFailed(product, reason);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription description)
        {
            HandlePurchaseFailed(product, description.reason);
        }

        void HandlePurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.Log(product.definition.id);
            Debug.Log(reason);

            failCallback?.Invoke(reason.ToString());

            callback = null;
            failCallback = null;
        }
    }
}