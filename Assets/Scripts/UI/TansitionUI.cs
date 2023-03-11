using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TansitionUI : MonoBehaviour
{
    public float duration = 5f;
    public bool loadingScene = false;
    private VisualElement transition;
    private VisualElement transitionBackground;

    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        transition = root.Q<VisualElement>("Transition");
        transitionBackground = root.Q<VisualElement>("TransitionBackground");

        if (!loadingScene)
        {
            StartCoroutine(Close());
        }
    }

    public void Open()
    {
        transition.style.bottom = new Length(0, LengthUnit.Percent);
        transitionBackground.style.top = new Length(-500, LengthUnit.Pixel);
    }

    IEnumerator Close()
    {
        transition.style.display = DisplayStyle.Flex;
        transitionBackground.style.display = DisplayStyle.Flex;

        yield return new WaitForSeconds(duration);

        transition.style.bottom = new Length(100, LengthUnit.Percent);
        transitionBackground.style.top = new Length(500, LengthUnit.Pixel);
    }
}
