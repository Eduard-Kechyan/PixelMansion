using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip[] sfxClips;
    public AudioClip[] bgClips;
    public AudioSource sourceSFX;
    public AudioSource sourceBG;

    public static SoundManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
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

    public void PlaySFX(string clipName = "", AudioClip clip = null)
    {
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

    public void PlayBg(string clipName = "", AudioClip clip = null)
    {
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
        StartCoroutine(FadeCoroutine(seconds, 0, 1));
    }

    public void FadeOutBg(float seconds)
    {
        StartCoroutine(FadeCoroutine(seconds, 1, 0));
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
