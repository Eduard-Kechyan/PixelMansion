using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Merge
{
    public class SceneLoader : MonoBehaviour
    {
        // Variables
        public TransitionUI transitionUI;
        public float duration = 0.6f;
        public float fadeDuration = 0.5f;

        [HideInInspector]
        public SceneType scene;

        // Enums
        public enum SceneType
        {
            Loading,
            World,
            Merge,
            None
        };

        // References
        private SoundManager soundManager;
        private LocaleManager localeManager;
        private GameData gameData;

        void Start()
        {
            // Cache
            soundManager = SoundManager.Instance;
            localeManager = Settings.Instance.GetComponent<LocaleManager>();
            gameData = GameData.Instance;

            InitializeScene();
        }

        void OnEnable()
        {
            // Subscribe to events
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            // Unsubscribe from events
            SceneManager.sceneLoaded -= OnSceneLoaded;

            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InitializeScene();
        }

        void OnSceneUnloaded(Scene scene)
        {
            gameData.lastScene = Glob.ParseEnum<SceneType>(scene.name);

            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        public void Load(SceneType scene)
        {
            soundManager.FadeOutMusic(fadeDuration);

            transitionUI.Open(() =>
            {
                SceneManager.LoadScene((int)scene);
            });
        }

        public void LoadAsync(SceneType scene)
        {
            transitionUI.Open();
            soundManager.FadeOutMusic(fadeDuration);
            StartCoroutine(LoadAsyncScene(scene));
        }

        IEnumerator LoadAsyncScene(SceneType scene)
        {
            yield return new WaitForSeconds(duration);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync((int)scene);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        public SceneType GetScene()
        {
            return Glob.ParseEnum<SceneType>(SceneManager.GetActiveScene().name);
        }

        void InitializeScene()
        {
            soundManager = SoundManager.Instance;
            localeManager = Settings.Instance.GetComponent<LocaleManager>();

            // Play background music when the scene starts from the editor
            scene = Glob.ParseEnum<SceneType>(SceneManager.GetActiveScene().name);

            switch (scene)
            {
                case SceneType.World:
                    if (PlayerPrefs.HasKey("tutorialFinished"))
                    {
                        // Play world background music
                        soundManager.PlayMusic(SoundManager.MusicType.World);
                    }
                    else
                    {
                        // Play tutorial background music
                        soundManager.PlayMusic(SoundManager.MusicType.Magical);
                    }

                    soundManager.FadeInMusic(fadeDuration);

                    GameData.Instance.Init(scene);

                    Settings.Instance.Init();

                    localeManager.Init(scene);

                    break;

                case SceneType.Merge:
                    // Play background music
                    soundManager.PlayMusic(SoundManager.MusicType.Merge);

                    soundManager.FadeInMusic(fadeDuration);

                    GameData.Instance.Init(scene);

                    localeManager.Init(scene);

                    break;
            }
        }
    }
}
