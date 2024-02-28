using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Merge
{
    public class BuildHandler : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        // NOTE - Use "throw new BuildFailedException("Error!");" for ending the build sooner

        // Variables
        public int callbackOrder => 0;

        private BuildData buildData = null;
        private string tempAppVersion = "";
        private int tempBuildNumber = 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            CombineLocale();

            SetupKeystore();

            HandlePreBuild();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            HandlePostBuild();
        }

        void CombineLocale()
        {
            LocaleCombiner localeCombiner = GameObject.Find("DebugManager").GetComponent<LocaleCombiner>();

            if (localeCombiner != null)
            {
                localeCombiner.Combine(true, false, true);
            }
        }

        void SetupKeystore()
        {
            KeystoreData keystoreData = AssetDatabase.LoadAssetAtPath<KeystoreData>("Assets/Resources/KeystoreData.asset");

            if (keystoreData == null)
            {
                throw new BuildFailedException("Build was canceled since KeystoreData.asset was not present in Assets/Resources! Create one and assign the proper data!");
            }
            else
            {
                PlayerSettings.Android.keystorePass = keystoreData.keystorePass;
                PlayerSettings.Android.keyaliasName = keystoreData.keyaliasName;
                PlayerSettings.Android.keyaliasPass = keystoreData.keyaliasPass;
            }
        }

        void HandlePreBuild()
        {
            // Get Data
            buildData = AssetDatabase.LoadAssetAtPath<BuildData>("Assets/Resources/BuildData.asset");

            if (EditorUserBuildSettings.buildAppBundle)
            {
                Debug.Log("Building release app");

                EditorUserBuildSettings.development = false;

                buildData.isBundling = true;

                tempBuildNumber = GetNextBuild(buildData);
                tempAppVersion = GetNextVersion(buildData, tempBuildNumber);

                if (EditorUserBuildSettings.buildAppBundle)
                {
                    EditorUserBuildSettings.development = false;
                }
                else
                {
                    EditorUserBuildSettings.development = true;
                }

                // Set Data
                PlayerSettings.bundleVersion = tempAppVersion;

                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, buildData.packageName);

                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP); // Needed for publishing on Google Play Console
                    AndroidArchitecture aac = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
                    PlayerSettings.Android.targetArchitectures = aac;

                    if (buildData.isPublishableRelease)
                    {
                        PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Android, Il2CppCodeGeneration.OptimizeSize); // Faster build time
                    }
                    else
                    {
                        PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Android, Il2CppCodeGeneration.OptimizeSpeed); // Faster gameplay
                    }

                    PlayerSettings.Android.bundleVersionCode = tempBuildNumber;
                }
                else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
                {
                    // TODO - Setup seperate iOS build number
                    PlayerSettings.iOS.buildNumber = "" + tempBuildNumber;
                }

                // Log Data
                Debug.Log("//// Pre build config ////");
                /*Debug.Log("App Version: " + tempAppVersion);
                Debug.Log("Bundle Version: " + tempBuildNumber);*/
            }
            else
            {
                Debug.Log("Building debug app");

                EditorUserBuildSettings.development = true;

                buildData.isBundling = false;

                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x); // Faster build time
                }
            }
            
            // Save Data
            EditorUtility.SetDirty(buildData);
        }

        void HandlePostBuild()
        {
            if (EditorUserBuildSettings.buildAppBundle)
            {
                Debug.Log("//// Post build config ////");

                if (buildData != null)
                {
                    buildData.appVersion = tempAppVersion;
                    buildData.buildNumber = tempBuildNumber;

                    buildData.isBundling = false;

                    /*Debug.Log(buildData.appVersion);
                    Debug.Log(buildData.buildNumber);*/

                    // Save Data
                    EditorUtility.SetDirty(buildData);
                }
                else
                {
                    Debug.Log("\"buildData\" was null and wasn't updated!");
                }
            }
        }

        string GetNextVersion(BuildData buildData, int tempBuildNumber)
        {
            string[] parts = buildData.appVersion.Split(".");

            string appVersion;

            parts[2] = "" + (int.Parse(parts[2]) + 1); // For both Android and iOS

            appVersion = parts[0] + "." + parts[1] + "." + parts[2] + "." + tempBuildNumber;

            if (buildData.isAlpha)
            {
                appVersion = appVersion + ".a";
            }

            if (!EditorUserBuildSettings.buildAppBundle)
            {
                appVersion = appVersion + ".dev";
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
    }
}
