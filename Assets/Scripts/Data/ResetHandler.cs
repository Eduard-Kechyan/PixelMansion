using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Merge
{
    public class ResetHandler : MonoBehaviour
{
    public bool resetData = false;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (resetData)
        {
            resetData = false;

            ResetData();
        }
    }
#endif

    public void RestartAndResetApp(){
        RestartApp(true);
    }

    public void RestartApp(bool reset = false)
    {
        //if(Application.isEditor) return;

        // Reset the game's data
        if (reset)
        {
            ResetData();
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

    public void ResetData()
    {
        string folderPath = Application.persistentDataPath + "/QuickSave";

        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath, true);
        }

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
}