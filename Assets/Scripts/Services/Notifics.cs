using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*using UnityEngine.Android;
using Unity.Notifications.Android;
using NotificationSamples;*/
using System.Threading.Tasks;

/*#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif*/

namespace Merge
{
    public class Notices : MonoBehaviour
{
    // Variables
    public Color accentColor;

    private const string channelId = "notifs_channel_id";

    private bool allowed = false;

    // References
    //private GameNotificationsManager gameNotificationsManager;
    //private GameNotificationsManager manager;

    void Starppt()
    {
        /*manager = GetComponent<GameNotificationsManager>();

        Debug.Log("AAA");

        var channel = new GameNotificationChannel(channelId, "Default Channel", "Default notifications");
        manager.Initialize(channel);
        Debug.Log(manager);*/
    }

    public void Sendl()
    {
        if (allowed)
        {

        }
        /*Debug.Log(manager);

        IGameNotification notif = manager.CreateNotification();

        if (notif != null)
        {
            notif.Title = "Hello!";
            notif.Body = "Body Text Dummy.";
            notif.DeliveryTime = System.DateTime.Now.AddSeconds(10);

            Debug.Log(notif);
            Debug.Log(manager);

            manager.ScheduleNotification(notif);
        }*/
    }

    void Start()
    {
        //AndroidNotificationCenter.CancelAllScheduledNotifications();

        /*AndroidNotificationChannel chanel = new AndroidNotificationChannel()
        {
            Id = "main_notifications_chanel",
            Name = "Main Channel",
            Importance = Importance.High,
            Description = "Main Notifications",
            CanShowBadge = true,
            EnableVibration = true
        };*/

        /*AndroidNotification notification = new AndroidNotification
       {
           Title = "Hello!",
           Text = "It's working! Yeah!",
           FireTime = System.DateTime.Now.AddSeconds(30),
           // SmallIcon = "small_icon",
           // LargeIcon = "large_icon",
           Color = accentColor,
           IntentData = "HELP ME!",
           ShowTimestamp = true,
           // ShowInForeground = false
       };

      var id = AndroidNotificationCenter.SendNotification(notification, "main_notifications_chanel");

       if (AndroidNotificationCenter.CheckScheduledNotificationStatus(id) == NotificationStatus.Scheduled)
       {
           AndroidNotificationCenter.CancelAllNotifications();
           AndroidNotificationCenter.SendNotification(notification, "main_notifications_chanel");
       }*/



        /*var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();

        if (notificationIntentData != null)
        {
            Debug.Log(notificationIntentData);
            Debug.Log(notificationIntentData.Channel);
            Debug.Log(notificationIntentData.Notification);
            Debug.Log(notificationIntentData.Id);
        }

        StartCoroutine(RequestPermission());*/

        /*#if UNITY_ANDROID
                var chanel = new AndroidNotificationChannel()
                {
                    Id = "channel_id",
                    Name = "Default Channel",
                    Importance = Importance.High,
                    Description = "Gerneric notifications",
                    CanShowBadge = true,
                    EnableVibration = true,
                    LockScreenVisibility = LockScreenVisibility.Public
                };
        #elif UNITY_IOS

        #endif*/

    }

    public void Send()
    {
        if (allowed)
        {
           /* var notification = new AndroidNotification
            {
                Title = "Hello!",
                Text = "It's working! Yeah!",
                FireTime = System.DateTime.Now.AddSeconds(5),
                SmallIcon = "small_icon",
                LargeIcon = "large_icon",
                Color = accentColor,
                IntentData = "HELP ME!",
                ShowTimestamp = true,
                // ShowInForeground = false
            };

            AndroidNotificationCenter.SendNotification(notification, "main_notifications_chanel");*/
        }
    }

    public async Task CheckNotifications()
    {
#if UNITY_ANDROID
       /* if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += PermissionGranted;
            callbacks.PermissionDenied += PermissionDenied;
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS", callbacks);
        }
        else
        {
            allowed = true;
        }*/
#elif UNITY_IOS
        // Check IOS notifications
#endif
        await Task.Delay(500);
    }

    internal void PermissionGranted(string permissionName)
    {
        //Debug.Log(permissionName + " permission granted!");
        allowed = true;
    }

    internal void PermissionDenied(string permissionName)
    {
        //Debug.Log(permissionName + " permission denied!");
        allowed = false;
    }
}
}