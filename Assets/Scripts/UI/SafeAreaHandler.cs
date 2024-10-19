using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace Merge
{
    public class SafeAreaHandler : MonoBehaviour
    {
        // Variables
        public float manualSizes = 0f;
        public bool alternativeAspectRatio;
        public Camera cam;

        private float height;
        private float calculatedHeight;
        private float dividedHeight;

        [HideInInspector]
        public float topPadding;
        private float devicePixelWidth;
        private float singlePixelWidth;

        // UI
        private VisualElement root;
        public VisualElement valuesBox;
        private VisualElement bottomBox;
        private VisualElement options;
        private VisualElement infoBox;
        private VisualElement board;

        void Start()
        {
            // UI
            valuesBox = GameRefs.Instance.valuesUIDoc.rootVisualElement.Q<VisualElement>("ValuesBox");

            Init();
        }

        void Init()
        {
            // Get the safe area height
            height = Screen.height - Screen.safeArea.height;

            // Calculate the pixel widths
            devicePixelWidth = cam.pixelWidth;
            singlePixelWidth = devicePixelWidth / GameData.GAME_PIXEL_WIDTH;

            // Set top padding for the values box
            topPadding = Mathf.RoundToInt(height / singlePixelWidth);
            valuesBox.style.top = topPadding;

            Scene scene = SceneManager.GetActiveScene();

            SceneLoader.SceneType sceneName = Glob.ParseEnum<SceneLoader.SceneType>(scene.name);

            switch (sceneName)
            {
                case SceneLoader.SceneType.Loading:
                    break;

                case SceneLoader.SceneType.World:
                    break;

                case SceneLoader.SceneType.Merge:
                    SetMergeUI();
                    break;

                default:
                    Debug.Log("Unknown scene name: " + scene.name);
                    break;
            }

        }

        void SetMergeUI()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            bottomBox = root.Q<VisualElement>("BottomBox");
            options = root.Q<VisualElement>("Options");
            infoBox = root.Q<VisualElement>("InfoBox");
            board = root.Q<VisualElement>("Board");

            // Calculated sizes
            calculatedHeight = ((Screen.height - height) / singlePixelWidth) - manualSizes;

            if (alternativeAspectRatio)
            {
                dividedHeight = Mathf.FloorToInt(calculatedHeight / 5);
            }
            else
            {
                dividedHeight = Mathf.FloorToInt(calculatedHeight / 4);
            }

            bottomBox.style.paddingBottom = dividedHeight;
            infoBox.style.marginBottom = dividedHeight;
            board.style.marginBottom = dividedHeight;
        }

        public float GetBottomOffset()
        {
            float bottomOffset = options.resolvedStyle.height + (dividedHeight * 2);

            return bottomOffset;
        }

        public float GetTopOffset()
        {
            float topOffset = topPadding + valuesBox.resolvedStyle.height;

            return topOffset;
        }
    }
}