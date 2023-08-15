using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharMain : MonoBehaviour
{
    // References
    private CharSpeech charSpeech;
    private CharMove charMove;

    // Instance
    public static CharMain Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        charSpeech = GetComponent<CharSpeech>();
        charMove = GetComponent<CharMove>();
    }

    public void SelectableTapped(Vector2 position, Selectable selectable = null)
    {
        charMove.SetDestination(position);

        if (selectable != null)
        {
            if (!charSpeech.isSpeaking && !charSpeech.isTimeOut)
            {
                SelectableSpeech selectableSpeech = selectable.GetComponent<SelectableSpeech>();

                charSpeech.TryToSpeak(selectableSpeech.GetSpeech(), false);
            }
        }
    }
}
