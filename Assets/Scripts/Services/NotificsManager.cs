using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using Unity.Notifications.Android;

#if UNITY_ANDROID
//using UnityEngine.Android;
//using Unity.Notifications.Android;
#elif UNITY_IOS
    using Unity.Notifications.iOS;
#endif

// POST_NOTIFICATIONS

namespace Merge
{
    public class NotificsManager : MonoBehaviour
    {
        // Variables
        public Color accentColor;

        [Header("Debug")]
        public bool log = false;

        private const string CHANNEL_GROUP_ID = "Merge_channel_group";
        private const string CHANNEL_GROUP_NAME = "Merge notifications";

        private const string CHANNEL_ID = "Merge_channel";
        private const string CHANNEL_NAME = " Notifications Channel";
        private const string CHANNEL_DESC = "'s main channel of notifications";

        private const string NOTIFICATION_GROUP_ID = "Merge_notification_group";

        private bool checkedPre = false;
        private bool initialized = false;
        private bool allowed = false;

        // Classes
        [Serializable]
        public class Notification
        {
            public int id;
            public DateTime fireTime;
            public NotificationType type;
            public string itemName;
        }

        [Serializable]
        public class NotificationJson
        {
            public int id;
            public string fireTime;
            public string type;
            public string itemName;
        }

        // Enums
        public enum NotificationType
        {
            Gen,
            Chest,
            Energy,
        }

        // References
        private Settings settings;
        private DataManager dataManager;
        private GameData gameData;
        private I18n LOCALE;

        void Start()
        {
            // Cache
            settings = Settings.Instance;
            dataManager = DataManager.Instance;
            gameData = GameData.Instance;
            LOCALE = I18n.Instance;
        }

        void Init()
        {
            if (!initialized)
            {
                initialized = true;
                allowed = true;

                AndroidNotificationChannelGroup channelGroup = new()
                {
                    Id = CHANNEL_GROUP_ID,
                    Name = CHANNEL_GROUP_NAME,
                };

                AndroidNotificationCenter.RegisterNotificationChannelGroup(channelGroup);

                string gameTitle = LOCALE.Get("game_title");

                AndroidNotificationChannel channel = new()
                {
                    Id = CHANNEL_ID,
                    Name = gameTitle + CHANNEL_NAME,
                    Description = gameTitle + CHANNEL_DESC,
                    Importance = Importance.High,
                    Group = CHANNEL_GROUP_ID,
                    CanShowBadge = true,
                    EnableVibration = true,
                    LockScreenVisibility = LockScreenVisibility.Public,
                };

                AndroidNotificationCenter.RegisterNotificationChannel(channel);

                AndroidNotificationCenter.CancelAllDisplayedNotifications();
            }
        }

        //// Check ////

        public void CheckPermission(Action callback = null, Action<bool> altCallback = null)
        {
            CheckNotifications();

            if (log)
            {
                Debug.Log("Initializing notifications (A)");
            }

            if (Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                Init();

                if (log)
                {
                    Debug.Log("Initializing notifications (B)");
                }

                callback?.Invoke();
                altCallback?.Invoke(true);
                settings.SetNotifications(true);

                return;
            }

            if (PlayerPrefs.HasKey("NotificsPermissionStatus"))
            {
                PermissionStatus status = Glob.ParseEnum<PermissionStatus>(PlayerPrefs.GetString("NotificsPermissionStatus"));

                if (status == PermissionStatus.Allowed)
                {
                    if (log)
                    {
                        Debug.Log("Initializing notifications (C)");
                    }

                    Init();

                    callback?.Invoke();
                    altCallback?.Invoke(true);
                    settings.SetNotifications(true);

                    return;
                }

                if (status == PermissionStatus.DeniedDontAskAgain || status == PermissionStatus.NotificationsBlockedForApp)
                {
                    if (log)
                    {
                        Debug.Log("Initializing notifications (D)");
                    }

                    callback?.Invoke();
                    altCallback?.Invoke(false);
                    settings.SetNotifications(false);

                    return;
                }
            }

            StartCoroutine(RequestPermission(callback, altCallback));
        }

        IEnumerator RequestPermission(Action callback, Action<bool> altCallback)
        {
            var request = new PermissionRequest();

            while (request.Status == PermissionStatus.RequestPending && !Application.isEditor)
            {
                yield return null;
            }

            if (log)
            {
                Debug.Log("Initializing notifications (" + request.Status.ToString() + ")");
            }

            if (request.Status == PermissionStatus.Allowed)
            {
                Init();

                settings.SetNotifications(true);

                altCallback?.Invoke(true);
            }
            else
            {
                settings.SetNotifications(false);

                altCallback?.Invoke(false);
            }

            PlayerPrefs.SetString("NotificsPermissionStatus", request.Status.ToString());

            PlayerPrefs.Save();

            callback?.Invoke();
        }

        void CheckNotifications()
        {
            if (!checkedPre)
            {
                bool removedAny = false;

                for (int i = gameData.notifications.Count - 1; i >= 0; i--)
                {
                    DateTime currentTime = DateTime.UtcNow;

                    if (gameData.notifications[i].fireTime < currentTime)
                    {
                        gameData.notifications.Remove(gameData.notifications[i]);

                        removedAny = true;

                        // Check if we also need to remove the scheduled notification
                    }
                }

                if (removedAny)
                {
                    dataManager.SaveNotifications();
                }

                checkedPre = true;
            }
        }

        //// Send ////

        public int Add(NotificationType notificationType, DateTime fireTime, string itemName = "", bool addToGameData = true)
        {
            if (allowed && settings.notificationsOn)
            {
                if (log)
                {
                    Debug.Log("Adding notifications");
                }

                AndroidNotification newNotification = HandleNotificationType(notificationType, itemName);

                newNotification.FireTime = fireTime;
                newNotification.ShowTimestamp = true;
                newNotification.Color = accentColor;
                // newNotification.IntentData = "Custom Intent Data!";
                newNotification.ShowInForeground = false;
                newNotification.SmallIcon = "main_icon"; // Main icon
                newNotification.ShouldAutoCancel = true;
                newNotification.Group = NOTIFICATION_GROUP_ID;

                int id = AndroidNotificationCenter.SendNotification(newNotification, CHANNEL_ID);

                if (addToGameData)
                {
                    gameData.notifications.Add(new()
                    {
                        id = id,
                        type = notificationType,
                        itemName = itemName,
                        fireTime = fireTime
                    });
                }

                dataManager.SaveNotifications();

                return id;
            }

            return 0;
        }

        public void Remove(int id)
        {
            if (log)
            {
                Debug.Log("Removing notifications");
            }

            for (int i = 0; i < gameData.notifications.Count; i++)
            {
                if (gameData.notifications[i].id == id)
                {
                    NotificationStatus notificationStatus = AndroidNotificationCenter.CheckScheduledNotificationStatus(id);

                    if (notificationStatus == NotificationStatus.Scheduled)
                    {
                        AndroidNotificationCenter.CancelScheduledNotification(id);
                    }

                    gameData.notifications.Remove(gameData.notifications[i]);

                    break;
                }
            }

            dataManager.SaveNotifications();
        }

        AndroidNotification HandleNotificationType(NotificationType notificationType, string itemName)
        {
            string title = "";
            string text = "";
            string largeIcon = "";

            // TODO - Add different colors for the different types

            switch (notificationType)
            {
                case NotificationType.Gen:
                    title = LOCALE.Get("notification_gen_title", itemName);
                    text = LOCALE.Get("notification_gen_text", itemName);
                    largeIcon = "ready_icon";
                    break;
                case NotificationType.Chest:
                    title = LOCALE.Get("notification_chest_title", itemName);
                    text = LOCALE.Get("notification_chest_text", itemName);
                    largeIcon = "open_icon";
                    break;
                case NotificationType.Energy:
                    title = LOCALE.Get("notification_energy_title");
                    text = LOCALE.Get("notification_energy_text");
                    largeIcon = "energy_icon";
                    break;
            }

            return new AndroidNotification()
            {
                Title = title,
                Text = text,
                LargeIcon = largeIcon // Secondary icon
            };
        }

        //// Other ////

        public void DisableNotifications()
        {
            AndroidNotificationCenter.CancelAllScheduledNotifications();
        }

        public void EnableNotifications()
        {
            for (int i = gameData.notifications.Count - 1; i >= 0; i--)
            {
                DateTime currentTime = DateTime.UtcNow;

                if (gameData.notifications[i].fireTime > currentTime)
                {
                    Add(gameData.notifications[i].type, gameData.notifications[i].fireTime, gameData.notifications[i].itemName, false);
                }
                else
                {
                    gameData.notifications.Remove(gameData.notifications[i]);
                }
            }
        }
    }
}
