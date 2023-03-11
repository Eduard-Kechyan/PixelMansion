using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GamePlayButtons : MonoBehaviour
{
    private VisualElement root;
    private Button storageButton;

    private InventoryMenu inventoryMenu;

    void Start()
    {
        // UI
        root = GetComponent<UIDocument>().rootVisualElement;

        storageButton = root.Q<Button>("InventoryButton");

        // Menus
        inventoryMenu = MenuManager.Instance.GetComponent<InventoryMenu>();

        //menuManager
        storageButton.clickable.clicked += () => inventoryMenu.Open();
    }
}
