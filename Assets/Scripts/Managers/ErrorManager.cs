using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Locale;

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
    public class FullError
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

    public void Throw(Types.ErrorType type, string errorCode, bool showToUser = false, bool sendError = false)
    {
        if (showToUser && noteMenu != null)
        {
            string[] notes = new string[] { errorCode };

            noteMenu.Open("error_type_" + type.ToString(), notes);
        }
        else
        {
            Debug.LogWarning(type.ToString() + " Error: " + errorCode);
        }

        if (sendError)
        {
            apiCalls.SendError("\"code\": \"" + errorCode + "\"");
        }
    }

    public void ThrowFull(Types.ErrorType type, string errorSource,string errorMessage, string errorCode, bool sendError = false)
    {
        Debug.LogWarning(type.ToString() + " Error, Code: " + errorCode + ", Message:" + errorMessage);

        FullError newFullerror = new FullError
        {
            userId = GameData.Instance.userId,
            source = errorSource,
            message = errorMessage,
            code = errorCode,
            type = type.ToString(),
            created = DateTime.UtcNow.ToString()
        };

        if (sendError)
        {
            apiCalls.SendError(JsonConvert.SerializeObject(newFullerror));
        }
    }

    public void FindUsed(string objectName)
    {
        Debug.LogWarning("FIND was used for " + objectName + ", which was NULL! That's a performance hit and needs to be FIXED!");
    }
}
