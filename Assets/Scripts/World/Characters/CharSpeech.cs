using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharSpeech : MonoBehaviour
{
    // Variables
    public SpeechBubble speechBubble;
    public float speechLength = 3f;
    public float speechTimeOut = 3f;

    public bool greet = true;
    [Condition("greet", true)]
    public string[] greetings;

    [Header("Random Speech")]
    public bool canSpeakRandomly = true;
    [Condition("canSpeakRandomly", true)]
    public float randomSpeechDelay = 2f;
    [Condition("canSpeakRandomly", true)]
    public float minRandomSpeechTime = 5f;
    [Condition("canSpeakRandomly", true)]
    public float maxRandomSpeechTime = 10f;
    [Condition("canSpeakRandomly", true)]
    public string[] randomSpeech;

    [Header("States")]
    [ReadOnly]
    public bool isSpeaking = false;
    [ReadOnly]
    public bool isTimeOut = false;
    [ReadOnly]
    public bool isRandomSpeechTimeOut = false;

    private int lastRandomContent;

    // References
    private Camera cam;

    void Start()
    {
        // Cache
        cam = Camera.main;

        // Say Hello
        if (greet)
        {
            isSpeaking = true;

            StartCoroutine(GreetThePlayer());
        }
    }

    void Update()
    {
        if (Input.touchCount == 1)
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

        if (canSpeakRandomly && !isRandomSpeechTimeOut && !isSpeaking && !isTimeOut)
        {
            StartCoroutine(RandomSpeech());
        }

        // Send the position of the character to the bubble if it's speaking
        if (isSpeaking)
        {
            speechBubble.SetPos(transform.position);
        }
    }

    void Speak(string content = "", bool speakIfEmpty = true)
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

    IEnumerator StopSpeaking(bool bubbleClsoed = false)
    {
        if (bubbleClsoed)
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
        yield return new WaitForSeconds(0.5f);

        int randomGreeting = Random.Range(0, greetings.Length - 1);

        Speak(greetings[randomGreeting]);
    }

    IEnumerator RandomSpeech(bool useDelay = true)
    {
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

        if (randomSpeech.Length > 0)
        {
            lastRandomContent = GetRandomInt();

            TryToSpeak(randomSpeech[lastRandomContent]);
        }
        else
        {
            TryToSpeak("Huh? I'm out of topics to talk about. Please! Help me out!");
        }
    }

    int GetRandomInt()
    {
        int randomContent = Random.Range(0, randomSpeech.Length - 1);

        if (lastRandomContent == randomContent)
        {
            GetRandomInt();
            return 0;
        }

        return randomContent;
    }
}
