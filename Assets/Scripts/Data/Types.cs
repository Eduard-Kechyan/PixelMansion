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

    public enum CollGroup
    {
        Experience,
        Gold,
        Gems
    };

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
        Item,
        Gen,
        Coll
    };

    //// TIMERS ///
    public enum TimerType
    {
        Item,
        Energy
    };

    [Serializable]
    public class Timer
    {
        public DateTime dateTime;
        public TimerType type;
        public string timerName;
    }

    [Serializable]
    public class TimerJson
    {
        public string dateTime;
        public string type;
        public string timerName;
    }

    [Serializable]
    public class Creates
    {
        public Group group;
        public float chance;
    }

    //// ITEMS DATA ////

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

        [HideInInspector]
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
    public class Generators
    {
        public GenGroup genGroup;
        public bool hasLevel;
        public string itemName;

        [ReadOnly]
        public int createsTotal;
        public Creates[] creates;

        public GeneratorsData[] content;
    }

    [Serializable]
    public class GeneratorsData
    {
        public Sprite sprite;
        public string itemName;

        [HideInInspector]
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

    //// COLLECTABLES ////
    [Serializable]
    public class Collectables
    {
        public CollGroup collGroup;
        public CollectablesData[] content;
    }

    [Serializable]
    public class CollectablesData
    {
        public Sprite sprite;

        [HideInInspector]
        public string itemName;

        [HideInInspector]
        public int level;

        [HideInInspector]
        public CollGroup collGroup;

        [HideInInspector]
        public bool unlocked;

        [HideInInspector]
        public bool isMaxLavel;

        [HideInInspector]
        public bool hasLevel;
    }

    //// BOARD ////
    [Serializable]
    public class BoardEmpty
    {
        public int order;
        public Vector2Int loc;
        public float distance;
    }

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
        public CollGroup collGroup;
        public State state;
        public int crate;
        public int order;
    }

    //// SHOP ////
    public enum ShopValuesType
    {
        Gems,
        Gold
    };

    [Serializable]
    public class ShopItemsContent
    {
        public int left;
        public int price;
        public Group group;
        public Type type;
        public ShopValuesType priceType;
        public Sprite sprite;
    }

    [Serializable]
    public class ShopValuesContent
    {
        public int amount;
        public int bonusAmount;
        public float price;
        public ShopValuesType type;
        public Sprite sprite;
        public bool hasBonus;
        public bool isPopular;
    }
}
