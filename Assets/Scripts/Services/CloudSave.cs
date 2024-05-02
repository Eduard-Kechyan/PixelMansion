using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave;

namespace Merge
{
    public class CloudSave : MonoBehaviour
    {
        // Variables
        public bool cloudSaveEnabled = true;
        [Tooltip("In minutes")]
        [Condition("checkUnsentDataRegularly", true)]
        public float unsentDataCheckInterval = 30f;
        public float tooLongDelay = 5f;
        public bool throwCalledBeforeServicesError = false;
        public bool checkUnsentDataRegularly = false;
        public bool logData = false;

        [HideInInspector]
        public Dictionary<string, object> unsentData = new();

        private bool initializationTookTooLong = false;

        [HideInInspector]
        public bool checkedForUserData = false;

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

            if (!Debug.isDebugBuild || !services.servicesAvailable)
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
            while (!services.cloudSaveAvailable || !checkedForUserData || !dataManager.loaded || dataManager.reader == null)
            {
                yield return null;
            }

            if (checkUnsentDataRegularly)
            {
                Glob.SetInterval(() =>
                {
                    CheckUnsavedData();
                }, unsentDataCheckInterval * 60);
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
                            dataManager.SetValue(dataItem.Key, dataItem.Value.Value.GetAsString());

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
            if (cloudSaveEnabled)
            {
                if (services == null)
                {
                    services = Services.Instance;
                }

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
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveValidationException: " + exception
                        );

                        SetUnsavedData(newData);

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveRateLimitedException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveRateLimitedException: " + exception
                        );

                        SetUnsavedData(newData);

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
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
                        // WARNING
                        errorManager.ThrowWarning(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "Called before \"services.cloudSaveAvailable\""
                        );
                    }

                    SetUnsavedData(newData);

                    failCallback?.Invoke();
                }
            }
        }

        public void SetUnsavedData<T>(string key, T value)
        {
            SetUnsavedData(new Dictionary<string, object> { { key, value } });
        }

        public void SetUnsavedData(Dictionary<string, object> unsavedData)
        {
            if (cloudSaveEnabled)
            {
                if (dataManager == null)
                {
                    dataManager = DataManager.Instance;
                }

                if (dataManager.loaded && (unsentData == null || unsentData.Count == 0))
                {
                    unsentData = dataManager.LoadUnsentData();
                }

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
        }

        void RemoveFromUnsavedData(Dictionary<string, object> savedData)
        {
            if (cloudSaveEnabled)
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
        }

        void CheckUnsavedData()
        {
            if (PlayerPrefs.HasKey("hasUnsentData") && cloudSaveEnabled)
            {
                bool isReady = false;

                Dictionary<string, object> tempUnsentData = null;

                // Load from disk
                if (dataManager.loaded)
                {
                    tempUnsentData = dataManager.LoadUnsentData();
                }

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
                                ErrorManager.ErrorType.Code,
                                GetType().Name,
                                    "Wrong key given: " + key
                            );

                            failCallback?.Invoke();
                        }
                    }
                    catch (CloudSaveValidationException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveValidationException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveRateLimitedException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveRateLimitedException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                }
                else
                {
                    if (throwCalledBeforeServicesError)
                    {
                        // WARNING
                        errorManager.ThrowWarning(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
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
                            errorManager.ThrowWarning(
                                ErrorManager.ErrorType.Code,
                                GetType().Name,
                                 "Result is empty"
                            );

                            failCallback?.Invoke();
                        }
                    }
                    catch (CloudSaveValidationException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveValidationException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveRateLimitedException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveRateLimitedException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                }
                else
                {
                    if (throwCalledBeforeServicesError)
                    {
                        // WARNING
                        errorManager.ThrowWarning(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
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
                                ErrorManager.ErrorType.Code,
                                GetType().Name,
                                "Result is empty"
                            );

                            failCallback?.Invoke();
                        }
                    }
                    catch (CloudSaveValidationException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveValidationException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveRateLimitedException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveRateLimitedException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                }
                else
                {
                    if (throwCalledBeforeServicesError)
                    {
                        // WARNING
                        errorManager.ThrowWarning(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "Called before \"services.cloudSaveAvailable\""
                        );
                    }

                    failCallback?.Invoke();
                }
            }
        }

        /*public async void LoadCustomItemData(string customItemId, Action<T> callback = null, Action failCallback = null)
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
                                ErrorManager.ErrorType.Code,
                                GetType().Name,
                                    "Wrong key given: " + key
                            );

                            failCallback?.Invoke();
                        }
                    }
                    catch (CloudSaveValidationException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveValidationException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveRateLimitedException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveRateLimitedException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                }
                else
                {
                    if (throwCalledBeforeServicesError)
                    {
                        // WARNING
                        errorManager.ThrowWarning(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "Called before \"services.cloudSaveAvailable\""
                        );
                    }

                    failCallback?.Invoke();
                }

                string newCustomItemId = customItemId == "" ? "TermsHtml" : customItemId;

                var customItems = await CloudSaveService.Instance.Data.Custom.LoadAllAsync(newCustomItemId);

                foreach (var customItem in customItems)
                {
                    Debug.Log(customItem.Key);
                    Debug.Log(customItem.Value.Value);
                }
            }
    }*/

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
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveValidationException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveRateLimitedException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveRateLimitedException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                }
                else
                {
                    if (throwCalledBeforeServicesError)
                    {
                        // WARNING
                        errorManager.ThrowWarning(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
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
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveValidationException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveRateLimitedException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveRateLimitedException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                    catch (CloudSaveException exception)
                    {
                        // ERROR
                        errorManager.Throw(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "CloudSaveException: " + exception
                        );

                        failCallback?.Invoke();
                    }
                }
                else
                {
                    if (throwCalledBeforeServicesError)
                    {
                        // WARNING
                        errorManager.ThrowWarning(
                            ErrorManager.ErrorType.Code,
                            GetType().Name,
                            "Called before \"services.cloudSaveAvailable\""
                        );
                    }

                    failCallback?.Invoke();
                }
            }
        }
    }
}
