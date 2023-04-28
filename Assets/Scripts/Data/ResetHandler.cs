using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ResetHandler : MonoBehaviour
{
    // Varaibles
    public SceneLoader sceneLoader;

    // Instances
    private DataManager dataManager;

    void Start()
    {
        // Cache instances
        dataManager = DataManager.Instance;
    }
    
    public void RestartApp(bool reset = false)
    {
        if (reset)
        {
            ResetData();
        }

        if (dataManager != null)
        {
            Destroy(dataManager.gameObject);
        }

        Debug.Log("A");

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
        yield return new WaitForSeconds(0.2f);

        Debug.Log("B");

        sceneLoader.Load(0);
    }
}
