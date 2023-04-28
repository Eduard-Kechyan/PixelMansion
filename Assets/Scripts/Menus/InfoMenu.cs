using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;
using UnityEngine.SceneManagement;

public class InfoMenu : MonoBehaviour
{
    // Variables
    public Sprite unlockedQuestionMarkSprite;
    public Sprite infoCurrentSprite;
    public Sprite infoParentItemSprite;
    public Color textColor;
    public Color lineColor;

    // References
    private MenuUI menuUI;
    private InfoBox infoBox;

    // Instances
    private GameData gameData;
    private I18n LOCALE;

    // UI
    private VisualElement root;
    private VisualElement infoMenu;
    private VisualElement itemSprite;
    private VisualElement unlockedItems;
    private VisualElement infoParent;
    private Label itemName;
    private Label itemData;

    void Start()
    {
        // Cache
        menuUI = GetComponent<MenuUI>();

        if (SceneManager.GetActiveScene().name == "Gameplay")
        {
            infoBox = GameRefs.Instance.gameplayUI.GetComponent<InfoBox>();
        }

        // Cache instances
        gameData = GameData.Instance;
        LOCALE = I18n.Instance;

        // Cache UI
        root = GetComponent<UIDocument>().rootVisualElement;

        infoMenu = root.Q<VisualElement>("InfoMenu");

        unlockedItems = infoMenu.Q<VisualElement>("UnlockedItems");

        infoParent = infoMenu.Q<VisualElement>("InfoParent");

        itemSprite = infoMenu.Q<VisualElement>("ItemSprite");
        itemName = infoMenu.Q<Label>("ItemName");
        itemData = infoMenu.Q<Label>("ItemData");

        InitMenu();
    }

    void InitMenu()
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

            if (item.type == Types.Type.Gen)
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

            if (infoBox != null)
            {
                itemData.text = infoBox.GetItemData(item);
            }
            else
            {
                // TODO - Change this DUMMY
                itemData.text = "DUMMY";
            }

            // Unlocked items
            GetUnlockedItems(item);

            // Check info parent
            CheckInfoParent(item);

            // Open menu
            menuUI.OpenMenu(infoMenu, title);
        }
    }

    void GetUnlockedItems(Item item)
    {
        for (int i = 0; i < gameData.itemsData.Length; i++)
        {
            if (item.group == gameData.itemsData[i].group)
            {
                int count = 0;

                for (int j = 0; j < gameData.itemsData[i].content.Length; j++)
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

                    if (item.sprite == gameData.itemsData[i].content[j].sprite)
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

                    if (count == gameData.itemsData[i].content.Length)
                    {
                        line.style.display = DisplayStyle.None;
                    }

                    if (
                        gameData.itemsData[i].content[j].unlocked
                        || gameData.itemsData[i].content[j].sprite.name == item.sprite.name
                    )
                    {
                        unlockedItem.style.backgroundImage = new StyleBackground(
                            gameData.itemsData[i].content[j].sprite
                        );
                    }
                    else
                    {
                        unlockedItem.style.backgroundImage = new StyleBackground(
                            unlockedQuestionMarkSprite
                        );
                    }

                    // Order
                    order.text = (j + 1) + "";
                    order.style.width = 24f;
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

    Sprite GetParentSprite(Types.GenGroup parentGroup)
    {
        Sprite sprite = null;

        for (int i = 0; i < gameData.generatorsData.Length; i++)
        {
            if (parentGroup == gameData.generatorsData[i].genGroup)
            {
                if (gameData.generatorsData[i].content.Length == 1)
                {
                    sprite = gameData.generatorsData[i].content[0].sprite;
                }
                else
                {
                    for (int j = 0; j < gameData.generatorsData[i].content.Length; j++)
                    {
                        if (!gameData.generatorsData[i].content[j].unlocked && j >= 0)
                        {
                            sprite = gameData.generatorsData[i].content[j - 1].sprite;

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
