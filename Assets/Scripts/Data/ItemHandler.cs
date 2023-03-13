using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHandler : MonoBehaviour
{
    public GameObject item;

    public Sprite[] crateSprites;

    // Create a new item on the board
    public Item CreateItem(
        Vector3 pos,
        GameObject tile,
        float tileSize,
        Types.Group group,
        string spriteName,
        Types.State state = Types.State.Default,
        int crate = 0
    )
    {
        Types.ItemsData itemData = FindItem(group, spriteName);

        // Instantiate item
        GameObject newItemPre = Instantiate(item, pos, Quaternion.identity);
        Item newItem = newItemPre.GetComponent<Item>();

        newItem.sprite = itemData.sprite;
        newItem.itemName = itemData.itemName;
        newItem.level = itemData.level;
        newItem.state = state; // From the board
        newItem.type = Types.Type.Default;
        newItem.hasLevel = itemData.hasLevel;
        newItem.parents = itemData.parents;
        newItem.isMaxLavel = itemData.isMaxLavel;
        newItem.group = group; // From the board
        newItem.crateSprite = crateSprites[crate]; // From the board

        if (!itemData.isMaxLavel)
        {
            newItem.nextName = GetNextItem(group, spriteName);
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

    public Item CreateGenerator(
        Vector3 pos,
        GameObject tile,
        float tileSize,
        Types.GenGroup genGroup,
        string spriteName,
        Types.State state = Types.State.Default,
        int crate = 0
    )
    {
        Types.GeneratorsData generatorData = FindGenerator(genGroup, spriteName);

        // Instantiate item
        GameObject newItemPre = Instantiate(item, pos, Quaternion.identity);
        Item newItem = newItemPre.GetComponent<Item>();

        newItem.sprite = generatorData.sprite;
        newItem.itemName = generatorData.itemName;
        newItem.level = generatorData.level;
        newItem.state = state; // From the board
        newItem.type = Types.Type.Default;
        newItem.hasLevel = generatorData.hasLevel;
        newItem.creates = generatorData.creates;
        newItem.isMaxLavel = generatorData.isMaxLavel;
        newItem.genGroup = genGroup; // From the board
        newItem.crateSprite = crateSprites[crate]; // From the board

        if (!generatorData.isMaxLavel)
        {
            newItem.nextName = GetNextGenerator(genGroup, spriteName);
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

    //////// FIND ////////
    Types.ItemsData FindItem(Types.Group group, string spriteName)
    {
        Types.ItemsData foundItem = new Types.ItemsData();

        Types.Items[] data = GameData.itemsData;

        for (int i = 0; i < data.Length; i++)
        {
            if (group == data[i].group)
            {
                for (int j = 0; j < data[i].content.Length; j++)
                {
                    if (spriteName == data[i].content[j].sprite.name)
                    {
                        foundItem = data[i].content[j];
                    }
                }
            }
        }

        return foundItem;
    }

    Types.GeneratorsData FindGenerator(Types.GenGroup genGroup, string spriteName)
    {
        Types.GeneratorsData foundItem = new Types.GeneratorsData();

        Types.Generators[] data = GameData.generatorsData;

        for (int i = 0; i < data.Length; i++)
        {
            if (genGroup == data[i].genGroup)
            {
                for (int j = 0; j < data[i].content.Length; j++)
                {
                    if (spriteName == data[i].content[j].sprite.name)
                    {
                        foundItem = data[i].content[j];
                    }
                }
            }
        }

        return foundItem;
    }

    //////// NEXT NAME ////////
    string GetNextItem(Types.Group group, string spriteName)
    {
        string nextName = "";

        Types.Items[] data = GameData.itemsData;

        for (int i = 0; i < data.Length; i++)
        {
            if (group == data[i].group)
            {
                for (int j = 0; j < data[i].content.Length; j++)
                {
                    if (
                        spriteName == data[i].content[j].sprite.name
                        && data[i].content[j + 1] != null
                    )
                    {
                        nextName = data[i].content[j + 1].itemName;
                    }
                }
            }
        }

        return nextName;
    }

    string GetNextGenerator(Types.GenGroup group, string spriteName)
    {
        string nextName = "";

        Types.Generators[] data = GameData.generatorsData;

        for (int i = 0; i < data.Length; i++)
        {
            if (group == data[i].genGroup)
            {
                for (int j = 0; j < data[i].content.Length; j++)
                {
                    if (
                        spriteName == data[i].content[j].sprite.name
                        && data[i].content[j + 1] != null
                    )
                    {
                        nextName = data[i].content[j + 1].itemName;
                    }
                }
            }
        }

        return nextName;
    }
}
