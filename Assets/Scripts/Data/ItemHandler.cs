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

        // Instances
        private GameData gameData;

        void Start()
        {
            // Cache instances
            gameData = GameData.Instance;
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
    }
}