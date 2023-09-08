using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace Merge
{
    public class InfoMenu : MonoBehaviour
    {
        // Variables
        public Sprite unlockedQuestionMarkSprite;
        public Sprite infoCurrentSprite;

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
            gameData = GameData.Instance;
            LOCALE = I18n.Instance;

            if (SceneManager.GetActiveScene().name == "Gameplay")
            {
                infoBox = GameRefs.Instance.gameplayUI.GetComponent<InfoBox>();
            }

            // Cache UI
            root = GetComponent<UIDocument>().rootVisualElement;

            infoMenu = root.Q<VisualElement>("InfoMenu");

            unlockedItems = infoMenu.Q<VisualElement>("UnlockedItems");

            infoParent = infoMenu.Q<VisualElement>("InfoParent");

            itemSprite = infoMenu.Q<VisualElement>("ItemSprite");
            itemName = infoMenu.Q<Label>("ItemName");
            itemData = infoMenu.Q<Label>("ItemData");

            Init();
        }

        void Init()
        {
            // Make sure the menu is closed
            infoMenu.style.display = DisplayStyle.None;
            infoMenu.style.opacity = 0;

            infoParent.style.display = DisplayStyle.None;
        }

        public void Open(Item item)
        {
            if (infoMenu != null)
            {
                ClearData();

                // Title
                string title;

                switch (item.type)
                {
                    case Types.Type.Item:
                        title = LOCALE.Get("Item_" + item.group + "_" + item.level);
                        break;
                    case Types.Type.Gen:
                        title = LOCALE.Get("Gen_" + item.genGroup);
                        break;
                    case Types.Type.Coll:
                        title = LOCALE.Get("Coll_" + item.collGroup);
                        break;
                    default: // Types.Type.Chest
                        title = LOCALE.Get("Chest_" + item.chestGroup);
                        break;
                }

                // Item data
                itemSprite.style.backgroundImage = new StyleBackground(item.sprite);

                itemName.text = item.itemName;

                if (infoBox != null)
                {
                    itemData.text = infoBox.GetItemData(item, true);
                }
                else
                {
                    itemData.text = LOCALE.Get("info_box_null");

                    ErrorManager.Instance.ThrowFull(
                        Types.ErrorType.Code,
                        "InfoMenu.cs -> Open()",
                        "InfoBox is null!",
                        "0",
                        true
                    );
                }

                // Unlocked items
                if (item.type == Types.Type.Chest)
                {
                    if (item.chestGroup == Types.ChestGroup.Item)
                    {
                        GetUnlockedItems(item);
                    }
                    else
                    {
                        infoParent.style.display = DisplayStyle.None;

                        GetChestItems(item);
                    }
                }
                else
                {
                    GetUnlockedItems(item);
                }

                if (item.type != Types.Type.Coll && item.type != Types.Type.Chest)
                {
                    // Check info parent
                    CheckInfoParent(item);
                }

                // Open menu
                menuUI.OpenMenu(infoMenu, title);
            }
        }

        void GetUnlockedItems(Item item)
        {
            Types.Items[] items;
            bool isGroup = false;
            bool isGenGroup = false;
            bool isCollGroup = false;
            bool isChestGroup = false;

            switch (item.type)
            {
                case Types.Type.Item:
                    items = gameData.itemsData;
                    isGroup = true;
                    break;
                case Types.Type.Gen:
                    items = gameData.generatorsData;
                    isGenGroup = true;
                    break;
                case Types.Type.Coll:
                    items = gameData.collectablesData;
                    isCollGroup = true;
                    break;
                default: // Types.Type.Chest
                    items = gameData.chestsData;
                    isChestGroup = true;
                    break;
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (
                    (isGroup && item.group == items[i].group)
                    || (isGenGroup && item.genGroup == items[i].genGroup)
                    || (isCollGroup && item.collGroup == items[i].collGroup)
                    || (isChestGroup && item.chestGroup == items[i].chestGroup)
                )
                {
                    for (int j = 0; j < items[i].content.Length; j++)
                    {
                        VisualElement current = new() { name = "Current" };
                        VisualElement unlockedItem = new() { name = "UnlockedItem" + i };
                        VisualElement line = new() { name = "Line" };
                        Label order = new() { name = "Order" };

                        // Current
                        current.AddToClassList("current_item");

                        if (item.sprite == items[i].content[j].sprite)
                        {
                            current.style.backgroundImage = new StyleBackground(infoCurrentSprite);
                        }

                        // Item
                        unlockedItem.AddToClassList("item");

                        if (j % 5 == 4)
                        {
                            current.style.marginRight = 0;

                            line.style.display = DisplayStyle.None;
                        }

                        if (j == items[i].content.Length - 1)
                        {
                            line.style.display = DisplayStyle.None;
                        }

                        if (
                            items[i].content[j].unlocked
                            || items[i].content[j].sprite.name == item.sprite.name
                        )
                        {
                            unlockedItem.style.backgroundImage = new StyleBackground(
                                items[i].content[j].sprite
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
                        order.AddToClassList("order");

                        // Line
                        line.AddToClassList("line");

                        // Add to UI
                        unlockedItem.Add(order);

                        unlockedItem.Add(line);

                        current.Add(unlockedItem);

                        unlockedItems.Add(current);
                    }
                }
            }
        }

        void GetChestItems(Item item)
        {
            List<Sprite> itemSprites = new();

            if (item.chestGroup == Types.ChestGroup.Item)
            {
                // TODO - Handle item chests
                // Have a list of possible items that the chest might hold
            }
            else if (item.chestGroup == Types.ChestGroup.Piggy)
            {
                for (int i = 0; i < gameData.collectablesData.Length; i++)
                {
                    if (
                        gameData.collectablesData[i].collGroup == Types.CollGroup.Gold
                        || gameData.collectablesData[i].collGroup == Types.CollGroup.Gems
                    )
                    {
                        for (int j = 0; j < gameData.collectablesData[i].content.Length; j++)
                        {
                            itemSprites.Add(gameData.collectablesData[i].content[j].sprite);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < gameData.collectablesData.Length; i++)
                {
                    if (gameData.collectablesData[i].collGroup == Types.CollGroup.Energy)
                    {
                        for (int j = 0; j < gameData.collectablesData[i].content.Length; j++)
                        {
                            itemSprites.Add(gameData.collectablesData[i].content[j].sprite);
                        }
                    }
                }
            }

            // Show the items
            for (int i = 0; i < itemSprites.Count; i++)
            {
                VisualElement current = new VisualElement { name = "Current" };
                VisualElement unlockedItem = new VisualElement { name = "UnlockedItem" + i };
                VisualElement line = new VisualElement { name = "Line" };

                // Current
                current.AddToClassList("current_item");

                // Item
                unlockedItem.AddToClassList("item");

                if (i % 5 == 4)
                {
                    current.style.marginRight = 0;

                    line.style.display = DisplayStyle.None;
                }

                if (i == itemSprites.Count - 1)
                {
                    line.style.display = DisplayStyle.None;
                }

                unlockedItem.style.backgroundImage = new StyleBackground(itemSprites[i]);

                // Line
                line.AddToClassList("line");

                // Add to UI
                unlockedItem.Add(line);

                current.Add(unlockedItem);

                unlockedItems.Add(current);
            }
        }

        void CheckInfoParent(Item item)
        {
            infoParent.style.display = DisplayStyle.None;

            if (item.parents.Length >= 0)
            {
                Label value = new() { name = "Value", text = LOCALE.Get("info_menu_found_in") };

                value.AddToClassList("value");

                infoParent.Add(value);

                infoParent.style.display = DisplayStyle.Flex;

                for (int i = 0; i < item.parents.Length; i++)
                {
                    Sprite parentItemSprite = GetParentSprite(item.parents[i]);

                    if (parentItemSprite != null)
                    {
                        VisualElement parent = new() { name = "Parent" + i };
                        VisualElement parentItem = new() { name = "ParentItem" + i };

                        parent.AddToClassList("parent");

                        parentItem.AddToClassList("parent_item");
                        parentItem.style.backgroundImage = new StyleBackground(parentItemSprite);

                        // Add to UI
                        parent.Add(parentItem);

                        infoParent.Add(parent);
                    }
                }
            }
        }

        Sprite GetParentSprite(ItemTypes.GenGroup parentGroup)
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
                        for (int j = gameData.generatorsData[i].content.Length-1; j >= 0;)
                        {
                            // TODO - Check this
                            /*if (gameData.generatorsData[i].content[j].unlocked)
                            {
                                sprite = gameData.generatorsData[i].content[j].sprite;

                                break;
                            }*/

                            sprite = gameData.generatorsData[i].content[j].sprite;

                            break;
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
}
