using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public GameObject uiDoc;
    public float duration = 0.6f;
    public float fadeDuration = 0.5f;
    private TansitionUI tansitionUI;

    private SoundManager soundManager;
    private GameData gameData;

    void Start()
    {
        tansitionUI = uiDoc.GetComponent<TansitionUI>();

        soundManager = SoundManager.Instance;

        gameData=GameData.Instance;

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

        if (scene.name == "Loading")
        {
            soundManager.PlayBg(scene.name, 0.7f);
        }
        else
        {
            soundManager.PlayBg(scene.name);

            gameData.CheckEnergy();

            soundManager.FadeInBg(fadeDuration);

            Values values = DataManager.Instance.GetComponent<Values>();

            if (values.set)
            {
                DataManager.Instance.GetComponent<Values>().UpdateValues();
            }
            else
            {
                DataManager.Instance.GetComponent<Values>().InitializeValues();
            }
        }
    }
}
