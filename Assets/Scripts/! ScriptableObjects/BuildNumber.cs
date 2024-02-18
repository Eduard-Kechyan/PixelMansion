using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "BuildData", menuName = "ScriptableObject/BuildData")]
    public class BuildData : ScriptableObject
    {
        [ReadOnly]
        [TextArea]
        public string defaultData = "Is Alpha = false;\r\nApp Version= \"0.0.-1\";\r\nBuildNumber = 0;";

        public bool isAlpha = false;

        public string appVersion = "0.0.-1";
        public int buildNumber = 0;
    }
}
