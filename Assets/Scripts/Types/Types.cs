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
            [HideInInspector]
            public string name;
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
            public bool isMaxLevel;

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
            public bool gemPopped;
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
            public bool gemPopped;

            [HideInInspector]
            public bool isCompleted;

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
            public bool gemPopped;
            public bool isCompleted;
            /*public string startTime;
            public bool hasTimer;
            public int seconds;*/
        }

        //// BONUS ////
        public class Bonus
        {
            public Sprite sprite;
            public Type type;
            public ItemTypes.Group group;
            public ItemTypes.GenGroup genGroup;
            public ChestGroup chestGroup;
        }

        public class BonusJson
        {
            public string sprite;
            public string type;
            public string group;
            public string genGroup;
            public string chestGroup;
        }

        //// INVENTORY ////
        public class Inventory
        {
            public Sprite sprite;
            public Type type;
            public ItemTypes.Group group;
            public ItemTypes.GenGroup genGroup;
            public ChestGroup chestGroup;
            public bool isCompleted;
        }

        public class InventoryJson
        {
            public string sprite;
            public string type;
            public string group;
            public string genGroup;
            public string chestGroup;
            public bool isCompleted;
        }

        //// PROGRESS ////
        public enum StepType
        {
            Task,
            Conversation,
            RoomUnlocking
        };

        [Serializable]
        public class Area
        {
            [HideInInspector]
            public string name;
            public string id;
            public Step[] steps;
        }

        [Serializable]
        public class Step
        {
            [HideInInspector]
            public string name;
            public string id;
            public StepType stepType;
            public string[] nextIds;
            public string[] requiredIds;
        }

        [Serializable]
        public class NextStep
        {
            [HideInInspector]
            public string name;
            public string id;
            public bool isArea;
        }

        //// TASKS ////
        public enum TaskRefType
        {
            Area,
            Floor,
            Wall,
            Furniture,
            Item,
        };

        [Serializable]
        public class TaskGroup
        {
            [HideInInspector]
            public string name;
            public string id;
            public List<Task> tasks;
            [HideInInspector]
            public int total;
            [HideInInspector]
            public int completed;
        }

        [Serializable]
        public class Task
        {
            [HideInInspector]
            public string name;
            public string id;
            public string taskRefName;
            public TaskRefType taskRefType;
            public bool isTaskRefRight;
            public TaskItem[] needs;
            public TaskItem[] rewards;
            [HideInInspector]
            public int completed;
        }

        [Serializable]
        public class TaskItem
        {
            public Sprite sprite;
            public Type type;
            public ItemTypes.Group group;
            public ItemTypes.GenGroup genGroup;
            public CollGroup collGroup;
            public ChestGroup chestGroup;
            public int amount;
            [HideInInspector]
            public int completed;
        }

        [Serializable]
        public class FinishedTask
        {
            public string groupId;
            public string taskId;
        }

        [Serializable]
        public class TaskGroupJson
        {
            public string id;
            public string tasks;
            public int total;
            public int completed;
        }

        [Serializable]
        public class TaskJson
        {
            public string id;
            public string taskRefName;
            public string taskRefType;
            public bool isTaskRefRight;
            public string needs;
            public string rewards;
            public int completed;
        }

        [Serializable]
        public class TaskItemJson
        {
            public string sprite;
            public string type;
            public string group;
            public string genGroup;
            public string collGroup;
            public string chestGroup;
            public int amount;
            public int completed;
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

        public enum ShopItemType
        {
            Daily,
            Item,
            Gold,
            Gems
        }

        [Serializable]
        public class ShopItemsContent
        {
            public int total;
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
            public int total;
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

        //// OTHER ////
        public enum Locale
        {
            English, // English (en-US)
            French, // Français (fr-FR)
            Spanish, // Española (es-ES)
            German, // Deutsch (de-DE)
            Italian, // Italiano (it-IT)
            Russian, // Русский (ru-RU)
            Armenian, // Հայերեն (hy-HY)
            Japanese, // 日本語 (ja-JP)
            Korean, // 한국어 (ko-KR)
            Chinese // 中文 (zh-CN)
        };
    }
}