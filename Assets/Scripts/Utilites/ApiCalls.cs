using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ApiCalls : MonoBehaviour
{
    public bool logConnection = false;
    public bool useDevUrl = false;

    private string URL = "https://game-dev-backup.onrender.com/api"; // TODO - Add real api address

    public bool isConnected = false;
    public bool canCheckForUnsent = false;

    // Enums
    public enum UnsentType
    {
        NewUser,
        Error
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

    private List<UnityWebRequest> requests = new List<UnityWebRequest>();

    private bool waitingForUnsentData = true;

    // References
    private DataManager dataManager;

    // Instance
    public static ApiCalls Instance;

    void Awake()
    {
#if UNITY_EDITOR
        if (useDevUrl)
        {
            URL = "http://192.168.18.164:7007/api"; // Dev server
        }
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

        // StartCoroutine(CheckConnection());
    }

    void Start()
    {
        dataManager = DataManager.Instance;
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

                            if (!sendingUnsentData && canCheckForUnsent)
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

    public void CheckIfUserExists()
    {

    }

    public IEnumerator CreateUser(string jsonData, Action callback = null)
    {
        using (UnityWebRequest request = UnityWebRequest.Post(URL + "/users", jsonData, "application/json"))
        {
            yield return request.SendWebRequest();

            requests.Add(request);

            if (request.result == UnityWebRequest.Result.Success)
            {
                requests.RemoveAt(requests.Count - 1);

                if (callback != null)
                {
                    callback();
                }

                PlayerPrefs.DeleteKey("tempAge");
                PlayerPrefs.Save();
            }
            else
            {
                SetUnsentData(UnsentType.NewUser, jsonData);

                if (callback != null)
                {
                    callback();
                }

                if (isConnected)
                {
                }
                ErrorManager.Instance.ThrowFull(Types.ErrorType.Network, "ApiCalls.cs -> CreateUser()", request.result.ToString(), request.responseCode.ToString(), true);
            }
        }
    }

    public IEnumerator SendError(string jsonData)
    {
        using (UnityWebRequest request = UnityWebRequest.Post(URL + "/system/error", jsonData, "application/json"))
        {
            yield return request.SendWebRequest();

            requests.Add(request);

            if (request.result == UnityWebRequest.Result.Success)
            {
                requests.RemoveAt(requests.Count - 1);
            }
            else
            {
                ErrorManager.Instance.ThrowFull(Types.ErrorType.Network, "ApiCalls.cs -> SendError()", request.result.ToString(), request.responseCode.ToString(), true);
            }
        }
    }

    void SetUnsentData(UnsentType newUnsentType, string newJsonData)
    {
        PlayerPrefs.SetInt("hasUnsentData", 1);
        PlayerPrefs.Save();

        unsentData.Add(new UnsentData
        {
            unsentType = newUnsentType,
            jsonData = newJsonData,
            priority = (int)newUnsentType
        });

        dataManager.SaveUnsentData();
    }

    void CheckForUnsentData()
    {
        if (PlayerPrefs.HasKey("hasUnsentData") || unsentData.Count > 0)
        {
            sendingUnsentData = true;

            if (unsentData.Count > 0)
            {
                // Send unsent data
                for (int i = 0; i < unsentData.Count; i++)
                {
                    switch (unsentData[i].unsentType)
                    {
                        case UnsentType.NewUser:
                            StartCoroutine(CreateUser(unsentData[i].jsonData, null));
                            break;

                        default: // Error
                            StartCoroutine(SendError(unsentData[i].jsonData));
                            break;
                    }
                }

                StartCoroutine(CheckDoneRequests());
            }
        }
    }

    IEnumerator CheckDoneRequests()
    {
        while (waitingForUnsentData)
        {
            bool done = true;

            foreach (UnityWebRequest request in requests)
            {
                if (!request.isDone) done = false;
            }

            if (done)
            {
                waitingForUnsentData = false;

                sendingUnsentData = false;

                unsentData = new List<UnsentData>();
                requests = new List<UnityWebRequest>();

                dataManager.SaveUnsentData();
            }
            else
            {
                yield return null;
            }
        }
    }
}
