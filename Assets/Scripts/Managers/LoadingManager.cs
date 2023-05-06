using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;

public class LoadingManager : MonoBehaviour
{
    // Variables
    public bool stayOnScene = false;
    public float fillSpeed = 3f;
    public SceneLoader sceneLoader;
    public GameObject uiDocument;

    private VisualElement fill;
    private float fillCount = 0;
    private bool loading = false;
    private bool startedLoading = false;

    // References
    private DataManager dataManager;
    private Notifics notifics;

    void Start()
    {
        // Cache
        dataManager = DataManager.Instance;
        notifics = Notifics.Instance;

        // Cache UI
        VisualElement root = uiDocument.GetComponent<UIDocument>().rootVisualElement;

        fill = root.Q<VisualElement>("Fill");

        loading = true;
    }

    void Update()
    {
        if (loading && fillCount < 100f)
        {
            Vector2 current = new Vector2(fillCount, fillCount);
            Vector2 to = new Vector2(100, 100);

            fillCount = Vector2.MoveTowards(current, to, fillSpeed * Time.deltaTime).x;

            fill.style.width = new Length(fillCount, LengthUnit.Pixel);

            if (fillCount >= 10f && !startedLoading)
            {
                startedLoading = true;
                LoadData();
            }

            if (fillCount >= 100f)
            {
                if (!stayOnScene)
                {
                    sceneLoader.Load(1);
                }
            }
        }
    }

    async void LoadData()
    {
        await Task.Delay(500);

        // Get data
        loading = false;
        await dataManager.CheckInitialData();
        loading = true;

        await Task.Delay(1000);

        // Check notifications
        if (SystemInfo.operatingSystem.Contains("13")&&SystemInfo.operatingSystem.Contains("33"))
        {
            loading = false;
            //StartCoroutine(notifics.RequestPermission()); await Task.Delay(750);
            await notifics.CheckNotifications();
            loading = true;
        }
    }
}
