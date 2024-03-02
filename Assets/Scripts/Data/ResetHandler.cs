using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Merge
{
    public class ResetHandler : MonoBehaviour
    {
        // Variables
        public bool resetData = false;

        // References
        private CloudSave cloudSave;

        void Start()
        {
            // Cache
            cloudSave = Services.Instance.GetComponent<CloudSave>();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (resetData)
            {
                resetData = false;

                StartCoroutine(ResetDataHandler(null, true));
            }
        }
#endif

        public void ResetAndRestartApp()
        {
            RestartApp(true);
        }

        public void RestartApp(bool reset = false)
        {
            //if(Application.isEditor) return;

            // Reset the game's data
            if (reset)
            {
                ResetData(() =>
                {

#if UNITY_EDITOR
                    Debug.Log("Restarting application!");
#elif UNITY_ANDROID
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            const int kIntent_FLAG_ACTIVITY_CLEAR_TASK = 0x00008000;
            const int kIntent_FLAG_ACTIVITY_NEW_TASK = 0x10000000;

            var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            var intent = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", Application.identifier);

            intent.Call<AndroidJavaObject>("setFlags", kIntent_FLAG_ACTIVITY_NEW_TASK | kIntent_FLAG_ACTIVITY_CLEAR_TASK);
            currentActivity.Call("startActivity", intent);
            currentActivity.Call("finish");
            var process = new AndroidJavaClass("android.os.Process");
            int pid = process.CallStatic<int>("myPid");
            process.CallStatic("killProcess", pid);
        }
#elif UNITY_IOS
        // TODO - Show a message that says the following: 
        // The game will be closing, please open it up again!
        // OR - Find a way to restart the game
#endif
                });
            }

#if UNITY_EDITOR
            Debug.Log("Restarting application!");
#elif UNITY_ANDROID
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            const int kIntent_FLAG_ACTIVITY_CLEAR_TASK = 0x00008000;
            const int kIntent_FLAG_ACTIVITY_NEW_TASK = 0x10000000;

            var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            var intent = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", Application.identifier);

            intent.Call<AndroidJavaObject>("setFlags", kIntent_FLAG_ACTIVITY_NEW_TASK | kIntent_FLAG_ACTIVITY_CLEAR_TASK);
            currentActivity.Call("startActivity", intent);
            currentActivity.Call("finish");
            var process = new AndroidJavaClass("android.os.Process");
            int pid = process.CallStatic<int>("myPid");
            process.CallStatic("killProcess", pid);
        }
#elif UNITY_IOS
        // TODO - Show a message that says the following: 
        // The game will be closing, please open it up again!
        // OR - Find a way to restart the game
#endif
        }

        public void ResetData(Action callback = null, bool quit = false)
        {
            StartCoroutine(ResetDataHandler(callback, quit));
        }

        IEnumerator ResetDataHandler(Action callback = null, bool quit = false)
        {
            bool cloudDataHandled = false;
            bool cloudDeletionFailed = false;

            // Reset Cloud Save data
            if (cloudSave == null)
            {
                if (Application.isEditor)
                {
                    Debug.LogWarning("Game needs to be in play mode to be able to delete the cloud data!");
                }

                cloudDataHandled = true;
            }
            else
            {
                cloudSave.DeleteAllDataAsync(() =>
                {
                    cloudDataHandled = true;
                }, () =>
                {
                    Debug.LogWarning("Data deletion from the CloudSave failed. Try again later!");

                    cloudDataHandled = true;
                    cloudDeletionFailed = true;
                });
            }

            while (!cloudDataHandled)
            {
                yield return null;
            }

            if (cloudDeletionFailed)
            {
                // TODO - Notify that reseting the game failed
            }
            else
            {
                // Reset Player Prefs
                if (Application.isEditor)
                {
                    string sessionToken = PlayerPrefs.GetString(GameData.Instance.buildData.tokenKeyName, "");
                    string playerId = PlayerPrefs.GetString(GameData.Instance.buildData.playerIdName, "");

                    PlayerPrefs.DeleteAll();

                    if (sessionToken != "")
                    {
                        PlayerPrefs.SetString(GameData.Instance.buildData.tokenKeyName, sessionToken);
                    }

                    if (playerId != "")
                    {
                        PlayerPrefs.SetString(GameData.Instance.buildData.playerIdName, playerId);
                    }
                }
                else
                {
                    PlayerPrefs.DeleteAll();
                }

                PlayerPrefs.Save();

                // Reset disk data
                string folderPath = Application.persistentDataPath + "/QuickSave";

                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                }

                if (quit)
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID || UNITY_IOS
                        Application.Quit();
#endif
                }
                else
                {
                    callback?.Invoke();
                }
            }
        }
    }
}