using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ResetHandler : MonoBehaviour
{
    public SceneLoader sceneLoader;
    private DataManager dataManager;

    void Start()
    {
        dataManager = DataManager.Instance;
    }

    [ContextMenu("Restart App")]
    public void RestartApp()
    {
        ResetData();

        if (dataManager != null)
        {
            Destroy(dataManager.gameObject);
        }

        StartCoroutine(GoToLoadingScene());
    }

    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        string folderPath = Application.persistentDataPath + "/QuickSave";

        //Debug.Log(folderPath);

        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath, true);
        }
    }

    IEnumerator GoToLoadingScene()
    {
        yield return new WaitForSeconds(2f);

        sceneLoader.Load(0);
    }
}
