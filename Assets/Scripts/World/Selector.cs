using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    public SelectorUIHandler selectorUIHandler;
    public float tapDuration = 1f;

    void Start()
    {
        
    }

    void Update()
    {

    }

    public void SelectOption(int option)
    {
        Debug.Log("Selected option: " + option);
    }

    public void Select(Vector2 position)
    {
        Debug.Log("Selected: " + position.x + "x" + position.y);

       // selectorUIHandler.Open();
    }
}
