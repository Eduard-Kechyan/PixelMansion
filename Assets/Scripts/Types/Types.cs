using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class Types : MonoBehaviour
    {
        //// CHARACTER ////
        public enum Character
        {
            NONE,
            Julia,
            James,
        };

        public enum CharacterExpression
        {
            Natural,
            Happy,
            Surprised,
            Angry,
            Sad,
            Sleepy,
            Thinking
        };

        [Serializable]
        public class CharacterColor
        {
            [HideInInspector]
            public string name;
            public Character character;
            public Color accentColor = Color.black;
        }

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
            [HideInInspector]
            public string name;
            public Sprite sprite;
            public Type type;
            public ItemTypes.Group group;
            public ItemTypes.GenGroup genGroup;
            public CollGroup collGroup;
            public int maxLevel;
            public bool canIncreaseMaxLevel;
            public float chance;
        }

        //// TIMERS ///
        public enum TimerType
        {
            Item,
            Energy,
            Bubble
        };

        [Serializable]
        public class Timer
        {
            public DateTime startTime;
            public int seconds;
            public bool running = true;
            public TimerType timerType;
            public Type itemType;
            public string id;
            public int notificationId;
            public NotificationType notificationType;
        }

        [Serializable]
        public class TimerJson
        {
            public string startTime;
            public int seconds;
            public bool running;
            public string timerType;
            public string id;
            public int notificationId;
            public string notificationType;
        }

        [Serializable]
        public class CoolDown
        {
            public int[] maxCounts;
            public int seconds;
            [ReadOnly]
            public int minutes;
        }

        [Serializable]
        public class CoolDownCount
        {
            public int count;
            public int level;
            public string id;
            public DateTime startTime;
        }

        [Serializable]
        public class CoolDownCountJson
        {
            public int count;
            public int level;
            public string id;
            public string startTime;
        }

        //// ITEMS DATA ////

        [Serializable]
        public class Item
        {
            [HideInInspector]
            public string name;
            public Type type;
            public ItemTypes.Group group;
            public bool hasLevel;
            public bool customName;

            public ParentData[] parents;
            public ItemData[] content;

            [HideInInspector]
            public CollGroup collGroup;
            [HideInInspector]
            public ChestGroup chestGroup;
            [HideInInspector]
            public ItemTypes.GenGroup genGroup;
            [HideInInspector]
            public bool hasTimer;
            [HideInInspector]
            public int generatesAtLevel;
            [HideInInspector]
            public int generatesMaxCount;
            [HideInInspector]
            public Creates[] creates;
            [HideInInspector]
            public CoolDown coolDown;
        }

        [Serializable]
        public class Gen
        {
            [HideInInspector]
            public string name;
            public ItemTypes.GenGroup genGroup;
            public bool hasLevel;
            public bool customName;
            public int generatesAtLevel;

            public CoolDown coolDown;
            public ParentData[] parents;
            public Creates[] creates;
            public ItemData[] content;
        }

        [Serializable]
        public class Chest
        {
            [HideInInspector]
            public string name;
            public ChestGroup chestGroup;
            public bool hasLevel;
            public bool customName;

            public Creates[] creates;
            public ItemData[] content;
        }

        [Serializable]
        public class Coll
        {
            [HideInInspector]
            public string name;
            public CollGroup collGroup;
            public bool hasLevel;
            public bool customName;

            public ParentData[] parents;
            public ItemData[] content;
        }

        [Serializable]
        public class ParentData
        {
            [HideInInspector]
            public string name;
            public Type type;
            public ItemTypes.GenGroup genGroup;
            public ChestGroup chestGroup;
        }

        [Serializable]
        public class ItemData
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
            public int generatesAtLevel;

            [HideInInspector]
            public ItemTypes.GenGroup genGroup;

            [HideInInspector]
            public CollGroup collGroup;

            [HideInInspector]
            public ParentData[] parents;

            [HideInInspector]
            public Creates[] creates;

            [HideInInspector]
            public CoolDown coolDown;

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
        public class TileEmpty
        {
            public int order;
            public Vector2Int loc;
            public float distance;
        }

        [Serializable]
        public class Tile
        {
            public Sprite sprite;
            public Type type;
            public State state;
            public ItemTypes.Group group;
            public ItemTypes.GenGroup genGroup;
            public CollGroup collGroup;
            public ChestGroup chestGroup;
            public bool hasTimer;

            [HideInInspector]
            public string id;

            [HideInInspector]
            public int generatesAtLevel;

            [HideInInspector]
            public int crate;

            [HideInInspector]
            public int order;

            [HideInInspector]
            public int chestItems;

            [HideInInspector]
            public bool chestItemsSet;

            [HideInInspector]
            public bool chestOpen;

            [HideInInspector]
            public bool gemPopped;

            [HideInInspector]
            public bool isCompleted;

            [HideInInspector]
            public bool timerOn;
        }

        [Serializable]
        public class TileJson
        {
            public string sprite;
            public string type;
            public string group;
            public string genGroup;
            public string collGroup;
            public string chestGroup;
            public bool hasTimer;
            public string id;
            public int generatesAtLevel;
            public int crate;
            public string state;
            public int chestItems;
            public bool chestItemsSet;
            public bool chestOpen;
            public bool gemPopped;
            public bool isCompleted;
            public bool timerOn;
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
            public string id;
            public bool isCompleted;
            public bool timerOn;
            public DateTime timerAltTime;
            public bool gemPopped;
        }

        public class InventoryJson
        {
            public string sprite;
            public string type;
            public string group;
            public string genGroup;
            public string chestGroup;
            public string id;
            public bool isCompleted;
            public bool timerOn;
            public string timerAltTime;
            public bool gemPopped;
        }

        //// PROGRESS ////
        public enum StepType
        {
            Task,
            Convo,
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

        [Serializable]
        public class ProgressStep
        {
            public string stepType;
            public string id;
        }

        //// CONVERSATIONS ////
        [Serializable]
        public class ConvoGroup
        {
            [HideInInspector]
            public string name;
            public string id;
            public bool hasTimeOut = true;
            public Character characterA = Character.Julia;
            public Character characterB = Character.NONE;
            public List<Convo> content;
        }

        [Serializable]
        public class Convo
        {
            public Character character;
            public CharacterExpression expression;
            public bool isRight;
            public bool isSide;
            public string convoExtra;
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

        //// TUTORIAL ////
        public enum TutorialStepType
        {
            Task,
            Convo,
            Input,
            Story,
        };

        public enum TutorialStepTask
        {
            Press,
            Merge,
            Gen,
            Menu
        };

        [Serializable]
        public class TutorialStep
        {
            [HideInInspector]
            public string name;
            public string id;
            public Scene scene;
            public TutorialStepType type;
            public TutorialStepTask taskType;
            public Button taskButton;
            public Sprite taskSprite;
            public int taskOrder;
            public bool keepConvoOpen;
        }

        //// ERROR ////
        public enum ErrorType
        {
            Code,
            GamePlay,
            Unity,
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
            public string id;
            public Sprite sprite;
            public bool isPopular;
        }

        //// NOTIFICATIONS ////
        public enum NotificationType
        {
            Gen,
            Chest,
            Energy,
        }

        [Serializable]
        public class Notification
        {
            public int id;
            public DateTime fireTime;
            public NotificationType type;
            public string itemName;
        }

        [Serializable]
        public class NotificationJson
        {
            public int id;
            public string fireTime;
            public string type;
            public string itemName;
        }


        //// AUDIO ////
        public enum MusicType
        {
            Loading,
            World,
            Merge,
            Magical,
        };

        public enum SoundType
        {
            None,
            Merge,
            Generate,
            UnlockLock,
            OpenCrate,
            LevelUp,
            LevelUpIndicator,
            Transition,
            Experience,
            Energy,
            Gold,
            Gems,
            Pop,
            Buzz
        };

        //// OTHER ////
        public enum SelectType
        {
            None,
            Both,
            Info,
            Only
        }

        public enum Scene
        {
            Loading,
            World,
            Merge,
            None
        };

        public enum Button
        {
            None,
            Play,
            Home,
            Task,
            TaskMenu,
            Shop,
            Bonus,
            Settings,
            Inventory,
        };

        public enum Locale
        {
            English, // English (en-US)
            French, // Français (fr-FR)
            Spanish, // Español (es-ES)
            German, // Deutsch (de-DE)
            Italian, // Italiano (it-IT)
            Russian, // Русский (ru-RU)
            Armenian, // Հայերեն (hy-HY)
            Japanese, // 日本語 (ja-JP)
            Korean, // 한국어 (ko-KR)
            Chinese // 中文 (zh-CN)
        };

        public enum SocialMediaType
        {
            Instagram,
            Facebook,
            Youtube
        }

        public enum AdType
        {
            Energy,
            Bubble
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        //// DEBUG ////

        [Serializable]
        public class LogData
        {
            public string message;
            public List<string> stackTrace;
            public Color color;
        }
#endif
    }
}