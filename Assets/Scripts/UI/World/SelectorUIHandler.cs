using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class SelectorUIHandler : MonoBehaviour
    {
        // Variables
        public Selector selector;
        public float bottomOffset = 50f;
        [ReadOnly]
        public bool isSelectorOpen = false;
        [ReadOnly]
        public bool isSelectAlt = false;

        // References
        private WorldUI worldUI;
        private ValuesUI valuesUI;
        private CharMain charMain;
        private Camera cam;

        // UI
        private VisualElement root;
        private VisualElement selectorBox;

        private Button denyButton;
        private Button confirmButton;

        private Button option1Button;
        private Button option2Button;
        private Button option3Button;

        void Start()
        {
            // Cache
            worldUI = GameRefs.Instance.worldUI;
            valuesUI = GameRefs.Instance.valuesUI;
            charMain = CharMain.Instance;
            cam = Camera.main;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            selectorBox = root.Q<VisualElement>("SelectorBox");

            denyButton = selectorBox.Q<Button>("DenyButton");
            confirmButton = selectorBox.Q<Button>("ConfirmButton");

            option1Button = selectorBox.Q<Button>("Option1Button");
            option2Button = selectorBox.Q<Button>("Option2Button");
            option3Button = selectorBox.Q<Button>("Option3Button");

            // UI taps
            denyButton.clicked += () => CancelSelection();
            confirmButton.clicked += () => ConfirmSelection();

            option1Button.clicked += () => SelectOption(0);
            option2Button.clicked += () => SelectOption(1);
            option3Button.clicked += () => SelectOption(2);
        }

        // Open the selector and set the appropriate sprites
        public void Open(Sprite[] sprites, int order, bool initialSelection, bool isAlt = false)
        {
            option1Button.style.backgroundImage = new StyleBackground(sprites[0]);
            option2Button.style.backgroundImage = new StyleBackground(sprites[1]);
            option3Button.style.backgroundImage = new StyleBackground(sprites[2]);

            SelectOptionButton(order);

            isSelectAlt = isAlt;

            if (initialSelection)
            {
                ToggleSelector(isAlt);
            }
        }

        void ToggleSelector(bool isAlt, bool canceled = false)
        {
            isSelectorOpen = !isSelectorOpen;

            UpdateSelector(isAlt, canceled);
        }

        void SelectOption(int order)
        {
            SelectOptionButton(order);

            selector.SelectOption(order);
        }

        void SelectOptionButton(int order)
        {
            option1Button.RemoveFromClassList("selector_option_selected");
            option2Button.RemoveFromClassList("selector_option_selected");
            option3Button.RemoveFromClassList("selector_option_selected");

            if (order == 0 || order == -1)
            {
                option1Button.AddToClassList("selector_option_selected");
            }
            else if (order == 1)
            {
                option2Button.AddToClassList("selector_option_selected");
            }
            else
            {
                option3Button.AddToClassList("selector_option_selected");
            }
        }

        // Open or close the selectorL
        void UpdateSelector(bool isAlt, bool canceled = false)
        {
            List<TimeValue> nullDelay = new() { new TimeValue(0.0f) };
            List<TimeValue> fullDelay = new() { new TimeValue(0.3f) };

            if (isSelectorOpen)
            {
                worldUI.CloseUI();

                valuesUI.CloseUI();

                charMain.Hide();

                selectorBox.style.bottom = 0;
                selectorBox.style.transitionDelay = nullDelay;
            }
            else
            {
                if (isSelectAlt && !canceled)
                {
                    isSelectAlt = false;
                }
                else
                {
                    worldUI.OpenUI();

                    valuesUI.OpenUI();

                    charMain.Show();

                    charMain.SelectSelectableAtPosition(cam.transform.position);
                }

                selectorBox.style.bottom = -60f;
                selectorBox.style.transitionDelay = fullDelay;
            }

            if (isAlt)
            {
                worldUI.SetUIButtons();
            }
        }

        // Handle canceling
        void CancelSelection()
        {
            selector.CancelSelecting(true);

            ToggleSelector(false, true);
        }

        // Handle confirming the selector
        void ConfirmSelection()
        {
            selector.SelectionConfirmed();

            ToggleSelector(false);
        }
    }
}