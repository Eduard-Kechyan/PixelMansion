using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Merge
{
    public class ErrorManager : MonoBehaviour
    {
        // References
        private NoteMenu noteMenu;
        private AuthManager authManager;

        // Instance
        public static ErrorManager Instance;

        [Serializable]
        public class ErrorData
        {
            public string playerId;
            public string source;
            public string message;
            public string code;
            public string type;
            public string created;
        }

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
            if (GameRefs.Instance != null)
            {
                noteMenu = GameRefs.Instance.noteMenu;
            }
            authManager=Services.Instance.GetComponent<AuthManager>();
        }

        public void Throw(Types.ErrorType type, string errorSource, string errorMessage, string errorCode = "", bool sendError = true, bool logError = true, bool showToPlayer = false)
        {
            // Create and send the error to the backend
            if (sendError)
            {
                ErrorData newErrorData = new ErrorData
                {
                    playerId = authManager.playerId,
                    source = errorSource,
                    message = errorMessage,
                    code = errorCode,
                    type = type.ToString(),
                    created = DateTime.UtcNow.ToString()
                };

                // TODO - Send JsonConvert.SerializeObject(newErrorData) to the server
            }

            // Log the warning
            if (logError)
            {
                if (errorCode == "")
                {
                    Debug.LogWarning(type + " Error, Message: " + errorMessage + ", At: " + errorSource);
                }
                else
                {
                    Debug.LogWarning(type + " Error, Code: " + errorCode + ", Message: " + errorMessage + ", At: " + errorSource);
                }
            }

            // Notify the player
            if (showToPlayer && noteMenu != null)
            {
                noteMenu.Open("error_type_" + type.ToString(), new List<string>() { errorMessage });
            }
        }

        public void FindUsed(string objectName)
        {
            Debug.LogWarning("FIND was used for " + objectName + ". FIND's a performance hit and needs to be FIXED!");
        }
    }
}