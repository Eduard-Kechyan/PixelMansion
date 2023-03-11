using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Types : MonoBehaviour
{
    //// ITEMS ////
    [Serializable]
    public class ItemsJson
    {
        public string group;
        public string type;
        public bool hasLevel;
        public string itemName;
        public string[] parents;
        public string[] generates;
        public ItemsDataJson[] content;
    }

    [Serializable]
    public class ItemsDataJson
    {
        public string sprite;
        public string itemName;
        public int level;
        public string group;
        public string type;
        public string[] parents;
        public string[] generates;
        public bool unlocked;
        public bool isMaxLavel;
        public bool hasLevel;
    }

    [Serializable]
    public class Items
    {
        public Item.Group group;
        public Item.Type type;
        public bool hasLevel;
        public string itemName;
        public Item.Group[] parents;
        public Generates[] generates;
        public ItemsData[] content;
    }

    [Serializable]
    public class ItemsData
    {
        public Sprite sprite;
        public string itemName;
        public int level;

        [HideInInspector]
        public Item.Group group;

        [HideInInspector]
        public Item.Type type;

        [HideInInspector]
        public Item.Group[] parents;

        [HideInInspector]
        public Generates[] generates;

        [HideInInspector]
        public bool unlocked;

        [HideInInspector]
        public bool isMaxLavel;

        [HideInInspector]
        public bool hasLevel;
    }

    [Serializable]
    public class Generates
    {
        public Item.Group group;
        public float chance;
    }

    //// BOARD ////
    [Serializable]
    public class BoardJson
    {
        public string sprite;
        public string group;
        public string state;
        public int crate;
    }

    [Serializable]
    public class Board
    {
        public Sprite sprite;
        public Item.Group group;
        public Item.State state;
        public int crate;
    }
}
