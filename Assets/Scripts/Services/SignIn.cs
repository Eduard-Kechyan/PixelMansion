using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class SignIn : MonoBehaviour
    {
        // Variables
        private Action<bool> callback;
        private Action<string> failCallback;

        public enum SignInType
        {
            Google,
            Facebook,
            Apple,
        }

        // References
        private Services services;
        private ErrorManager errorManager;

        void Start()
        {
            // Cache
            services = Services.Instance;
            errorManager = ErrorManager.Instance;
        }

        public void LogIn(SignInType type, Action<bool> newCallback = null, Action<string> newFailCallback = null)
        {
            callback = newCallback;
            failCallback = newFailCallback;

            switch (type)
            {
                case SignInType.Google:
                    if (services.googleSignIn)
                    {
                        callback?.Invoke(false);
                    }
                    else
                    {
                        LogInToGoogle();
                    }
                    break;
                case SignInType.Facebook:
                    if (services.facebookSignIn)
                    {
                        callback?.Invoke(false);
                    }
                    else
                    {
                        LogInToFacebook();
                    }
                    break;
                default: // SignInType.Apple:
                    if (services.appleSignIn)
                    {
                        callback?.Invoke(false);
                    }
                    else
                    {
                        LogInToApple();
                    }
                    break;
            }
        }

        void LogInToGoogle()
        {
            errorManager.Throw(Types.ErrorType.Code, "SignIn.cs -> LogInToGoogle()", "Google sign in isn't setup yet!");

            failCallback?.Invoke("unknown");
        }

        void LogInToFacebook()
        {
            errorManager.Throw(Types.ErrorType.Code, "SignIn.cs -> LogInToFacebook()", "Facebook sign in isn't setup yet!");

            failCallback?.Invoke("unknown");
        }

        void LogInToApple()
        {
            errorManager.Throw(Types.ErrorType.Code, "SignIn.cs -> LogInToApple()", "Apple sign in isn't setup yet!");

            failCallback?.Invoke("unknown");
        }
    }
}
