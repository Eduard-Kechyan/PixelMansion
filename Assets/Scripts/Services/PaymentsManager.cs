using System;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.SceneManagement;
using UnityEngine.Purchasing.Security;

namespace Merge
{
    public class PaymentsManager : MonoBehaviour, IDetailedStoreListener
    {
        // Variables
        public bool canPurchase = true;
        public ProductCatalog catalog;

        private IStoreController controller;
        private IExtensionProvider provider;
        private IGooglePlayStoreExtensions googleExtensions;

        private Action<bool> callback;
        private Action<string> failCallback;

        // References
        private ErrorManager errorManager;
        private ValuePop valuePop;
        private Services services;
        private GameData gameData;

        // Instance
        public static PaymentsManager Instance;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                // Cache
                services = GetComponent<Services>();

                // Load IAP Catalog
                ResourceRequest operation = Resources.LoadAsync<TextAsset>("IAPProductCatalog");
                operation.completed += HandleIAPCatalog;
            }
        }

        void Start()
        {
            // Cache
            errorManager = ErrorManager.Instance;
            valuePop = GameRefs.Instance.valuePop;
            gameData = GameData.Instance;
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
            while (!services.unityServicesAvailable)
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

            googleExtensions = provider.GetExtension<IGooglePlayStoreExtensions>();

            services.iapAvailable = true;
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

        public void Purchase(string productId, Action<bool> newCallback = null, Action<string> newFailCallback = null)
        {
            callback = newCallback;
            failCallback = newFailCallback;

            controller.InitiatePurchase(productId);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            bool isPurchaseValid = true;

            // Validate
            if (!Application.isEditor)
            {
                var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);

                try
                {
                    var result = validator.Validate(purchaseEvent.purchasedProduct.receipt);

                   // Debug.Log(purchaseEvent.purchasedProduct.receipt);

                    int count = 0;

                    bool foundReceipt = false;

                    // TODO - Add server-side valdiation here

                    foreach (IPurchaseReceipt receipt in result)
                    {
                        /*  Debug.Log(receipt.productID);

                          GooglePlayReceipt googleReceipt = receipt as GooglePlayReceipt;

                          if(googleReceipt!=null){
                              Debug.Log(googleReceipt.purchaseState);
                          }*/

                        if (count == 0)
                        {
                            for (int i = 0; i < controller.products.all.Length; i++)
                            {
                                if (controller.products.all[i].definition.id == receipt.productID)
                                {
                                    foundReceipt = true;
                                    break;
                                }
                            }
                        }

                        break;
                    }

                    if (!foundReceipt)
                    {
                        // ERROR
                        throw new Exception("Receipt product id not found in the controller!");
                    }
                }
                catch
                {
                    isPurchaseValid = false;

                    // ERROR
                    errorManager.Throw(Types.ErrorType.Code, "PaymentsManager.cs -> Purchase()", "Failed to validate product with receipt: " + purchaseEvent.purchasedProduct.receipt);
                }
            }

            if (isPurchaseValid)
            {
                if (googleExtensions.IsPurchasedProductDeferred(purchaseEvent.purchasedProduct))
                {
                    return PurchaseProcessingResult.Pending;
                }

                callback?.Invoke(true);

                callback = null;
                failCallback = null;

                if (valuePop == null)
                {
                    valuePop = GameRefs.Instance.valuePop;
                }

                foreach (var productItem in catalog.allProducts)
                {
                    if (productItem.id == purchaseEvent.purchasedProduct.definition.id)
                    {
                        for (int i = 0; i < productItem.Payouts.Count; i++)
                        {
                            HandlePayout(productItem.Payouts[i]);
                        }

                        break;
                    }
                }
            }

            // TODO - Send purchase data to the servers

            return PurchaseProcessingResult.Complete;
        }

        void HandlePayout(ProductCatalogPayout payout)
        {
            if (payout.type == ProductCatalogPayout.ProductCatalogPayoutType.Currency || payout.type == ProductCatalogPayout.ProductCatalogPayoutType.Resource)
            {
                switch (payout.subtype)
                {
                    case "Gold":
                        if (valuePop != null)
                        {
                            valuePop.PopValue((int)payout.quantity, Types.CollGroup.Gold, false, true);
                        }
                        else
                        {
                            gameData.UpdateValue((int)payout.quantity, Types.CollGroup.Gold, false);
                        }
                        break;
                    case "Gems":
                        if (valuePop != null)
                        {
                            valuePop.PopValue((int)payout.quantity, Types.CollGroup.Gems, false, true);
                        }
                        else
                        {
                            gameData.UpdateValue((int)payout.quantity, Types.CollGroup.Gems, false);
                        }
                        break;
                    case "Energy":
                        if (valuePop != null)
                        {
                            valuePop.PopValue((int)payout.quantity, Types.CollGroup.Energy, false, true);
                        }
                        else
                        {
                            gameData.UpdateValue((int)payout.quantity, Types.CollGroup.Energy, false);
                        }
                        break;
                    default:
                        errorManager.Throw(Types.ErrorType.Code, "PaymentsManager.cs -> HandlePayout()", "Payout Subtype " + payout.subtype + " has not been implemented yet!");
                        break;
                }
            }
            else
            {
                errorManager.Throw(Types.ErrorType.Code, "PaymentsManager.cs -> HandlePayout()", "Payout Type " + payout.type + " has not been implemented yet!");
            }
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
            if (reason != PurchaseFailureReason.UserCancelled)
            {
                if (reason == PurchaseFailureReason.PaymentDeclined)
                {
                    failCallback?.Invoke("declined");
                }
                else if (reason == PurchaseFailureReason.PurchasingUnavailable)
                {
                    failCallback?.Invoke("unavailable");
                }
                else
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

                failCallback?.Invoke("other");
            }
            else
            {
                callback?.Invoke(false);
            }

            callback = null;
            failCallback = null;
        }
    }
}