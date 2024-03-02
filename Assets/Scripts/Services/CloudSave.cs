using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave;

// TODO - Handle this data
/*    
    UserData newUserData = new()
    {
        playerId = playerId,
        email = dummyEmail, // FIX - Email should be empty and should only be filled if the player signs ins using social media
        location = "USA",
        language = "en-US",
        age = tempAge
    };

    // deviceId = SystemInfo.deviceUniqueIdentifier,
    // deviceModel = SystemInfo.deviceModel,
    // deviceOs = SystemInfo.operatingSystem,
    // deviceLanguage = Application.systemLanguage.ToString(),
    // devicePlatform = Application.platform.ToString(),
    // deviceName = SystemInfo.deviceName,
    // deviceType = SystemInfo.deviceType.ToString(),
    // deviceOsFamily = SystemInfo.operatingSystemFamily.ToString(),

    /* Debug.Log(newUserData.deviceId);
     Debug.Log(newUserData.deviceModel);
     Debug.Log(newUserData.deviceOs);
     Debug.Log(newUserData.deviceLanguage);

     Debug.Log(Application.absoluteURL);
     Debug.Log(Application.installerName);
     Debug.Log(Application.installMode);
*/

namespace Merge
{
    public class CloudSave : MonoBehaviour
    {
        // Variables
        public bool cloudSaveEnabled = true;
        [Tooltip("In minutes")]
        [Condition("checkSendDataRegularly", true)]
        public float unsentDataCheckInterval = 30f;
        public float tooLongDelay = 2f;
        public bool throwCalledBeforeServicesError = false;
        public bool logData = false;

        [HideInInspector]
        public Dictionary<string, object> unsentData = new();

        private bool initializationTookTooLong = false;

        private bool checkSendDataRegularly = false;

        // References
        private Services services;
        private ErrorManager errorManager;
        private DataManager dataManager;

        void Start()
        {
            // Cache
            services = Services.Instance;
            errorManager = ErrorManager.Instance;
            dataManager = DataManager.Instance;

            if (!Debug.isDebugBuild)
            {
                logData = false;

                cloudSaveEnabled = false;
            }

            if (cloudSaveEnabled)
            {
                StartCoroutine(WaitForUnityServicesAndAuthorization());

                StartCoroutine(WaitForGameData());
            }
        }

        IEnumerator WaitForUnityServicesAndAuthorization()
        {
            while (!services.unityServicesAvailable || !services.authAvailable)
            {
                yield return null;
            }

            services.cloudSaveAvailable = true;
        }

        IEnumerator WaitForGameData()
        {
            while (!services.cloudSaveAvailable || !dataManager.loaded || dataManager.reader == null)
            {
                yield return null;
            }

            if (checkSendDataRegularly)
            {
                Glob.SetInterval(() =>
                {
                    CheckUnsavedData();
                }, unsentDataCheckInterval + 60);
            }
            else
            {
                CheckUnsavedData();
            }
        }

        IEnumerator WaitForTookTooLongDelay()
        {
            yield return new WaitForSeconds(tooLongDelay);

            initializationTookTooLong = true;
        }

        IEnumerator WaitForCloudSave(Action callback)
        {
            if (PlayerPrefs.HasKey("accountDataSet"))
            {
                callback();

                yield break;
            }
            else
            {
                while (!services.cloudSaveAvailable && !initializationTookTooLong)
                {
                    yield return null;
                }

                if (services.cloudSaveAvailable)
                {
                    LoadAllDataAsync((Dictionary<string, Unity.Services.CloudSave.Models.Item> data) =>
                    {
                        bool tutorialFinished = false;

                        foreach (var dataItem in data)
                        {
                            Debug.Log(dataItem.Key);
                            Debug.Log(dataItem.Value);
                            Debug.Log(dataItem.Value.Key);
                            Debug.Log(dataItem.Value.Value);

                            dataManager.SetValue(dataItem.Key, dataItem.Value.Value);

                            if (dataItem.Key == "tutorialFinished")
                            {
                                tutorialFinished = true;
                            }
                        }

                        dataManager.FinishSettingValues();

                        if (!tutorialFinished)
                        {
                            DeleteAllDataAsync();
                        }

                        PlayerPrefs.SetInt("accountDataSet", 1);

                        callback();
                    }, () =>
                    {
                        // TODO - Inform player that getting cloud data failed

                        PlayerPrefs.SetInt("accountDataSet", 1);

                        callback();
                    });
                }
                else
                {
                    callback();
                }
            }
        }

        public void CheckUserData(Action callback)
        {
            StartCoroutine(WaitForTookTooLongDelay());

            StartCoroutine(WaitForCloudSave(callback));
        }

        //// SAVE ////

        public void SaveDataAsync<T>(string key, T value, Action callback = null, Action failCallback = null)
        {
            SaveDataAsync(new Dictionary<string, object> { { key, value } }, callback, failCallback);
        }

        public async void SaveDataAsync(Dictionary<string, object> newData, Action callback = null, Action failCallback = null)
        {
            if (services == null)
            {
                services = Services.Instance;
            }

            if (cloudSaveEnabled)
            {
                if (services.cloudSaveAvailable)
                {
                    try
                    {
                        await CloudSaveService.Instance.Data.Player.SaveAsync(newData);

                        RemoveFromUnsavedData(newData);

                        callback?.Invoke();
                    }
                    catch (CloudSaveValidationException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "CloudSave.cs -> SaveDataAsync()",
                            "CloudSaveValidationException: " + exception
                        );

                        SetUnsavedData(newData);

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveRateLimitedException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "CloudSave.cs -> SaveDataAsync()",
                            "CloudSaveRateLimitedException: " + exception
                        );

                        SetUnsavedData(newData);

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "CloudSave.cs -> SaveDataAsync()",
                            "CloudSaveException: " + exception
                        );

                        SetUnsavedData(newData);

                        failCallback?.Invoke();
                    }
                }
                else
                {
                    if (throwCalledBeforeServicesError)
                    {
                        // ERROR
                        errorManager.ThrowWarning(
                            Types.ErrorType.Code,
                            "Called before \"services.cloudSaveAvailable\""
                        );
                    }

                    SetUnsavedData(newData);

                    failCallback?.Invoke();
                }
            }
        }

        void SetUnsavedData(Dictionary<string, object> unsavedData)
        {
            foreach (var dataItem in unsavedData)
            {
                bool found = false;

                // Update previous
                foreach (var unsentDataItem in unsentData)
                {
                    if (unsentDataItem.Key == dataItem.Key)
                    {
                        found = true;

                        unsentData[dataItem.Key] = dataItem.Value;

                        break;
                    }
                }

                // Create new 
                if (!found)
                {
                    unsentData.Add(dataItem.Key, dataItem.Value);
                }
            }

            PlayerPrefs.SetInt("hasUnsentData", 1);
            PlayerPrefs.Save();

            // Save to disk
            dataManager.SaveUnsentData(unsentData);
        }

        void RemoveFromUnsavedData(Dictionary<string, object> savedData)
        {
            if (unsentData != null && unsentData.Count > 0)
            {
                List<string> foundKeys = new();

                foreach (var dataItem in savedData)
                {
                    foreach (var unsentDataItem in unsentData)
                    {
                        if (unsentDataItem.Key == dataItem.Key)
                        {
                            foundKeys.Add(unsentDataItem.Key);

                            break;
                        }
                    }
                }

                for (int i = 0; i < foundKeys.Count; i++)
                {
                    unsentData.Remove(foundKeys[i]);
                }

                if (unsentData.Count == 0)
                {
                    PlayerPrefs.DeleteKey("hasUnsentData");
                    PlayerPrefs.Save();
                }

                // Save to disk
                dataManager.SaveUnsentData(unsentData);
            }
        }

        void CheckUnsavedData()
        {
            if (PlayerPrefs.HasKey("hasUnsentData"))
            {
                bool isReady = false;

                // Load from disk    
                Dictionary<string, object> tempUnsentData = dataManager.LoadUnsentData();

                if (tempUnsentData != null && tempUnsentData.Count > 0)
                {
                    SetUnsavedData(tempUnsentData);

                    isReady = true;
                }

                if (unsentData != null && unsentData.Count > 0)
                {
                    isReady = true;
                }

                if (isReady)
                {
                    SaveDataAsync(unsentData, () =>
                    {
                        unsentData.Clear();

                        // Save to disk
                        dataManager.SaveUnsentData(unsentData);

                        PlayerPrefs.DeleteKey("hasUnsentData");
                        PlayerPrefs.Save();
                    });
                }
            }
        }

        //// LOAD ////

        public async void LoadDataAsync<T>(string key, Action<T> callback = null, Action failCallback = null)
        {
            if (cloudSaveEnabled)
            {
                if (services.cloudSaveAvailable)
                {
                    try
                    {
                        var result = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });

                        if (result.TryGetValue(key, out var keyData))
                        {
                            callback?.Invoke(keyData.Value.GetAs<T>());
                        }
                        else
                        {
                            // ERROR
                            errorManager.Throw(
                                Types.ErrorType.Code,
                                "CloudSave.cs -> LoadDataAsync()",
                                "Wrong key given: " + key
                            );

                            failCallback?.Invoke();
                        }
                    }
                    catch (CloudSaveValidationException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "CloudSave.cs -> LoadDataAsync()",
                            "CloudSaveValidationException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveRateLimitedException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "CloudSave.cs -> LoadDataAsync()",
                            "CloudSaveRateLimitedException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "CloudSave.cs -> LoadDataAsync()",
                            "CloudSaveException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                }
                else
                {
                    if (throwCalledBeforeServicesError)
                    {
                        // ERROR
                        errorManager.ThrowWarning(
                            Types.ErrorType.Code,
                            "Called before \"services.cloudSaveAvailable\""
                        );
                    }

                    failCallback?.Invoke();
                }
            }
        }

        public async void LoadAllDataAsync(Action<Dictionary<string, Unity.Services.CloudSave.Models.Item>> callback = null, Action failCallback = null)
        {
            if (cloudSaveEnabled)
            {
                if (services.cloudSaveAvailable)
                {
                    try
                    {
                        var result = await CloudSaveService.Instance.Data.Player.LoadAllAsync();

                        if (result.Count > 0)
                        {
                            callback?.Invoke(result);
                        }
                        else
                        {
                            // ERROR
                            /* errorManager.Throw(
                                 Types.ErrorType.Code,
                                 "CloudSave.cs -> LoadAllDataAsync()",
                                 "Result is empty"
                             );*/

                            failCallback?.Invoke();
                        }
                    }
                    catch (CloudSaveValidationException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "CloudSave.cs -> LoadAllDataAsync()",
                            "CloudSaveValidationException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveRateLimitedException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "CloudSave.cs -> LoadAllDataAsync()",
                            "CloudSaveRateLimitedException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "CloudSave.cs -> LoadAllDataAsync()",
                            "CloudSaveException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                }
                else
                {
                    if (throwCalledBeforeServicesError)
                    {
                        // ERROR
                        errorManager.ThrowWarning(
                            Types.ErrorType.Code,
                            "Called before \"services.cloudSaveAvailable\""
                        );
                    }

                    failCallback?.Invoke();
                }
            }
        }

        public async void ListAllKeysAsync(Action<List<Unity.Services.CloudSave.Models.ItemKey>> callback = null, Action failCallback = null)
        {
            if (cloudSaveEnabled)
            {
                if (services.cloudSaveAvailable)
                {
                    try
                    {
                        var result = await CloudSaveService.Instance.Data.Player.ListAllKeysAsync();

                        if (result.Count > 0)
                        {
                            callback?.Invoke(result);
                        }
                        else
                        {
                            // ERROR
                            errorManager.Throw(
                                Types.ErrorType.Code,
                                "CloudSave.cs -> LoadAllDataAsync()",
                                "Result is empty"
                            );

                            failCallback?.Invoke();
                        }
                    }
                    catch (CloudSaveValidationException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "CloudSave.cs -> GetAllKeysAsync()",
                            "CloudSaveValidationException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveRateLimitedException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "CloudSave.cs -> GetAllKeysAsync()",
                            "CloudSaveRateLimitedException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "CloudSave.cs -> GetAllKeysAsync()",
                            "CloudSaveException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                }
                else
                {
                    if (throwCalledBeforeServicesError)
                    {
                        // ERROR
                        errorManager.ThrowWarning(
                            Types.ErrorType.Code,
                            "Called before \"services.cloudSaveAvailable\""
                        );
                    }

                    failCallback?.Invoke();
                }
            }
        }

        //// DELETE ////

        public async void DeleteDataAsync(string key, Action callback = null, Action failCallback = null)
        {
            if (cloudSaveEnabled)
            {
                if (services.cloudSaveAvailable)
                {
                    try
                    {
                        Unity.Services.CloudSave.Models.Data.Player.DeleteOptions deleteOptions = new();

                        await CloudSaveService.Instance.Data.Player.DeleteAsync(key, deleteOptions);

                        callback?.Invoke();
                    }
                    catch (CloudSaveValidationException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "CloudSave.cs -> DeleteDataAsync()",
                            "CloudSaveValidationException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveRateLimitedException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "CloudSave.cs -> DeleteDataAsync()",
                            "CloudSaveRateLimitedException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "CloudSave.cs -> DeleteDataAsync()",
                            "CloudSaveException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                }
                else
                {
                    if (throwCalledBeforeServicesError)
                    {
                        // ERROR
                        errorManager.ThrowWarning(
                            Types.ErrorType.Code,
                            "Called before \"services.cloudSaveAvailable\""
                        );
                    }

                    failCallback?.Invoke();
                }
            }
        }

        public async void DeleteAllDataAsync(Action callback = null, Action failCallback = null)
        {
            if (cloudSaveEnabled)
            {
                if (services.cloudSaveAvailable)
                {
                    try
                    {
                        Unity.Services.CloudSave.Models.Data.Player.DeleteAllOptions deleteAllOptions = new();

                        await CloudSaveService.Instance.Data.Player.DeleteAllAsync(deleteAllOptions);

                        callback?.Invoke();
                    }
                    catch (CloudSaveValidationException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "CloudSave.cs -> DeleteAllDataAsync()",
                            "CloudSaveValidationException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveRateLimitedException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "CloudSave.cs -> DeleteAllDataAsync()",
                            "CloudSaveRateLimitedException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            Types.ErrorType.Code,
                            "CloudSave.cs -> DeleteAllDataAsync()",
                            "CloudSaveException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                }
                else
                {
                    if (throwCalledBeforeServicesError)
                    {
                        // ERROR
                        errorManager.ThrowWarning(
                            Types.ErrorType.Code,
                            "Called before \"services.cloudSaveAvailable\""
                        );
                    }

                    failCallback?.Invoke();
                }
            }
        }
    }
}
