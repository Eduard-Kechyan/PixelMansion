using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class LoadingAnimationsHandler : MonoBehaviour
    {
        // Variables
        public LoadingScene loadingScene = LoadingScene.First;

        public enum LoadingScene
        {
            First,
            Christmas,
        }

        // UI
        private VisualElement root;

        // First
        public Sprite[] frogEyesSprites;
        public Sprite[] floatingStoneSprites;

        public float frogEyesDelay = 0.2f;
        public float floatingStoneDelay = 0.2f;

        private VisualElement frogEyes;
        private VisualElement floatingStone;

        void Start()
        {
            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            // First
            frogEyes = root.Q<VisualElement>("FrogEyes");
            floatingStone = root.Q<VisualElement>("FloatingStone");

            // Initialize
            Init();
        }

        void Init()
        {
            switch (loadingScene)
            {
                case LoadingScene.Christmas:
                    Debug.LogWarning("LoadingScene.Christmas not implemented yet!");
                    break;
                default:
                    StartCoroutine(AnimateFrogEyes());
                    StartCoroutine(AnimateFloatingStone());
                    break;
            }
        }

        //// SCENE FIRST ////

        IEnumerator AnimateFrogEyes()
        {
            while (true)
            {

                frogEyes.style.backgroundImage = new StyleBackground(frogEyesSprites[1]);

                yield return new WaitForSeconds(frogEyesDelay);

                frogEyes.style.backgroundImage = new StyleBackground(frogEyesSprites[2]);

                yield return new WaitForSeconds(frogEyesDelay * 5);

                frogEyes.style.backgroundImage = new StyleBackground(frogEyesSprites[1]);

                yield return new WaitForSeconds(frogEyesDelay);

                frogEyes.style.backgroundImage = new StyleBackground(frogEyesSprites[0]);

                yield return new WaitForSeconds(frogEyesDelay * 10);
            }
        }

        IEnumerator AnimateFloatingStone()
        {
            while (true)
            {
                floatingStone.style.marginBottom = 0;

                yield return new WaitForSeconds(floatingStoneDelay);

                floatingStone.style.marginBottom = 1;

                yield return new WaitForSeconds(floatingStoneDelay);

                floatingStone.style.backgroundImage = new StyleBackground(floatingStoneSprites[0]);
                floatingStone.style.marginBottom = 2;

                yield return new WaitForSeconds(floatingStoneDelay);

                floatingStone.style.backgroundImage = new StyleBackground(floatingStoneSprites[1]);
                floatingStone.style.marginBottom = 3;

                yield return new WaitForSeconds(floatingStoneDelay);

                floatingStone.style.backgroundImage = new StyleBackground(floatingStoneSprites[2]);
                floatingStone.style.marginBottom = 4;

                yield return new WaitForSeconds(floatingStoneDelay * 2);

                floatingStone.style.backgroundImage = new StyleBackground(floatingStoneSprites[1]);
                floatingStone.style.marginBottom = 3;

                yield return new WaitForSeconds(floatingStoneDelay);

                floatingStone.style.backgroundImage = new StyleBackground(floatingStoneSprites[0]);
                floatingStone.style.marginBottom = 2;

                yield return new WaitForSeconds(floatingStoneDelay);

                floatingStone.style.marginBottom = 1;

                yield return new WaitForSeconds(floatingStoneDelay);
            }
        }
    }
}
