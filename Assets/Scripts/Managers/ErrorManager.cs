using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Locale;

public class ErrorManager : MonoBehaviour
{
    // References
    private NoteMenu noteMenu;

    // Instances
    private I18n LOCALE;

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
        if (GameRefs.Instance != null)
        {
            noteMenu = GameRefs.Instance.noteMenu;
        }

        // Cache instances
        LOCALE = I18n.Instance;
    }

    public void Throw(Types.ErrorType type, string errorCode, bool showToUser = false)
    {
        if (showToUser && noteMenu != null)
        {
            string[] notes = new string[] { errorCode };

            noteMenu.Open("error_type_" + type.ToString(), notes);
        }
        else
        {
            Debug.Log(type.ToString() + " Error: " + errorCode);
        }
    }
}
