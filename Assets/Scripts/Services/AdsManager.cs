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

        // Make sure to reset the rewards in the ResetData function
        [Header("Rewards")]
        public int energyRewardAmount = 20;

        private string adUnitId = "unused";

        private RewardedAd rewardedAd;

        private Action<int> rewardCallback;
        private Action rewardFailedCallback;

        private int failedCount = 0;
        private const int maxFailedCount = 3;

        private bool failedAlt = false;

        // References
        private Services services;
        private ErrorManager errorManager;

        void Awake()
        {
#if UNITY_EDITOR
#else
    if(!enableAds){
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
            errorManager = ErrorManager.Instance;

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

        // Watch an simple ad
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
                    rewardCallback?.Invoke(energyRewardAmount);
                }
                else
                {
                    rewardCallback?.Invoke(0);
                }

                ResetData();
            }
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
                if (error != null)
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

                    // ERROR
                    errorManager.Throw(Types.ErrorType.Code, "AdsManager.cs -> LoadAd()", error.ToString());

                    return;
                }

                // Failed
                if (ad == null)
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

                    // ERROR
                    errorManager.Throw(Types.ErrorType.Code, "AdsManager.cs -> LoadAd()", "Ad was null!");

                    return;
                }

                rewardedAd = ad;

                HandleReward(rewardedAd.GetRewardItem(), true);

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
            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                rewardedAd.Show((Reward reward) =>
                {
                    HandleReward(reward);
                });
            }
            else
            {
                if (failedAlt)
                {
                    failedAlt = false;

                    rewardFailedCallback?.Invoke();

                    ResetData();
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

        void HandleReward(Reward reward, bool pre = false)
        {
            if (pre)
            {
                switch (reward.Type)
                {
                    case "Energy":
                        energyRewardAmount = (int)reward.Amount;
                        break;
                    case "Reward":// FIX - This is a dummy and should be fixed
                        energyRewardAmount = (int)reward.Amount;
                        break;
                    default:
                        // FIX - Handle error
                        Debug.LogWarning("[AdsManager.cs] Ad reward type not found! Type: " + reward.Type + ", Amount: " + reward.Amount);
                        break;
                }
            }
            else
            {
                rewardCallback?.Invoke((int)reward.Amount);

                ResetData();
            }
        }

        void ResetData()
        {
            rewardCallback = null;
            rewardFailedCallback = null;

            energyRewardAmount = 20;
        }
    }
}