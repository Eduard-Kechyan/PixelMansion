using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class Services : MonoBehaviour
    {
        // Variables
        public bool networkAvailable = false; // TODO - Check network availability
        public bool unityServicesAvailable = false;
        public bool iapAvailable = false;
        public bool adsAvailable = false;

        [Header("Sign Ins")]
        public bool googleSignIn = false;
        public bool facebookSignIn = false;
        public bool appleSignIn = false;

        // Instance
        public static Services Instance;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}