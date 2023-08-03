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
        Locker,
        Bubble
    };

    public enum Type
    {
        Item,
        Gen,
        Coll
    };

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
        public Type type;
        public Group group;
        public GenGroup genGroup;
        public CollGroup collGroup;
        public bool hasLevel;
        public bool customName;
        public GenGroup[] parents;

        [ReadOnly]
        public int createsTotal;
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
        public Group group;

        [HideInInspector]
        public Type type;

        [HideInInspector]
        public GenGroup genGroup;

        [HideInInspector]
        public CollGroup collGroup;

        [HideInInspector]
        public GenGroup[] parents;

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
        public string collGroup;
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

    //// BONUS ////
    public class BonusJson
    {
        public string sprite;
        public string type;
        public string group;
        public string genGroup;
    }

    public class Bonus
    {
        public Sprite sprite;
        public Type type;
        public Group group;
        public GenGroup genGroup;
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
        public Group group;
        public GenGroup genGroup;
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
        public Group group;
        public GenGroup genGroup;
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
