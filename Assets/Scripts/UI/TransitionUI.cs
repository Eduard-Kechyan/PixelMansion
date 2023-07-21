using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class TransitionUI : MonoBehaviour
{
    // Variables
    public float duration = 0.5f;
    public Color[] backgroundColors = new Color[0];
    public Sprite[] iconSprites = new Sprite[0];

    // UI
    private VisualElement root;
    private VisualElement transition;

    void Start()
    {
        // UI
        root = GetComponent<UIDocument>().rootVisualElement;

        transition = root.Q<VisualElement>("Transition");

        if (SceneManager.GetActiveScene().name == "Loading")
        {
            transition.style.display = DisplayStyle.Flex;
            transition.style.opacity = 0;
            transition.style.visibility = Visibility.Hidden;
        }
        else
        {
            StartCoroutine(Close());
        }
    }

    public void Open()
    {
        if (backgroundColors.Length > 0)
        {
            transition.style.backgroundColor = backgroundColors[Random.Range(0, backgroundColors.Length)];
        }

        // Do the rest afterwards

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
