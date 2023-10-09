using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace Merge
{
    public class SplashManager : MonoBehaviour
    {
        // Variables
        public LoadingManager loadingManager;

        // References
        private SoundManager soundManager;

        // UI
        private VisualElement root;
        private VisualElement splashContainer;
        private VisualElement splashLogo;

        void Start()
        {
            // Cache
            soundManager = SoundManager.Instance;

            // Cache UI
            root = GetComponent<UIDocument>().rootVisualElement;
            splashContainer = root.Q<VisualElement>("SplashContainer");
            splashLogo = splashContainer.Q<VisualElement>("Logo");

            splashContainer.style.display = DisplayStyle.Flex;
            splashContainer.style.opacity = 1;
        }

#if UNITY_EDITOR
        public void Init()
        {
            if (loadingManager.skipSplash)
            {
                splashContainer.style.display = DisplayStyle.None;

                // PLay background music
                soundManager.PlayMusic("Loading");

                loadingManager.StartLoading();
            }
            else
            {
                StartCoroutine(ShowAndHideSplash());
            }
        }
#else
        public void Init(){
            StartCoroutine(ShowAndHideSplash());
        }
#endif

        IEnumerator ShowAndHideSplash()
        {
            yield return new WaitForSeconds(1.5f);

            splashLogo.style.display = DisplayStyle.None;
            splashLogo.style.opacity = 0;

            yield return new WaitForSeconds(0.5f);

            List<TimeValue> durations = new();
            durations.Add(new TimeValue(0.5f, TimeUnit.Second));

            splashContainer.style.transitionDuration = new StyleList<TimeValue>(durations);
            splashContainer.style.display = DisplayStyle.None;
            splashContainer.style.opacity = 0;

            // PLay background music
            soundManager.PlayMusic("Loading");

            yield return new WaitForSeconds(0.5f);

            loadingManager.StartLoading();
        }
    }
}
