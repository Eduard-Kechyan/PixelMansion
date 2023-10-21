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

        // References
        private UIDocument valuesUIDoc;
        private UIDocument hubUIDoc;
        private HubUI hubUI;
        private SafeAreaHandler safeAreaHandler;

        // UI
        private VisualElement root;
        private VisualElement topBox;
        private VisualElement bottomBox;
        private VisualElement selectorBox;

        private Button denyButton;
        private Button confirmButton;

        private Button option1Button;
        private Button option2Button;
        private Button option3Button;

        private VisualElement valuesBox;

        void Start()
        {
            // Cache
            valuesUIDoc = GameRefs.Instance.valuesUIDoc;
            hubUIDoc = GameRefs.Instance.hubUIDoc;
            hubUI = GameRefs.Instance.hubUI;
            safeAreaHandler = GameRefs.Instance.hubUI.GetComponent<SafeAreaHandler>();

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            selectorBox = root.Q<VisualElement>("SelectorBox");

            denyButton = selectorBox.Q<Button>("DenyButton");
            confirmButton = selectorBox.Q<Button>("ConfirmButton");

            option1Button = selectorBox.Q<Button>("Option1Button");
            option2Button = selectorBox.Q<Button>("Option2Button");
            option3Button = selectorBox.Q<Button>("Option3Button");

            valuesBox = valuesUIDoc.rootVisualElement.Q<VisualElement>("ValuesBox");

            topBox = hubUIDoc.rootVisualElement.Q<VisualElement>("TopBox");
            bottomBox = hubUIDoc.rootVisualElement.Q<VisualElement>("BottomBox");

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

            if (initialSelection)
            {
                ToggleSelector(isAlt);
            }
        }

        void ToggleSelector(bool isAlt)
        {
            isSelectorOpen = !isSelectorOpen;

            UpdateSelector(isAlt);
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
        void UpdateSelector(bool isAlt)
        {
            List<TimeValue> nullDelay = new();
            List<TimeValue> fullDelay = new();

            nullDelay.Add(new TimeValue(0.0f));
            fullDelay.Add(new TimeValue(0.3f));

            if (isSelectorOpen)
            {
                topBox.style.left = -50f;
                topBox.style.right = -50f;
                topBox.style.transitionDelay = fullDelay;
                bottomBox.style.bottom = -bottomOffset; // Note the -
                bottomBox.style.transitionDelay = fullDelay;

                valuesBox.style.top = -50f;
                valuesBox.style.transitionDelay = fullDelay;

                selectorBox.style.bottom = 0;
                selectorBox.style.transitionDelay = nullDelay;
            }
            else
            {
                topBox.style.left = 0;
                topBox.style.right = 0;
                topBox.style.transitionDelay = nullDelay;
                bottomBox.style.bottom = 0;
                bottomBox.style.transitionDelay = nullDelay;

                valuesBox.style.top = safeAreaHandler.topPadding;
                valuesBox.style.transitionDelay = nullDelay;

                selectorBox.style.bottom = -60f;
                selectorBox.style.transitionDelay = fullDelay;
            }

            if (isAlt)
            {
                hubUI.SetUIButtons();
            }
        }

        // Handle canceling
        void CancelSelection()
        {
            selector.CancelSelecting(true);

            ToggleSelector(false);
        }

        // Handle confirming the selector
        void ConfirmSelection()
        {
            selector.SelectionConfirmed();

            ToggleSelector(false);
        }
    }
}