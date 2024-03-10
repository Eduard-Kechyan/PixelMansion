using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.CrashReportHandler;

namespace Merge
{
    public class ErrorManager : MonoBehaviour
    {
        // Variables
        public bool diagnosticsEnabled = false;

#if UNITY_EDITOR
        [Space(10)]
        public bool throwTestException = false;
        public bool throwTestWarning = false;

        private bool readyToThrowTestErrors = false;
#endif

        // References
        private AuthManager authManager;
        private NoteMenu noteMenu;

        // Instance
        public static ErrorManager Instance;

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

                diagnosticsEnabled = false;

                CrashReportHandler.enableCaptureExceptions = false;
            }
        }

        void Start()
        {
            // Cache
            authManager = Services.Instance.GetComponent<AuthManager>();
            if (GameRefs.Instance != null)
            {
                noteMenu = GameRefs.Instance.noteMenu;
            }

            // Init
            StartCoroutine(WaitForPlayerId());
        }

        IEnumerator WaitForPlayerId()
        {
            while (authManager.playerId == "" || authManager.playerId.Contains("ER_"))
            {
                yield return null;
            }

#if UNITY_EDITOR
            readyToThrowTestErrors = true;
#endif

            CrashReportHandler.SetUserMetadata("playerId", authManager.playerId);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (throwTestException)
            {
                throwTestException = false;

                if (readyToThrowTestErrors)
                {
                    Debug.LogException(new Exception(Types.ErrorType.Code + " Error, Code: n/a, Message: This is a test exception!, At: " + GetType().Name + " -> OnValidate()"));
                }
                else
                {
                    Debug.LogWarning("\"readyToThrowTestException\" is false!");
                }
            }

            if (throwTestWarning)
            {
                throwTestWarning = false;

                Debug.LogWarning(Types.ErrorType.Code + " Error, Code: n/a, Message: This is a test warning!, At: " + GetType().Name + " -> OnValidate()");
            }
        }
#endif

        public void Throw(Types.ErrorType type, string className, string errorMessage, string errorCode = "n/a", bool showToPlayer = false, [CallerMemberName] string functionName = "")
        {
            // Throw and log the exception
            string exceptionString = type + " Error, Code: " + errorCode + ", Message: " + errorMessage + ", At: " + className + ".cs" + " -> " + functionName + "()";

            Debug.LogException(new Exception(exceptionString));

            // Notify the player
            if (showToPlayer && noteMenu != null)
            {
                noteMenu.Open("error_type_" + type.ToString(), new List<string>() { errorMessage });
            }
        }

        public void ThrowWarning(Types.ErrorType type, string className, string errorMessage, string errorCode = "", [CallerMemberName] string functionName = "")
        {
            Debug.LogWarning(type + " Warning, Code: " + errorCode + ", Message: " + errorMessage + ", At: " + className + ".cs" + " -> " + functionName + "()");
        }

        public void FindUsed(string objectName, string className, [CallerMemberName] string functionName = "")
        {
            Debug.LogWarning("FIND was used for " + objectName + ", At:" + className + ".cs" + " -> " + functionName + "(). FIND's a performance hit and needs to be FIXED!");
        }

        public void ToggleDiagnostic()
        {
            if (Debug.isDebugBuild || Application.isEditor)
            {
                diagnosticsEnabled = !diagnosticsEnabled;

                CrashReportHandler.enableCaptureExceptions = diagnosticsEnabled;
            }
        }
    }
}