using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class InfoMenu : MonoBehaviour
    {
        // Variables
        public Sprite unlockedQuestionMarkSprite;
        public Sprite infoCurrentSprite;

        private MenuUI.Menu menuType = MenuUI.Menu.Info;

        // References
        private GameData gameData;
        private I18n LOCALE;
        private MenuUI menuUI;
        private UIData uiData;
        private ItemHandler itemHandler;

        // UI
        private VisualElement content;
        private VisualElement itemSprite;
        private VisualElement unlockedItems;
        private VisualElement containItems;
        private VisualElement infoParent;
        private Label itemName;
        private Label itemData;

        void Start()
        {
            // Cache
            gameData = GameData.Instance;
            LOCALE = I18n.Instance;
            menuUI = GetComponent<MenuUI>();
            uiData = GameData.Instance.GetComponent<UIData>();
            itemHandler = DataManager.Instance.GetComponent<ItemHandler>();

            DataManager.Instance.CheckLoaded(() =>
            {
                // UI
                content = uiData.GetMenuAsset(menuType);

                unlockedItems = content.Q<VisualElement>("UnlockedItems");
                containItems = content.Q<VisualElement>("ContainItems");
                infoParent = content.Q<VisualElement>("InfoParent");

                itemSprite = content.Q<VisualElement>("ItemSprite");

                itemName = content.Q<Label>("ItemName");
                itemData = content.Q<Label>("ItemData");

                Init();
            });
        }

        void Init()
        {
            containItems.style.display = DisplayStyle.Flex;
            infoParent.style.display = DisplayStyle.None;
        }

        public void Open(Item item)
        {
            // Check menu
            if (menuUI.IsMenuOpen(menuType))
            {
                return;
            }

            // Set menu content
            ClearData();

            // Item data
            itemSprite.style.backgroundImage = new StyleBackground(item.sprite);

            itemName.text = item.itemName;

            itemData.text = itemHandler.GetItemData(item, true);

            // Content
            string title;

            switch (item.type)
            {
                case Item.Type.Item:
                    title = LOCALE.Get("Item_" + item.group + "_" + item.level);

                    GetUnlockedItems(item);
                    GetParent(item);
                    break;
                case Item.Type.Gen:
                    title = LOCALE.Get("Gen_" + item.genGroup);

                    GetUnlockedItems(item);
                    GetParent(item);
                    break;
                case Item.Type.Coll:
                    if (item.collGroup == Item.CollGroup.Gems)
                    {
                        title = LOCALE.Get("Coll_" + item.collGroup, item.level);
                    }
                    else
                    {
                        title = LOCALE.Get("Coll_" + item.collGroup);
                    }

                    GetUnlockedItems(item, true);

                    if (item.collGroup != Item.CollGroup.Experience)
                    {
                        GetParent(item);
                    }
                    break;
                default: // Item.Type.Chest
                    title = LOCALE.Get("Chest_" + item.chestGroup);

                    GetUnlockedItems(item);
                    GetChestItems(item);
                    break;
            }

            // Open menu
            menuUI.OpenMenu(content, menuType, title);
        }

        void GetUnlockedItems(Item item, bool showAll = false)
        {
            BoardManager.TypeItem[] items;
            bool isGroup = false;
            bool isGenGroup = false;
            bool isCollGroup = false;
            bool isChestGroup = false;

            switch (item.type)
            {
                case Item.Type.Item:
                    items = gameData.itemsData;
                    isGroup = true;
                    break;
                case Item.Type.Gen:
                    items = gameData.generatorsData;
                    isGenGroup = true;
                    break;
                case Item.Type.Coll:
                    items = gameData.collectablesData;
                    isCollGroup = true;
                    break;
                default: // Item.Type.Chest
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
                            showAll ||
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

        void GetParent(Item item)
        {
            infoParent.style.display = DisplayStyle.None;

            if (item.parents.Length > 0)
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

        void GetChestItems(Item item)
        {
            List<Sprite> itemSprites = new();

            containItems.style.display = DisplayStyle.Flex;

            if (item.chestGroup == Item.ChestGroup.Item)
            {
                Debug.Log("ITEM");
                Debug.Log(item.creates.Length);
                for (int i = 0; i < item.creates.Length; i++)
                {
                    Debug.Log("//////////");
                    Debug.Log(item.creates[i].sprite);
                    itemSprites.Add(item.creates[i].sprite);
                }
            }
            else if (item.chestGroup == Item.ChestGroup.Piggy)
            {
                for (int i = 0; i < gameData.collectablesData.Length; i++)
                {
                    if (
                        gameData.collectablesData[i].collGroup == Item.CollGroup.Gold
                        || gameData.collectablesData[i].collGroup == Item.CollGroup.Gems
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
                    if (gameData.collectablesData[i].collGroup == Item.CollGroup.Energy)
                    {
                        for (int j = 0; j < gameData.collectablesData[i].content.Length; j++)
                        {
                            itemSprites.Add(gameData.collectablesData[i].content[j].sprite);
                        }
                    }
                }
            }

            Label value = new() { name = "Value", text = LOCALE.Get("info_menu_may_contain") };

            value.AddToClassList("value");

            containItems.Add(value);

            // Show the items
            for (int i = 0; i < itemSprites.Count; i++)
            {
                VisualElement current = new() { name = "Current" };
                VisualElement unlockedItem = new() { name = "UnlockedItem" + i };
                VisualElement line = new() { name = "Line" };

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

                containItems.Add(current);
            }
        }

        Sprite GetParentSprite(BoardManager.ParentData parentData)
        {
            Sprite sprite = null;

            if (parentData.type == Item.Type.Gen)
            {
                for (int i = 0; i < gameData.generatorsData.Length; i++)
                {
                    if (gameData.generatorsData[i].genGroup == parentData.genGroup)
                    {
                        if (gameData.generatorsData[i].content.Length == 1)
                        {
                            sprite = gameData.generatorsData[i].content[0].sprite;
                        }
                        else
                        {
                            for (int j = gameData.generatorsData[i].content.Length - 1; j >= 0; j--)
                            {
                                if (gameData.generatorsData[i].content[j].unlocked)
                                {
                                    sprite = gameData.generatorsData[i].content[j].sprite;

                                    break;
                                }
                            }
                        }

                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < gameData.chestsData.Length; i++)
                {
                    if (gameData.chestsData[i].chestGroup == parentData.chestGroup)
                    {
                        if (gameData.chestsData[i].content.Length == 1)
                        {
                            sprite = gameData.chestsData[i].content[0].sprite;
                        }
                        else
                        {
                            for (int j = gameData.chestsData[i].content.Length - 1; j >= 0; j--)
                            {
                                if (gameData.chestsData[i].content[j].unlocked)
                                {
                                    sprite = gameData.chestsData[i].content[j].sprite;

                                    break;
                                }
                            }
                        }

                        break;
                    }
                }
            }

            if (sprite == null)
            {
                sprite = unlockedQuestionMarkSprite;
            }

            return sprite;
        }

        void ClearData()
        {
            if (unlockedItems.childCount > 0)
            {
                unlockedItems.Clear();
            }

            if (containItems.childCount > 0)
            {
                containItems.Clear();
            }

            if (infoParent.childCount > 0)
            {
                infoParent.Clear();
            }

            containItems.style.display = DisplayStyle.None;
            infoParent.style.display = DisplayStyle.None;
        }
    }
}
