using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TansitionUI : MonoBehaviour
{
    public float duration = 5f;
    public bool loadingScene = false;
    private VisualElement root ;
    private VisualElement transition;
    private VisualElement transitionBackground;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        transition = root.Q<VisualElement>("Transition");
        //transitionBackground = root.Q<VisualElement>("TransitionBackground");

        if (!loadingScene)
        {
            StartCoroutine(Close());
        }
    }

    public void Open()
    {
        transition.style.opacity = 1;
        transition.style.visibility = Visibility.Visible;
        /*transition.style.bottom = new Length(0, LengthUnit.Percent);
        transitionBackground.style.top = new Length(-500, LengthUnit.Pixel);*/
    }

    IEnumerator Close()
    {
        /*transition.style.display = DisplayStyle.Flex;
        transitionBackground.style.display = DisplayStyle.Flex;*/
        transition.style.display = DisplayStyle.Flex;

        yield return new WaitForSeconds(duration);

        transition.style.opacity = 0;
        transition.style.visibility = Visibility.Hidden;
        /*transition.style.bottom = new Length(100, LengthUnit.Percent);
        transitionBackground.style.top = new Length(500, LengthUnit.Pixel);*/
    }
}
