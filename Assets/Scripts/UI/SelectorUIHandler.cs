using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SelectorUIHandler : MonoBehaviour
{
    // Variables
    public Selector selector;
    public bool selectorOpen = false;

    // References
    private UIDocument valuesUIDoc;

    // UI
    private VisualElement root;
    private VisualElement topBox;
    private VisualElement bottomBox;
    private VisualElement selectorBox;

    private Button denyButton;
    private Button confirmButton;

    private Button option1;
    private Button option2;
    private Button option3;

    private VisualElement valuesBox;

    void Start()
    {
        // Cache
        valuesUIDoc = GameRefs.Instance.valuesUIDoc;

        // UI
        root = GetComponent<UIDocument>().rootVisualElement;

        topBox = root.Q<VisualElement>("TopBox");
        bottomBox = root.Q<VisualElement>("BottomBox");
        selectorBox = root.Q<VisualElement>("SelectorBox");

        denyButton = selectorBox.Q<Button>("DenyButton");
        confirmButton = selectorBox.Q<Button>("ConfirmButton");

        option1 = selectorBox.Q<Button>("Option1Button");
        option2 = selectorBox.Q<Button>("Option2Button");
        option3 = selectorBox.Q<Button>("Option3Button");

        valuesBox = valuesUIDoc.rootVisualElement.Q<VisualElement>("ValuesBox");

        denyButton.clicked += () => ToggleSelector();
        confirmButton.clicked += () => ToggleSelector();

        option1.clicked += () => selector.SelectOption(1);
        option2.clicked += () => selector.SelectOption(2);
        option3.clicked += () => selector.SelectOption(3);
    }

    public void Open(Sprite[] sprites)
    {
        // TODO - Set option sprites here

        option1.style.backgroundImage = new StyleBackground(sprites[0]);
        option2.style.backgroundImage = new StyleBackground(sprites[1]);
        option3.style.backgroundImage = new StyleBackground(sprites[2]);

        //ToggleSelector();
    }

    void ToggleSelector()
    {
        selectorOpen = !selectorOpen;

        UpdateSelector();
    }

    void UpdateSelector()
    {
        List<TimeValue> nullDelay = new List<TimeValue>();
        List<TimeValue> fullDelay = new List<TimeValue>();

        nullDelay.Add(new TimeValue(0.0f));
        fullDelay.Add(new TimeValue(0.3f));

        if (selectorOpen)
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
}
