using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class UserDataHandler : MonoBehaviour
{
    public bool saveUserId = true;

    private Action loadingCallback;

    [Serializable]
    public class UserData
    {
        public string userId;
        public string email;
        public string location;
        public string language;
    }

    private ApiCalls apiCalls;

    void Start()
    {
        apiCalls = ApiCalls.Instance;
    }

    public void CheckUser(Action callback)
    {
        loadingCallback = callback;

        string userId;

        if (PlayerPrefs.HasKey("userId"))
        {
            userId = PlayerPrefs.GetString("userId");
        }
        else
        {
            byte[] guidByteArray = Guid.NewGuid().ToByteArray();

            string base64 = Convert.ToBase64String(guidByteArray);

            userId = base64.Substring(0, base64.Length - 2);

            GetReadyToCreateUser(userId);

            if (saveUserId)
            {
                PlayerPrefs.SetString("userId", userId);
            }
        }

        GameData.Instance.userId = userId;
    }

    void GetReadyToCreateUser(string userId)
    {
        UserData newUserData = new()
        {
            userId = userId,
            email = "test@gmail.com",
            location = "USA",
            language = "en-US"
        };

        // deviceId = SystemInfo.deviceUniqueIdentifier,
        // deviceModel = SystemInfo.deviceModel,
        // deviceOs = SystemInfo.operatingSystem,
        // deviceLanguage = Application.systemLanguage.ToString(),
        // devicePlatform = Application.platform.ToString(),
        // deviceName = SystemInfo.deviceName,
        // deviceType = SystemInfo.deviceType.ToString(),
        // deviceOsFamily = SystemInfo.operatingSystemFamily.ToString(),

        /* Debug.Log(newUserData.deviceId);
         Debug.Log(newUserData.deviceModel);
         Debug.Log(newUserData.deviceOs);
         Debug.Log(newUserData.deviceLanguage);

         Debug.Log(Application.absoluteURL);
         Debug.Log(Application.installerName);
         Debug.Log(Application.installMode);*/

        StartCoroutine(apiCalls.CreateUser(JsonConvert.SerializeObject(newUserData), loadingCallback));
    }
}
