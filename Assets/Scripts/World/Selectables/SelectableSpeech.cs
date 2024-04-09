using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class SelectableSpeech : MonoBehaviour
    {
        // Variables
        public float speechTimeOut = 3f;
        public string speechCode = "";

        // References
        private Selectable selectable;
        private I18n LOCALE;
        private CharSpeech charSpeech;

        void Start()
        {
            // Cache
            selectable = GetComponent<Selectable>();
            LOCALE = I18n.Instance;
            charSpeech = CharMain.Instance.charSpeech;

            if (speechCode == "")
            {
                speechCode = gameObject.name;
            }
        }

        public string GetSpeech()
        {
            // if (selectable.canBeTapped && charSpeech.canSpeakRandomly && !charSpeech.isSpeaking && !charSpeech.isTimeOut)
            if (selectable.canBeTapped)
            {
                string newSpeechCode = "speech_" + speechCode + "_" + selectable.spriteOrder;

                int speechCount = LOCALE.GetNestedLength(newSpeechCode, true);

                if (speechCount >= 0)
                {
                    int randomInt = Random.Range(0, speechCount);

                    return LOCALE.Get(newSpeechCode, randomInt);
                }
                else if (speechCount == 0)
                {
                    return LOCALE.Get(newSpeechCode);
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
    }
}