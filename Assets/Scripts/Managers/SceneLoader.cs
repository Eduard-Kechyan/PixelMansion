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

    void Start()
    {
        Init();
    }

    void Init()
    {
        // Cache
        soundManager = SoundManager.Instance;
        localeManager = Settings.Instance.GetComponent<LocaleManager>();

        InitializeScene();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeScene();
    }

    public void Load(int sceneIndex)
    {
        soundManager.FadeOutMusic(fadeDuration);

        transitionUI.Open(()=>
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

    public string GetSceneName(){
        return SceneManager.GetActiveScene().name;
    }

    void InitializeScene()
    {
        // Play background music when the scene starts from the editor
        sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "Loading":
                // Play background music
                soundManager.PlayMusic(sceneName);

                break;

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

            default:
                Debug.Log("Unknown scene name: " + sceneName);

                break;
        }
    }
}
}