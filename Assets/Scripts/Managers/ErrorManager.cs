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
        public bool throwTestException = false;

        private bool readyToThrowTestException = false;

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

            readyToThrowTestException = true;

            CrashReportHandler.SetUserMetadata("playerId", authManager.playerId);
        }

        void OnValidate()
        {
            if (throwTestException)
            {
                throwTestException = false;

                if (readyToThrowTestException)
                {
                    Debug.LogException(new Exception(Types.ErrorType.Code + " Error, Code: n/a, Message: This is a test exception!, At: " + GetType().Name + " -> Start()"));
                }
                else
                {
                    Debug.LogWarning("\"readyToThrowTestException\" is false!");
                }
            }
        }

        public void Throw(Types.ErrorType type, string className, string errorMessage, string errorCode = "n/a", bool showToPlayer = false, [CallerMemberName] string functionName = "")
        {
            // Throw and log the exception
            Debug.LogException(new Exception(type + " Error, Code: " + errorCode + ", Message: " + errorMessage + ", At: " + "CLASS_NAME" + " -> " + functionName));

            // Notify the player
            if (showToPlayer && noteMenu != null)
            {
                noteMenu.Open("error_type_" + type.ToString(), new List<string>() { errorMessage });
            }
        }

        public void ThrowWarning(Types.ErrorType type,  string errorMessage, string errorCode = "", [CallerMemberName] string functionName = "")
        {
            Debug.LogWarning(type + " Warning, Code: " + errorCode + ", Message: " + errorMessage + ", At: " + "CLASS_NAME" + " -> " + functionName);
        }

        public void FindUsed(string objectName,  [CallerMemberName] string functionName = "")
        {
            Debug.LogWarning("FIND was used for " + objectName + ", At:" + "CLASS_NAME" + " -> " + functionName + " . FIND's a performance hit and needs to be FIXED!");
        }
    }
}