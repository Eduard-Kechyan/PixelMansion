using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Merge
{
    [InitializeOnLoad]
    public class PrePlayMode
    {
        static PrePlayMode()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                HandleLocale();
            }
        }

        static void HandleLocale()
        {
            // Get the references
            GameObject debugManager = GameObject.Find("DebugManager");
            LocaleCombiner localeCombiner = null;

            if (debugManager != null)
            {
                localeCombiner = debugManager.GetComponent<LocaleCombiner>();
            }

            // Combine Locale
            if (localeCombiner != null)
            {
                localeCombiner.Combine(false, false, false);
            }
        }
    }
}
