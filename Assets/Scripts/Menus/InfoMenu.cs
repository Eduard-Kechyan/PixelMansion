using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class InfoMenu : MonoBehaviour
{
    public Sprite unlockedQuestionMarkSprite;
    public Sprite infoCurrentSprite;
    public Sprite infoParentItemSprite;
    public Color textColor;
    public Color lineColor;

    private MenuManager menuManager;
    private InfoBox infoBox;

    private VisualElement root;

    private VisualElement infoMenu;
    private VisualElement itemSprite;
    private VisualElement unlockedItems;
    private VisualElement infoParent;
    private Label itemName;
    private Label itemData;

    private I18n LOCALE = I18n.Instance;

    void Start()
    {
        // Cache
        menuManager = GetComponent<MenuManager>();

        infoBox = menuManager.uiDoc.GetComponent<InfoBox>();

        // Cache UI
        root = menuManager.uiDoc.rootVisualElement;

        infoMenu = root.Q<VisualElement>("InfoMenu");

        unlockedItems = infoMenu.Q<VisualElement>("UnlockedItems");

        infoParent = infoMenu.Q<VisualElement>("InfoParent");

        itemSprite = infoMenu.Q<VisualElement>("ItemSprite");
        itemName = infoMenu.Q<Label>("ItemName");
        itemData = infoMenu.Q<Label>("ItemData");

        InitializeMenu();
    }

    void InitializeMenu()
    {
        infoMenu.style.display = DisplayStyle.None;
        infoParent.style.display = DisplayStyle.None;
    }

    public void Open(Item item)
    {
        if (infoMenu != null)
        {
            ClearData();

            // Title
            string title;

            if (item.type == Item.Type.Gen)
            {
                title = item.itemLevelName;
            }
            else
            {
                title = item.group.ToString();
            }

            // Item data
            itemSprite.style.backgroundImage = new StyleBackground(item.sprite);

            itemName.text = item.itemName;

            itemData.text = infoBox.GetItemData(item);

            // Unlocked items
            GetUnlockedItems(item);

            // Check info parent
            CheckInfoParent(item);

            // Open menu
            menuManager.OpenMenu(infoMenu, title);
        }
    }

    void GetUnlockedItems(Item item)
    {
        for (int i = 0; i < GameData.itemsData.Length; i++)
        {
            if (item.group == GameData.itemsData[i].group)
            {
                int count = 0;

                for (int j = 0; j < GameData.itemsData[i].content.Length; j++)
                {
                    VisualElement current = new VisualElement { name = "Current" };
                    VisualElement unlockedItem = new VisualElement { name = "UnlockedItem" + i };
                    VisualElement line = new VisualElement { name = "Line" };
                    Label order = new Label { name = "Order" };

                    // Current
                    current.style.width = 24f;
                    current.style.height = 24f;
                    current.style.marginRight = 4f;
                    current.style.marginBottom = 10f;

                    if (item.sprite == GameData.itemsData[i].content[j].sprite)
                    {
                        current.style.backgroundImage = new StyleBackground(infoCurrentSprite);
                    }

                    // Item
                    unlockedItem.style.width = 24f;
                    unlockedItem.style.height = 24f;
                    unlockedItem.style.position = Position.Absolute;
                    unlockedItem.style.left = 0;
                    unlockedItem.style.top = 0;

                    if (j % 5 == 4)
                    {
                        current.style.marginRight = 0;

                        line.style.display = DisplayStyle.None;
                    }

                    if (count == GameData.itemsData[i].content.Length)
                    {
                        line.style.display = DisplayStyle.None;
                    }

                    if (GameData.itemsData[i].content[j].unlocked)
                    {
                        unlockedItem.style.backgroundImage = new StyleBackground(
                            GameData.itemsData[i].content[j].sprite
                        );
                    }
                    else
                    {
                        unlockedItem.style.backgroundImage = new StyleBackground(
                            unlockedQuestionMarkSprite
                        );
                    }

                    // Order
                    order.style.width = 24f;
                    order.text = (j + 1) + "";
                    order.style.position = Position.Absolute;
                    order.style.left = Length.Percent(50);
                    order.style.bottom = -10f;
                    order.style.paddingLeft = 0;
                    order.style.paddingTop = 0;
                    order.style.paddingRight = 4f;
                    order.style.paddingBottom = 0;
                    order.style.fontSize = 6f;
                    order.style.color = new StyleColor(textColor);
                    order.style.translate = new Translate(Length.Percent(-50), 0f);
                    order.style.unityTextAlign = TextAnchor.MiddleCenter;
                    order.style.width = 24f;

                    // Line
                    line.style.width = 2f;
                    line.style.height = 1f;
                    line.style.position = Position.Absolute;
                    line.style.left = 25f;
                    line.style.top = Length.Percent(50);
                    line.style.translate = new Translate(0f, Length.Percent(-50));
                    line.style.backgroundColor = new StyleColor(lineColor);

                    // Add to UI
                    unlockedItem.Add(order);

                    unlockedItem.Add(line);

                    current.Add(unlockedItem);

                    unlockedItems.Add(current);

                    count++;
                }
            }
        }
    }

    void CheckInfoParent(Item item)
    {
        infoParent.style.display = DisplayStyle.None;

        if (item.parents.Length >= 0)
        {
            Label value = new Label { name = "Value" };

            value.style.position = Position.Absolute;
            value.style.left = 0f;
            value.style.top = 0f;
            value.style.right = 0f;
            value.style.height = 12f;
            value.style.paddingLeft = 0;
            value.style.paddingTop = 0;
            value.style.paddingRight = 0;
            value.style.paddingBottom = 0;
            value.style.marginLeft = 0;
            value.style.marginTop = 0;
            value.style.marginRight = 0;
            value.style.marginBottom = 0;
            value.style.fontSize = 6f;
            value.style.color = new StyleColor(textColor);
            value.style.unityTextAlign = TextAnchor.MiddleCenter;

            value.text = LOCALE.Get("info_menu_found_in");

            infoParent.Add(value);

            infoParent.style.display = DisplayStyle.Flex;

            for (int i = 0; i < item.parents.Length; i++)
            {
                Sprite parentItemSprite = GetParentSprite(item.parents[i]);

                if (parentItemSprite != null)
                {
                    VisualElement parent = new VisualElement { name = "Parent" + i };
                    VisualElement parentItem = new VisualElement { name = "ParentItem" + i };

                    parent.style.width = 24f;
                    parent.style.height = 24f;
                    parent.style.backgroundImage = new StyleBackground(infoParentItemSprite);

                    parentItem.style.width = 24f;
                    parentItem.style.height = 24f;
                    parentItem.style.backgroundImage = new StyleBackground(parentItemSprite);

                    // Add to UI
                    parent.Add(parentItem);

                    infoParent.Add(parent);
                }
            }
        }
    }

    Sprite GetParentSprite(Item.Group parentGroup)
    {
        Sprite sprite = null;

        for (int i = 0; i < GameData.itemsData.Length; i++)
        {
            if (parentGroup == GameData.itemsData[i].group)
            {
                if (GameData.itemsData[i].content.Length == 1)
                {
                    sprite = GameData.itemsData[i].content[0].sprite;
                }
                else
                {
                    for (int j = 0; j < GameData.itemsData[i].content.Length; j++)
                    {
                        if (!GameData.itemsData[i].content[j].unlocked && j >= 0)
                        {
                            sprite = GameData.itemsData[i].content[j - 1].sprite;

                            break;
                        }
                    }
                }
            }
        }

        return sprite;
    }

    void ClearData()
    {
        if (unlockedItems.childCount > 0)
        {
            unlockedItems.Clear();
        }

        if (infoParent.childCount > 0)
        {
            infoParent.Clear();
        }

        infoParent.style.display = DisplayStyle.None;
    }
}
