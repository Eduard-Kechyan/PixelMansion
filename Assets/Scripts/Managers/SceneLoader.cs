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
        public LikeMenu rateMenu;
        public float duration = 0.6f;
        public float fadeDuration = 0.5f;

        [HideInInspector]
        public string sceneName;

        // References
        private SoundManager soundManager;
        private LocaleManager localeManager;

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
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
            InitializeScene();
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

            Debug.Log(soundManager);

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

                    if (rateMenu != null && rateMenu.shouldShow)
                    {
                        rateMenu.Open();
                    }

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
