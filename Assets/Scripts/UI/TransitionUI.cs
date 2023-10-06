using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace Merge
{
    public class TransitionUI : MonoBehaviour
    {
        // Variables
        public TransitionData transitionData;
        public WorldDataManager worldDataManager;
        public SceneLoader sceneLoader;
        public float duration = 0.5f;
        public Color[] backgroundColors = new Color[0];
        public Sprite[] iconSprites = new Sprite[0];

        // References
        private DataManager dataManager;

        // UI
        private VisualElement root;
        private VisualElement transition;

        void Start()
        {
            // Cache
            dataManager = DataManager.Instance;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            transition = root.Q<VisualElement>("Transition");

            if (SceneManager.GetActiveScene().name == "Loading")
            {
                transition.style.display = DisplayStyle.Flex;
                transition.style.opacity = 0;
                transition.style.visibility = Visibility.Hidden;
            }
            else
            {
                StartCoroutine(Close());
            }
        }

        public void Open(Action callback = null)
        {
            if (backgroundColors.Length > 0)
            {
                int randomBackgroundColor = UnityEngine.Random.Range(0, backgroundColors.Length);

                transitionData.backgroundColor = randomBackgroundColor;

                transition.style.backgroundColor = backgroundColors[randomBackgroundColor];
            }

            // Do the rest afterwards

            transition.style.opacity = 1;
            transition.style.visibility = Visibility.Visible;

            if (callback != null)
            {
                Glob.SetTimeout(() =>
                {
                    callback();
                }, duration);
            }
        }

        IEnumerator Close()
        {
            if (transitionData.backgroundColor != -1 && backgroundColors.Length > 0)
            {
                transition.style.backgroundColor = backgroundColors[transitionData.backgroundColor];
            }

            transition.style.opacity = 1;
            transition.style.visibility = Visibility.Visible;
            transition.style.display = DisplayStyle.Flex;

            while (!CheckLoaded())
            {
                yield return null;
            }

            yield return new WaitForSeconds(duration);

            transition.style.opacity = 0;
            transition.style.visibility = Visibility.Hidden;
        }

        bool CheckLoaded()
        {
            bool readyToOpen = false;
            
            if (dataManager.loaded)
            {
                string sceneName = sceneLoader.GetSceneName();

                switch (sceneName)
                {
                    case "Hub":
                        if (worldDataManager.loaded)
                        {
                            readyToOpen = true;
                        }
                        break;

                    case "Gameplay":
                        readyToOpen = true;
                        break;

                    default:
                        readyToOpen = true;
                        break;
                }
            }

            return readyToOpen;
        }
    }
}