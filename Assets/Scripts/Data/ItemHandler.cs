using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHandler : MonoBehaviour
{
    public GameObject item;

    public Sprite[] crateSprites;

    private GameData gameData;

    void Start()
    {
        gameData = GameData.Instance;
    }

    // Create a new item on the board
    public Item CreateItem(
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
        GameObject newItemPre = Instantiate(item, tile.transform.position, Quaternion.identity);
        Item newItem = newItemPre.GetComponent<Item>();

        newItem.sprite = itemData.sprite;
        newItem.itemName = itemData.itemName;
        newItem.level = itemData.level;
        newItem.state = state; // From the board
        newItem.type = Types.Type.Item;
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
        GameObject newItemPre = Instantiate(item, tile.transform.position, Quaternion.identity);
        Item newItem = newItemPre.GetComponent<Item>();

        newItem.sprite = generatorData.sprite;
        newItem.itemName = generatorData.itemName;
        newItem.level = generatorData.level;
        newItem.state = state; // From the board
        newItem.type = Types.Type.Gen;
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

    public Item CreateCollection(
        GameObject tile,
        float tileSize,
        Types.CollGroup collGroup,
        string spriteName,
        Types.State state = Types.State.Default,
        int crate = 0
    )
    {
        Types.CollectablesData collectableData = FindCollectable(collGroup, spriteName);

        // Instantiate item
        GameObject newItemPre = Instantiate(item, tile.transform.position, Quaternion.identity);
        Item newItem = newItemPre.GetComponent<Item>();

        newItem.sprite = collectableData.sprite;
        newItem.itemName = collectableData.itemName;
        newItem.level = collectableData.level;
        newItem.state = state; // From the board
        newItem.type = Types.Type.Coll;
        newItem.hasLevel = collectableData.hasLevel;
        newItem.isMaxLavel = collectableData.isMaxLavel;
        newItem.collGroup = collGroup; // From the board
        newItem.crateSprite = crateSprites[crate]; // From the board

        if (!collectableData.isMaxLavel)
        {
            newItem.nextName = GetNextCollectable(collGroup, spriteName);
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

    public Item CreateItemTemp(Types.Group group, Types.Type type,string spriteName){
        Types.ItemsData itemData = FindItem(group, spriteName);

        // Instantiate item
        GameObject newItemPre = Instantiate(item, Vector3.zero, Quaternion.identity);

        newItemPre.transform.localScale = Vector3.zero;

        Item newItem = newItemPre.GetComponent<Item>();

        newItem.sprite = itemData.sprite;
        newItem.itemName = itemData.itemName;
        newItem.level = itemData.level;
        newItem.state = Types.State.Default;
        newItem.type = type;
        newItem.hasLevel = itemData.hasLevel;
        newItem.parents = itemData.parents;
        newItem.isMaxLavel = itemData.isMaxLavel;
        newItem.group = group;

        Destroy(newItemPre);

        return newItem;
    }

    //////// FIND ////////
    public Types.ItemsData FindItem(Types.Group group, string spriteName)
    {
        Types.ItemsData foundItem = new Types.ItemsData();

        Types.Items[] data = gameData.itemsData;

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

        Types.Generators[] data = gameData.generatorsData;

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

    Types.CollectablesData FindCollectable(Types.CollGroup collGroup, string spriteName)
    {
        Types.CollectablesData foundItem = new Types.CollectablesData();

        Types.Collectables[] data = gameData.collectablesData;

        for (int i = 0; i < data.Length; i++)
        {
            if (collGroup == data[i].collGroup)
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

        Types.Items[] data = gameData.itemsData;

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

    string GetNextGenerator(Types.GenGroup genGroup, string spriteName)
    {
        string nextName = "";

        Types.Generators[] data = gameData.generatorsData;

        for (int i = 0; i < data.Length; i++)
        {
            if (genGroup == data[i].genGroup)
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

    string GetNextCollectable(Types.CollGroup collGroup, string spriteName)
    {
        string nextName = "";

        Types.Collectables[] data = gameData.collectablesData;

        for (int i = 0; i < data.Length; i++)
        {
            if (collGroup == data[i].collGroup)
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
