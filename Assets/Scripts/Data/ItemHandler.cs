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
        Item.Group group,
        string spriteName,
        Item.State state = Item.State.Default,
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
        newItem.type = itemData.type;
        newItem.hasLevel = itemData.hasLevel;
        newItem.parents = itemData.parents;
        newItem.generates = itemData.generates;
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

    // Find the item by group and anme
    Types.ItemsData FindItem(Item.Group group, string spriteName)
    {
        //Debug.Log(group);
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

    // Get next item's name
    string GetNextItem(Item.Group group, string spriteName)
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
}
