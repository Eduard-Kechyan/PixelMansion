using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace Merge
{
    public class PopupManager : MonoBehaviour
    {
        // Variables
        public Color textColor;
        public Color outlineColor;
        public Color shadowColor;
        public float timeOut = 2f;

        [HideInInspector]
        public bool isSelectorPopup = false;

        private bool isPopupShowing = false;

        // Reference
        private SoundManager soundManager;
        private LocaleManager localeManager;
        private UIDocument popupUI;
        private SceneLoader sceneLoader;

        // UI
        private VisualElement root;

        // Instance
        public static PopupManager Instance;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            // Cache references
            soundManager = SoundManager.Instance;
            localeManager = Settings.Instance.GetComponent<LocaleManager>();
            popupUI = GetComponent<UIDocument>();
            sceneLoader = GameRefs.Instance.sceneLoader;

            // UI
            root = popupUI.rootVisualElement;
        }

        public void Pop(string newText, Vector2 position, SoundManager.SoundType soundType = SoundManager.SoundType.None, bool convertPosToUI = false, bool fromSelector = false)
        {
            isSelectorPopup = fromSelector;

            // Initialize popup label
            Label newPopupLabel = new() { name = "PopupLabel" + newText, text = newText, };

            newPopupLabel.AddToClassList("popup_label");

            // Handle font size
            int grownFontSize = 8;

            if (newText.Length > 14)
            {
                grownFontSize = 6;
            }

            switch (Settings.Instance.currentLocale)
            {
                case I18n.Locale.Armenian:
                    newPopupLabel.style.unityFontDefinition = new StyleFontDefinition(localeManager.hyFont);
                    break;
                case I18n.Locale.Japanese:
                    newPopupLabel.style.unityFontDefinition = new StyleFontDefinition(localeManager.jpFont);
                    break;
                case I18n.Locale.Korean:
                    newPopupLabel.style.unityFontDefinition = new StyleFontDefinition(localeManager.krFont);
                    break;
                case I18n.Locale.Chinese:
                    newPopupLabel.style.unityFontDefinition = new StyleFontDefinition(localeManager.cnFont);
                    break;
                default:
                    newPopupLabel.style.unityFontDefinition = new StyleFontDefinition(localeManager.enFont);

                    if (Settings.Instance.currentLocale != I18n.Locale.German)
                    {
                        grownFontSize += 2;
                    }
                    break;
            }

            newPopupLabel.style.fontSize = grownFontSize;

            // Add popup to the UI
            root.Add(newPopupLabel);

            newPopupLabel.RegisterCallback<GeometryChangedEvent>(evt => CheckForPopup(evt, position, newPopupLabel, soundType, convertPosToUI, fromSelector));
        }

        void CheckForPopup(GeometryChangedEvent evt, Vector2 position, Label newPopupLabel, SoundManager.SoundType soundType, bool convertPosToUI, bool fromSelector)
        {
            root.UnregisterCallback<GeometryChangedEvent>(evt => CheckForPopup(evt, position, newPopupLabel, soundType, convertPosToUI, fromSelector));

            if (!isPopupShowing)
            {
                StartCoroutine(PopText(position, newPopupLabel, soundType, convertPosToUI, fromSelector));
            }
        }

        IEnumerator PopText(Vector2 position, Label newPopupLabel, SoundManager.SoundType soundType, bool convertPosToUI = false, bool fromSelector = false)
        {
            isPopupShowing = true;

            // Calc position
            Vector2 newPos = new(position.x, position.y);

            float fullWidth = newPopupLabel.resolvedStyle.width;
            float halfWidth = fullWidth / 2;

            if (convertPosToUI)
            {
                newPos = RuntimePanelUtils.CameraTransformWorldToPanel(
                    root.panel,
                    newPos,
                    Camera.main
                );
            }

            // Check for clamps
            if (newPos.x > root.worldBound.width - halfWidth)
            {
                newPos = new Vector2(root.worldBound.width - halfWidth, newPos.y);
            }
            else if ((newPos.x - halfWidth) < 0)
            {
                newPos = new Vector2(halfWidth, newPos.y);
            }

            // Set position
            newPopupLabel.style.left = newPos.x;
            newPopupLabel.style.top = newPos.y;

            // Set popup 
            newPopupLabel.style.visibility = Visibility.Visible;
            newPopupLabel.style.opacity = 0.2f;

            yield return new WaitForSeconds(0.1f);

            // Show popup
            newPopupLabel.style.opacity = 1f;
            newPopupLabel.style.scale = new StyleScale(new Vector2(1f, 1f));

            yield return new WaitForSeconds(timeOut / 2.5f); // 0.4f

            // Move popup higher
            newPopupLabel.style.top = newPos.y - 10;

            // Play popup sound
            if (soundType != SoundManager.SoundType.None)
            {
                soundManager.PlaySound(soundType);
            }

            yield return new WaitForSeconds(timeOut * 2);

            // Hide popup
            newPopupLabel.style.opacity = 0;

            yield return new WaitForSeconds(timeOut);

            // Remove popup from the UI
            isSelectorPopup = !fromSelector;

            root.Remove(newPopupLabel);

            isPopupShowing = false;
        }
    }
}