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
        public bool enableAds = true;
        public bool logData = false;
        public bool failImmediately = false;

        // Make sure to reset the rewards in the ResetData function
        [Header("Rewards")]
        public int energyRewardAmount = 20;
        [ReadOnly]
        public int energyRewardAmountInner = 20;

        private string adUnitId = "unused";

        private bool rewardReady = false;
        private Reward rewardToGive = null;

        private RewardedAd rewardedAd;

        private Action<int> rewardCallback;
        private Action rewardFailedCallback;

        private int failedCount = 0;
        private const int maxFailedCount = 3;

        // References
        private Services services;
        private AnalyticsManager analyticsManager;
        private ErrorManager errorManager;

        void Awake()
        {
#if UNITY_EDITOR
#else
            if (!enableAds)
            {
                enableAds = true;
            }
#endif

#if UNITY_ANDROID
            adUnitId = "ca-app-pub-5910627528492422/2571416965";
#elif UNITY_IOS
            adUnitId = "ca-app-pub-3940256099942544/1712485313";
#endif
        }

        void Start()
        {
            // Cache
            services = Services.Instance;
            //analyticsManager = services.GetComponent<AnalyticsManager>();
            analyticsManager = AnalyticsManager.Instance;
            errorManager = ErrorManager.Instance;

            energyRewardAmountInner = energyRewardAmount;

            if (!Debug.isDebugBuild)
            {
                logData = false;
            }

            // Initialize Google ads
            if (enableAds)
            {
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
        }

        void Update()
        {
            CheckForRewards();
        }

        // Watch a simple ad
        public void WatchAd(Types.AdType adType, Action<int> callback = null, Action failCallback = null)
        {
            rewardCallback = callback;
            rewardFailedCallback = failCallback;

            if (enableAds)
            {
                ShowAd();
            }
            else
            {
                if (adType == Types.AdType.Energy)
                {
                    rewardCallback?.Invoke(energyRewardAmountInner);
                }
                else
                {
                    rewardCallback?.Invoke(0);
                }

                ResetData();
            }
        }

        // Preload the ad to be watched when necessary
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
                CheckForError(ad, error, () =>
                {
                    rewardedAd = ad;

                    SetRewardData(rewardedAd.GetRewardItem());

                    HandleAdEvent(rewardedAd);

                    if (showAfterLoading)
                    {
                        ShowAd();
                    }
                });
            });
        }

        // Show the ad to the player
        void ShowAd()
        {
            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                rewardedAd.Show((Reward reward) =>
                {
                    rewardReady = true;
                    rewardToGive = reward;
                });
            }
            else
            {
                HandleFail();
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

                // TODO - Convert adValue.Value to USD, if it isn't
                Debug.Log("//// Ad Value ////");
                Debug.Log(adValue.Value);
                Debug.Log(adValue.CurrencyCode);
                Debug.Log(adValue.Precision);

                Reward reward = ad.GetRewardItem();

                // analyticsManager.FireAdImpressionEvent(reward.Type, true, adValue.Value);
            };

            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                if (logData)
                {
                    Debug.Log("Rewarded ad recorded an impression.");
                }

                Reward reward = ad.GetRewardItem();

                analyticsManager.FireAdImpressionEvent(reward.Type, false);
            };

            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                if (logData)
                {
                    Debug.Log("Rewarded ad was clicked.");
                }

                Reward reward = ad.GetRewardItem();

                // analyticsManager.FireAdImpressionEvent(reward.Type, true);
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
                // ERROR
                errorManager.Throw(
                    Types.ErrorType.Code,
                    GetType().Name,
                    "Rewarded ad failed to open full screen content with error: " + error
                );

                HandleFail();
            };
        }

        void HandleReward(Reward reward)
        {
            rewardCallback?.Invoke((int)reward.Amount);

            ResetData();
        }

        void CheckForError(RewardedAd ad, LoadAdError error, Action errorCallback = null)
        {
            bool hasError = false;

            if (error != null)
            {
                hasError = true;

                // ERROR
                errorManager.Throw(
                    Types.ErrorType.Code,
                    GetType().Name,
                    error.ToString()
                );
            }

            if (ad == null)
            {
                hasError = true;

                // ERROR
                errorManager.Throw(
                    Types.ErrorType.Code,
                    GetType().Name,
                    "Ad was null!"
                );
            }

            if (hasError)
            {
                if (failedCount < maxFailedCount)
                {
                    failedCount++;

                    HandleFail();
                }
                else
                {
                    failedCount = 0;

                    rewardFailedCallback?.Invoke();
                }
            }
            else
            {
                errorCallback?.Invoke();
            }
        }

        void HandleFail()
        {
            if (failImmediately)
            {
                failImmediately = false;

                rewardFailedCallback?.Invoke();

                ResetData();
            }
            else
            {
                LoadAd(rewardCallback != null && rewardFailedCallback != null);
            }
        }

        void SetRewardData(Reward reward)
        {
            switch (reward.Type)
            {
                case "Energy":
                    energyRewardAmountInner = (int)reward.Amount;
                    break;
                case "Reward":// FIX - This is a dummy and should be fixed
                    energyRewardAmountInner = (int)reward.Amount;
                    break;
                default:
                    // FIX - Handle error
                    Debug.LogWarning("[AdsManager.cs] Ad reward type not found! Type: " + reward.Type + ", Amount: " + reward.Amount);
                    break;
            }
        }

        void CheckForRewards()
        {
            if (rewardReady && rewardToGive != null)
            {
                HandleReward(rewardToGive);
            }
        }

        void ResetData()
        {
            rewardReady = false;
            rewardToGive = null;

            rewardCallback = null;
            rewardFailedCallback = null;

            energyRewardAmountInner = energyRewardAmount;
        }
    }
}