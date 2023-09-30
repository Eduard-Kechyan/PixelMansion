using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace Merge
{
    public class InfoBox : MonoBehaviour
    {
        // Variables
        public Sprite goldValue;
        public Sprite gemValue;
        public BoardInteractions boardInteractions;
        public SelectionManager selectionManager;
        public int openAmount = 10;
        public int unlockAmount = 5;
        public int speedUpAmount = 5;
        public int popAmount = 5;
        public int sellAmount = 5;

        private Item item;
        private string textToSet = "";

        // References

        private InfoMenu infoMenu;
        private ShopMenu shopMenu;

        // Instances
        private GameData gameData;
        private I18n LOCALE = I18n.Instance;

        // UI
        private VisualElement root;
        private VisualElement infoItem;
        private VisualElement infoItemLocked;
        private VisualElement infoItemBubble;
        private Button infoButton;
        private Button infoActionButton;
        private VisualElement infoActionValue;
        private Label infoName;
        private Label infoData;
        private Sprite sprite;

        public enum ActionType
        {
            Open,
            Unlock,
            UnlockChest,
            SpeedUp,
            Pop,
            Sell,
            Remove,
            Undo,
            None
        };

        private ActionType actionType;

        void Start()
        {
            // Cache
            infoMenu = GameRefs.Instance.infoMenu;
            shopMenu = GameRefs.Instance.shopMenu;

            // Cache instances
            gameData = GameData.Instance;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            infoItem = root.Q<VisualElement>("InfoItem");
            infoItemLocked = root.Q<VisualElement>("InfoItemLocked");
            infoItemBubble = root.Q<VisualElement>("InfoItemBubble");

            infoButton = root.Q<Button>("InfoButton");

            infoActionButton = root.Q<Button>("InfoActionButton");
            infoActionValue = infoActionButton.Q<VisualElement>("Value");

            infoName = root.Q<Label>("InfoName");
            infoData = root.Q<Label>("InfoData");

            // Initiate info box
            infoName.text = "";
            infoData.text = LOCALE.Get("info_box_default");

            CheckForTaps();
        }

        void CheckForTaps()
        {
            //Open info menu
            infoButton.clicked += () => infoMenu.Open(item);

            infoActionButton.clicked += () => InfoAction();
        }

        public void Select(Item newItem)
        {
            item = newItem;

            switch (item.state)
            {
                case Types.State.Crate: //////// CRATE ////////
                    infoName.text = LOCALE.Get("info_box_crate");

                    sprite = item.crateChild.GetComponent<SpriteRenderer>().sprite;

                    actionType = ActionType.Open;
                    break;
                case Types.State.Locker: //////// LOCKER ////////
                    infoName.text = item.itemName + " " + LOCALE.Get("info_box_locker");

                    sprite = item.itemChild.GetComponent<SpriteRenderer>().sprite;

                    infoItemLocked.style.display = DisplayStyle.Flex;

                    actionType = ActionType.Unlock;
                    break;
                case Types.State.Bubble: //////// BUBBLE ////////
                    infoName.text = item.itemName + " " + LOCALE.Get("info_box_bubble");

                    sprite = item.itemChild.GetComponent<SpriteRenderer>().sprite;

                    infoItemBubble.style.display = DisplayStyle.Flex;

                    actionType = ActionType.Pop;
                    break;
                default: //////// ITEM ////////
                    infoName.text = item.itemLevelName;

                    sprite = item.itemChild.GetComponent<SpriteRenderer>().sprite;

                    infoItemLocked.style.display = DisplayStyle.None;
                    infoItemBubble.style.display = DisplayStyle.None;

                    infoButton.style.display = DisplayStyle.Flex;

                    if (item.type == Types.Type.Chest)
                    {
                        if (item.chestLocked)
                        {
                            actionType = ActionType.UnlockChest;
                        }
                        else if (item.hasTimer && item.timerOn)
                        {
                            actionType = ActionType.SpeedUp;
                        }
                        else
                        {
                            actionType = ActionType.None;
                            // infoButton.style.display = DisplayStyle.None;
                        }
                    }
                    else if (item.type == Types.Type.Coll)
                    {
                        actionType = ActionType.None;
                        infoButton.style.display = DisplayStyle.None;
                    }
                    else if (item.level > 3 || item.isMaxLevel)
                    {
                        actionType = ActionType.Sell;
                    }
                    else
                    {
                        actionType = ActionType.Remove;
                    }

                    break;
            }

            infoItem.style.backgroundImage = new StyleBackground(sprite);

            infoData.text = GetItemData(newItem);

            HandleActionButton();
        }

        void HandleActionButton()
        {
            switch (actionType)
            {
                case ActionType.Open:
                    infoActionButton.text = openAmount.ToString();
                    infoActionButton.AddToClassList("info_box_action_button_has_value");
                    infoActionButton.style.display = DisplayStyle.Flex;
                    infoActionButton.style.unityBackgroundImageTintColor = Glob.colorBlue;

                    infoActionValue.style.backgroundImage = new StyleBackground(gemValue);
                    infoActionValue.style.display = DisplayStyle.Flex;
                    break;
                case ActionType.Unlock:
                    infoActionButton.text = unlockAmount.ToString();
                    infoActionButton.AddToClassList("info_box_action_button_has_value");
                    infoActionButton.style.display = DisplayStyle.Flex;
                    infoActionButton.style.unityBackgroundImageTintColor = Glob.colorBlue;

                    infoActionValue.style.backgroundImage = new StyleBackground(gemValue);
                    infoActionValue.style.display = DisplayStyle.Flex;
                    break;
                case ActionType.UnlockChest:
                    infoActionButton.RemoveFromClassList("info_box_action_button_has_value");
                    infoActionButton.style.display = DisplayStyle.Flex;
                    infoActionButton.style.unityBackgroundImageTintColor = Glob.colorBlue;

                    infoActionValue.style.display = DisplayStyle.None;
                    break;
                case ActionType.SpeedUp:
                    infoActionButton.text = speedUpAmount.ToString();
                    infoActionButton.AddToClassList("info_box_action_button_has_value");
                    infoActionButton.style.display = DisplayStyle.Flex;
                    infoActionButton.style.unityBackgroundImageTintColor = Glob.colorGreen;

                    infoActionValue.style.backgroundImage = new StyleBackground(gemValue);
                    infoActionValue.style.display = DisplayStyle.Flex;
                    break;
                case ActionType.Sell:
                    infoActionButton.text = sellAmount.ToString();
                    infoActionButton.AddToClassList("info_box_action_button_has_value");
                    infoActionButton.style.display = DisplayStyle.Flex;
                    infoActionButton.style.unityBackgroundImageTintColor = Glob.colorGreen;

                    infoActionValue.style.backgroundImage = new StyleBackground(goldValue);
                    infoActionValue.style.display = DisplayStyle.Flex;
                    break;
                case ActionType.Remove:
                    infoActionButton.text = LOCALE.Get("info_box_button_remove");
                    infoActionButton.RemoveFromClassList("info_box_action_button_has_value");
                    infoActionButton.style.display = DisplayStyle.Flex;
                    infoActionButton.style.unityBackgroundImageTintColor = Glob.colorRed;

                    infoActionValue.style.display = DisplayStyle.None;
                    break;
                case ActionType.None:
                    infoActionButton.style.display = DisplayStyle.None;

                    infoActionValue.style.display = DisplayStyle.None;
                    break;
            }
        }

        public void Unselect(bool isUndo = false)
        {
            item = null;

            infoItem.style.backgroundImage = null;

            infoItemLocked.style.display = DisplayStyle.None;

            sprite = null;

            infoButton.style.display = DisplayStyle.None;
            infoName.text = "";

            textToSet = "";

            if (isUndo || boardInteractions.canUndo)
            {
                infoData.text = LOCALE.Get("info_box_undo");
            }
            else
            {
                infoActionButton.style.display = DisplayStyle.None;

                infoActionButton.RemoveFromClassList("info_box_action_button_has_value");

                infoActionButton.RemoveFromClassList("info_box_button_disabled");

                infoActionValue.style.display = DisplayStyle.None;

                infoData.text = LOCALE.Get("info_box_default");
            }
        }

        public string GetItemData(Item newItem, bool alt = false)
        {
            switch (newItem.state)
            {
                case Types.State.Crate:
                    textToSet = LOCALE.Get("info_box_crate_text");
                    break;
                case Types.State.Locker:

                    textToSet = LOCALE.Get("info_box_locker_text");
                    break;
                case Types.State.Bubble:

                    textToSet = LOCALE.Get("info_box_bubble_text");
                    break;
                default:
                    switch (newItem.type)
                    {
                        case Types.Type.Gen:
                            if (newItem.isMaxLevel)
                            {
                                textToSet = LOCALE.Get("info_box_gen_max");
                            }
                            else
                            {
                                textToSet = LOCALE.Get("info_box_gen", newItem.nextName);
                            }
                            break;
                        case Types.Type.Coll:
                            int multipliedValue = GetMultipliedValue(item.level, item.collGroup);

                            if (item.collGroup == Types.CollGroup.Gems)
                            {
                                if (item.isMaxLevel)
                                {
                                    textToSet = LOCALE.Get(
                                        "info_box_gems_max",
                                        multipliedValue,
                                        LOCALE.Get("Coll_Gems", item.level)
                                    );
                                }
                                else
                                {
                                    textToSet = LOCALE.Get(
                                        "info_box_gems",
                                        multipliedValue,
                                        LOCALE.Get("Coll_" + item.collGroup.ToString())
                                    );
                                }
                            }
                            else
                            {
                                if (item.isMaxLevel)
                                {
                                    textToSet = LOCALE.Get(
                                        "info_box_coll_max",
                                        multipliedValue,
                                        LOCALE.Get("Coll_" + item.collGroup.ToString())
                                    );
                                }
                                else
                                {
                                    textToSet = LOCALE.Get(
                                        "info_box_coll",
                                        multipliedValue,
                                        LOCALE.Get("Coll_" + item.collGroup.ToString())
                                    );
                                }
                            }
                            break;
                        case Types.Type.Chest:
                            if (alt)
                            {
                                textToSet = LOCALE.Get("info_box_chest_alt_" + newItem.chestGroup); // Note the +
                            }
                            else
                            {
                                if (newItem.chestLocked)
                                {
                                    if (newItem.isMaxLevel)
                                    {
                                        textToSet = LOCALE.Get("info_box_chest_max_locked");
                                    }
                                    else
                                    {
                                        textToSet = LOCALE.Get("info_box_chest_locked", newItem.nextName);
                                    }
                                }
                                else if (newItem.hasTimer && newItem.timerOn)
                                {
                                    if (newItem.isMaxLevel)
                                    {
                                        textToSet = LOCALE.Get("info_box_chest_max_timer");
                                    }
                                    else
                                    {
                                        textToSet = LOCALE.Get("info_box_chest_timer", newItem.nextName);
                                    }
                                }
                                else
                                {
                                    if (!newItem.hasLevel)
                                    {
                                        textToSet = LOCALE.Get("info_box_chest_single_" + newItem.chestGroup); // Note the +
                                    }
                                    else if (newItem.isMaxLevel)
                                    {
                                        textToSet = LOCALE.Get("info_box_chest_max_" + newItem.chestGroup); // Note the +
                                    }
                                    else
                                    {
                                        textToSet = LOCALE.Get("info_box_chest_" + newItem.chestGroup, newItem.chestGroup.ToString(), newItem.nextName); // Note the +
                                    }
                                }
                            }

                            break;
                        default:
                            if (newItem.isMaxLevel)
                            {
                                textToSet = LOCALE.Get("info_box_item_max");
                            }
                            else
                            {
                                textToSet = LOCALE.Get("info_box_item", newItem.nextName);
                            }
                            break;
                    }
                    break;
            }

            return textToSet;
        }

        void InfoAction()
        {
            switch (actionType)
            {
                case ActionType.Open:
                    if (gameData.gems < openAmount)
                    {
                        shopMenu.Open("Gems");
                    }
                    else
                    {
                        boardInteractions.OpenItem(item, openAmount, Types.State.Crate);
                        Select(item);
                        selectionManager.Select("both", false);
                    }
                    break;
                case ActionType.Unlock:
                    if (gameData.gems < unlockAmount)
                    {
                        shopMenu.Open("Gems");
                    }
                    else
                    {
                        boardInteractions.OpenItem(item, unlockAmount, Types.State.Locker);
                        Select(item);
                        selectionManager.Select("both", false);
                    }
                    break;
                case ActionType.UnlockChest:
                    boardInteractions.UnlockChest(item);
                    Select(item);
                    selectionManager.Select("both", false);
                    break;
                case ActionType.SpeedUp:
                    if (gameData.gems < speedUpAmount)
                    {
                        shopMenu.Open("Gems");
                    }
                    else
                    {
                        boardInteractions.SpeedUpItem(item, speedUpAmount);
                        Select(item);
                        selectionManager.Select("both", false);
                    }
                    break;
                case ActionType.Pop:
                    if (gameData.gems < popAmount)
                    {
                        shopMenu.Open("Gems");
                    }
                    else
                    {
                        boardInteractions.OpenItem(item, unlockAmount, Types.State.Bubble);
                        Select(item);
                        selectionManager.Select("both", false);
                    }
                    break;
                case ActionType.Sell:
                    boardInteractions.RemoveItem(item, sellAmount);
                    Unselect(true);
                    selectionManager.UnselectAlt();
                    SetUndoButton(true);
                    break;
                case ActionType.Remove:
                    boardInteractions.RemoveItem(item);
                    Unselect(true);
                    selectionManager.UnselectAlt();
                    SetUndoButton();
                    break;
                case ActionType.Undo:
                    boardInteractions.UndoLastStep();
                    break;
            }
        }

        void SetUndoButton(bool sold = false)
        {
            actionType = ActionType.Undo;

            infoActionButton.text = LOCALE.Get("info_box_button_undo");
            infoActionButton.style.display = DisplayStyle.Flex;
            infoActionButton.style.unityBackgroundImageTintColor = Glob.colorYellow;

            if (sold)
            {
                infoActionButton.AddToClassList("info_box_action_button_has_value");

                infoActionValue.style.backgroundImage = new StyleBackground(goldValue);
                infoActionValue.style.display = DisplayStyle.Flex;
            }
        }

        public void Refresh()
        {
            if (boardInteractions.isSelected)
            {
                Select(boardInteractions.currentItem);
            }
            else
            {
                infoData.text = LOCALE.Get("info_box_default");
            }
        }

        int GetMultipliedValue(int level, Types.CollGroup collGroup)
        {
            int multipliedValue = 0;
            switch (collGroup)
            {
                case Types.CollGroup.Experience:
                    multipliedValue = gameData.valuesData.experienceMultiplier[level - 1];
                    break;
                case Types.CollGroup.Gold:
                    multipliedValue = gameData.valuesData.goldMultiplier[level - 1];
                    break;
                case Types.CollGroup.Gems:
                    multipliedValue = gameData.valuesData.gemsMultiplier[level - 1];
                    break;
                case Types.CollGroup.Energy:
                    multipliedValue = gameData.valuesData.energyMultiplier[level - 1];
                    break;
                default:
                    Debug.Log("Wrong collectables group");
                    break;
            }

            return multipliedValue;
        }
    }
}