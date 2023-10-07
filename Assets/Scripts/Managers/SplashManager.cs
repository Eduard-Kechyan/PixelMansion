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
        public SceneLoader sceneLoader;

        // UI
        private VisualElement root;
        private VisualElement splashBox;

        void Start()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            splashBox = root.Q<VisualElement>("SplashBox");

            StartCoroutine(ShowAndHideSplash());
        }

        IEnumerator ShowAndHideSplash()
        {
            splashBox.AddToClassList("splash_box_show");

            yield return new WaitForSeconds(0.5f);

            splashBox.RemoveFromClassList("splash_box_show");

            yield return new WaitForSeconds(0.5f);

            SceneManager.LoadScene(1);
        }
    }
}
