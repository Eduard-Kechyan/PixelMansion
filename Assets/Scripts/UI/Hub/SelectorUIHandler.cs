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
    [ReadOnly]
    public bool isSelectorOpen = false;

    // References
    private UIDocument valuesUIDoc;
    private UIDocument hubUIDoc;

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

        denyButton.clicked += () => DenySlection();
        confirmButton.clicked += () => ConfirmSelection();

        option1Button.clicked += () => selector.SelectOption(0);
        option2Button.clicked += () => selector.SelectOption(1);
        option3Button.clicked += () => selector.SelectOption(2);
    }

    public void Open(Sprite[] sprites, bool initialSelection)
    {
        option1Button.style.backgroundImage = new StyleBackground(sprites[0]);
        option2Button.style.backgroundImage = new StyleBackground(sprites[1]);
        option3Button.style.backgroundImage = new StyleBackground(sprites[2]);

        if (initialSelection)
        {
            ToggleSelector();
        }
    }

    void ToggleSelector()
    {
        isSelectorOpen = !isSelectorOpen;

        UpdateSelector();
    }

    void UpdateSelector()
    {
        List<TimeValue> nullDelay = new List<TimeValue>();
        List<TimeValue> fullDelay = new List<TimeValue>();

        nullDelay.Add(new TimeValue(0.0f));
        fullDelay.Add(new TimeValue(0.3f));

        if (isSelectorOpen)
        {
            topBox.style.left = -50f;
            topBox.style.right = -50f;
            topBox.style.transitionDelay = fullDelay;
            bottomBox.style.bottom = -50f;
            bottomBox.style.transitionDelay = fullDelay;
            selectorBox.style.bottom = 0;
            selectorBox.style.transitionDelay = nullDelay;

            valuesBox.style.top = -50f;
            valuesBox.style.transitionDelay = fullDelay;
        }
        else
        {
            topBox.style.left = 0;
            topBox.style.right = 0;
            topBox.style.transitionDelay = nullDelay;
            bottomBox.style.bottom = 0;
            bottomBox.style.transitionDelay = nullDelay;
            selectorBox.style.bottom = -60f;
            selectorBox.style.transitionDelay = fullDelay;

            valuesBox.style.top = 0;
            valuesBox.style.transitionDelay = nullDelay;
        }
    }

    void DenySlection()
    {
        selector.CancelSelecting(true);

        ToggleSelector();
    }

    void ConfirmSelection()
    {
        selector.SelectionConfirmed();

        ToggleSelector();
    }
}
}