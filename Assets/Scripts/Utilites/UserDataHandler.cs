using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Merge
{
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
        public int age;
    }

    private ApiCalls apiCalls;

    void Start()
    {
        apiCalls = ApiCalls.Instance;
    }

    public void CheckUser(Action callback, int tempAge)
    {

        if (PlayerPrefs.HasKey("userId"))
        {
            GameData.Instance.userId = PlayerPrefs.GetString("userId");

            callback();
        }
        else
        {
            byte[] guidByteArray = Guid.NewGuid().ToByteArray();

            string base64 = Convert.ToBase64String(guidByteArray);

            string userId = base64.Substring(0, base64.Length - 2);

            loadingCallback = callback;

            GetReadyToCreateUser(userId, tempAge);

            if (saveUserId)
            {
                PlayerPrefs.SetString("userId", userId);
                PlayerPrefs.Save();
            }

            GameData.Instance.userId = userId;
        }
    }

    void GetReadyToCreateUser(string userId, int tempAge)
    {
        string dummyEmail = RandomEmailDummy(); // Or use test@gmail.com

        UserData newUserData = new()
        {
            userId = userId,
            email = dummyEmail, //TODO - Email should be empty and should only be filled if the player signs ins using social media
            location = "USA",
            language = "en-US",
            age = tempAge
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

    string RandomEmailDummy()
    {
        string randomText = "";

        const string glyphs = "abcdefghijklmnopqrstuvwxyz";

        int charAmount = UnityEngine.Random.Range(3, 6);

        for (int i = 0; i < charAmount; i++)
        {
            randomText += glyphs[UnityEngine.Random.Range(0, glyphs.Length)];
        }

        return randomText + "@" + randomText + ".com";
    }
}
}