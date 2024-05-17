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
        private float valuesBoxHeight;
        private float devicePixelWidth;
        private float singlePixelWidth;

        // UI
        private VisualElement valuesRoot;
        public VisualElement valuesBox;
        private VisualElement options;

        void Start()
        {
            // UI
            valuesRoot = GameRefs.Instance.valuesUIDoc.rootVisualElement;
            valuesBox = valuesRoot.Q<VisualElement>("ValuesBox");

            valuesRoot.RegisterCallback<GeometryChangedEvent>(Init);
        }

        void Init(GeometryChangedEvent evt)
        {
            valuesRoot.UnregisterCallback<GeometryChangedEvent>(Init);

            // Get the safe area height
            height = Screen.height - Screen.safeArea.height;

            // Calculate the pixel widths
            devicePixelWidth = cam.pixelWidth;
            singlePixelWidth = devicePixelWidth / GameData.GAME_PIXEL_WIDTH;

            // Set top padding for the values box
            topPadding = Mathf.RoundToInt(height / singlePixelWidth);
            valuesBox.style.top = topPadding;

            valuesBoxHeight = topPadding + valuesBox.resolvedStyle.height + valuesBox.resolvedStyle.marginTop;

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
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            VisualElement bottomBox = root.Q<VisualElement>("BottomBox");
            options = root.Q<VisualElement>("Options");

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

            bottomBox.style.top = valuesBoxHeight;
        }

        public float GetTopOffset()
        {
            float topOffset = topPadding + valuesBox.resolvedStyle.height;

            return topOffset;
        }
    }
}