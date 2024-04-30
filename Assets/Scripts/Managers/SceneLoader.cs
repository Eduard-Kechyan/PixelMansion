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
        public Types.Scene scene;

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
            gameData.lastScene = Glob.ParseEnum<Types.Scene>(scene.name);

            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        public void Load(Types.Scene scene)
        {
            soundManager.FadeOutMusic(fadeDuration);

            transitionUI.Open(() =>
            {
                SceneManager.LoadScene((int)scene);
            });
        }

        public void LoadAsync(Types.Scene scene)
        {
            transitionUI.Open();
            soundManager.FadeOutMusic(fadeDuration);
            StartCoroutine(LoadAsyncScene(scene));
        }

        IEnumerator LoadAsyncScene(Types.Scene scene)
        {
            yield return new WaitForSeconds(duration);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync((int)scene);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        public Types.Scene GetScene()
        {
            return Glob.ParseEnum<Types.Scene>(SceneManager.GetActiveScene().name);
        }

        void InitializeScene()
        {
            soundManager = SoundManager.Instance;
            localeManager = Settings.Instance.GetComponent<LocaleManager>();

            // Play background music when the scene starts from the editor
            scene = Glob.ParseEnum<Types.Scene>(SceneManager.GetActiveScene().name);

            switch (scene)
            {
                case Types.Scene.World:
                    if (PlayerPrefs.HasKey("tutorialFinished"))
                    {
                        // Play world background music
                        soundManager.PlayMusic(Types.MusicType.World);
                    }
                    else
                    {
                        // Play tutorial background music
                        soundManager.PlayMusic(Types.MusicType.Magical);
                    }

                    soundManager.FadeInMusic(fadeDuration);

                    GameData.Instance.Init(scene);

                    Settings.Instance.Init();

                    localeManager.Init(scene);

                    break;

                case Types.Scene.Merge:
                    // Play background music
                    soundManager.PlayMusic(Types.MusicType.Merge);

                    soundManager.FadeInMusic(fadeDuration);

                    GameData.Instance.Init(scene);

                    localeManager.Init(scene);

                    break;
            }
        }
    }
}
