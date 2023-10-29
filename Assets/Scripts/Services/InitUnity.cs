using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace Merge
{
    public class InitUnity : MonoBehaviour
    {
        // Variables
        [HideInInspector]
        public bool initialized = false;

        public async void Init()
        {
            try
            {
                var options = new InitializationOptions().SetEnvironmentName("production");

                await UnityServices.InitializeAsync(options);

                initialized = true;
            }
            catch (Exception exception)
            {
                Debug.Log("Couldn't initialize unity services!");
                Debug.Log(exception.Message);
            }
        }
    }
}
