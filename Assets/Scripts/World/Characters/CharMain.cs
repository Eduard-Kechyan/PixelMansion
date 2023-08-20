using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class CharMain : MonoBehaviour
    {
        // References
        [HideInInspector]
        public CharSpeech charSpeech;
        [HideInInspector]
        public CharMove charMove;
        [HideInInspector]
        public CharOrderSetter charOrderSetter;

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
            charOrderSetter = GetComponent<CharOrderSetter>();
        }

        public void SelectableTapped(Vector2 position, Selectable selectable = null)
        {
            charMove.SetDestination(position, CheckIfInRoom(selectable));

            if (selectable != null)
            {
                charMove.SetDestination(position, CheckIfInRoom(selectable));

                SelectableSpeech selectableSpeech = selectable.GetComponent<SelectableSpeech>();

                charSpeech.Speak(selectableSpeech.GetSpeech(), false);
                //charSpeech.TryToSpeak(selectableSpeech.GetSpeech(), false);
            }
            else
            {
                charMove.SetDestination(position);
            }
        }

        // TODO - Do we really need this method?
        bool CheckIfInRoom(Selectable selectable)
        {
            bool inRoom = false;

            string selectableRoomName = "";

            // Get selectable's room name
            if (selectable.type == Selectable.Type.Furniture)
            {
                selectableRoomName = selectable.transform.parent.transform.parent.name;
            }
            else
            {
                selectableRoomName = selectable.transform.parent.name;
            }

            // Check if we got the selectable's room name
            if (selectableRoomName != "" && selectableRoomName == charOrderSetter.currentRoomName)
            {
                inRoom = true;
            }

            return inRoom;
        }
    }
}