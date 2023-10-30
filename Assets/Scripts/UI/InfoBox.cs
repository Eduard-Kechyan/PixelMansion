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
        public TimeManager timeManager;
        public int openAmount = 10;
        public int unlockAmount = 5;
        public int speedUpAmount = 5;
        public int popAmount = 5;

        private Item item;

        private Sprite sprite;

        private string textToSet = "";

        private int sellAmount = 1;

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

        private ActionType mainActionType;
        private ActionType secondaryActionType;

        // References
        private InfoMenu infoMenu;
        private ShopMenu shopMenu;
        private ConfirmMenu confirmMenu;
        private GameData gameData;
        private I18n LOCALE;
        private AdsManager adsManager;

        // UI
        private VisualElement root;
        private VisualElement infoItem;
        private VisualElement infoItemLocked;
        private VisualElement infoItemBubble;
        private Button infoButton;
        private Button infoMainButton;
        private VisualElement infoMainValue;
        private Label infoMainAmountLabel;
        private Label infoMainNameLabel;
        private VisualElement infoMainRemoveIcon;
        private VisualElement infoSecondaryWatchIcon;
        private Button infoSecondaryButton;
        private VisualElement infoSecondaryValue;
        private Label infoSecondaryAmountLabel;
        private Label infoSecondaryNameLabel;
        private Label infoName;
        private Label infoData;
        private Label infoTimer;

        void Start()
        {
            // Cache
            infoMenu = GameRefs.Instance.infoMenu;
            shopMenu = GameRefs.Instance.shopMenu;
            confirmMenu = GameRefs.Instance.confirmMenu;
            gameData = GameData.Instance;
            LOCALE = I18n.Instance;
            adsManager = Services.Instance.GetComponent<AdsManager>();

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            infoItem = root.Q<VisualElement>("InfoItem");
            infoItemLocked = root.Q<VisualElement>("InfoItemLocked");
            infoItemBubble = root.Q<VisualElement>("InfoItemBubble");

            infoButton = root.Q<Button>("InfoButton");

            infoMainButton = root.Q<Button>("InfoMainButton");
            infoMainValue = infoMainButton.Q<VisualElement>("Value");
            infoMainAmountLabel = infoMainButton.Q<Label>("AmountLabel");
            infoMainNameLabel = infoMainButton.Q<Label>("NameLabel");
            infoMainRemoveIcon = infoMainButton.Q<VisualElement>("RemoveIcon");

            infoSecondaryButton = root.Q<Button>("InfoSecondaryButton");
            infoSecondaryValue = infoSecondaryButton.Q<VisualElement>("Value");
            infoSecondaryAmountLabel = infoSecondaryButton.Q<Label>("AmountLabel");
            infoSecondaryNameLabel = infoSecondaryButton.Q<Label>("NameLabel");
            infoSecondaryWatchIcon = infoSecondaryButton.Q<VisualElement>("WatchIcon");

            infoName = root.Q<Label>("InfoName");
            infoData = root.Q<Label>("InfoData");

            infoTimer = root.Q<Label>("InfoTimer");

            // UI taps
            infoButton.clicked += () => infoMenu.Open(item);

            infoMainButton.clicked += () => InfoAction();

            infoSecondaryButton.clicked += () => InfoAction(false);

            Init();
        }

        void Init()
        {
            infoName.text = "";
            infoData.text = LOCALE.Get("info_box_default");
        }

        // Show the selected item's info
        public void Select(Item newItem)
        {
            item = newItem;

            infoItemLocked.style.display = DisplayStyle.None;
            infoItemBubble.style.display = DisplayStyle.None;

            infoButton.style.display = DisplayStyle.None;

            switch (item.state)
            {
                case Types.State.Crate: //// CRATE ////
                    infoName.text = LOCALE.Get("info_box_crate");

                    sprite = item.crateChild.GetComponent<SpriteRenderer>().sprite;

                    mainActionType = ActionType.Open;
                    break;
                case Types.State.Locker: //// LOCKER ////
                    infoName.text = item.itemName + " " + LOCALE.Get("info_box_locker");

                    sprite = item.itemChild.GetComponent<SpriteRenderer>().sprite;

                    mainActionType = ActionType.Unlock;
                    break;
                case Types.State.Bubble: //// BUBBLE ////
                    infoName.text = item.itemName + " " + LOCALE.Get("info_box_bubble");

                    sprite = item.itemChild.GetComponent<SpriteRenderer>().sprite;

                    infoItemBubble.style.display = DisplayStyle.Flex;

                    mainActionType = ActionType.Pop;
                    secondaryActionType = ActionType.Pop;
                    break;
                default: //// ITEM ////
                    infoName.text = item.itemLevelName;

                    sprite = item.itemChild.GetComponent<SpriteRenderer>().sprite;

                    infoButton.style.display = DisplayStyle.Flex;

                    if (item.type == Types.Type.Chest)
                    {
                        if (!item.chestOpen && item.chestGroup == Types.ChestGroup.Item && !item.timerOn)
                        {
                            mainActionType = ActionType.UnlockChest;
                        }
                        else
                        {
                            mainActionType = ActionType.None;
                        }
                    }
                    else if (item.type == Types.Type.Coll)
                    {
                        mainActionType = ActionType.None;
                    }
                    else if (item.type == Types.Type.Gen || (item.type == Types.Type.Item && item.level > 3) || item.isMaxLevel)
                    {
                        CalcSellPrice(item.level);

                        mainActionType = ActionType.Sell;
                    }
                    else
                    {
                        mainActionType = ActionType.Remove;
                    }

                    if (item.timerOn)
                    {
                        secondaryActionType = ActionType.SpeedUp;

                        infoTimer.text = timeManager.GetTimerText(item.id);
                    }
                    else
                    {
                        secondaryActionType = ActionType.None;
                    }

                    break;
            }

            infoItem.style.backgroundImage = new StyleBackground(sprite);

            infoData.text = GetItemData(newItem);

            HandleActionButton();
        }

        // Hide the existing info
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
                infoMainButton.style.display = DisplayStyle.None;

                infoMainButton.RemoveFromClassList("info_box_action_button_has_value");

                infoMainButton.RemoveFromClassList("info_box_button_disabled");

                infoMainValue.style.display = DisplayStyle.None;

                infoSecondaryButton.style.display = DisplayStyle.None;

                infoSecondaryButton.RemoveFromClassList("info_box_action_button_has_value");

                infoSecondaryButton.RemoveFromClassList("info_box_button_disabled");

                infoSecondaryValue.style.display = DisplayStyle.None;

                infoData.text = LOCALE.Get("info_box_default");

                infoItem.RemoveFromClassList("info_item_has_timer");
                infoItemLocked.RemoveFromClassList("info_item_has_timer");
                infoItemBubble.RemoveFromClassList("info_item_has_timer");

                infoTimer.style.display = DisplayStyle.None;
            }
        }

        // Refresh the item's info
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

        // Set timer text
        public void TryToSetTimer(string id, string time)
        {
            if (item != null && item.id == id)
            {
                infoTimer.text = time;
            }
        }

        // Set the buttons
        void HandleActionButton()
        {
            infoSecondaryWatchIcon.style.display = DisplayStyle.None;
            infoMainRemoveIcon.style.display = DisplayStyle.None;
            infoItem.RemoveFromClassList("info_item_has_timer");
            infoItemLocked.RemoveFromClassList("info_item_has_timer");
            infoItemBubble.RemoveFromClassList("info_item_has_timer");
            infoTimer.style.display = DisplayStyle.None;

            switch (mainActionType)
            {
                case ActionType.Open:
                    infoMainButton.style.display = DisplayStyle.Flex;
                    infoMainButton.style.unityBackgroundImageTintColor = Glob.colorBlue;

                    infoMainAmountLabel.text = openAmount.ToString();
                    infoMainNameLabel.text = LOCALE.Get("info_box_button_open");

                    infoMainButton.AddToClassList("info_box_action_button_has_value");
                    infoMainValue.style.backgroundImage = new StyleBackground(gemValue);
                    infoMainValue.style.display = DisplayStyle.Flex;
                    break;
                case ActionType.Unlock:
                    infoMainButton.style.display = DisplayStyle.Flex;
                    infoMainButton.style.unityBackgroundImageTintColor = Glob.colorBlue;

                    infoMainAmountLabel.text = unlockAmount.ToString();
                    infoMainNameLabel.text = LOCALE.Get("info_box_button_unlock");

                    infoMainButton.AddToClassList("info_box_action_button_has_value");
                    infoMainValue.style.backgroundImage = new StyleBackground(gemValue);
                    infoMainValue.style.display = DisplayStyle.Flex;
                    break;
                case ActionType.UnlockChest:
                    infoMainButton.style.display = DisplayStyle.Flex;
                    infoMainButton.style.unityBackgroundImageTintColor = Glob.colorBlue;

                    infoMainAmountLabel.text = LOCALE.Get("info_box_button_unlock");
                    infoMainNameLabel.text = "";

                    infoMainButton.RemoveFromClassList("info_box_action_button_has_value");
                    infoMainValue.style.display = DisplayStyle.None;
                    break;
                case ActionType.Pop:
                    infoMainButton.style.display = DisplayStyle.Flex;
                    infoMainButton.style.unityBackgroundImageTintColor = Glob.colorBlue;

                    infoMainAmountLabel.text = popAmount.ToString();
                    infoMainNameLabel.text = LOCALE.Get("info_box_button_pop");

                    infoMainButton.AddToClassList("info_box_action_button_has_value");
                    infoMainValue.style.backgroundImage = new StyleBackground(gemValue);
                    infoMainValue.style.display = DisplayStyle.Flex;
                    break;
                case ActionType.Sell:
                    infoMainButton.style.display = DisplayStyle.Flex;
                    infoMainButton.style.unityBackgroundImageTintColor = Glob.colorGreen;

                    infoMainAmountLabel.text = sellAmount.ToString();
                    infoMainNameLabel.text = LOCALE.Get("info_box_button_sell");

                    infoMainButton.AddToClassList("info_box_action_button_has_value");
                    infoMainValue.style.backgroundImage = new StyleBackground(goldValue);
                    infoMainValue.style.display = DisplayStyle.Flex;
                    break;
                case ActionType.Remove:
                    infoMainButton.style.display = DisplayStyle.Flex;
                    infoMainButton.style.unityBackgroundImageTintColor = Glob.colorRed;

                    infoMainAmountLabel.text = "";
                    infoMainNameLabel.text = LOCALE.Get("info_box_button_remove");

                    infoMainButton.RemoveFromClassList("info_box_action_button_has_value");
                    infoMainValue.style.display = DisplayStyle.None;

                    infoMainRemoveIcon.style.display = DisplayStyle.Flex;
                    break;
                case ActionType.None:
                    infoMainButton.style.display = DisplayStyle.None;
                    break;
            }

            switch (secondaryActionType)
            {
                case ActionType.SpeedUp:
                    infoSecondaryButton.style.display = DisplayStyle.Flex;
                    infoSecondaryButton.style.unityBackgroundImageTintColor = Glob.colorGreen;

                    infoSecondaryAmountLabel.text = speedUpAmount.ToString();
                    infoSecondaryNameLabel.text = LOCALE.Get("info_box_button_speed_up");

                    infoItem.AddToClassList("info_item_has_timer");
                    infoItemLocked.AddToClassList("info_item_has_timer");
                    infoItemBubble.AddToClassList("info_item_has_timer");
                    infoTimer.style.display = DisplayStyle.Flex;

                    infoSecondaryButton.AddToClassList("info_box_action_button_has_value");
                    infoSecondaryValue.style.backgroundImage = new StyleBackground(gemValue);
                    infoSecondaryValue.style.display = DisplayStyle.Flex;
                    break;
                case ActionType.Pop:
                    infoSecondaryButton.style.display = DisplayStyle.Flex;
                    infoSecondaryButton.style.unityBackgroundImageTintColor = Glob.colorGreen;

                    infoSecondaryAmountLabel.text = LOCALE.Get("info_box_button_watch_ad");
                    infoSecondaryNameLabel.text = "";
                    break;
                case ActionType.None:
                    infoSecondaryButton.style.display = DisplayStyle.None;

                    infoSecondaryWatchIcon.style.display = DisplayStyle.Flex;
                    break;
            }
        }

        // Handle the buttons
        void InfoAction(bool mainButton = true)
        {
            if (mainButton)
            {
                switch (mainActionType)
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
                        // Confirm selling item if it's a generator or at least level 10
                        if (item.type == Types.Type.Gen || item.level > 9)
                        {
                            confirmMenu.Open("sell_gen", () =>
                            {
                                boardInteractions.RemoveItem(item, sellAmount);
                                Unselect(true);
                                selectionManager.UnselectAlt();
                                SetUndoButton(true);
                            });
                        }
                        break;
                    case ActionType.Remove:
                        boardInteractions.RemoveItem(item);
                        Unselect(true);
                        selectionManager.UnselectAlt();
                        SetUndoButton();
                        break;
                    case ActionType.Undo:
                        boardInteractions.UndoLastStep();
                        Refresh();
                        break;
                }
            }
            else
            {
                switch (secondaryActionType)
                {
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
                        adsManager.WatchAd(Types.AdType.Bubble, (int reward) =>
                        {
                            boardInteractions.OpenItem(item, unlockAmount, Types.State.Bubble);
                            Select(item);
                            selectionManager.Select("both", false);
                        });
                        break;
                }
            }
        }

        // Set the undo button either sold or removed
        void SetUndoButton(bool sold = false)
        {
            mainActionType = ActionType.Undo;

            infoMainButton.style.display = DisplayStyle.Flex;
            infoMainButton.style.unityBackgroundImageTintColor = Glob.colorYellow;

            infoMainAmountLabel.text = LOCALE.Get("info_box_button_undo");
            infoMainNameLabel.text = "";

            infoMainRemoveIcon.style.display = DisplayStyle.None;

            if (sold)
            {
                infoMainButton.AddToClassList("info_box_action_button_has_value");

                infoMainValue.style.backgroundImage = new StyleBackground(goldValue);
                infoMainValue.style.display = DisplayStyle.Flex;
            }
        }

        // Show the collectables multiplied value
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
            }

            return multipliedValue;
        }

        void CalcSellPrice(int level)
        {
            sellAmount = gameData.valuesData.sellPriceMultiplier[level - 1];
        }

        // Get the current selected item's data
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
                                if (newItem.level >= newItem.generatesAt)
                                {
                                    textToSet = LOCALE.Get("info_box_gen", newItem.nextName);
                                }
                                else
                                {

                                    textToSet = LOCALE.Get("info_box_gen_pre", newItem.nextName);
                                }
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
                                if (!newItem.chestOpen)
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
                                else
                                {
                                    textToSet = LOCALE.Get("info_box_chest_alt_" + newItem.chestGroup); // Note the +
                                }
                            }
                            else
                            {
                                if (newItem.timerOn)
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
                                else if (!newItem.chestOpen)
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
    }
}