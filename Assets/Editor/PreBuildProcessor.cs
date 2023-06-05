using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class PreBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        LocaleCombiner localeCombiner = GameObject.Find("LocaleCombiner").GetComponent<LocaleCombiner>();

        if (localeCombiner != null)
        {
            localeCombiner.Combine(true);
        }
    }
}
