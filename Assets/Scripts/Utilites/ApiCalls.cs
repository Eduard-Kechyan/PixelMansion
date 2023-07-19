using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ApiCalls : MonoBehaviour
{
    public bool postData = false;
    public bool getData = false;
    public bool logConnection = false;

    private string URL = "https://server.com"; // TODO - Add real api address

    public bool isConnected = false;

    // Enums
    public enum UnsentType
    {
        NewUser,
        Terms
    };

    public class UnsentData
    {
        public UnsentType unsentType;
        public string jsonData;
        public int priority;
    }

    public class UnsentDataJson
    {
        public string unsentType;
        public string jsonData;
        public int priority;
    }

    public List<UnsentData> unsentData = new List<UnsentData>(0);

    private bool sendingUnsentData = false;

    // References
    private DataManager dataManager;

    // Instance
    public static ApiCalls Instance;

    void Awake()
    {
#if UNITY_EDITOR
        URL = "http://192.168.18.164:7007/api";
#endif

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        StartCoroutine(CheckConnection());
    }

    void Start()
    {
        dataManager = DataManager.Instance;

        CheckForUnsentData();
    }

    void OnValidate()
    {
        if (postData)
        {
            postData = false;

            if (Application.isPlaying && isConnected)
            {
                StartCoroutine(CreateUser(""));
            }
        }
        if (getData)
        {
            getData = false;

            if (Application.isPlaying && isConnected)
            {
                StartCoroutine(GetData("/users/_id/" + "64a47313420305bb515d8b8b"));
            }
        }

    }

    IEnumerator GetData(string path = "")
    {
        using (UnityWebRequest request = UnityWebRequest.Get(URL + path))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.data);
                Debug.Log(request.downloadHandler.text);
            }
        }
    }

    IEnumerator CheckConnection()
    {
        while (true)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(URL + "/system"))
            {
                yield return request.SendWebRequest();

                switch (request.result)
                {
                    case UnityWebRequest.Result.Success:
                        {
                            isConnected = true;

                            if (!sendingUnsentData)
                            {
                                CheckForUnsentData();
                            }

                            if (logConnection)
                            {
                                Debug.Log("Success");
                            }
                        }
                        break;
                    default:
                        {
                            isConnected = false;

                            //ErrorManager.Instance.ThrowFull(Types.ErrorType.Network, "ApiCalls.cs -> CheckConnection()", request.result.ToString(), request.responseCode.ToString(), true);

                            if (logConnection)
                            {
                                Debug.Log(request.result + ": " + request.error);
                            }
                        }
                        break;
                }
            }
        }
    }

    public IEnumerator CreateUser(string jsonData, Action callback = null)
    {
        using (UnityWebRequest request = UnityWebRequest.Post(URL + "/users", jsonData, "application/json"))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {

                // Checked
                if (callback != null)
                {
                    callback();
                }
            }
            else
            {
                SetUnsentData(UnsentType.NewUser, jsonData);

                // Checked
                if (callback != null)
                {
                    callback();
                }

                if (isConnected)
                {
                    ErrorManager.Instance.ThrowFull(Types.ErrorType.Network, "ApiCalls.cs -> CreateUser()", request.result.ToString(), request.responseCode.ToString(), true);
                }
            }
        }
    }

    public IEnumerator SendError(string jsonData)
    {
        using (UnityWebRequest request = UnityWebRequest.Post(URL + "/system/error", jsonData, "application/json"))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                ErrorManager.Instance.ThrowFull(Types.ErrorType.Network, "ApiCalls.cs -> SendError()", request.result.ToString(), request.responseCode.ToString(), true);
            }
        }
    }

    void SetUnsentData(UnsentType newUnsentType, string newJsonData)
    {
        PlayerPrefs.SetInt("hasUnsentData", 1);

        unsentData.Add(new UnsentData
        {
            unsentType = newUnsentType,
            jsonData = newJsonData,
            priority = (int)newUnsentType
        });

        dataManager.SaveUnsentData();
    }

    bool CheckForUnsentData()
    {
        if (PlayerPrefs.HasKey("hasUnsentData") || unsentData.Count > 0)
        {
            sendingUnsentData = true;
            // Send unsent data
            Debug.Log("Trying to send unsent data!");

            return true;
        }
        else
        {
            return false;
        }
    }
}
