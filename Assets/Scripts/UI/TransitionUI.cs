using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TransitionUI : MonoBehaviour
{
    // Variables
    public float duration = 5f;
    public bool loadingScene = false;
    
    // UI
    private VisualElement root;
    private VisualElement transition;

    void Start()
    {
        // UI
        root = GetComponent<UIDocument>().rootVisualElement;

        transition = root.Q<VisualElement>("Transition");

        if (!loadingScene)
        {
            StartCoroutine(Close());
        }
    }

    public void Open()
    {
        transition.style.opacity = 1;
        transition.style.visibility = Visibility.Visible;
    }

    IEnumerator Close()
    {
        transition.style.display = DisplayStyle.Flex;

        yield return new WaitForSeconds(duration);

        transition.style.opacity = 0;
        transition.style.visibility = Visibility.Hidden;
    }
}
