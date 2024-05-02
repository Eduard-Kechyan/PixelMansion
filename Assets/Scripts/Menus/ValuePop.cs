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

        [ReadOnly]
        public bool popping = false;
        [ReadOnly]
        public bool mightPop = false;

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

        public void PopValue(int amount, Item.CollGroup type, Vector2 initialPosition, bool multiply = false, bool useUIPosition = false, Action callback = null)
        {
            popping = true;

            // Run this first
            gameData.UpdateValue(amount, type, multiply);

            valuesUI.ShowButton(type);

            StartCoroutine(HandlePopValue(amount, type, initialPosition, multiply, useUIPosition, callback));
        }

        public void PopValue(int amount, Item.CollGroup type, bool multiply = false, bool useUIPosition = false, Action callback = null)
        {
            popping = true;
            mightPop = false;

            Vector2 screenCenter;

            if (useUIPosition)
            {
                screenCenter = new Vector2(root.worldBound.width / 2, root.worldBound.height / 2);
            }
            else
            {
                screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
            }

            PopValue(amount, type, screenCenter, multiply, useUIPosition, callback);
        }

        public void PopInventoryItem(Sprite sprite, Vector2 initialPosition, Vector2 position, Action callback = null)
        {
            popping = true;
            mightPop = false;

            StartCoroutine(HandlePopInventoryItem(sprite, initialPosition, position, callback));
        }

        public void PopBonus(
            Item item,
            Vector2 initialPosition,
            Vector2 bonusButtonPosition,
            bool useUIPosition = false
        )
        {
            popping = true;
            mightPop = false;

            StartCoroutine(
                HandlePopBonus(item, initialPosition, bonusButtonPosition, useUIPosition)
            );
        }

        public IEnumerator HandlePopValue(
            int amount,
            Item.CollGroup type,
            Vector2 initialPosition,
            bool multiply = false,
            bool useUIPosition = false,
            Action callback = null
        )
        {
            Sprite valuePopSprite;
            float valuePopOffset;
            SoundManager.SoundType valuePopSoundType;
            WaitForSeconds waitA = new WaitForSeconds(0.1f);
            WaitForSeconds waitB = new WaitForSeconds(0.1f);


            // Get the value pop's sprite, offset and SFX name
            switch (type)
            {
                case Item.CollGroup.Energy:
                    valuePopSprite = energySprite;
                    valuePopOffset = valuesUI.energyButton.resolvedStyle.display == DisplayStyle.Flex ? valuesUI.energyButton.layout.x : valuesUI.dummyEnergyButton.layout.x;
                    valuePopSoundType = SoundManager.SoundType.Energy;
                    break;
                case Item.CollGroup.Gold:
                    valuePopSprite = goldSprite;
                    valuePopOffset = valuesUI.goldButton.resolvedStyle.display == DisplayStyle.Flex ? valuesUI.goldButton.layout.x : valuesUI.dummyGoldButton.layout.x;
                    valuePopSoundType = SoundManager.SoundType.Gold;
                    break;
                case Item.CollGroup.Gems:
                    valuePopSprite = gemsSprite;
                    valuePopOffset = valuesUI.gemsButton.resolvedStyle.display == DisplayStyle.Flex ? valuesUI.gemsButton.layout.x : valuesUI.dummyGemsButton.layout.x;
                    valuePopSoundType = SoundManager.SoundType.Gems;
                    break;
                default: // Item.CollGroup.Experience
                    valuePopSprite = experienceSprite;
                    valuePopOffset = valuesUI.levelButton.resolvedStyle.display == DisplayStyle.Flex ? valuesUI.levelButton.layout.x : valuesUI.dummyLevelButton.layout.x;
                    valuePopSoundType = SoundManager.SoundType.Experience;
                    break;
            }

            // Add value pop element to the root
            VisualElement valuePop = InitializePopValueElement(valuePopSprite, initialPosition, false, useUIPosition);

            // Add the value pop to the root
            root.Add(valuePop);

            yield return waitA;

            // Increase the size of the value pop
            Scale scale = new(new Vector2(1f, 1f));

            valuePop.style.scale = new StyleScale(scale);

            yield return waitB;

            // Move the value pop to it's intended position
            valuePop.style.left = Mathf.Ceil(valuePopOffset + offset);
            valuePop.style.top = safeAreaHandler.topPadding + offset;

            yield return waitB;

            // Decrease the size of the value pop
            scale = new Scale(new Vector2(0f, 0f));

            valuePop.style.scale = new StyleScale(scale);

            yield return waitA;

            // Play value pop sound
            soundManager.PlaySound(valuePopSoundType);

            // Hide the value pop
            valuePop.style.visibility = Visibility.Hidden;
            valuePop.style.opacity = 0;

            yield return waitB;

            // Remove the value pop
            root.Remove(valuePop);

            // Update data
            if (type != Item.CollGroup.Experience)
            {
                gameData.UpdateValueUI(amount, type, multiply, () =>
                {
                    popping = false;

                    valuesUI.HideButtons();

                    callback?.Invoke();
                });
            }
            else
            {
                popping = false;

                valuesUI.HideButtons();

                callback?.Invoke();
            }
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

            popping = false;
        }

        IEnumerator HandlePopBonus(
            Item item,
            Vector2 initialPosition,
            Vector2 bonusButtonPosition,
            bool useUIPosition = false
        )
        {
            gameData.AddToBonus(item);

            Sprite valuePopSprite = item.sprite;

            // Add value pop element to the root
            VisualElement valuePop = InitializePopValueElement(valuePopSprite, initialPosition, true, useUIPosition);

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
            soundManager.PlaySound(SoundManager.SoundType.Experience);

            // Hide the value pop
            valuePop.style.visibility = Visibility.Hidden;
            valuePop.style.opacity = 0;

            yield return new WaitForSeconds(0.5f);

            popping = false;

            // Remove the value pop
            root.Remove(valuePop);

            gameData.CheckBonus();
        }

        VisualElement InitializePopValueElement(
            Sprite sprite,
            Vector2 position,
            bool isItem = true,
            bool useUIPosition = false
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
                if (useUIPosition)
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