using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

#if UNITY_ANDROID || UNITY_EDITOR
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#elif UNITY_IOS || UNITY_EDITOR
using Apple.GameKit;
#endif

namespace Merge
{
    public class AuthManager : MonoBehaviour
    {
        // Variables
        public bool logData = false;

        [HideInInspector]
        public bool hasLinkingConflict = false;

        private Action callback;
        private Action<bool, string> failCallback;

        private AuthType currentAuthType = AuthType.Unknown;

        // Google
        private string googlePlayToken;

        private string googlePlayerId;
        public string playerId;

        // Apple
        private string appleSignature;
        private string appleTeamPlayerId;
        private string appleSalt;
        private string applePublicKeyUrl;
        private ulong appleTimeStamp;

        // Enums
        public enum AuthType
        {
            Unknown,
            Anonym,
            Google,
            Apple
        }

        // References
        private Services services;
        private ErrorManager errorManager;

        void Start()
        {
            // Cache
            services = Services.Instance;
            errorManager = ErrorManager.Instance;

            SetPlayerIdTemp();

            StartCoroutine(WaitForUnityServices());

            if (!Debug.isDebugBuild || Application.isEditor)
            {
                logData = false;
            }
        }

        IEnumerator WaitForUnityServices()
        {
            while (!services.unityServicesAvailable)
            {
                yield return null;
            }

            Init();
        }

        async void Init()
        {
            SetupEvents();

            await SignInAnonymOrCachedAsync();

#if UNITY_ANDROID || UNITY_EDITOR
            PlayGamesPlatform.Activate();

            if (currentAuthType == AuthType.Anonym || AuthenticationService.Instance.IsExpired)
            {
                SignInToGoogleAuto();
            }
#elif UNITY_IOS || UNITY_EDITOR
            if (currentAuthType == AuthType.Anonym || !AuthenticationService.Instance.IsExpired)
            {
                await SignInToApple();
            }
#endif
        }

        void SetupEvents()
        {
            AuthenticationService.Instance.SignedIn += () =>
            {
                if (logData)
                {
                    Debug.Log(currentAuthType + " Sign In!");
                }
            };

            /*   AuthenticationService.Instance.SignInFailed += (err) =>
               {
                   errorManager.Throw(
                       Types.ErrorType.Code,
                       "AuthManager.cs -> SetupEvents()",
                       "Message: " + err.Message + ", Code: " + err.ErrorCode
                   );
               };*/

            AuthenticationService.Instance.SignedOut += () =>
            {
                Debug.Log("Player signed out!");
            };

            AuthenticationService.Instance.Expired += () =>
            {
                Debug.Log("Player session expired!");
            };
        }

        void SetPlayerIdTemp()
        {
            if (PlayerPrefs.HasKey("playerId"))
            {
                playerId = PlayerPrefs.GetString("playerId");
            }
            else
            {
                byte[] guidByteArray = Guid.NewGuid().ToByteArray();

                string base64 = Convert.ToBase64String(guidByteArray);

                playerId = "ER_" + base64.Substring(0, base64.Length - 2);
            }
        }

        async Task SignInAnonymOrCachedAsync()
        {
            bool tokenExists = AuthenticationService.Instance.SessionTokenExists;

            if (tokenExists)
            {
                currentAuthType = services.GetSignInData();
            }
            else
            {
                currentAuthType = AuthType.Anonym;
            }

            try
            {
                // Either signs in cached player or sings in anonymous player
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (tokenExists)
                {
                    if (currentAuthType == AuthType.Unknown)
                    {
                        currentAuthType = AuthType.Anonym;
                    }

                    services.SetSignInData(currentAuthType);
                }
                else
                {
                    services.SetSignInData(AuthType.Anonym);
                }

                services.authAvailable = true;

                PlayerInfo playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();

                playerId = playerInfo.Id;

                PlayerPrefs.SetString("playerId", playerId);
            }
            catch (AuthenticationException ex)
            {
                currentAuthType = AuthType.Unknown;

                // Compare to AuthenticationErrorCodes
                // ERROR
                errorManager.Throw(
                    Types.ErrorType.Code,
                    GetType().Name,
                    ex.Message,
                    ex.ErrorCode.ToString()
                );
            }
            catch (RequestFailedException ex)
            {
                currentAuthType = AuthType.Unknown;

                // Compare to CommonErrorCodes
                if (ex.ErrorCode != CommonErrorCodes.TransportError)
                {
                    // ERROR
                    errorManager.Throw(
                        Types.ErrorType.Code,
                        "AuthManager.cs -> SignInAnonymOrCachedAsync()",
                        ex.Message,
                        ex.ErrorCode.ToString()
                    );
                }
                else
                {
                    Debug.LogWarning("No internet for authentication!");
                }
            }
        }

        public void SignOut()
        {
            AuthenticationService.Instance.ClearSessionToken();
            AuthenticationService.Instance.SignOut();
            AuthenticationService.Instance.UnlinkGooglePlayGamesAsync();
        }

        async void LinkToAccount(AuthType type, bool isManual = false, bool forceLinking = false)
        {
            currentAuthType = type;

            // Whether the link should be forced
            LinkOptions linkOptions = new LinkOptions() { ForceLink = true }; // TODO - Set to "forceLinking" 

            try
            {
                if (type == AuthType.Google)
                {
                    await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(googlePlayToken, linkOptions);
                }
                else
                {
                    await AuthenticationService.Instance.LinkWithAppleGameCenterAsync(appleSignature, appleTeamPlayerId, applePublicKeyUrl, appleSalt, appleTimeStamp, linkOptions);
                }

                services.SetSignInData(type);

                await HandleDSANotifications();

                if (logData)
                {
                    Debug.Log(type + " sign in Success");
                }

                if (isManual)
                {
                    callback?.Invoke();
                }
            }
            catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                hasLinkingConflict = true;

                PlayerInfo playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();

                List<Identity> playerIdentities = playerInfo.Identities;

                Debug.Log(playerIdentities.Count);

                for (int i = 0; i < playerIdentities.Count; i++)
                {
                    Debug.Log("Player identity: " + i);
                    Debug.Log(playerIdentities[i].UserId);
                    Debug.Log(playerIdentities[i].TypeId);
                }

                Debug.Log(playerInfo.GetGooglePlayGamesId());

                if (logData)
                {
                    Debug.Log(type + " user already linked with another account!");
                }
            }
            catch (AuthenticationException ex)
            {
                // ERROR
                errorManager.Throw(
                        Types.ErrorType.Code,
                        "AuthManager.cs -> LinkToAccount()",
                        type + " linking failed with Auth error! Message: " + ex.Message + ", Code: " + ex.ErrorCode
                    );

                if (isManual)
                {
                    failCallback?.Invoke(false, "unknown");
                }
            }
            catch (RequestFailedException ex)
            {
                // ERROR
                errorManager.Throw(
                    Types.ErrorType.Code,
                    "AuthManager.cs -> LinkToAccount()",
                    type + " linking failed with Common error! Message: " + ex.Message + ", Code: " + ex.ErrorCode
                );

                if (isManual)
                {
                    failCallback?.Invoke(false, "unknown");
                }
            }
        }

        public void ResolveLinkingConflict(bool forceLinking = false)
        {
            hasLinkingConflict = false;

            if (forceLinking) // Force
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    SignInToGoogleAuto(true);
                }
                else
                {
                    // SignInToApple();
                }
            }
            else // Switch
            {
                // TODO - Implement switch profile
            }
        }

        // As of February 17, 2024, application need to comply with the DSA (Digital Service Act) Notifications
        async Task HandleDSANotifications()
        {
            string lastNotificationDate = AuthenticationService.Instance.LastNotificationDate;
            /*string storedNotificationDate = PlayerPrefs.GetString("storedNotificationDate");

            Debug.Log(lastNotificationDate);

            if (lastNotificationDate != null && long.Parse(lastNotificationDate) > long.Parse(storedNotificationDate))
            {
                List<Notification> notifications = await AuthenticationService.Instance.GetNotificationsAsync();

                for (int i = 0; i < notifications.Count; i++)
                {
                    Debug.Log("Notification " + i);
                    Debug.Log(notifications[i].Id);
                    Debug.Log(notifications[i].ProjectId);
                    Debug.Log(notifications[i].CaseId);
                    Debug.Log(notifications[i].CreatedAt);
                    Debug.Log(notifications[i].Message);
                    Debug.Log(notifications[i].Type);
                }

                PlayerPrefs.SetString("storedNotificationDate", lastNotificationDate);
            }*/

            if (lastNotificationDate != null)
            {
                Debug.Log("Last Notification Date: " + lastNotificationDate);

                List<Notification> notifications = await AuthenticationService.Instance.GetNotificationsAsync();

                for (int i = 0; i < notifications.Count; i++)
                {
                    Debug.Log("Notification " + i);
                    Debug.Log(notifications[i].Id);
                    Debug.Log(notifications[i].ProjectId);
                    Debug.Log(notifications[i].CaseId);
                    Debug.Log(notifications[i].CreatedAt);
                    Debug.Log(notifications[i].Message);
                    Debug.Log(notifications[i].Type);
                }

                PlayerPrefs.SetString("storedNotificationDate", lastNotificationDate);
            }
        }

#if UNITY_ANDROID || UNITY_EDITOR
        public void SignIn(Action newCallback = null, Action<bool, string> newFailCallback = null)
        {
            callback = newCallback;
            failCallback = newFailCallback;

            if (services.googleSignIn)
            {
                callback?.Invoke();
            }
            else
            {
                SignInToGoogleManual();
            }
        }

        void SignInToGoogleAuto(bool forceLinking = false)
        {
            PlayGamesPlatform.Instance.Authenticate((status) =>
            {
                if (status == SignInStatus.Success)
                {
                    googlePlayerId = PlayGamesPlatform.Instance.GetUserId();

                    if (logData)
                    {
                        Debug.Log("Google Play Sign In Auto Success!");
                    }

                    PlayGamesPlatform.Instance.RequestServerSideAccess(true, (code) =>
                    {
                        if (code == "" || code == null)
                        {
                            errorManager.Throw(
                                Types.ErrorType.Code,
                                "AuthManager.cs -> LogInToGoogleAuto()",
                                "Token is empty!"
                            );
                        }
                        else
                        {
                            googlePlayToken = code;

                            if (logData)
                            {
                                Debug.Log("Google Play Servers Side Access Success!");
                                Debug.Log("Auth Code: " + code);
                            }

                            if (currentAuthType == AuthType.Anonym)
                            {
                                LinkToAccount(AuthType.Google, false, forceLinking);
                            }
                        }
                    });
                }
                else
                {
                    if (status != SignInStatus.Canceled || !Application.isEditor)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "AuthManager.cs -> LogInToGoogleAuto()",
                            "Google logging in failed! Status: " + status
                        );
                    }

                    if (logData)
                    {
                        Debug.LogWarning("Attempting to sing in manually.");
                    }

                    SignInToGoogleManual(false);
                }
            });
        }

        void SignInToGoogleManual(bool trulyManually = true)
        {
            PlayGamesPlatform.Instance.ManuallyAuthenticate((status) =>
            {
                if (status == SignInStatus.Success)
                {
                    if (logData)
                    {
                        Debug.Log("Google Play Sign In Manual Success!");
                    }

                    PlayGamesPlatform.Instance.RequestServerSideAccess(true, (code) =>
                    {
                        if (code == "" || code == null)
                        {
                            errorManager.Throw(
                                Types.ErrorType.Code,
                                "AuthManager.cs -> LogInToGoogleManual()",
                                "Token is empty!"
                            );

                            if (trulyManually)
                            {
                                failCallback?.Invoke(false, "token");
                            }
                        }
                        else
                        {
                            googlePlayToken = code;

                            if (logData)
                            {
                                Debug.Log("Google Play Servers Side Access Success!");
                                Debug.Log("Auth Code: " + code);
                            }

                            if (currentAuthType == AuthType.Anonym)
                            {
                                LinkToAccount(AuthType.Google);
                            }
                        }
                    });
                }
                else
                {
                    if (status != SignInStatus.Canceled || !Application.isEditor)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "AuthManager.cs -> LogInToGoogleManual()",
                            "Google logging in failed! Status: " + status
                        );
                    }

                    if (trulyManually)
                    {
                        if (status == SignInStatus.InternalError)
                        {
                            failCallback?.Invoke(false, "login");
                        }
                        else
                        {
                            failCallback?.Invoke(true, "");
                        }
                    }
                }
            });
        }
#elif UNITY_IOS || UNITY_EDITOR
        public async void SignIn(Action newCallback = null, Action<bool, string> newFailCallback = null)
        {
            callback = newCallback;
            failCallback = newFailCallback;

            if (services.appleSignIn)
            {
                callback?.Invoke();
            }
            else
            {
               await SignInToApple(true);
            }
        }

        async Task SignInToApple(bool isManual = false)
        {
            Debug.LogWarning("Apple sign in isn't setup yet!");
        }
#endif     
    }
}
