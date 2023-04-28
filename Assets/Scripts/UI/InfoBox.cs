using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class InfoBox : MonoBehaviour
{
    // Variables
    public Sprite goldValue;
    public Sprite gemValue;
    public Color redColor;
    public Color greenColor;
    public Color greyColor;
    public Color blueColor;
    public Color yellowColor;
    public BoardInteractions boardInteractions;
    public SelectionManager selectionManager;
    public int openAmount = 10;
    public int unlockAmount = 5;
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
        Sell,
        Remove,
        Undo,
        None
    };

    private ActionType actionType;

    void Start()
    {
        // Cache
        infoMenu =  GameRefs.Instance.infoMenu;
        shopMenu =  GameRefs.Instance.shopMenu;

        // Cache instances
        gameData = GameData.Instance;

        // UI
        root = GetComponent<UIDocument>().rootVisualElement;

        infoItem = root.Q<VisualElement>("InfoItem");
        infoItemLocked = root.Q<VisualElement>("InfoItemLocked");

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
            default: //////// ITEM ////////
                infoName.text = item.itemLevelName;

                sprite = item.itemChild.GetComponent<SpriteRenderer>().sprite;

                infoItemLocked.style.display = DisplayStyle.None;

                infoButton.style.display = DisplayStyle.Flex;

                if (item.level > 3 || item.isMaxLavel)
                {
                    actionType = ActionType.Sell;
                }
                else if (item.type == Types.Type.Coll)
                {
                    actionType = ActionType.None;
                    infoButton.style.display = DisplayStyle.None;
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
                infoActionButton.AddToClassList("info_box_button_value");
                infoActionButton.style.display = DisplayStyle.Flex;
                infoActionButton.style.backgroundColor = blueColor;

                infoActionValue.style.backgroundImage = new StyleBackground(gemValue);
                infoActionValue.style.display = DisplayStyle.Flex;
                break;
            case ActionType.Unlock:
                infoActionButton.text = unlockAmount.ToString();
                infoActionButton.AddToClassList("info_box_button_value");
                infoActionButton.style.display = DisplayStyle.Flex;
                infoActionButton.style.backgroundColor = blueColor;

                infoActionValue.style.backgroundImage = new StyleBackground(gemValue);
                infoActionValue.style.display = DisplayStyle.Flex;
                break;
            case ActionType.Sell:
                infoActionButton.text = sellAmount.ToString();
                infoActionButton.AddToClassList("info_box_button_value");
                infoActionButton.style.display = DisplayStyle.Flex;
                infoActionButton.style.backgroundColor = greenColor;

                infoActionValue.style.backgroundImage = new StyleBackground(goldValue);
                infoActionValue.style.display = DisplayStyle.Flex;
                break;
            case ActionType.Remove:
                infoActionButton.text = LOCALE.Get("info_box_button_remove");
                infoActionButton.RemoveFromClassList("info_box_button_value");
                infoActionButton.style.display = DisplayStyle.Flex;
                infoActionButton.style.backgroundColor = redColor;

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

            infoActionButton.RemoveFromClassList("info_box_button_value");

            infoActionButton.RemoveFromClassList("info_box_button_disabled");

            infoActionValue.style.display = DisplayStyle.None;

            infoData.text = LOCALE.Get("info_box_default");
        }
    }

    public string GetItemData(Item newItem)
    {
        switch (newItem.state)
        {
            case Types.State.Crate:
                textToSet = LOCALE.Get("info_box_crate_text");
                break;
            case Types.State.Locker:

                textToSet = LOCALE.Get("info_box_locker_text");
                break;
            default:
                switch (newItem.type)
                {
                    case Types.Type.Gen:
                        if (newItem.isMaxLavel)
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
                            if (item.isMaxLavel)
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
                            if (item.isMaxLavel)
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
                    default:
                        if (newItem.isMaxLavel)
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
                    boardInteractions.OpenItem(item, openAmount, true);
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
                    boardInteractions.OpenItem(item, unlockAmount, false);
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
        infoActionButton.style.backgroundColor = yellowColor;

        if (sold)
        {
            infoActionButton.AddToClassList("info_box_button_value");

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
            default:
                Debug.Log("Wrong collectables group");
                break;
        }

        return multipliedValue;
    }
}
