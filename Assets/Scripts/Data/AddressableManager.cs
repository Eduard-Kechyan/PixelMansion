using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Merge
{
    public class AddressableManager : MonoBehaviour
    {
        // Variables
        private List<AddressableData> loadHandles = new();

        [HideInInspector]
        public bool initialized = false;

        // Classes
        public class AddressableData
        {
            public AsyncOperationHandle loadHandle;
            public string key;
        }

        // References
        private ErrorManager errorManager;

        void Awake()
        {
            // Cache
            errorManager = ErrorManager.Instance;

            // Initialize addressables
            try
            {
                Addressables.InitializeAsync();
            }
            catch (Exception ex)
            {
                // ERROR
                errorManager.Throw(
                    ErrorManager.ErrorType.Code,
                    GetType().Name,
                    ex.Message
                );
            }

            initialized = true;
        }

        //// LOAD ////

        public async Task<TypeToGet> LoadAssetAsync<TypeToGet>(string key)
        {
            if (initialized)
            {
                AddressableData newData = new()
                {
                    loadHandle = Addressables.LoadAssetAsync<TypeToGet>(key),
                    key = key
                };

                await newData.loadHandle.Task;

                if (newData.loadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    loadHandles.Add(newData);

                    return (TypeToGet)newData.loadHandle.Result;
                }
                else
                {
                    // ERROR
                    errorManager.Throw(
                        ErrorManager.ErrorType.Code,
                        GetType().Name,
                        newData.loadHandle.OperationException.Message
                    );

                    return default;
                }
            }
            else
            {
                // ERROR
                errorManager.Throw(
                    ErrorManager.ErrorType.Code,
                    GetType().Name,
                    "Addressable was trying to load without initializing!"
                );

                return default;
            }
        }

        public async Task<TypeToGet[]> LoadAssetAllArrayAsync<TypeToGet>(string key)
        {
            if (initialized)
            {
                List<TypeToGet> newList = await LoadAssetAllAsync<TypeToGet>(key);

                int count = 0;

                if (newList != null)
                {
                    count = newList.Count;
                }

                TypeToGet[] newArray = new TypeToGet[count];

                for (int i = 0; i < count; i++)
                {
                    newArray[i] = newList[i];
                }

                return newArray;
            }
            else
            {
                // ERROR
                errorManager.Throw(
                    ErrorManager.ErrorType.Code,
                    GetType().Name,
                    "Addressable was trying to load without initializing!"
                );

                return default;
            }
        }

        public async Task<List<TypeToGet>> LoadAssetAllAsync<TypeToGet>(string key)
        {
            if (initialized)
            {
                AddressableData newData = new()
                {
                    loadHandle = Addressables.LoadAssetsAsync<TypeToGet>(key, null),
                    key = key
                };

                await newData.loadHandle.Task;

                if (newData.loadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    loadHandles.Add(newData);

                    return (List<TypeToGet>)newData.loadHandle.Result;
                }
                else
                {
                    // ERROR
                    errorManager.Throw(
                        ErrorManager.ErrorType.Code,
                        GetType().Name,
                        newData.loadHandle.OperationException.Message
                    );

                    return null;
                }
            }
            else
            {
                // ERROR
                errorManager.Throw(
                    ErrorManager.ErrorType.Code,
                    GetType().Name,
                    "Addressable was trying to load without initializing!"
                );

                return default;
            }
        }

        //// RELEASE ////

        public void ReleaseAsset(string key)
        {
            int order = -1;

            for (int i = 0; i < loadHandles.Count; i++)
            {
                if (loadHandles[i].key == key)
                {
                    Addressables.Release(loadHandles[i].loadHandle);

                    order = i;

                    break;
                }
            }

            if (order >= 0)
            {
                loadHandles.RemoveAt(order);
            }
        }

        public void ReleaseAllAssets()
        {
            for (int i = 0; i < loadHandles.Count; i++)
            {
                Addressables.Release(loadHandles[i].loadHandle);
            }

            loadHandles.Clear();
        }
    }
}
