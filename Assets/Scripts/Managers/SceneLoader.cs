using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public GameObject uiDoc;
    public float duration = 5f;
    public float fadeDuration = 1f;
    private TansitionUI tansitionUI;

    private SoundManager soundManager;

    void Start()
    {
        tansitionUI = uiDoc.GetComponent<TansitionUI>();

        soundManager = SoundManager.Instance;

        StartBg();
    }

    public void Load(int sceneIndex)
    {
        tansitionUI.Open();
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
        tansitionUI.Open();
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

    void StartBg()
    {
        Scene scene = SceneManager.GetActiveScene();

        soundManager.PlayBg(scene.name);

        if (scene.name == "Loading")
        {
            soundManager.SetVolumeBG(1);
        }
        else
        {
            soundManager.FadeInBg(fadeDuration);
        }
    }
}
