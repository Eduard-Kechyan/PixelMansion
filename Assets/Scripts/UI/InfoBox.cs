using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class InfoBox : MonoBehaviour
{
    public Sprite goldValue;
    public Sprite gemValue;
    public Color redColor;
    public Color greenColor;
    public Color blueColor;
    public Color yellowColor;
    public BoardInteractions boardInteractions;
    public SelectionManager selectionManager;
    public int openAmount = 10;
    public int unlockAmount = 5;
    public int sellAmount = 5;

    private Item item;
    private VisualElement root;
    private VisualElement infoItem;
    private VisualElement infoItemLocked;
    private Button infoButton;
    private Button infoActionButton;
    private VisualElement infoActionValue;
    private Label infoName;
    private Label infoData;
    private Sprite sprite;
    private string textToSet = "";

    private InfoMenu infoMenu;

    private I18n LOCALE = I18n.Instance;

    public enum ActionType
    {
        Open,
        Unlock,
        Sell,
        Remove,
        Undo
    };

    private ActionType actionType;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        infoItem = root.Q<VisualElement>("InfoItem");
        infoItemLocked = root.Q<VisualElement>("InfoItemLocked");

        infoButton = root.Q<Button>("InfoButton");

        infoActionButton = root.Q<Button>("InfoActionButton");

        infoActionValue = infoActionButton.Q<VisualElement>("Value");

        infoName = root.Q<Label>("InfoName");
        infoData = root.Q<Label>("InfoData");

        infoMenu = MenuManager.Instance.GetComponent<InfoMenu>();

        // Initiate info box
        infoName.text = "";
        infoData.text = LOCALE.Get("info_box_default");

        CheckForTaps();
    }

    void CheckForTaps()
    {
        //Open info menu
        infoButton.clickable.clicked += () => infoMenu.Open(item);

        infoActionButton.clickable.clicked += () => InfoAction();
    }

    public void Select(Item newItem)
    {
        item = newItem;

        switch (item.state)
        {
            case Types.State.Crate: //////// CRATE ////////
                infoName.text = LOCALE.Get("info_box_crate");

                textToSet = LOCALE.Get("info_box_crate_text");

                sprite = item.transform.Find("Crate").GetComponent<SpriteRenderer>().sprite;

                actionType = ActionType.Open;
                break;
            case Types.State.Locker: //////// LOCKER ////////
                infoName.text = item.itemName + " " + LOCALE.Get("info_box_locker");

                textToSet = LOCALE.Get("info_box_locker_text");

                sprite = item.transform.Find("Item").GetComponent<SpriteRenderer>().sprite;

                infoItemLocked.style.display = DisplayStyle.Flex;

                actionType = ActionType.Unlock;
                break;
            default: //////// ITEM ////////
                infoName.text = item.itemLevelName;

                sprite = item.transform.Find("Item").GetComponent<SpriteRenderer>().sprite;

                infoItemLocked.style.display = DisplayStyle.None;

                switch (item.type)
                {
                    case Types.Type.Gen:
                        if (item.isMaxLavel)
                        {
                            textToSet = LOCALE.Get("info_box_gen_max");
                        }
                        else
                        {
                            textToSet = LOCALE.Get("info_box_gen", item.nextName);
                        }
                        break;
                    default:
                        if (item.isMaxLavel)
                        {
                            textToSet = LOCALE.Get("info_box_item_max");
                        }
                        else
                        {
                            textToSet = LOCALE.Get("info_box_item", item.nextName);
                        }
                        break;
                }

                if (item.level > 3 || item.isMaxLavel)
                {
                    actionType = ActionType.Sell;
                }
                else
                {
                    actionType = ActionType.Remove;
                }

                infoButton.style.display = DisplayStyle.Flex;
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
                boardInteractions.OpenItem(item, openAmount, true);
                Select(item);
                selectionManager.Select("both", false);
                break;
            case ActionType.Unlock:
                boardInteractions.OpenItem(item, unlockAmount, false);
                Select(item);
                selectionManager.Select("both", false);
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
}
