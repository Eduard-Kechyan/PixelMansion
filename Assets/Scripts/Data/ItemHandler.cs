using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class ItemHandler : MonoBehaviour
    {
        // Variables
        public GameObject item;
        public Sprite[] crateSprites;

        // References
        private GameData gameData;
        private I18n LOCALE;

        void Start()
        {
            // Cache
            gameData = GameData.Instance;
            LOCALE = I18n.Instance;
        }

        public Item CreateItem(
            GameObject tile,
            float tileSize,
            BoardManager.Tile tileItem,
            string spriteName = ""
        )
        {
            BoardManager.ItemData itemData = FindItem(tileItem, null, spriteName);

            // Instantiate item
            GameObject newItemPre = Instantiate(item, tile.transform.position, Quaternion.identity);
            Item newItem = newItemPre.GetComponent<Item>();

            newItem.id = tileItem.id;
            newItem.sprite = itemData.sprite;
            newItem.itemName = itemData.itemName;
            newItem.level = itemData.level;
            newItem.state = tileItem.state;
            newItem.isCompleted = tileItem.isCompleted;
            newItem.type = itemData.type;
            newItem.hasLevel = itemData.hasLevel;
            newItem.parents = itemData.parents;
            newItem.creates = itemData.creates;
            newItem.coolDown = itemData.coolDown;
            newItem.isMaxLevel = itemData.isMaxLevel;
            newItem.group = itemData.group;
            newItem.genGroup = itemData.genGroup;
            newItem.collGroup = itemData.collGroup;
            newItem.chestGroup = itemData.chestGroup;
            newItem.timerOn = tileItem.timerOn;
            newItem.generatesAtLevel = itemData.generatesAtLevel;
            newItem.chestOpen = tileItem.chestOpen;
            newItem.chestItems = itemData.chestItems;
            newItem.chestItemsSet = itemData.chestItemsSet;
            newItem.crate = tileItem.crate;
            newItem.gemPopped = itemData.gemPopped;

            if (tileItem.state == Item.State.Crate)
            {
                newItem.SetCrateSprite(crateSprites[tileItem.crate]);
            }

            if (!itemData.isMaxLevel)
            {
                newItem.nextName = GetNextItem(tileItem);
            }

            // Set item scale
            newItem.transform.localScale = new Vector3(
                tileSize,
                tileSize,
                newItem.transform.localScale.y
            );

            // Set item position
            newItem.transform.parent = tile.transform;

            return newItem;
        }

        public Item CreateItemTemp(ShopMenu.ShopItemsContent shopItem)
        {
            BoardManager.ItemData itemData = FindItem(null, shopItem);

            // Instantiate item
            GameObject newItemPre = Instantiate(item, Vector3.zero, Quaternion.identity);

            newItemPre.transform.localScale = Vector3.zero;

            Item newItem = newItemPre.GetComponent<Item>();

            newItem.sprite = itemData.sprite;
            newItem.itemName = itemData.itemName;
            newItem.level = itemData.level;
            newItem.state = Item.State.Default;
            newItem.type = shopItem.type;
            newItem.hasLevel = itemData.hasLevel;
            newItem.parents = itemData.parents;
            newItem.isMaxLevel = itemData.isMaxLevel;
            newItem.group = shopItem.group;
            newItem.genGroup = shopItem.genGroup;
            newItem.chestGroup = shopItem.chestGroup;

            Destroy(newItemPre);

            return newItem;
        }

        // Find the given item for the scriptable object
        public BoardManager.ItemData FindItem(
            BoardManager.Tile tileItem = null,
            ShopMenu.ShopItemsContent shopItem = null,
            string newSpriteName = ""
        )
        {
            BoardManager.ItemData foundItem = new();

            string spriteName;
            Item.Type type;
            Item.Group group;
            Item.GenGroup genGroup;
            Item.CollGroup collGroup = Item.CollGroup.Experience;
            Item.ChestGroup chestGroup;

            if (tileItem == null)
            {
                spriteName = shopItem.sprite.name;
                type = shopItem.type;
                group = shopItem.group;
                genGroup = shopItem.genGroup;
                chestGroup = shopItem.chestGroup;
            }
            else
            {
                spriteName = tileItem.sprite.name;
                type = tileItem.type;
                group = tileItem.group;
                genGroup = tileItem.genGroup;
                collGroup = tileItem.collGroup;
                chestGroup = tileItem.chestGroup;
            }

            if (newSpriteName != "")
            {
                spriteName = newSpriteName;
            }

            switch (type)
            {
                case Item.Type.Item:
                    BoardManager.TypeItem[] itemsData = gameData.itemsData;

                    for (int i = 0; i < itemsData.Length; i++)
                    {
                        if (group == itemsData[i].group)
                        {
                            for (int j = 0; j < itemsData[i].content.Length; j++)
                            {
                                if (spriteName == itemsData[i].content[j].sprite.name)
                                {
                                    foundItem = itemsData[i].content[j];

                                    break;
                                }
                            }

                            break;
                        }
                    }
                    break;

                case Item.Type.Gen:
                    BoardManager.TypeItem[] generatorsData = gameData.generatorsData;

                    for (int i = 0; i < generatorsData.Length; i++)
                    {
                        if (genGroup == generatorsData[i].genGroup)
                        {
                            for (int j = 0; j < generatorsData[i].content.Length; j++)
                            {
                                if (spriteName == generatorsData[i].content[j].sprite.name)
                                {
                                    foundItem = generatorsData[i].content[j];

                                    break;
                                }
                            }

                            break;
                        }
                    }
                    break;

                case Item.Type.Coll:
                    BoardManager.TypeItem[] collectablesData = gameData.collectablesData;

                    for (int i = 0; i < collectablesData.Length; i++)
                    {
                        if (collGroup == collectablesData[i].collGroup)
                        {
                            for (int j = 0; j < collectablesData[i].content.Length; j++)
                            {
                                if (spriteName == collectablesData[i].content[j].sprite.name)
                                {
                                    foundItem = collectablesData[i].content[j];

                                    break;
                                }
                            }

                            break;
                        }
                    }
                    break;

                case Item.Type.Chest:
                    BoardManager.TypeItem[] chestData = gameData.chestsData;

                    for (int i = 0; i < chestData.Length; i++)
                    {
                        if (chestGroup == chestData[i].chestGroup)
                        {
                            for (int j = 0; j < chestData[i].content.Length; j++)
                            {
                                if (spriteName == chestData[i].content[j].sprite.name)
                                {
                                    foundItem = chestData[i].content[j];

                                    break;
                                }
                            }

                            break;
                        }
                    }
                    break;

                default:
                    // ERROR
                    ErrorManager.Instance.Throw(ErrorManager.ErrorType.Code, "ItemHandler.cs -> FindItem()", "Wrong type: " + type);
                    break;
            }

            return foundItem;
        }

        // Get the next item's name
        string GetNextItem(BoardManager.Tile tileItem)
        {
            string nextName = "";

            switch (tileItem.type)
            {
                case Item.Type.Item:
                    BoardManager.TypeItem[] itemsData = gameData.itemsData;

                    for (int i = 0; i < itemsData.Length; i++)
                    {
                        if (tileItem.group == itemsData[i].group)
                        {
                            for (int j = 0; j < itemsData[i].content.Length; j++)
                            {
                                if (
                                    tileItem.sprite.name == itemsData[i].content[j].sprite.name
                                    && itemsData[i].content[j + 1] != null
                                )
                                {
                                    nextName = itemsData[i].content[j + 1].itemName;

                                    break;
                                }
                            }

                            break;
                        }
                    }
                    break;

                case Item.Type.Gen:
                    BoardManager.TypeItem[] generatorsData = gameData.generatorsData;

                    for (int i = 0; i < generatorsData.Length; i++)
                    {
                        if (tileItem.genGroup == generatorsData[i].genGroup)
                        {
                            for (int j = 0; j < generatorsData[i].content.Length; j++)
                            {
                                if (
                                    tileItem.sprite.name == generatorsData[i].content[j].sprite.name
                                    && generatorsData[i].content[j + 1] != null
                                )
                                {
                                    nextName = generatorsData[i].content[j + 1].itemName;

                                    break;
                                }
                            }

                            break;
                        }
                    }
                    break;

                case Item.Type.Coll:
                    BoardManager.TypeItem[] collectablesData = gameData.collectablesData;

                    for (int i = 0; i < collectablesData.Length; i++)
                    {
                        if (tileItem.collGroup == collectablesData[i].collGroup)
                        {
                            for (int j = 0; j < collectablesData[i].content.Length; j++)
                            {
                                if (
                                    tileItem.sprite.name == collectablesData[i].content[j].sprite.name
                                    && collectablesData[i].content[j + 1] != null
                                )
                                {
                                    nextName = collectablesData[i].content[j + 1].itemName;

                                    break;
                                }
                            }

                            break;
                        }
                    }
                    break;

                case Item.Type.Chest:
                    BoardManager.TypeItem[] chestsData = gameData.chestsData;

                    for (int i = 0; i < chestsData.Length; i++)
                    {
                        if (tileItem.chestGroup == chestsData[i].chestGroup)
                        {
                            for (int j = 0; j < chestsData[i].content.Length; j++)
                            {
                                if (
                                    tileItem.sprite.name == chestsData[i].content[j].sprite.name
                                    && chestsData[i].content[j + 1] != null
                                )
                                {
                                    nextName = chestsData[i].content[j + 1].itemName;

                                    break;
                                }
                            }

                            break;
                        }
                    }
                    break;

                default:
                    // ERROR
                    ErrorManager.Instance.Throw(ErrorManager.ErrorType.Code, "ItemHandler.cs -> GetNextItem()", "Wrong type: " + tileItem.type);
                    break;
            }

            return nextName;
        }

        public string GetItemData(Item newItem, bool alt = false)
        {
            string textToSet;

            switch (newItem.state)
            {
                case Item.State.Crate:
                    textToSet = LOCALE.Get("info_box_crate_text");
                    break;
                case Item.State.Locker:

                    textToSet = LOCALE.Get("info_box_locker_text");
                    break;
                case Item.State.Bubble:

                    textToSet = LOCALE.Get("info_box_bubble_text");
                    break;
                default:
                    switch (newItem.type)
                    {
                        case Item.Type.Gen:
                            if (newItem.isMaxLevel)
                            {
                                textToSet = LOCALE.Get("info_box_gen_max");
                            }
                            else
                            {
                                if (newItem.level >= newItem.generatesAtLevel)
                                {
                                    textToSet = LOCALE.Get("info_box_gen", newItem.nextName);
                                }
                                else
                                {

                                    textToSet = LOCALE.Get("info_box_gen_pre", newItem.nextName);
                                }
                            }
                            break;
                        case Item.Type.Coll:
                            int multipliedValue = GetMultipliedValue(newItem.level, newItem.collGroup);

                            if (newItem.collGroup == Item.CollGroup.Gems)
                            {
                                if (newItem.isMaxLevel)
                                {
                                    textToSet = LOCALE.Get(
                                        "info_box_gems_max",
                                        multipliedValue,
                                        LOCALE.Get("Coll_Gems", newItem.level)
                                    );
                                }
                                else
                                {
                                    textToSet = LOCALE.Get(
                                        "info_box_gems",
                                        multipliedValue,
                                        LOCALE.Get("Coll_" + newItem.collGroup.ToString())
                                    );
                                }
                            }
                            else
                            {
                                if (newItem.isMaxLevel)
                                {
                                    textToSet = LOCALE.Get(
                                        "info_box_coll_max",
                                        multipliedValue,
                                        LOCALE.Get("Coll_" + newItem.collGroup.ToString())
                                    );
                                }
                                else
                                {
                                    textToSet = LOCALE.Get(
                                        "info_box_coll",
                                        multipliedValue,
                                        LOCALE.Get("Coll_" + newItem.collGroup.ToString())
                                    );
                                }
                            }
                            break;
                        case Item.Type.Chest:
                            if (alt)
                            {
                                if (!newItem.chestOpen)
                                {
                                    if (newItem.isMaxLevel)
                                    {
                                        textToSet = LOCALE.Get("info_box_chest_max_locked");
                                    }
                                    else
                                    {
                                        textToSet = LOCALE.Get("info_box_chest_locked", newItem.nextName);
                                    }
                                }
                                else
                                {
                                    textToSet = LOCALE.Get("info_box_chest_alt_" + newItem.chestGroup); // Note the +
                                }
                            }
                            else
                            {
                                if (newItem.timerOn)
                                {
                                    if (newItem.isMaxLevel)
                                    {
                                        textToSet = LOCALE.Get("info_box_chest_max_timer");
                                    }
                                    else
                                    {
                                        textToSet = LOCALE.Get("info_box_chest_timer", newItem.nextName);
                                    }
                                }
                                else if (!newItem.chestOpen)
                                {
                                    if (newItem.isMaxLevel)
                                    {
                                        textToSet = LOCALE.Get("info_box_chest_max_locked");
                                    }
                                    else
                                    {
                                        textToSet = LOCALE.Get("info_box_chest_locked", newItem.nextName);
                                    }
                                }
                                else
                                {
                                    if (!newItem.hasLevel)
                                    {
                                        textToSet = LOCALE.Get("info_box_chest_single_" + newItem.chestGroup); // Note the +
                                    }
                                    else if (newItem.isMaxLevel)
                                    {
                                        textToSet = LOCALE.Get("info_box_chest_max_" + newItem.chestGroup); // Note the +
                                    }
                                    else
                                    {
                                        textToSet = LOCALE.Get("info_box_chest_" + newItem.chestGroup, newItem.chestGroup.ToString(), newItem.nextName); // Note the +
                                    }
                                }
                            }

                            break;
                        default:
                            if (newItem.isMaxLevel)
                            {
                                textToSet = LOCALE.Get("info_box_item_max");
                            }
                            else
                            {
                                textToSet = LOCALE.Get("info_box_item", newItem.nextName);
                            }
                            break;
                    }
                    break;
            }

            return textToSet;
        }

        // Show the collectables multiplied value
        int GetMultipliedValue(int level, Item.CollGroup collGroup)
        {
            int multipliedValue = 0;

            switch (collGroup)
            {
                case Item.CollGroup.Experience:
                    multipliedValue = gameData.valuesData.experienceMultiplier[level - 1];
                    break;
                case Item.CollGroup.Gold:
                    multipliedValue = gameData.valuesData.goldMultiplier[level - 1];
                    break;
                case Item.CollGroup.Gems:
                    multipliedValue = gameData.valuesData.gemsMultiplier[level - 1];
                    break;
                case Item.CollGroup.Energy:
                    multipliedValue = gameData.valuesData.energyMultiplier[level - 1];
                    break;
            }

            return multipliedValue;
        }
    }
}