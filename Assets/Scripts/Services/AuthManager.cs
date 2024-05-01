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
        public bool logDataAlt = false;
        public MessageMenu messageMenu;

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

        void Awake()
        {
            if (!Debug.isDebugBuild || Application.isEditor)
            {
                logData = false;
            }

            if (Debug.isDebugBuild && !Application.isEditor)
            {
                logDataAlt = true;
            }
        }

        void Start()
        {
            // Cache
            services = Services.Instance;
            errorManager = ErrorManager.Instance;

            SetPlayerIdTemp();

            StartCoroutine(WaitForUnityServices());
        }

        IEnumerator WaitForUnityServices()
        {
            while (!services.unityServicesAvailable)
            {
                yield return null;
            }

            if (Application.isEditor)
            {
                if (Application.internetReachability != NetworkReachability.NotReachable)
                {
                    Init();
                }
            }
            else
            {
                Init();
            }
        }

        async void Init()
        {
            if (logDataAlt)
            {
                Debug.Log("AUTH-0");
            }
            SetupEvents();

            if (AuthenticationService.Instance.SessionTokenExists || Application.isEditor)
            {
                if (logDataAlt)
                {
                    Debug.Log("AUTH-1");
                }
                await SignInAnonymOrCachedAsync();
            }

#if UNITY_ANDROID || UNITY_EDITOR
            PlayGamesPlatform.Activate();

            if (!Application.isEditor)
            {
                if (logDataAlt)
                {
                    Debug.Log("AUTH-2");
                }
                SignInToGoogleAuto();
            }

            /* if (currentAuthType == AuthType.Anonym || AuthenticationService.Instance.IsExpired)
             {
                 SignInToGoogleAuto();
             }*/
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
                // ERROR
                    errorManager.Throw(
                        Types.ErrorType.Code,
                        GetType().Name,
                        ex.Message,
                        ex.ErrorCode.ToString()
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

        async void SetPlayerData()
        {
            PlayerInfo playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();

            playerId = playerInfo.Id;

            PlayerPrefs.SetString("playerId", playerId);
        }

        async Task SignInAnonymOrCachedAsync()
        {
            bool tokenExists = AuthenticationService.Instance.SessionTokenExists;

            if (tokenExists)
            {
                if (logDataAlt)
                {
                    Debug.Log("AUTH-3");
                }
                currentAuthType = services.GetSignInData();
            }
            else
            {
                if (logDataAlt)
                {
                    Debug.Log("AUTH-4");
                }
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

                SetPlayerData();
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
                        GetType().Name,
                        ex.Message,
                        ex.ErrorCode.ToString()
                    );
                }
                else
                {
                    // WARNING
                    errorManager.ThrowWarning(
                        Types.ErrorType.Code,
                        GetType().Name,
                        "No internet for authentication!"
                    );
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

                SetPlayerData();

                if (logData)
                {
                    Debug.Log(type + " linking Success");
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

                if (logData)
                {
                    Debug.Log(playerIdentities.Count);
                }

                for (int i = 0; i < playerIdentities.Count; i++)
                {
                    Debug.Log("Player identity: " + i);
                    Debug.Log(playerIdentities[i].UserId);
                    Debug.Log(playerIdentities[i].TypeId);
                }

                Debug.Log(playerInfo.GetGooglePlayGamesId());

                if (logData)
                {
                    Debug.Log(playerInfo.GetGooglePlayGamesId());

                    Debug.Log(type + " user already linked with another account!");
                }
            }
            catch (AuthenticationException ex)
            {
                // ERROR
                errorManager.Throw(
                    Types.ErrorType.Code,
                    GetType().Name,
                    ex.Message,
                    ex.ErrorCode.ToString()
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
                    GetType().Name,
                    ex.Message,
                    ex.ErrorCode.ToString()
                );

                if (isManual)
                {
                    failCallback?.Invoke(false, "unknown");
                }
            }
        }

        async void SignInToAccount(AuthType type, bool isManual = false)
        {
            currentAuthType = type;

            try
            {
                if (type == AuthType.Google)
                {
                    await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(googlePlayToken);
                }
                else
                {
                    await AuthenticationService.Instance.SignInWithAppleGameCenterAsync(appleSignature, appleTeamPlayerId, applePublicKeyUrl, appleSalt, appleTimeStamp);
                }

                services.SetSignInData(type);

                SetPlayerData();

                if (logData)
                {
                    Debug.Log(type + " sign in Success");
                }

                if (isManual)
                {
                    callback?.Invoke();
                }
            }
            catch (AuthenticationException ex)
            {
                // ERROR
                errorManager.Throw(
                    Types.ErrorType.Code,
                    GetType().Name,
                    ex.Message,
                    ex.ErrorCode.ToString()
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
                    GetType().Name,
                    ex.Message,
                    ex.ErrorCode.ToString()
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

        // NOTE - As of February 17, 2024, application need to comply with the DSA (Digital Service Act) Notifications
        public async Task HandleDSANotifications(Action notificationsCallback)
        {
            if (!services.anonymousSignIn && !services.googleSignIn && !services.appleSignIn)
            {
                notificationsCallback();
                return;
            }


            List<Notification> notifications = null;

            // Get notifications
            try
            {
                string lastNotificationDate = AuthenticationService.Instance.LastNotificationDate;
                int storedNotificationDate = PlayerPrefs.GetInt("storedNotificationDate");

                if (lastNotificationDate != null && long.Parse(lastNotificationDate) > storedNotificationDate)
                {
                    notifications = await AuthenticationService.Instance.GetNotificationsAsync();
                }
            }
            catch (AuthenticationException ex)
            {
                notifications = ex.Notifications;

                // ERROR
                errorManager.Throw(
                    Types.ErrorType.Code,
                    GetType().Name,
                    ex.Message,
                    ex.ErrorCode.ToString()
                );
            }
            catch (Exception ex)
            {
                // ERROR
                errorManager.Throw(
                    Types.ErrorType.Code,
                    GetType().Name,
                    ex.Message
                );
            }

            // Handle notifications
            if (notifications != null && notifications.Count > 0 && messageMenu != null)
            {
                StartCoroutine(HandleNotifications(notifications, notificationsCallback));
            }
            else
            {
                notificationsCallback();
            }
        }

        IEnumerator HandleNotifications(List<Notification> notifications, Action notificationsCallback)
        {
            for (int i = 0; i < notifications.Count; i++)
            {
                Debug.Log("Notification " + i);
                Debug.Log(notifications[i].Id);
                Debug.Log(notifications[i].ProjectId);
                Debug.Log(notifications[i].CaseId);
                Debug.Log(notifications[i].CreatedAt);
                Debug.Log(notifications[i].Message);
                Debug.Log(notifications[i].Type);

                yield return WaitForNotificationToBeRead(notifications[i]);

                if (i == (notifications.Count - 1))
                {
                    notificationsCallback();
                }
            }
        }


        IEnumerator WaitForNotificationToBeRead(Notification notification)
        {
            bool loading = true;

            messageMenu.Open(Types.MessageType.DigitalServiceAct, "", notification.Message, "", false, () =>
            {
                NotificationRead(notification);

                loading = false;
            });

            while (loading)
            {
                yield return null;
            }
        }

        void NotificationRead(Notification notification)
        {
            long lastNotificationDate = long.Parse(notification.CreatedAt);
            int storedNotificationDate = PlayerPrefs.GetInt("storedNotificationDate");

            if (lastNotificationDate > storedNotificationDate)
            {
                PlayerPrefs.SetInt("storedNotificationDate", (int)lastNotificationDate);

                PlayerPrefs.Save();
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
            PlayGamesPlatform.Instance.Authenticate(async (status) =>
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
                                GetType().Name,
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
                                if (logDataAlt)
                                {
                                    Debug.Log("AUTH-5");
                                }
                                LinkToAccount(AuthType.Google, false, forceLinking);
                            }
                            else if (currentAuthType == AuthType.Unknown)
                            {
                                if (logDataAlt)
                                {
                                    Debug.Log("AUTH-6");
                                }
                                SignInToAccount(AuthType.Google, false);
                            }
                            else
                            {
                                if (logDataAlt)
                                {
                                    Debug.Log("AUTH-67");
                                }
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
                            GetType().Name,
                            "Google logging in failed! Status: " + status
                        );
                    }

                    if (logData)
                    {
                        Debug.LogWarning("Attempting to sing in manually.");
                    }

                    if (Application.isEditor)
                    {
                        if (currentAuthType != AuthType.Anonym)
                        {
                            await SignInAnonymOrCachedAsync();
                        }
                    }
                    else
                    {
                        SignInToGoogleManual(false, forceLinking);
                    }
                }
            });
        }

        void SignInToGoogleManual(bool trulyManually = true, bool forceLinking = false)
        {
            PlayGamesPlatform.Instance.ManuallyAuthenticate(async (status) =>
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
                                 GetType().Name,
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
                                 LinkToAccount(AuthType.Google, false, forceLinking);
                             }
                             else
                             {
                                 SignInToAccount(AuthType.Google, false);
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
                             GetType().Name,
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

                     if (currentAuthType != AuthType.Anonym)
                     {
                         await SignInAnonymOrCachedAsync();
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
