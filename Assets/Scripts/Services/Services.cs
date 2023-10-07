using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class Services : MonoBehaviour
    {
        // Variables
        public bool networkAvailable = false;
        public bool adsAvailable = false;

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