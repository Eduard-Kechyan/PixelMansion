using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class CharMain : MonoBehaviour
    {
        // Variable
        public WorldDataManager worldDataManager;

        // References
        private Selector selector;
        [HideInInspector]
        public CharSpeech charSpeech;
        [HideInInspector]
        public CharMove charMove;
        [HideInInspector]
        public CharOrderSetter charOrderSetter;
        private SpeechBubble speechBubble;

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

            // Cache
            charSpeech = GetComponent<CharSpeech>();
            charMove = GetComponent<CharMove>();
            charOrderSetter = GetComponent<CharOrderSetter>();
        }

        void Start()
        {
            // Cache
            speechBubble = GameRefs.Instance.worldGameUI.GetComponent<SpeechBubble>();
            selector = worldDataManager.GetComponent<Selector>();
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

        public void SelectSelectableAtPosition(Vector2 position)
        {
            Selectable selectable = selector.SelectAndReturn(position);

            if (selectable == null)
            {
                return;
            }

            charMove.SetDestination(position, CheckIfInRoom(selectable));

            Debug.Log(selectable);

            SelectableSpeech selectableSpeech = selectable.GetComponent<SelectableSpeech>();

            Debug.Log(selectableSpeech);

            charSpeech.Speak(selectableSpeech.GetSpeech(), false);
        }

        public void SetRoom(Vector2 roomCenter, bool waitForData = false)
        {
            StartCoroutine(WaitForData(roomCenter, waitForData));
        }

        IEnumerator WaitForData(Vector2 roomCenter, bool waitForData = false)
        {
            while (waitForData && !worldDataManager.loaded)
            {
                yield return null;
            }

            charMove.SetPosition(roomCenter, () =>
            {
                charOrderSetter.CheckArea();
            });
        }

        // TODO - Do we really need this method?
        bool CheckIfInRoom(Selectable selectable)
        {
            bool inRoom = false;

            string selectableRoomName;

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

        public void Hide()
        {
            charSpeech.Closed();
            charSpeech.enabled = false;

            speechBubble.Close();

            charMove.StopMoving();

            charOrderSetter.FadeOut();
        }

        public void Show()
        {
            charSpeech.enabled = true;

            charMove.ContinueMoving();

            charOrderSetter.FadeIn();
        }
    }
}