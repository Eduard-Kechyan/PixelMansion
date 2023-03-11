using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public void CheckNotifications()
    {
        /* if (Application.platform == RuntimePlatform.Android)
        {
            if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Debug.Log("Write permission already granted!");
            }
            else
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionGranted += PermissionGranted;
                callbacks.PermissionDenied += PermissionDenied;

                Permission.RequestUserPermission(Permission.ExternalStorageWrite, callbacks);
            }

            if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Debug.Log("Read permission already granted!");
            }
            else
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionGranted += PermissionGranted;
                callbacks.PermissionDenied += PermissionDenied;

                Permission.RequestUserPermission(Permission.ExternalStorageRead, callbacks);
            }

            Debug.Log(path);
        }*/
    }

    internal void PermissionGranted(string permissionName)
    {
        Debug.Log(permissionName + " permission granted!");
    }

    internal void PermissionDenied(string permissionName)
    {
        Debug.Log(permissionName + " permission denied!");
    }
}
