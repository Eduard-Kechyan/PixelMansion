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
            Types.Board boardItem,
            string spriteName = ""
        )
        {
            Types.ItemData itemData = FindItem(boardItem, null, spriteName);

            // Instantiate item
            GameObject newItemPre = Instantiate(item, tile.transform.position, Quaternion.identity);
            Item newItem = newItemPre.GetComponent<Item>();

            string guid = Guid.NewGuid().ToString();

            AddIdToBoardData(guid, boardItem);

            newItem.id = guid;
            newItem.sprite = itemData.sprite;
            newItem.itemName = itemData.itemName;
            newItem.level = itemData.level;
            newItem.state = boardItem.state;
            newItem.isCompleted = boardItem.isCompleted;
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
            newItem.timerOn = boardItem.timerOn;
            newItem.timerStartTime = boardItem.timerStartTime;
            newItem.timerSeconds = boardItem.timerSeconds;
            newItem.generatesAt = itemData.generatesAt;
            newItem.chestItems = itemData.chestItems;
            newItem.chestItemsSet = itemData.chestItemsSet;
            newItem.crate = boardItem.crate;
            newItem.gemPopped = itemData.gemPopped;

            if (boardItem.state == Types.State.Crate)
            {
                newItem.SetCrateSprite(crateSprites[boardItem.crate]);
            }

            if (!itemData.isMaxLevel)
            {
                newItem.nextName = GetNextItem(boardItem);
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

        void AddIdToBoardData(string id, Types.Board boardItem)
        {
            for (int x = 0; x < gameData.boardData.GetLength(0); x++)
            {
                for (int y = 0; y < gameData.boardData.GetLength(1); y++)
                {
                    if (gameData.boardData[x, y].order == boardItem.order)
                    {
                        gameData.boardData[x, y].id = id;
                    }
                }
            }
        }

        public Item CreateItemTemp(Types.ShopItemsContent shopItem)
        {
            Types.ItemData itemData = FindItem(null, shopItem);

            // Instantiate item
            GameObject newItemPre = Instantiate(item, Vector3.zero, Quaternion.identity);

            newItemPre.transform.localScale = Vector3.zero;

            Item newItem = newItemPre.GetComponent<Item>();

            newItem.sprite = itemData.sprite;
            newItem.itemName = itemData.itemName;
            newItem.level = itemData.level;
            newItem.state = Types.State.Default;
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
        public Types.ItemData FindItem(
            Types.Board boardItem = null,
            Types.ShopItemsContent shopItem = null,
            string newSpriteName = ""
        )
        {
            Types.ItemData foundItem = new();

            string spriteName;
            Types.Type type;
            ItemTypes.Group group;
            ItemTypes.GenGroup genGroup;
            Types.CollGroup collGroup = Types.CollGroup.Experience;
            Types.ChestGroup chestGroup;

            if (boardItem == null)
            {
                spriteName = shopItem.sprite.name;
                type = shopItem.type;
                group = shopItem.group;
                genGroup = shopItem.genGroup;
                chestGroup = shopItem.chestGroup;
            }
            else
            {
                spriteName = boardItem.sprite.name;
                type = boardItem.type;
                group = boardItem.group;
                genGroup = boardItem.genGroup;
                collGroup = boardItem.collGroup;
                chestGroup = boardItem.chestGroup;
            }

            if (newSpriteName != "")
            {
                spriteName = newSpriteName;
            }

            switch (type)
            {
                case Types.Type.Item:
                    Types.Item[] itemsData = gameData.itemsData;

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

                case Types.Type.Gen:
                    Types.Item[] generatorsData = gameData.generatorsData;

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

                case Types.Type.Coll:
                    Types.Item[] collectablesData = gameData.collectablesData;

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

                case Types.Type.Chest:
                    Types.Item[] chestData = gameData.chestsData;

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
                    ErrorManager.Instance.Throw(Types.ErrorType.Code, "Wrong type: " + type);
                    break;
            }

            return foundItem;
        }

        // Get the next item's name
        string GetNextItem(Types.Board boardItem)
        {
            string nextName = "";

            switch (boardItem.type)
            {
                case Types.Type.Item:
                    Types.Item[] itemsData = gameData.itemsData;

                    for (int i = 0; i < itemsData.Length; i++)
                    {
                        if (boardItem.group == itemsData[i].group)
                        {
                            for (int j = 0; j < itemsData[i].content.Length; j++)
                            {
                                if (
                                    boardItem.sprite.name == itemsData[i].content[j].sprite.name
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

                case Types.Type.Gen:
                    Types.Item[] generatorsData = gameData.generatorsData;

                    for (int i = 0; i < generatorsData.Length; i++)
                    {
                        if (boardItem.genGroup == generatorsData[i].genGroup)
                        {
                            for (int j = 0; j < generatorsData[i].content.Length; j++)
                            {
                                if (
                                    boardItem.sprite.name == generatorsData[i].content[j].sprite.name
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

                case Types.Type.Coll:
                    Types.Item[] collectablesData = gameData.collectablesData;

                    for (int i = 0; i < collectablesData.Length; i++)
                    {
                        if (boardItem.collGroup == collectablesData[i].collGroup)
                        {
                            for (int j = 0; j < collectablesData[i].content.Length; j++)
                            {
                                if (
                                    boardItem.sprite.name == collectablesData[i].content[j].sprite.name
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

                case Types.Type.Chest:
                    Types.Item[] chestsData = gameData.chestsData;

                    for (int i = 0; i < chestsData.Length; i++)
                    {
                        if (boardItem.chestGroup == chestsData[i].chestGroup)
                        {
                            for (int j = 0; j < chestsData[i].content.Length; j++)
                            {
                                if (
                                    boardItem.sprite.name == chestsData[i].content[j].sprite.name
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
                    ErrorManager.Instance.Throw(Types.ErrorType.Code, "Wrong type: " + boardItem.type);
                    break;
            }

            return nextName;
        }
    }
}