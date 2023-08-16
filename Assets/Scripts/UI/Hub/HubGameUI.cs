using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
public class HubGameUI : MonoBehaviour
{
    // UI
    private VisualElement root;
    
    void Start()
    {
        // UI
        root = GetComponent<UIDocument>().rootVisualElement;        
    }
}
}