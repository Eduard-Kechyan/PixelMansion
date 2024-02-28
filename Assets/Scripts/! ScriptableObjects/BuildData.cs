using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "BuildData", menuName = "ScriptableObject/BuildData")]
    public class BuildData : ScriptableObject
    {
        /*
        Guidelines:

        Full release name: 0.1.2.3.a.dev

        0: Major update (0 for Alpha, main release starts with 1)
        1: Minor update (Content or story chapter)
        2: Every next update (Bugfixes, performance improvements and other)
        3: Bundle Version Code (Needs to be updated for every bundled release that needed to be published on Google Play Console)
        a: Alpha release (Initial development)
        dev: (Wether this a test build)
        */

        public bool isAlpha = false;
        public bool isPublishableRelease = false;
        public bool isBundling = false;
        [Space(10)]
        public string appVersion = "0.0.-1";
        public int buildNumber = 0;
        [Space(10)]
        public string packageName = "com.SoloNodeGames.PixelMansion";      

        void OnValidate(){
            if(isPublishableRelease)
            {
                isAlpha = false; 
            }
        }
    }
}
