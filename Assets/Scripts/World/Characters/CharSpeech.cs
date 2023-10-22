using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class CharSpeech : MonoBehaviour
    {
        // Variables
        public SpeechBubble speechBubble;
        public float speechLength = 3f;
        public float speechTimeOut = 3f;

        public bool greet = true;

        [Header("Random Speech")]
        public bool canSpeakRandomly = true;
        [Condition("canSpeakRandomly", true)]
        public float randomSpeechDelay = 2f;
        [Condition("canSpeakRandomly", true)]
        public float minRandomSpeechTime = 5f;
        [Condition("canSpeakRandomly", true)]
        public float maxRandomSpeechTime = 10f;

        [Header("States")]
        [ReadOnly]
        public bool isSpeaking = false;
        [ReadOnly]
        public bool isTimeOut = false;
        [ReadOnly]
        public bool isRandomSpeechTimeOut = false;

        private int lastRandomContent;
        private int greetingsCount = 0;
        private bool randomCounted = false;

        // References
        private Camera cam;
        private SelectorUIHandler selectorUIHandler;
        private I18n LOCALE;
        private GameData gameData;

        void Start()
        {
            // Cache
            cam = Camera.main;
            selectorUIHandler = speechBubble.GetComponent<SelectorUIHandler>();
            LOCALE = I18n.Instance;
            gameData = GameData.Instance;

            // Say Hello
            if (greet && !gameData.greeted)
            {
                isSpeaking = true;

                StartCoroutine(GreetThePlayer());
            }
        }

        void Update()
        {
            if (Input.touchCount == 1 && !selectorUIHandler.isSelectorOpen)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    RaycastHit2D hit = Physics2D.Raycast(
                        cam.ScreenToWorldPoint(touch.position),
                        Vector2.zero,
                        Mathf.Infinity,
                        LayerMask.GetMask("Character")
                    );

                    if (hit || hit.collider != null)
                    {
                        TryToSpeak();
                    }
                }
            }
            else if (canSpeakRandomly && !isRandomSpeechTimeOut && !isSpeaking && !isTimeOut)
            {
                StartCoroutine(RandomSpeech());
            }
            else if (selectorUIHandler.isSelectorOpen)
            {
                speechBubble.Close();
            }

            // Send the position of the character to the bubble if it's speaking
            if (isSpeaking)
            {
                speechBubble.SetPos(transform.position);
            }
        }

        public void Speak(string content = "", bool speakIfEmpty = true)
        {
            if (content != "")
            {
                speechBubble.Open(content, transform.position, this);

                isSpeaking = true;

                if (isRandomSpeechTimeOut)
                {
                    isRandomSpeechTimeOut = false;
                    StopCoroutine(RandomSpeech());
                }

                StartCoroutine(StopSpeaking());
            }
            else if (speakIfEmpty)
            {
                StartCoroutine(RandomSpeech(false));
            }
        }

        public void TryToSpeak(string content = "", bool speakIfEmpty = true)
        {
            if (!isSpeaking && !isTimeOut)
            {
                Speak(content, speakIfEmpty);
            }
        }

        public void Closed()
        {
            StartCoroutine(StopSpeaking(true));
        }

        IEnumerator StopSpeaking(bool bubbleClosed = false)
        {
            if (bubbleClosed)
            {
                isSpeaking = false;

                isTimeOut = true;

                StartCoroutine(StopTimeOut());
            }
            else
            {
                yield return new WaitForSeconds(speechLength);

                isSpeaking = false;

                isTimeOut = true;

                speechBubble.Close();

                StartCoroutine(StopTimeOut());
            }
        }

        IEnumerator StopTimeOut()
        {
            yield return new WaitForSeconds(speechTimeOut);

            isTimeOut = false;
        }

        IEnumerator GreetThePlayer()
        {
            greetingsCount = LOCALE.GetLength("speech_greeting_");

            yield return new WaitForSeconds(0.5f);

            int randomGreeting = Random.Range(0, greetingsCount);

            gameData.greeted = true;

            // TODO - Get player name and change Eduard
            Speak(string.Format(LOCALE.Get("speech_greeting_" + randomGreeting), "Eduard"));
        }

        IEnumerator RandomSpeech(bool useDelay = true)
        {
            int randomCount = 0;

            if (!randomCounted)
            {
                randomCounted = true;

                randomCount = LOCALE.GetLength("speech_" + gameObject.name + "_random_");
            }

            isRandomSpeechTimeOut = true;

            if (useDelay)
            {
                yield return new WaitForSeconds(randomSpeechDelay);
            }

            int waitTime = Mathf.RoundToInt(Random.Range(minRandomSpeechTime, maxRandomSpeechTime));

            if (useDelay)
            {
                yield return new WaitForSeconds(waitTime);
            }

            if (randomCount > 0)
            {
                lastRandomContent = GetRandomInt(randomCount);

                int randomSpeech = Random.Range(0, lastRandomContent);

                TryToSpeak(LOCALE.Get("speech_" + gameObject.name + "_random_" + randomSpeech));
            }
            else
            {
                TryToSpeak("Huh? I'm out of topics to talk about. Please! Help me out!");
            }
        }

        int GetRandomInt(int randomCount)
        {
            int randomContent = Random.Range(0, randomCount);

            if (lastRandomContent == randomContent)
            {
                GetRandomInt(randomCount);
                return 0;
            }

            return randomContent;
        }
    }
}