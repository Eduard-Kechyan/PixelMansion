using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
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
        public string sceneName;

        // References
        private SoundManager soundManager;
        private LocaleManager localeManager;

        void OnEnable()
        {
            // Subscribe to events
            SceneManager.sceneLoaded += OnSceneLoaded;

            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnDisable()
        {
            // Unsubscribe from events
            SceneManager.sceneLoaded -= OnSceneLoaded;

            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void Start()
        {
            // Cache
            soundManager = SoundManager.Instance;
            localeManager = Settings.Instance.GetComponent<LocaleManager>();

            InitializeScene();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("Loaded " + scene.name);
            InitializeScene();
        }

        void OnSceneUnloaded(Scene scene)
        {
            Debug.Log("Unloaded " + scene.name);
            Glob.lastSceneName = scene.name;
        }

        public void Load(int sceneIndex)
        {
            soundManager.FadeOutMusic(fadeDuration);

            transitionUI.Open(() =>
            {
                SceneManager.LoadScene(sceneIndex);
            });
        }

        public void LoadAsync(int sceneIndex)
        {
            transitionUI.Open();
            soundManager.FadeOutMusic(fadeDuration);
            StartCoroutine(LoadAsyncScene(sceneIndex));
        }

        IEnumerator LoadAsyncScene(int sceneIndex)
        {
            yield return new WaitForSeconds(duration);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        public string GetSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        void InitializeScene()
        {
            soundManager = SoundManager.Instance;
            localeManager = Settings.Instance.GetComponent<LocaleManager>();

            // Play background music when the scene starts from the editor
            sceneName = SceneManager.GetActiveScene().name;

            switch (sceneName)
            {
                case "Hub":
                    // Play background music
                    soundManager.PlayMusic(sceneName);

                    soundManager.FadeInMusic(fadeDuration);

                    GameData.Instance.Init(sceneName);

                    Settings.Instance.Init();

                    localeManager.Init(sceneName);

                    break;

                case "Gameplay":
                    // Play background music
                    soundManager.PlayMusic(sceneName);

                    soundManager.FadeInMusic(fadeDuration);

                    GameData.Instance.Init(sceneName);

                    localeManager.Init(sceneName);

                    break;
            }
        }
    }
}
