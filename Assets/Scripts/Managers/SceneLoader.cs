using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

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

    void Start()
    {
        // Cache
        soundManager = SoundManager.Instance;

        InitializeScene();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeScene();
    }

    public void Load(int sceneIndex)
    {
        transitionUI.Open();
        soundManager.FadeOutBg(fadeDuration);
        StartCoroutine(LoadScene(sceneIndex));
    }

    IEnumerator LoadScene(int sceneIndex)
    {
        yield return new WaitForSeconds(duration);

        SceneManager.LoadScene(sceneIndex);
    }

    public void LoadAsync(int sceneIndex)
    {
        transitionUI.Open();
        soundManager.FadeOutBg(fadeDuration);
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

    void InitializeScene()
    {
        // Play background music when the scene starts from the editor
        sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "Loading":
                // Play background music
                soundManager.PlayBg(sceneName, 0.7f);

                break;

            case "Hub":
                // Play background music
                soundManager.PlayBg(sceneName);

                soundManager.FadeInBg(fadeDuration);

                GameData.Instance.Init(sceneName);

                break;

            case "Gameplay":
                // Play background music
                soundManager.PlayBg(sceneName);

                soundManager.FadeInBg(fadeDuration);

                GameData.Instance.Init(sceneName);

                break;

            default:
                Debug.Log("Unknown scene name: " + sceneName);

                break;
        }
    }
}
