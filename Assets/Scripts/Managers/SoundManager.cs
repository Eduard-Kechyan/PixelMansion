using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Variables
    public SoundClip[] soundClips;
    public SoundClip[] musicClips;
    public AudioSource sourceSound;
    public AudioSource sourceMusic;


    [Serializable]
    public class SoundClip
    {
        public AudioClip clip;
        public float volume = 1f;
    }

    // References
    private Settings settings;

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

    void Start()
    {
        // Cache
        settings = Settings.Instance;
    }

    //////// Sound ////////

    public void SetVolumeSound(float volume)
    {
        sourceSound.volume = volume;
    }

    public void PlaySound(string clipName = "", AudioClip clip = null)
    {
        if (clip != null)
        {
            sourceSound.PlayOneShot(clip);
        }
        else
        {
            foreach (SoundClip i in soundClips)
            {
                if (i.clip.name == clipName + "Sound")
                {
                    if (settings.soundOn)
                    {
                        sourceSound.volume = i.volume;
                    }

                    sourceSound.PlayOneShot(i.clip);
                }
            }
        }
    }

    //////// Music ////////

    public void SetVolumeMusic(float volume)
    {
        sourceMusic.volume = volume;
    }

    public void PlayMusic(string clipName = "", AudioClip clip = null)
    {
        if (clip != null)
        {
            sourceMusic.clip = clip;
        }
        else
        {
            if (settings == null)
            {
                settings = Settings.Instance;
            }

            if (clipName == "Loading" && settings.musicOn)
            {
                sourceMusic.volume = 1f;
            }

            foreach (SoundClip i in musicClips)
            {
                if (i.clip.name == clipName + "Music")
                {
                    if (settings.soundOn)
                    {
                        sourceMusic.volume = i.volume;
                    }

                    sourceMusic.clip = i.clip;

                    sourceMusic.Play();
                }
            }
        }

        sourceMusic.Play();
    }

    public void FadeInMusic(float seconds)
    {
        StartCoroutine(FadeCoroutine(seconds, sourceMusic.volume, 1, true));
    }

    public void FadeOutMusic(float seconds)
    {
        StartCoroutine(FadeCoroutine(seconds, sourceMusic.volume, 0));
    }

    IEnumerator FadeCoroutine(float seconds, float from, float to, bool fadeIn = false)
    {
        var timePassed = 0f;
        while (timePassed < seconds)
        {
            var factor = timePassed / seconds;

            if (fadeIn)
            {
                if (settings.musicOn)
                {
                    sourceMusic.volume = Mathf.Lerp(from, to, factor);
                }
            }
            else
            {
                sourceMusic.volume = Mathf.Lerp(from, to, factor);
            }


            timePassed += Mathf.Min(Time.deltaTime, seconds - timePassed);

            yield return null;
        }
    }
}
