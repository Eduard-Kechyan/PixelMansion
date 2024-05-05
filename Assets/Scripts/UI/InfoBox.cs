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
        public Sprite avatarSprite;
        public int openAmount = 10;
        public int unlockAmount = 5;
        public int speedUpAmount = 5;
        public int popAmount = 5;
        public int itemSellLevel = 3;
        public int genSellLevel = 5;

        private Item item;

        private Sprite sprite;

        private int sellAmount = 1;

        private bool timeOn = false;

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
        private GameRefs gameRefs;
        private InfoMenu infoMenu;
        private ShopMenu shopMenu;
        private ConfirmMenu confirmMenu;
        private GameData gameData;
        private I18n LOCALE;
        private AdsManager adsManager;
        private BoardInteractions boardInteractions;
        private BoardSelection boardSelection;
        private TimeManager timeManager;
        private TutorialManager tutorialManager;
        private ItemHandler itemHandler;

        // UI
        private VisualElement root;
        private VisualElement infoBox;
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
            gameRefs = GameRefs.Instance;
            infoMenu = gameRefs.infoMenu;
            shopMenu = gameRefs.shopMenu;
            confirmMenu = gameRefs.confirmMenu;
            gameData = GameData.Instance;
            LOCALE = I18n.Instance;
            adsManager = Services.Instance.GetComponent<AdsManager>();
            boardInteractions = gameRefs.boardInteractions;
            boardSelection = gameRefs.boardSelection;
            timeManager = gameRefs.timeManager;
            tutorialManager = gameRefs.tutorialManager;
            itemHandler = DataManager.Instance.GetComponent<ItemHandler>();

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            infoBox = root.Q<VisualElement>("InfoBox");

            infoItem = infoBox.Q<VisualElement>("InfoItem");
            infoItemLocked = infoBox.Q<VisualElement>("InfoItemLocked");
            infoItemBubble = infoBox.Q<VisualElement>("InfoItemBubble");

            infoButton = infoBox.Q<Button>("InfoButton");

            infoMainButton = infoBox.Q<Button>("InfoMainButton");
            infoMainValue = infoMainButton.Q<VisualElement>("Value");
            infoMainAmountLabel = infoMainButton.Q<Label>("AmountLabel");
            infoMainNameLabel = infoMainButton.Q<Label>("NameLabel");
            infoMainRemoveIcon = infoMainButton.Q<VisualElement>("RemoveIcon");

            infoSecondaryButton = infoBox.Q<Button>("InfoSecondaryButton");
            infoSecondaryValue = infoSecondaryButton.Q<VisualElement>("Value");
            infoSecondaryAmountLabel = infoSecondaryButton.Q<Label>("AmountLabel");
            infoSecondaryNameLabel = infoSecondaryButton.Q<Label>("NameLabel");
            infoSecondaryWatchIcon = infoSecondaryButton.Q<VisualElement>("WatchIcon");

            infoName = infoBox.Q<Label>("InfoName");
            infoData = infoBox.Q<Label>("InfoData");

            infoTimer = infoBox.Q<Label>("InfoTimer");

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
            if (tutorialManager == null)
            {
                item = newItem;

                infoItemLocked.style.display = DisplayStyle.None;
                infoItemBubble.style.display = DisplayStyle.None;

                infoButton.style.display = DisplayStyle.None;

                timeOn = false;

                switch (item.state)
                {
                    case Item.State.Crate: //// CRATE ////
                        infoName.text = LOCALE.Get("info_box_crate");

                        sprite = item.crateChild.GetComponent<SpriteRenderer>().sprite;

                        mainActionType = ActionType.Open;
                        break;
                    case Item.State.Locker: //// LOCKER ////
                        infoName.text = item.itemName + " " + LOCALE.Get("info_box_locker");

                        sprite = item.itemChild.GetComponent<SpriteRenderer>().sprite;

                        mainActionType = ActionType.Unlock;
                        break;
                    case Item.State.Bubble: //// BUBBLE ////
                        infoName.text = item.itemName + " " + LOCALE.Get("info_box_bubble");

                        sprite = item.itemChild.GetComponent<SpriteRenderer>().sprite;

                        infoItemBubble.style.display = DisplayStyle.Flex;

                        mainActionType = ActionType.Pop;
                        secondaryActionType = ActionType.Pop;

                        if (item.timerOn)
                        {
                            infoTimer.text = timeManager.GetTimerText(item.id);

                            timeOn = true;
                        }
                        break;
                    default: //// ITEM ////
                        infoName.text = item.itemLevelName;

                        sprite = item.itemChild.GetComponent<SpriteRenderer>().sprite;

                        infoButton.style.display = DisplayStyle.Flex;

                        if (item.type == Item.Type.Chest)
                        {
                            if (!item.chestOpen && item.chestGroup == Item.ChestGroup.Item && !item.timerOn)
                            {
                                mainActionType = ActionType.UnlockChest;
                            }
                            else
                            {
                                mainActionType = ActionType.None;
                            }
                        }
                        else if (item.type == Item.Type.Coll)
                        {
                            mainActionType = ActionType.None;
                        }
                        else if ((item.type == Item.Type.Gen && item.level > genSellLevel) || (item.type == Item.Type.Item && item.level > itemSellLevel) || item.isMaxLevel)
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

                            timeOn = true;
                        }
                        else
                        {
                            secondaryActionType = ActionType.None;
                        }

                        break;
                }

                infoItem.style.backgroundImage = new StyleBackground(sprite);

                infoData.text = itemHandler.GetItemData(newItem);

                HandleActionButton();
            }
        }

        // Hide the existing info
        public void Unselect(bool isUndo = false)
        {
            if (tutorialManager == null)
            {
                item = null;

                infoItem.style.backgroundImage = null;

                infoItemLocked.style.display = DisplayStyle.None;

                infoItemBubble.style.display = DisplayStyle.None;

                sprite = null;

                infoButton.style.display = DisplayStyle.None;

                infoName.text = "";

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

            if (timeOn)
            {
                infoItem.AddToClassList("info_item_has_timer");
                infoItemLocked.AddToClassList("info_item_has_timer");
                infoItemBubble.AddToClassList("info_item_has_timer");
                infoTimer.style.display = DisplayStyle.Flex;
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
                            boardInteractions.OpenItem(item, openAmount, Item.State.Crate);
                            Select(item);
                            boardSelection.Select(BoardSelection.SelectType.Both, false);
                        }
                        break;
                    case ActionType.Unlock:
                        if (gameData.gems < unlockAmount)
                        {
                            shopMenu.Open("Gems");
                        }
                        else
                        {
                            boardInteractions.OpenItem(item, unlockAmount, Item.State.Locker);
                            Select(item);
                            boardSelection.Select(BoardSelection.SelectType.Both, false);
                        }
                        break;
                    case ActionType.UnlockChest:
                        boardInteractions.UnlockChest(item);
                        Select(item);
                        boardSelection.Select(BoardSelection.SelectType.Both, false);
                        break;
                    case ActionType.Pop:
                        if (gameData.gems < popAmount)
                        {
                            shopMenu.Open("Gems");
                        }
                        else
                        {
                            boardInteractions.OpenItem(item, unlockAmount, Item.State.Bubble);
                            Select(item);
                            boardSelection.Select(BoardSelection.SelectType.Both, false);
                        }
                        break;
                    case ActionType.Sell:
                        // Confirm selling item if it's a generator or at least level 10
                        if (item.type == Item.Type.Gen)
                        {
                            confirmMenu.Open("sell_gen", () =>
                            {
                                boardInteractions.RemoveItem(item, sellAmount);
                                Unselect(true);
                                boardSelection.UnselectAlt();
                                SetUndoButton(true);
                            });
                        }
                        else
                        {
                            boardInteractions.RemoveItem(item, sellAmount);
                            Unselect(true);
                            boardSelection.UnselectAlt();
                            SetUndoButton(true);
                        }
                        break;
                    case ActionType.Remove:
                        boardInteractions.RemoveItem(item);
                        Unselect(true);
                        boardSelection.UnselectAlt();
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
                            boardSelection.Select(BoardSelection.SelectType.Both, false);
                            timeOn = false;
                        }
                        break;
                    case ActionType.Pop:
                        adsManager.WatchAd(AdsManager.AdType.Bubble, (int reward) =>
                        {
                            boardInteractions.OpenItem(item, unlockAmount, Item.State.Bubble);
                            Select(item);
                            boardSelection.Select(BoardSelection.SelectType.Both, false);
                            timeOn = false;
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

        void CalcSellPrice(int level)
        {
            sellAmount = gameData.valuesData.sellPriceMultiplier[level - 1];
        }

        public void SetTutorialData(string stepId)
        {
            if (tutorialManager != null && LOCALE.TryCheckIfExists("tutorial_info_merge_" + stepId, out string foundText))
            {
                infoBox.AddToClassList("info_item_avatar");

                infoData.text = foundText;

                infoItem.style.backgroundImage = new StyleBackground(avatarSprite);
            }
        }

        public void ResetTutorialData()
        {
            if (tutorialManager != null)
            {
                infoBox.RemoveFromClassList("info_item_avatar");

                infoData.text = "";

                infoItem.style.backgroundImage = null;
            }
        }
    }
}