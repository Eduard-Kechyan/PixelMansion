using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Variables
    public AudioClip[] soundClips;
    public AudioClip[] musicClips;
    public AudioSource sourceSound;
    public AudioSource sourceMusic;

    // Instance
    public static SoundManager Instance;

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

    //////// Sound ////////

    public void SetVolumeSound(float volume)
    {
        sourceSound.volume = volume;
    }

    public void PlaySound(string clipName = "", float volume = 1f, AudioClip clip = null)
    {
        //sourceSound.volume = volume;

        if (clip != null)
        {
            sourceSound.PlayOneShot(clip);
        }
        else
        {
            foreach (AudioClip i in soundClips)
            {
                if (i.name == clipName + "Sound")
                {
                    sourceSound.PlayOneShot(i);
                }
            }
        }
    }

    //////// Music ////////

    public void SetVolumeMusic(float volume)
    {
        sourceMusic.volume = volume;
    }

    public void PlayMusic(string clipName = "", float volume = 1f, AudioClip clip = null)
    {
        //sourceMusic.volume = volume;

        if (clip != null)
        {
            sourceMusic.clip = clip;
        }
        else
        {
            if (clipName == "Loading")
            {
                sourceMusic.volume = 1f;
            }

            foreach (AudioClip i in musicClips)
            {
                if (i.name == clipName + "Music")
                {
                    sourceMusic.clip = i;
                }
            }
        }

        sourceMusic.Play();
    }

    public void FadeInMusic(float seconds)
    {
        StartCoroutine(FadeCoroutine(seconds, sourceMusic.volume, 1));
    }

    public void FadeOutMusic(float seconds)
    {
        StartCoroutine(FadeCoroutine(seconds, sourceMusic.volume, 0));
    }

    IEnumerator FadeCoroutine(float seconds, float from, float to)
    {
        var timePassed = 0f;
        while (timePassed < seconds)
        {
            var factor = timePassed / seconds;

            sourceMusic.volume = Mathf.Lerp(from, to, factor);

            timePassed += Mathf.Min(Time.deltaTime, seconds - timePassed);

            yield return null;
        }
    }
}
