using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Types : MonoBehaviour
{
    //// ITEMS ///
    public enum GenGroup
    {
        MineCart,
    }

    public enum Group
    {
        Metals,
        Crystals,
        Coals,
        Tree
    };

    public enum State
    {
        Default,
        Crate,
        Locker
    };

    public enum Type
    {
        Default,
        Gen
    };

    [Serializable]
    public class Creates
    {
        public Group group;
        public float chance;
    }

    //// ITEMS DATA ////
    [Serializable]
    public class ItemsJson
    {
        public string group;
        public bool hasLevel;
        public string itemName;
        public string[] parents;
        public ItemsDataJson[] content;
    }

    [Serializable]
    public class ItemsDataJson
    {
        public string sprite;
        public string itemName;
        public int level;
        public string group;
        public string[] parents;
        public bool unlocked;
        public bool isMaxLavel;
        public bool hasLevel;
    }

    [Serializable]
    public class Items
    {
        public Group group;
        public bool hasLevel;
        public string itemName;
        public GenGroup[] parents;
        public ItemsData[] content;
    }

    [Serializable]
    public class ItemsData
    {
        public Sprite sprite;
        public string itemName;

        [ReadOnly]
        public int level;

        [HideInInspector]
        public Group group;

        [HideInInspector]
        public GenGroup[] parents;

        [HideInInspector]
        public bool unlocked;

        [HideInInspector]
        public bool isMaxLavel;

        [HideInInspector]
        public bool hasLevel;
    }

    //// GENERATORS DATA ////
    [Serializable]
    public class GeneratorsJson
    {
        public string genGroup;
        public bool hasLevel;
        public string itemName;
        public string[] creates;
        public GeneratorsDataJson[] content;
    }

    [Serializable]
    public class GeneratorsDataJson
    {
        public string sprite;
        public string itemName;
        public int level;
        public string genGroup;
        public string[] creates;
        public bool unlocked;
        public bool isMaxLavel;
        public bool hasLevel;
    }

    [Serializable]
    public class Generators
    {
        public GenGroup genGroup;
        public bool hasLevel;
        public string itemName;
        public Creates[] creates;
        public GeneratorsData[] content;
    }

    [Serializable]
    public class GeneratorsData
    {
        public Sprite sprite;
        public string itemName;

        [ReadOnly]
        public int level;

        [HideInInspector]
        public GenGroup genGroup;

        [HideInInspector]
        public Creates[] creates;

        [HideInInspector]
        public bool unlocked;

        [HideInInspector]
        public bool isMaxLavel;

        [HideInInspector]
        public bool hasLevel;
    }

    //// BOARD ////
    [Serializable]
    public class BoardJson
    {
        public string sprite;
        public string type;
        public string group;
        public string genGroup;
        public string state;
        public int crate;
    }

    [Serializable]
    public class Board
    {
        public Sprite sprite;
        public Type type;
        public Group group;
        public GenGroup genGroup;
        public State state;
        public int crate;
    }
}
