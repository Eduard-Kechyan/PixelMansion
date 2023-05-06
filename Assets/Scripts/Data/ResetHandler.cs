using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ResetHandler : MonoBehaviour
{
    public void RestartApp(bool reset = false)
    {
        //if(Application.isEditor) return;

        // Reset the game's data
        if (reset)
        {
            ResetData();
        }

#if UNITY_ANDROID
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
        // OR - Find a way to reset the game
#endif
    }

    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        string folderPath = Application.persistentDataPath + "/QuickSave";

        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath, true);
        }
    }
}
