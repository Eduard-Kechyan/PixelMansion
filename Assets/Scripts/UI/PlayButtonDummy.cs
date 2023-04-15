using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayButtonDummy : MonoBehaviour
{
    public SceneLoader sceneLoader;
    private VisualElement root;
    private Button button;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        button = root.Q<Button>("PlayButtonDummy");

        button.clicked += () => sceneLoader.Load(2);
    }
}
