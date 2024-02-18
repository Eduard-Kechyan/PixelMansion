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

        // References
        private ErrorManager errorManager;

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

        void Start()
        {
            // Cache
            errorManager = ErrorManager.Instance;
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

        public void OnInitializeFailed(InitializationFailureReason error, string message) // A
        {
            if (error == InitializationFailureReason.PurchasingUnavailable)
            {
                // TODO - Disable IAP in the app
                Debug.LogWarning("IAP is disabled in app!");
            }
            else
            {
                // ERROR
                errorManager.Throw(Types.ErrorType.Unity, "PaymentsManager.cs -> OnInitializeFailed() // A", "Reason: " + error.ToString() + ", Message: " + message);
            }
        }

        public void OnInitializeFailed(InitializationFailureReason error) // B
        {
            if (error == InitializationFailureReason.PurchasingUnavailable)
            {
                // TODO - Disable IAP in the app
                Debug.LogWarning("IAP is disabled in app!");
            }
            else
            {
                // ERROR
                errorManager.Throw(Types.ErrorType.Unity, "PaymentsManager.cs -> OnInitializeFailed() // B", "Reason: " + error.ToString());
            }
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
            callback = newCallback;
            failCallback = newFailCallback;

            controller.InitiatePurchase(productId);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            callback?.Invoke();

            Debug.Log("Successfully purchased!");

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
            HandlePurchaseFailed(product, description.reason, description.message);
        }

        void HandlePurchaseFailed(Product product, PurchaseFailureReason reason, string message = "")
        {
            if (reason != PurchaseFailureReason.UserCancelled && reason != PurchaseFailureReason.PaymentDeclined)
            {
                if (message == "")
                {
                    // ERROR
                    errorManager.Throw(Types.ErrorType.Code, "PaymentsManager.cs -> HandlePurchaseFailed()", "Reason: " + reason.ToString() + ", Product Id: " + product.definition.id);
                }
                else
                {
                    // ERROR
                    errorManager.Throw(Types.ErrorType.Code, "PaymentsManager.cs -> HandlePurchaseFailed()", "Reason: " + reason.ToString() + ", Message: " + message + ", Product Id: " + product.definition.id);
                }
            }

            failCallback?.Invoke(reason.ToString());

            callback = null;
            failCallback = null;
        }
    }
}