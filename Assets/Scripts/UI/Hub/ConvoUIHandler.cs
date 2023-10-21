using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class ConvoUIHandler : MonoBehaviour
    {
        // Variables
        public float bottomOffset = 50f;
        [ReadOnly]
        public bool isConvoOpen = false;

        private Coroutine convoContainerTimeOut;

        // References
        private UIDocument valuesUIDoc;
        private UIDocument hubUIDoc;
        private SafeAreaHandler safeAreaHandler;

        // UI
        private VisualElement root;
        private VisualElement convoContainer;
        private VisualElement topShadow;
        private VisualElement bottomShadow;
        private Button skipButton;

        private VisualElement convoBox;
        private Button nextButton;
        private VisualElement avatarLeft;
        private VisualElement namePlate;
        private Label nameLabel;
        private Label convoLabel;

        private VisualElement valuesBox;

        private VisualElement topBox;
        private VisualElement bottomBox;

        void Start()
        {
            // Cache
            valuesUIDoc = GameRefs.Instance.valuesUIDoc;
            hubUIDoc = GameRefs.Instance.hubUIDoc;
            safeAreaHandler = GameRefs.Instance.hubUI.GetComponent<SafeAreaHandler>();

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            convoContainer = root.Q<VisualElement>("ConvoContainer");

            topShadow = convoContainer.Q<VisualElement>("TopShadow");
            bottomShadow = convoContainer.Q<VisualElement>("BottomShadow");
            skipButton = convoContainer.Q<Button>("SkipButton");

            convoBox = convoContainer.Q<VisualElement>("ConvoBox");
            nextButton = convoBox.Q<Button>("NextButton");
            avatarLeft = convoBox.Q<VisualElement>("AvatarLeft");
            namePlate = convoBox.Q<VisualElement>("NamePlate");
            nameLabel = namePlate.Q<Label>("NameLabel");
            convoLabel = namePlate.Q<Label>("ConvoLabel");

            valuesBox = valuesUIDoc.rootVisualElement.Q<VisualElement>("ValuesBox");

            topBox = hubUIDoc.rootVisualElement.Q<VisualElement>("TopBox");
            bottomBox = hubUIDoc.rootVisualElement.Q<VisualElement>("BottomBox");

            // UI Taps
            skipButton.clicked += () => HandleSkip();
            nextButton.clicked += () => HandleNext();
        }

        public void Converse()
        {
            nameLabel.text = "Tony";

            convoLabel.text = "It's a me! Tony!";

            ToggleConvo();
        }

        void ToggleConvo()
        {
            isConvoOpen = !isConvoOpen;

            UpdateConvo();
        }

        void UpdateConvo()
        {
            List<TimeValue> nullDelay = new();
            List<TimeValue> halfDelay = new();
            List<TimeValue> fullDelay = new();

            nullDelay.Add(new TimeValue(0.0f));
            halfDelay.Add(new TimeValue(0.3f));
            fullDelay.Add(new TimeValue(0.6f));

            if (isConvoOpen)
            {
                topBox.style.left = -50f;
                topBox.style.right = -50f;
                topBox.style.transitionDelay = fullDelay;
                bottomBox.style.bottom = -bottomOffset; // Note the -
                bottomBox.style.transitionDelay = fullDelay;

                valuesBox.style.top = -50f;
                valuesBox.style.transitionDelay = fullDelay;

                Glob.StopTimeout(convoContainerTimeOut);

                convoContainer.style.display = DisplayStyle.Flex;

                convoBox.style.bottom = 4;
                convoBox.style.transitionDelay = halfDelay;

                topShadow.style.top = -120;
                topShadow.style.transitionDelay = halfDelay;
                bottomShadow.style.top = -10;
                bottomShadow.style.transitionDelay = halfDelay;
                skipButton.style.opacity = 0;
                skipButton.style.transitionDelay = halfDelay;
                avatarLeft.style.left = 4;
                avatarLeft.style.transitionDelay = nullDelay;
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

                convoContainerTimeOut = Glob.SetTimeout(() =>
                {
                    convoContainer.style.display = DisplayStyle.None;
                }, 0.3f);

                convoBox.style.bottom = -120f;
                convoBox.style.transitionDelay = halfDelay;

                topShadow.style.top = -120;
                topShadow.style.transitionDelay = halfDelay;
                bottomShadow.style.top = -10;
                bottomShadow.style.transitionDelay = halfDelay;
                skipButton.style.opacity = 0;
                skipButton.style.transitionDelay = halfDelay;
                avatarLeft.style.left = -90;
                avatarLeft.style.transitionDelay = fullDelay;
            }
        }

        void HandleSkip()
        {
            Debug.Log("Skipping!");

            ToggleConvo();
        }

        void HandleNext()
        {
            Debug.Log("Next line here!");
        }
    }
}
