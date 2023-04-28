using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Variables
    public AudioClip[] sfxClips;
    public AudioClip[] bgClips;
    public AudioSource sourceSFX;
    public AudioSource sourceBG;

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

    //////// SFX ////////

    public void SetVolumeSFX(float volume)
    {
        sourceSFX.volume = volume;
    }

    public void PlaySFX(string clipName = "", float volume = 1f, AudioClip clip = null)
    {
        sourceSFX.volume = volume;

        if (clip != null)
        {
            sourceSFX.PlayOneShot(clip);
        }
        else
        {
            ;
            foreach (AudioClip i in sfxClips)
            {
                if (i.name == clipName + "SFX")
                {
                    sourceSFX.PlayOneShot(i);
                }
            }
        }
    }

    //////// BG ////////

    public void SetVolumeBG(float volume)
    {
        sourceBG.volume = volume;
    }

    public void PlayBg(string clipName = "", float volume = 1f, AudioClip clip = null)
    {
        sourceBG.volume = volume;

        if (clip != null)
        {
            sourceBG.clip = clip;
        }
        else
        {
            ;
            foreach (AudioClip i in bgClips)
            {
                if (i.name == clipName + "BG")
                {
                    sourceBG.clip = i;
                }
            }
        }

        sourceBG.Play();
    }

    public void FadeInBg(float seconds)
    {
        StartCoroutine(FadeCoroutine(seconds, sourceBG.volume, 1));
    }

    public void FadeOutBg(float seconds)
    {
        StartCoroutine(FadeCoroutine(seconds, sourceBG.volume, 0));
    }

    IEnumerator FadeCoroutine(float seconds, float from, float to)
    {
        var timePassed = 0f;
        while (timePassed < seconds)
        {
            var factor = timePassed / seconds;

            sourceBG.volume = Mathf.Lerp(from, to, factor);

            timePassed += Mathf.Min(Time.deltaTime, seconds - timePassed);

            yield return null;
        }
    }
}
