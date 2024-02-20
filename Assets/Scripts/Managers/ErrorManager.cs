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

        // Instances
        private I18n LOCALE;
        private ApiCalls apiCalls;

        // Instance
        public static ErrorManager Instance;

        [Serializable]
        public class ErrorData
        {
            public string userId;
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

            // Cache instances
            LOCALE = I18n.Instance;
            apiCalls = ApiCalls.Instance;
        }

        public void Throw(Types.ErrorType type, string errorSource, string errorMessage, string errorCode="", bool showToPlayer = false, bool sendError = true, bool logError=true)
        {
            // Log the warning
            if(logError)
            {
                if(errorCode=="")
                {
                    Debug.LogWarning(type + " Error, Message:" + errorMessage + ", At: " + errorSource);
                }
                else
                {
                    Debug.LogWarning(type + " Error, Code: " + errorCode + ", Message:" + errorMessage + ", At: " + errorSource);
                }
            }

            // Create  and send the error to the backend
            if (sendError)
            {
                ErrorData newErrorData = new ErrorData
                {
                    userId = GameData.Instance.userId,
                    source = errorSource,
                    message = errorMessage,
                    code = errorCode,
                    type = type.ToString(),
                    created = DateTime.UtcNow.ToString()
                };

                apiCalls.SendError(JsonConvert.SerializeObject(newErrorData));
            }

            // Notify the player
            if (showToPlayer && noteMenu != null)
            {
                string[] notes = new string[] { errorMessage };

                noteMenu.Open("error_type_" + type.ToString(), notes);
            }
        }

        public void FindUsed(string objectName)
        {
            Debug.LogWarning("FIND was used for " + objectName + ". FIND's a performance hit and needs to be FIXED!");
        }
    }
}