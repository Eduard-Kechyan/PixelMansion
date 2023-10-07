using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

namespace Merge
{
    public class AdsManager : MonoBehaviour
    {
        // Variables
        public bool logData = false;

        private string adUnitId = "unused";

        private RewardedAd rewardedAd;

        private Action successCallback;
        private Action failedCallback;

        private int failedCount = 0;
        private int maxFailedCount = 3;

        private bool failedAlt = false;

        // References
        private Services services;

        void Awake()
        {
#if UNITY_ANDROID
            adUnitId = "ca-app-pub-5910627528492422/2571416965";
#elif Unity_IPHONE
            adUnitId = "ca-app-pub-3940256099942544/1712485313";
#endif
        }

        void Start()
        {
            // Cache
            services = Services.Instance;

            //MobileAds.RaiseAdEventsOnUnityMainThread = true;

            // Initialize Google ads
            MobileAds.Initialize(initStatus =>
            {
                if (logData)
                {
                    Debug.Log(initStatus);
                }

                services.adsAvailable = true;

                LoadAd();
            });
        }

        // Watch an simple ad
        public void WatchAd(Action callback = null, Action failCallback = null)
        {
            successCallback = callback;
            failedCallback = failCallback;

            ShowAd();
        }

        // Pre load the ads to be watched when necessary
        void LoadAd(bool showAfterLoading = false)
        {
            // Clear up the old ad before loading a new one
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }

            AdRequest adRequest = new();

            RewardedAd.Load(adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                // Failed
                if (error != null || ad == null)
                {
                    if (failedCount < maxFailedCount)
                    {
                        LoadAd();

                        failedCount++;
                    }
                    else
                    {
                        failedCount = 0;
                    }

                    return;
                }

                rewardedAd = ad;

                HandleAdEvent(rewardedAd);

                if (showAfterLoading)
                {
                    ShowAd();
                }
            });
        }

        // Show the ad to the player
        void ShowAd()
        {
            const string rewardMsg = "Rewarded ad. Type: {0}, amount: {1}.";

            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                rewardedAd.Show((Reward reward) =>
                {
                    if (logData)
                    {
                        Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
                    }
                });
            }
            else
            {
                if (failedAlt)
                {
                    failedAlt = false;

                    failedCallback?.Invoke();
                }
                else
                {
                    LoadAd(true);
                }
            }
        }

        void HandleAdEvent(RewardedAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                if (logData)
                {
                    Debug.Log(String.Format("Rewarded ad paid {0} {1}.", adValue.Value, adValue.CurrencyCode));
                }
            };

            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                if (logData)
                {
                    Debug.Log("Rewarded ad recorded an impression.");
                }
            };

            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                if (logData)
                {
                    Debug.Log("Rewarded ad was clicked.");
                }
            };

            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                if (logData)
                {
                    Debug.Log("Rewarded ad full screen content opened.");
                }
            };

            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                if (logData)
                {
                    Debug.Log("Rewarded ad full screen content closed.");
                }

                // Reload the ad so that we can show another as soon as possible.
                LoadAd();
            };

            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                if (logData)
                {
                    Debug.LogError("Rewarded ad failed to open full screen content with error : " + error);
                }

                // Reload the ad so that we can show another as soon as possible.
                LoadAd();
            };
        }
    }
}