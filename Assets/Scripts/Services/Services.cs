using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace Merge
{
    public class Services : MonoBehaviour
    {
        // Variables
        public bool servicesAvailable = true;
        [ReadOnly]
        public bool networkAvailable = false;
        [ReadOnly]
        public bool unityServicesAvailable = false;
        [ReadOnly]
        public bool iapAvailable = false;
        [ReadOnly]
        public bool adsAvailable = false;
        [ReadOnly]
        public bool authAvailable = false;
        [ReadOnly]
        public bool cloudSaveAvailable = false;
        [ReadOnly]
        public bool analyticsAvailable = false;

        [Header("Sign Ins")]
        [ReadOnly]
        public bool anonymousSignIn = false;
        [ReadOnly]
        public bool googleSignIn = false;
        [ReadOnly]
        public bool appleSignIn = false;

        [Header("Other")]
        [ReadOnly]
        public bool termsAccepted = false;

        [Header("Options")]
        public float networkCheckDelay = 3f;

        // Instance
        public static Services Instance;

        async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // StartCoroutine(CheckNetwork());

                if (!servicesAvailable && !Debug.isDebugBuild)
                {
                    servicesAvailable = true;
                }

                GetInitialSignInData();

                await InitializeUnityServices();
            }
        }

        public async Task InitializeUnityServices()
        {
            // Initialize Unity Services
            InitializationOptions options = new InitializationOptions();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            options.SetEnvironmentName("development");
#else
            options.SetEnvironmentName("production");
#endif

            await UnityServices.InitializeAsync(options);

            unityServicesAvailable = true;
        }

        public void GetInitialSignInData()
        {
            if (PlayerPrefs.HasKey("anonymousSignIn") && PlayerPrefs.GetInt("anonymousSignIn") == 1)
            {
                anonymousSignIn = true;
            }

            if (PlayerPrefs.HasKey("googleSignIn") && PlayerPrefs.GetInt("googleSignIn") == 1)
            {
                anonymousSignIn = false;

                googleSignIn = true;
            }

            if (PlayerPrefs.HasKey("appleSignIn") && PlayerPrefs.GetInt("appleSignIn") == 1)
            {
                anonymousSignIn = false;

                appleSignIn = true;
            }
        }

        public void SetSignInData(AuthManager.AuthType type, bool isSignedIn = true)
        {
            switch (type)
            {
                case AuthManager.AuthType.Google:
                    anonymousSignIn = false;

                    googleSignIn = isSignedIn;

                    PlayerPrefs.SetInt("googleSignIn", isSignedIn ? 1 : 0);
                    break;
                case AuthManager.AuthType.Apple:
                    anonymousSignIn = false;

                    appleSignIn = isSignedIn;

                    PlayerPrefs.SetInt("appleSignIn", isSignedIn ? 1 : 0);
                    break;
                default: // AuthManager.AuthType.Anonym
                    anonymousSignIn = isSignedIn;

                    PlayerPrefs.SetInt("anonymousSignIn", isSignedIn ? 1 : 0);
                    break;
            }

            PlayerPrefs.Save();
        }

        public AuthManager.AuthType GetSignInData()
        {
            if (googleSignIn)
            {
                return AuthManager.AuthType.Google;
            }

            if (appleSignIn)
            {
                return AuthManager.AuthType.Apple;
            }

            if (anonymousSignIn)
            {
                return AuthManager.AuthType.Anonym;
            }

            return AuthManager.AuthType.Unknown;
        }

        /*IEnumerator CheckNetwork()
        {
            while (true)
            {
                yield return new WaitForSeconds(networkCheckDelay);

                UnityWebRequest request = new UnityWebRequest("https://example.com");

                yield return request.SendWebRequest();

                networkAvailable = request.error == null ? true : false;
            }
        }*/
    }
}