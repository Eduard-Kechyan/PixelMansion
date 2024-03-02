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
        [Space(10)]
        public string tokenKeyName = "9ba73c98-ccb2-498a-931b-f5c1ee75b123.default.unity.services.authentication.session_token";
        public string playerIdName = "9ba73c98-ccb2-498a-931b-f5c1ee75b123.default.unity.services.authentication.player_id";

        void OnValidate()
        {
            if (isPublishableRelease)
            {
                isAlpha = false;
            }
        }
    }
}
