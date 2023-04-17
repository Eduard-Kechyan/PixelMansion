using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public TansitionUI tansitionUI;
    public float duration = 0.6f;
    public float fadeDuration = 0.5f;

    private SoundManager soundManager;
    private Values values;
    private LevelMenu levelMenu;

    void Start()
    {
        soundManager = SoundManager.Instance;
        values = DataManager.Instance.GetComponent<Values>();

        StartBg();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Loading":
                break;

            case "Hub":
                GameData.Instance.InitializeGamedataCache();

                values.InitializeValues();
                break;

            case "GamePlay":
                InfoMenu infoMenu = MenuManager.Instance.GetComponent<InfoMenu>();

                infoMenu.enabled = true;

                GameData.Instance.InitializeGamedataCache(true);

                values.InitializeValues();

                levelMenu = MenuManager.Instance.GetComponent<LevelMenu>();

                levelMenu.InitializeLevelMenuCache();
                break;

            default:
                Debug.Log("Unknown scene name: " + scene.name);
                break;
        }
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

            soundManager.FadeInBg(fadeDuration);

            GameData.Instance.InitializeGamedataCache();

            if (values.set)
            {
                values.UpdateValues();
            }
            else
            {
                values.InitializeValues();
            }
        }
    }
}
