using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class ValuePop : MonoBehaviour
    {
        // Variables
        public float offset = 3f;
        public Sprite experienceSprite;
        public Sprite energySprite;
        public Sprite goldSprite;
        public Sprite gemsSprite;

        // References
        private ValuesUI valuesUI;
        private SafeAreaHandler safeAreaHandler;
        private float popWidth = 0f;

        // Instances
        private GameData gameData;

        // UI
        private VisualElement root;
        private SoundManager soundManager;

        public class Pop
        {
            public float amount;
            public string type;
        }

        void Start()
        {
            // Cache
            valuesUI = GameRefs.Instance.valuesUI;
            safeAreaHandler = GameRefs.Instance.safeAreaHandler;

            // Cache instances
            gameData = GameData.Instance;
            soundManager = SoundManager.Instance;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;
        }

        public void PopValue(int amount, Types.CollGroup type, Vector2 position = default, bool multiply = false, bool isUIPosition = false, Action callback = null)
        {
            // Run this first
            gameData.UpdateValue(amount, type, multiply);

            StartCoroutine(HandlePopValue(amount, type, position, multiply, isUIPosition, callback));
        }

        public void PopInventoryItem(Sprite sprite, Vector2 initialPosition, Vector2 position, Action callback = null)
        {
            StartCoroutine(HandlePopInventoryItem(sprite, initialPosition, position, callback));
        }

        public void PopBonus(
            Item item,
            Vector2 initialPosition,
            Vector2 bonusButtonPosition,
            bool check = true,
            bool isUIPosition = false
        )
        {
            StartCoroutine(
                HandlePopBonus(item, initialPosition, bonusButtonPosition, check, isUIPosition)
            );
        }

        public IEnumerator HandlePopValue(
            int amount,
            Types.CollGroup type,
            Vector2 position,
            bool multiply = false,
            bool isUIPosition = false,
            Action callback = null
        )
        {
            Sprite valuePopSprite;
            float valuePopOffset;
            string valuePopSFX;

            // Get the value pop's sprite, offset and SFX name
            switch (type)
            {
                case Types.CollGroup.Energy:
                    valuePopSprite = energySprite;
                    valuePopOffset = valuesUI.energyButton.layout.x;
                    break;
                case Types.CollGroup.Gold:
                    valuePopSprite = goldSprite;
                    valuePopOffset = valuesUI.goldButton.layout.x;
                    break;
                case Types.CollGroup.Gems:
                    valuePopSprite = gemsSprite;
                    valuePopOffset = valuesUI.gemsButton.layout.x;
                    break;
                default: // Types.CollGroup.Experience
                    valuePopSprite = experienceSprite;
                    valuePopOffset = valuesUI.levelButton.layout.x;
                    break;
            }

            valuePopSFX = type.ToString();

            // Add value pop element to the root
            VisualElement valuePop = InitializePopValueElement(valuePopSprite, position, false, isUIPosition);

            // Add the value pop to the root
            root.Add(valuePop);

            yield return new WaitForSeconds(0.1f);

            // Increase the size of the value pop
            Scale scale = new(new Vector2(1f, 1f));

            valuePop.style.scale = new StyleScale(scale);

            yield return new WaitForSeconds(0.5f);

            // Move the value pop to it's intended position
            valuePop.style.left = Mathf.Ceil(valuePopOffset + offset);
            valuePop.style.top = safeAreaHandler.topPadding + offset;

            yield return new WaitForSeconds(0.5f);

            // Decrease the size of the value pop
            scale = new Scale(new Vector2(0f, 0f));

            valuePop.style.scale = new StyleScale(scale);

            yield return new WaitForSeconds(0.1f);

            callback?.Invoke();

            // Play value pop sound
            soundManager.PlaySound(valuePopSFX);

            // Hide the value pop
            valuePop.style.visibility = Visibility.Hidden;
            valuePop.style.opacity = 0;

            yield return new WaitForSeconds(0.5f);

            // Remove the value pop
            root.Remove(valuePop);

            // Update data
            gameData.UpdateValueUI(amount, type, multiply);
        }

        public IEnumerator HandlePopInventoryItem(Sprite sprite, Vector2 initialPosition, Vector2 position, Action callback = null)
        {
            Sprite valuePopSprite = sprite;

            // Add value pop element to the root
            VisualElement valuePop = InitializePopValueElement(valuePopSprite, initialPosition, true, true);

            // Add the value pop to the root
            root.Add(valuePop);

            yield return new WaitForSeconds(0.1f);

            // Increase the size of the value pop
            Scale scale = new(new Vector2(1f, 1f));

            valuePop.style.scale = new StyleScale(scale);

            yield return new WaitForSeconds(0.5f);

            // Move the value pop to it's intended position
            Vector2 newUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
                root.panel,
                position,
                Camera.main
            );

            valuePop.style.left = newUIPos.x - (28 / 4);
            valuePop.style.top = newUIPos.y - (28 / 4);

            yield return new WaitForSeconds(0.5f);

            // Decrease the size of the value pop
            scale = new Scale(new Vector2(0f, 0f));

            valuePop.style.scale = new StyleScale(scale);

            yield return new WaitForSeconds(0.1f);

            callback?.Invoke();

            // Hide the value pop
            valuePop.style.visibility = Visibility.Hidden;
            valuePop.style.opacity = 0;

            yield return new WaitForSeconds(0.5f);

            // Remove the value pop
            root.Remove(valuePop);
        }

        public IEnumerator HandlePopBonus(
            Item item,
            Vector2 initialPosition,
            Vector2 bonusButtonPosition,
            bool check = true,
            bool isUIPosition = false
        )
        {
            Sprite valuePopSprite = item.sprite;
            string valuePopSFX = "Experience";

            // Add value pop element to the root
            VisualElement valuePop = InitializePopValueElement(valuePopSprite, initialPosition, true, isUIPosition);

            // Get position on the UI from the scene
            Vector2 newUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
                root.panel,
                bonusButtonPosition,
                Camera.main
            );

            float halfWidth = GameData.BOARD_ITEM_WIDTH / 2;

            // Add the value pop to the root
            root.Add(valuePop);

            yield return new WaitForSeconds(0.1f);

            // Increase the size of the value pop
            Scale scale = new(new Vector2(1f, 1f));

            valuePop.style.scale = new StyleScale(scale);

            yield return new WaitForSeconds(0.5f);

            // Move the value pop to it's intended position
            valuePop.style.left = newUIPos.x - halfWidth;
            valuePop.style.top = newUIPos.y - halfWidth;

            yield return new WaitForSeconds(0.5f);

            // Decrease the size of the value pop
            scale = new(new Vector2(0f, 0f));

            valuePop.style.scale = new StyleScale(scale);

            yield return new WaitForSeconds(0.1f);

            // Play value pop sound
            soundManager.PlaySound(valuePopSFX);

            // Hide the value pop
            valuePop.style.visibility = Visibility.Hidden;
            valuePop.style.opacity = 0;

            yield return new WaitForSeconds(0.5f);

            // Remove the value pop
            root.Remove(valuePop);

            gameData.AddToBonus(item, check); // TODO - Change the location of this line
        }

        VisualElement InitializePopValueElement(
            Sprite sprite,
            Vector2 position,
            bool isItem = true,
            bool isUIPosition = false
        )
        {
            VisualElement newValuePop = new() { name = "ValuePop" };

            // Set the value pop's styles
            Scale scale = new(new Vector2(0f, 0f));

            List<TimeValue> durations = new();
            durations.Add(new TimeValue(0.5f, TimeUnit.Second));

            popWidth = isItem ? GameData.BOARD_ITEM_WIDTH : sprite.rect.width;

            newValuePop.style.width = popWidth;
            newValuePop.style.height = popWidth;
            newValuePop.style.position = Position.Absolute;
            newValuePop.style.marginBottom = 0;
            newValuePop.style.backgroundImage = new StyleBackground(sprite);
            newValuePop.style.transitionDuration = new StyleList<TimeValue>(durations);

            newValuePop.style.scale = new StyleScale(scale);

            float halfWidth = popWidth / 2;

            // Check where we should initialize the pop value
            if (position.x == 0 && position.y == 0)
            {
                // Calculate the center of the UI
                float rootHalfWidth = root.resolvedStyle.width / 2;
                float rootHalfHeight = root.resolvedStyle.height / 2;

                // Set the value pop's position
                newValuePop.style.left = rootHalfWidth - halfWidth;
                newValuePop.style.top = rootHalfHeight - halfWidth;
            }
            else
            {
                if (isUIPosition)
                {
                    newValuePop.style.left = position.x - halfWidth;
                    newValuePop.style.top = position.y - halfWidth;
                }
                else
                {
                    // Get position on the UI from the scene
                    Vector2 newUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
                        root.panel,
                        position,
                        Camera.main
                    );

                    newValuePop.style.left = newUIPos.x - halfWidth;
                    newValuePop.style.top = newUIPos.y - halfWidth;
                }
            }

            return newValuePop;
        }
    }
}