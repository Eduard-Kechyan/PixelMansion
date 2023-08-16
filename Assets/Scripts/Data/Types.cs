using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class Types : MonoBehaviour
{
    //// ITEMS ///
    public enum ChestGroup
    {
        Item,
        Piggy,
        Energy
    }

    public enum CollGroup
    {
        Experience,
        Gold,
        Gems,
        Energy
    };

    public enum State
    {
        Default,
        Crate,
        Locker,
        Bubble
    };

    public enum Type
    {
        Item,
        Gen,
        Coll,
        Chest
    };

    [Serializable]
    public class Creates
    {
        public ItemTypes.Group group;
        public float chance;
    }

    //// OTHER ////
    public enum Locale
    {
        English,
        French,
        Spanish,
        German,
        Italian,
        Russian,
        Armenian,
        Japanese,
        Korean,
        Chinese
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
        public DateTime startDate;
        public int seconds;
        public bool on;
        public TimerType type;
        public string timerName;
    }

    [Serializable]
    public class TimerJson
    {
        public string startDate;
        public int seconds;
        public bool on;
        public string type;
        public string timerName;
    }

    //// ITEMS DATA ////

    [Serializable]
    public class Items
    {
        public Type type;
        public ItemTypes.Group group;
        public ItemTypes.GenGroup genGroup;
        public CollGroup collGroup;
        public ChestGroup chestGroup;
        public bool hasLevel;
        public bool customName;
        public bool hasTimer;
        public int generatesAt;
        public ItemTypes.GenGroup[] parents;

        public Creates[] creates;

        public ItemsData[] content;
    }

    [Serializable]
    public class ItemsData
    {
        public Sprite sprite;
        public bool customName;
        public string itemName;

        [HideInInspector]
        public int level;

        [HideInInspector]
        public ItemTypes.Group group;

        [HideInInspector]
        public Type type;

        [HideInInspector]
        public ChestGroup chestGroup;

        [HideInInspector]
        public int generatesAt;

        [HideInInspector]
        public ItemTypes.GenGroup genGroup;

        [HideInInspector]
        public CollGroup collGroup;

        [HideInInspector]
        public ItemTypes.GenGroup[] parents;

        [HideInInspector]
        public Creates[] creates;

        [HideInInspector]
        public bool unlocked;

        [HideInInspector]
        public bool isMaxLavel;

        [HideInInspector]
        public bool hasLevel;

        [HideInInspector]
        public bool hasTimer;

        [HideInInspector]
        public int chestItems;

        [HideInInspector]
        public bool chestItemsSet;

        [HideInInspector]
        public DateTime startTime;

        [HideInInspector]
        public int seconds;

        [HideInInspector]
        public bool gemPoped;
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
    public class Board
    {
        public Sprite sprite;
        public Type type;
        public State state;
        public ItemTypes.Group group;
        public ItemTypes.GenGroup genGroup;
        public CollGroup collGroup;
        public ChestGroup chestGroup;

        [HideInInspector]
        public int generatesAt;

        [HideInInspector]
        public int crate;

        [HideInInspector]
        public int order;

        [HideInInspector]
        public int chestItems;

        [HideInInspector]
        public bool chestItemsSet;

        [HideInInspector]
        public bool gemPoped;

        /*public bool hasTimer;
        public string startTime;
        public int seconds;*/
    }

    [Serializable]
    public class BoardJson
    {
        public string sprite;
        public string type;
        public string group;
        public string genGroup;
        public string collGroup;
        public string chestGroup;
        public int crate;
        public string state;
        public int chestItems;
        public int generatesAt;
        public bool chestItemsSet;
        public bool gemPoped;
        /*public string startTime;
        public bool hasTimer;
        public int seconds;*/
    }

    //// BONUS ////
    public class BonusJson
    {
        public string sprite;
        public string type;
        public string group;
        public string genGroup;
        public string chestGroup;
    }

    public class Bonus
    {
        public Sprite sprite;
        public Type type;
        public ItemTypes.Group group;
        public ItemTypes.GenGroup genGroup;
        public ChestGroup chestGroup;
    }

    //// INVENTORY ////
    public class InventoryJson
    {
        public string sprite;
        public string type;
        public string group;
        public string genGroup;
    }

    public class Inventory
    {
        public Sprite sprite;
        public Type type;
        public ItemTypes.Group group;
        public ItemTypes.GenGroup genGroup;
    }

    //// ERROR ////
    public enum ErrorType
    {
        Code,
        Gameplay,
        Locale,
        Network
    };

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
        public Type type;
        public ItemTypes.Group group;
        public ItemTypes.GenGroup genGroup;
        public ChestGroup chestGroup;
        public ShopValuesType priceType;
        public Sprite sprite;
    }

    [Serializable]
    public class ShopItemsContentJson
    {
        public int left;
        public int price;
        public string type;
        public string group;
        public string genGroup;
        public string chestGroup;
        public string priceType;
        public string sprite;
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
}