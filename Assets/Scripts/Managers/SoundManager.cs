using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class SoundManager : MonoBehaviour
    {
        // Variables
        [Header("Sound")]
        public SoundClip[] soundClips;
        public AudioSource sourceSound;

        [Header("Music")]
        public MusicClip[] musicClips;
        public AudioSource sourceMusic;

        [HideInInspector]
        public Types.MusicType currentMusic;

        [Serializable]
        public class MusicClip
        {
            public AudioClip clip;
            public float volume = 1f;
            public Types.MusicType type;
        }

        [Serializable]
        public class SoundClip
        {
            public AudioClip clip;
            public float volume = 1f;
            public Types.SoundType type;
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

        public void PlaySound(Types.SoundType soundType, string clipName = "")
        {
            bool found = false;

            foreach (SoundClip i in soundClips)
            {
                if (soundType == Types.SoundType.None ? i.clip.name == clipName : i.type == soundType)
                {
                    if (settings.soundOn)
                    {
                        sourceSound.volume = i.volume;
                    }

                    sourceSound.PlayOneShot(i.clip);

                    found = true;
                }
            }

            if (!found)
            {
                Debug.Log("Sound \"" + soundType.ToString() + "\" not found!");
            }
        }

        //////// Music ////////

        public void SetVolumeMusic(float volume)
        {
            sourceMusic.volume = volume;
        }

        public void PlayMusic(Types.MusicType musicType)
        {
            currentMusic = musicType;

            bool found = false;

            if (settings == null)
            {
                settings = Settings.Instance;
            }

            if (musicType == Types.MusicType.Loading && settings.musicOn)
            {
                sourceMusic.volume = 1f;
            }

            foreach (MusicClip i in musicClips)
            {
                if (i.type == musicType)
                {
                    if (settings.soundOn)
                    {
                        sourceMusic.volume = i.volume;
                    }

                    sourceMusic.clip = i.clip;

                    sourceMusic.Play();

                    found = true;
                }
            }

            if (!found)
            {
                Debug.Log("Music \"" + musicType.ToString() + "\" not found!");
            }
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
}