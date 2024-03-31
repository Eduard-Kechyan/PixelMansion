using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Merge
{
    public class BuildHandlerEditor : EditorWindow
    {
        // Variables
        public ReleaseType releaseType = ReleaseType.Release;
        public ReleasePlatform releasePlatform = ReleasePlatform.Android;

        public enum ReleaseType
        {
            Release,
            Debug,
            None
        }

        public enum ReleasePlatform
        {
            Android,
            IOS
        }

        [MenuItem("Window/Build Handler")]
        public static void OpenWindow()
        {
            EditorWindow window = GetWindow<GameDataExplorerEditor>();
            window.titleContent = new GUIContent("Build Handler");
        }

        /*async void BuildApp()
        {

            // Build the app
            Debug.Log("App build!");
             BuildPlayerOptions buildOptions = new()
             {
                 target = releasePlatform == ReleasePlatform.Android ? BuildTarget.Android : BuildTarget.iOS,
                 scenes = new[]{
                     "Assets/Scenes/Loading.unity",
                     "Assets/Scenes/World.unity",
                     "Assets/Scenes/Merge.unity",
                 },
             };

             BuildPipeline.BuildPlayer(buildOptions);
        }*/
    }
}
