using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;

namespace Merge
{
    public class AnalyticsManager : MonoBehaviour
    {
        // Variables
        public bool analyticsEnabled = true;

        private bool termsChecked = false;

        // References
        private Services services;
        private ErrorManager errorManager;

        // Instance
        public static AnalyticsManager Instance;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            // Cache
            services = Services.Instance;
            errorManager = ErrorManager.Instance;

            if (!services.servicesAvailable)
            {
                analyticsEnabled = false;
            }

            if (analyticsEnabled)
            {
                StartCoroutine(WaitForUnityServicesAndAuthorization());
            }
        }

        IEnumerator WaitForUnityServicesAndAuthorization()
        {
            while (!services.unityServicesAvailable)
            {
                yield return null;
            }

            services.analyticsAvailable = true;

            Debug.Log("Initializing Analytics!");

            if (PlayerPrefs.HasKey("termsAccepted") || Application.isEditor)
            {
                StartDataCollection();
            }
        }

        public void StartDataCollection()
        {
            // TODO - Add a genuinity check here

            if (!termsChecked && analyticsEnabled)
            {
                AnalyticsService.Instance.StartDataCollection();

                termsChecked = true;
            }
        }

        public void FireAdImpressionEvent(string rewardName, bool clicked = false, long value = default)
        {
            if (analyticsEnabled)
            {
                AdImpressionEvent adImpression;

                if (value == default)
                {
                    adImpression = new()
                    {
                        AdProvider = AdProvider.AdMob,
                        AdCompletionStatus = AdCompletionStatus.Completed,
                        // AdStoreDestinationId = "",
                        AdHasClicked = clicked,
                        PlacementId = AdPlacementType.REWARDED + "_" + rewardName,
                        PlacementName = rewardName,
                        PlacementType = AdPlacementType.REWARDED
                    };
                }
                else
                {
                    adImpression = new()
                    {
                        AdProvider = AdProvider.AdMob,
                        AdCompletionStatus = AdCompletionStatus.Completed,
                        // AdStoreDestinationId = "",
                        AdEcpmUsd = value,
                        AdHasClicked = clicked,
                        PlacementId = AdPlacementType.REWARDED + "_" + rewardName,
                        PlacementName = rewardName,
                        PlacementType = AdPlacementType.REWARDED
                    };
                }

                AnalyticsService.Instance.RecordEvent(adImpression);
            }
        }

        public void FireTutorialEvent(string step, bool started = false, bool ended = false)
        {
            if (analyticsEnabled)
            {
                TutorialEvent newTutorialEvent = new()
                {
                    tutorialStep = step,
                    tutorialStart = started,
                    tutorialEnded = ended,
                };

                AnalyticsService.Instance.RecordEvent(newTutorialEvent);
            }

        }

        // Bought with Gems
        public void FireEnergyBoughtEvent(int playerLevel, int energyCount, int gemCount, bool boughtWithIAP = false)
        {
            if (analyticsEnabled)
            {
                EnergyBoughtEvent newEnergyBoughtEvent = new()
                {
                    playerLevel = playerLevel,
                    energyCount = energyCount,
                    gemCount = gemCount,
                    boughtWithIAP = boughtWithIAP
                };

                AnalyticsService.Instance.RecordEvent(newEnergyBoughtEvent);
            }
        }
    }

    //// EVENT CLASSES ////

    public class TutorialEvent : Unity.Services.Analytics.Event
    {
        public TutorialEvent() : base("TutorialEvent") { }

        public string tutorialStep
        {
            set
            {
                SetParameter("tutorialStep", value);
            }
        }

        public bool tutorialStart
        {
            set
            {
                SetParameter("tutorialStart", value);
            }
        }

        public bool tutorialEnded
        {
            set
            {
                SetParameter("tutorialEnded", value);
            }
        }
    }

    public class EnergyBoughtEvent : Unity.Services.Analytics.Event
    {
        public EnergyBoughtEvent() : base("EnergyBoughtEvent") { }

        public int playerLevel
        {
            set
            {
                SetParameter("playerLevel", value);
            }
        }

        public int energyCount
        {
            set
            {
                SetParameter("energyCount", value);
            }
        }

        public int gemCount
        {
            set
            {
                SetParameter("gemCount", value);
            }
        }

        public bool boughtWithIAP
        {
            set
            {
                SetParameter("boughtWithIAP", value);
            }
        }
    }
}
