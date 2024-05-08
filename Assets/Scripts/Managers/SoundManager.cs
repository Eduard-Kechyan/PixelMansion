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
        public MusicType currentMusic;

        // Classes
        [Serializable]
        public class MusicClip
        {
            public AudioClip clip;
            public float volume = 1f;
            public MusicType type;
        }

        [Serializable]
        public class SoundClip
        {
            public AudioClip clip;
            public float volume = 1f;
            public SoundType type;
        }

        // Enums
        public enum MusicType
        {
            Loading,
            World,
            Merge,
            Magical,
        };

        public enum SoundType
        {
            None,
            Merge,
            Generate,
            UnlockLock,
            OpenCrate,
            LevelUp,
            LevelUpIndicator,
            Transition,
            Experience,
            Energy,
            Gold,
            Gems,
            Pop,
            Buzz,
            CleanDust,
            CleanGlass,
            On,
            Off,
            RoomUnlocking,
            Tap
        };

        // References
        private Settings settings;
        private SceneLoader sceneLoader;

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
            sceneLoader = GameRefs.Instance.sceneLoader;
        }

        //////// Music ////////

        public void SetVolumeMusic(float volume)
        {
            sourceMusic.volume = volume;
        }

        public void PlayMusic(MusicType musicType)
        {
            currentMusic = musicType;

            bool found = false;

            if (settings == null)
            {
                settings = Settings.Instance;
            }

            if (musicType == MusicType.Loading && settings.musicOn)
            {
                sourceMusic.volume = 1f;
            }

            foreach (MusicClip m in musicClips)
            {
                if (m.type == musicType)
                {
                    if (settings.musicOn)
                    {
                        sourceMusic.volume = m.volume;
                    }

                    sourceMusic.clip = m.clip;

                    sourceMusic.Play();

                    found = true;

                    break;
                }
            }

            if (!found)
            {
                Debug.Log("Music \"" + musicType.ToString() + "\" not found!");
            }
        }

        public void PlaySceneMusic()
        {
            Debug.Log("Playing Scene Music");
            SceneLoader.SceneType sceneType = sceneLoader.GetScene();
            Debug.Log(sceneType);

            switch (sceneType)
            {
                case SceneLoader.SceneType.World:
                    if (PlayerPrefs.HasKey("tutorialFinished"))
                    {
                        Debug.Log("A");
                        PlayMusic(MusicType.World);
                    }
                    else
                    {
                        Debug.Log("B");
                        PlayMusic(MusicType.Magical);
                    }
                    break;
                case SceneLoader.SceneType.Merge:
                    Debug.Log("C");
                    PlayMusic(MusicType.Merge);
                    break;
                default:
                    Debug.Log("D");
                    break;
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

        //////// Sound ////////

        public void SetVolumeSound(float volume)
        {
            sourceSound.volume = volume;
        }

        public void PlaySound(SoundType soundType, string clipName = "")
        {
            bool found = false;

            foreach (SoundClip s in soundClips)
            {
                if (soundType == SoundType.None ? s.clip.name == clipName : s.type == soundType)
                {
                    if (settings.soundOn)
                    {
                        sourceSound.volume = s.volume;
                    }

                    sourceSound.PlayOneShot(s.clip);

                    found = true;

                    break;
                }
            }

            if (!found)
            {
                Debug.Log("Sound \"" + soundType.ToString() + "\" not found!");
            }
        }

        public float GetSoundLength(SoundType soundType, string clipName = "")
        {
            foreach (SoundClip s in soundClips)
            {
                if (soundType == SoundType.None ? s.clip.name == clipName : s.type == soundType)
                {
                    return s.clip.length + 0.05f;
                }
            }

            return 0f;
        }

        public static void Tap(Action callback = null, SoundType soundType = SoundType.Generate, bool dummy = false) // TODO - Change SoundType.Generate to SoundType.Tap 
        {
            Instance.Tap(callback);
        }

        public void Tap(Action callback = null, SoundType soundType = SoundType.Generate) // TODO - Change SoundType.Generate to SoundType.Tap 
        {
            Instance.PlaySound(soundType);

            callback?.Invoke();
        }
    }
}