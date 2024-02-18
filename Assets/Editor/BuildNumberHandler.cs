using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Merge
{
    public class BuildNumberHandler : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        // Variables
        // public bool logNextVersion = false;

        public int callbackOrder => 0;

        private BuildData buildData = null;
        private string tempAppVersion = "";
        private int tempBuildNumber = 0;

        /* void OnValidate()
         {
             if (logNextVersion)
             {
                 logNextVersion = false;

                 HandleBuildData();
             }
         }*/

        string GetNextVersion(BuildData buildData)
        {
            string[] parts = buildData.appVersion.Split(".");

            string appVersion;

            parts[2] = "" + (int.Parse(parts[2]) + 1); // For both Android and iOS

            appVersion = parts[0] + "." + parts[1] + "." + parts[2];

            if (buildData.isAlpha)
            {
                appVersion = appVersion + ".a";
            }

            if (EditorUserBuildSettings.development)
            {
                appVersion = appVersion + ".dev";
            }

            if (!EditorUserBuildSettings.buildAppBundle)
            {
                appVersion = appVersion + ".apk";
            }

            return appVersion;
        }

        int GetNextBuild(BuildData buildData)
        {
            int buildNumber = buildData.buildNumber;

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android && EditorUserBuildSettings.buildAppBundle)
            {
                // Android
                buildNumber++;
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                // iOS
                // Note - There might be a check needed here like "EditorUserBuildSettings.buildAppBundle" 
                buildNumber++;
            }

            return buildNumber;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            HandleBuildData();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            HandlePostBuild();
        }

        void HandleBuildData()
        {
            // Get Data
            buildData = AssetDatabase.LoadAssetAtPath<BuildData>("Assets/Resources/BuildData.asset");

            tempAppVersion = GetNextVersion(buildData);
            tempBuildNumber = GetNextBuild(buildData);

            // Set Data
            PlayerSettings.bundleVersion = tempAppVersion;

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                PlayerSettings.Android.bundleVersionCode = tempBuildNumber;
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                PlayerSettings.iOS.buildNumber = "" + tempBuildNumber;
            }

            // Log Data
            Debug.Log("//// Pre build config [Step 1] ////");
            Debug.Log("App Version: " + tempAppVersion);
            Debug.Log("Bundle Version: " + tempBuildNumber);
        }

        void HandlePostBuild()
        {
            Debug.Log("//// Post build config [Step 2] ////");

            if (buildData != null)
            {
                Debug.Log(buildData.appVersion);
                Debug.Log(buildData.buildNumber);

                buildData.appVersion = tempAppVersion;
                buildData.buildNumber = tempBuildNumber;

                Debug.Log(buildData.appVersion);
                Debug.Log(buildData.buildNumber);

                // Save Data
                EditorUtility.SetDirty(buildData);
                /*AssetDatabase.CreateAsset(buildData, "Assets/Resources/BuildData.asset");
                AssetDatabase.SaveAssets();*/
            }else{
                Debug.Log("\"buildData\" is null!");
            }
        }
    }
}
