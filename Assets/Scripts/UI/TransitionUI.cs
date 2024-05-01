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
        public float duration = 0.5f;
        public Color[] backgroundColors = new Color[0];
        public Sprite[] iconSprites = new Sprite[0];

        private bool ignoreDuration = false;

        // References
        private GameRefs gameRefs;
        private DataManager dataManager;
        private BoardManager boardManager;
        private WorldDataManager worldDataManager;
        private SceneLoader sceneLoader;

        // UI
        private VisualElement root;
        private VisualElement transition;

        void Start()
        {
            // Cache
            gameRefs = GameRefs.Instance;
            dataManager = DataManager.Instance;
            boardManager = gameRefs.boardManager;
            worldDataManager = gameRefs.worldDataManager;
            sceneLoader = gameRefs.sceneLoader;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            transition = root.Q<VisualElement>("Transition");

            if (SceneManager.GetActiveScene().name == Types.Scene.Loading.ToString())
            {
                transition.style.display = DisplayStyle.Flex;
                transition.style.bottom = 500;
            }
            else
            {
                StartCoroutine(Close());
                StartCoroutine(IgnoreCloseDurationCheck());
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

            transition.style.bottom = 0;

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

            transition.style.bottom = 0;
            transition.style.display = DisplayStyle.Flex;

            while (!CheckLoaded())
            {
                yield return null;
            }

            if (!ignoreDuration)
            {
                yield return new WaitForSeconds(duration);
            }

            transition.style.bottom = 500;
        }

        IEnumerator IgnoreCloseDurationCheck()
        {
            ignoreDuration = true;

            yield return new WaitForSeconds(duration);

            ignoreDuration = false;
        }

        bool CheckLoaded()
        {
            bool readyToOpen = false;

            if (dataManager.loaded)
            {
                switch (sceneLoader.GetScene())
                {
                    case Types.Scene.World:
                        if (worldDataManager.loaded)
                        {
                            readyToOpen = true;
                        }
                        break;
                    case Types.Scene.Merge:
                        if (boardManager.boardSet)
                        {
                            readyToOpen = true;
                        }
                        break;
                    default: // Types.Scene.Loading
                        readyToOpen = true;
                        break;
                }
            }

            return readyToOpen;
        }
    }
}