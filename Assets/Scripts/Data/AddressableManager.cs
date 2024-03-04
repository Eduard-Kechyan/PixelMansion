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

        public class AddressableData
        {
            public AsyncOperationHandle loadHandle;
            public string key;
        }

        // References
        private ErrorManager errorManager;

        void Awake()
        {
            // Initialize addressables
            Addressables.InitializeAsync();
        }

        void Start()
        {
            // Cache
            errorManager = ErrorManager.Instance;
        }

        //// LOAD ////

        public async Task<TypeToGet> LoadAssetAsync<TypeToGet>(string key)
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
                    Types.ErrorType.Code,
                    GetType().Name,
                    newData.loadHandle.OperationException.Message
                );

                return default;
            }
        }

        public async Task<TypeToGet[]> LoadAssetAllArrayAsync<TypeToGet>(string key)
        {
            List<TypeToGet> newList = await LoadAssetAllAsync<TypeToGet>(key);

            TypeToGet[] newArray = new TypeToGet[newList.Count];

            for (int i = 0; i < newList.Count; i++)
            {
                newArray[i] = newList[i];
            }

            return newArray;
        }

        public async Task<List<TypeToGet>> LoadAssetAllAsync<TypeToGet>(string key)
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
                    Types.ErrorType.Code,
                    GetType().Name,
                    newData.loadHandle.OperationException.Message
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
