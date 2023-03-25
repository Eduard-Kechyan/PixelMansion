using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardInteractions : MonoBehaviour
{
    public bool interactionsEnabled = true; // Is the board currently interactable
    public float moveSpeed = 16f; // How fast should the item move
    public float scaleSpeed = 8f; // How fast should the item resize
    public float touchThreshold = 20f; // How far can the finger be moved before starting to drag
    public float radius = 0.5f;
    public float radiusAlt = 0.5f;
    public bool isDragging = false; // Are we currently dragging
    public bool isSelected = false; // Have we currently selected something
    public Item currentItem;

    // Undo
    [HideInInspector]
    public bool canUndo = false;
    private Item undoItem;
    Types.Board undoBoardItem = new Types.Board();
    private GameObject undoTile;
    private Vector2 undoScale;
    private int sellUndoAmount = 0;

    public GameObject boardTiles;
    public UIDocument uiDoc;
    private DataManager dataManager;
    private ItemHandler itemHandler;

    private SoundManager soundManager;
    private bool touchBeganOutsideItem = false;
    private VisualElement root;
    private VisualElement dragOverlay;
    private GameObject initialTile;
    private SelectionManager selectionManager;
    private InitializeBoard initializeBoard;
    private Action callback;
    private bool previousInteractionsEnabled = true;

    // Positions
    private Vector3 worldPos;
    private Vector2 initialPos;
    private Vector2 initialTouchPos = Vector2.zero;

    // Board item data
    Types.Board boardItem = new Types.Board();

    private void Start()
    {
        // Cache root and dragOverlay
        root = uiDoc.rootVisualElement;
        dragOverlay = root.Q<VisualElement>("DragOverlay");

        // Cache managers
        soundManager = SoundManager.Instance;
        dataManager = DataManager.Instance;
        itemHandler = dataManager.GetComponent<ItemHandler>();

        // Cache selectionManager
        selectionManager = GetComponent<SelectionManager>();

        // Cache initializeBoard
        initializeBoard = GetComponent<InitializeBoard>();

        // Drag overlay shouldn't be pickable
        dragOverlay.pickingMode = PickingMode.Ignore;
    }

    void Update()
    {
        // Check if interactions are enabled
        if (interactionsEnabled)
        {
            // Check for input
            if (Input.touchCount == 1)
            {
                // Get the current touch
                Touch touch = Input.GetTouch(0);

                // Convert current touch position to world position
                worldPos = Camera.main.ScreenToWorldPoint(touch.position);

                // See when the touch has been started
                if (touch.phase == TouchPhase.Began)
                {
                    // Get the initial touch position for comparing
                    initialTouchPos = touch.position;

                    RaycastHit2D hit = Physics2D.Raycast(
                        Camera.main.ScreenToWorldPoint(initialTouchPos),
                        Vector2.zero,
                        Mathf.Infinity,
                        LayerMask.GetMask("Tile")
                    );

                    // Check if the initial touch position is from an item or from outside
                    if (hit || hit.collider != null)
                    {
                        if (hit.transform.childCount == 0)
                        {
                            touchBeganOutsideItem = true;
                        }
                    }
                }

                // See when the touch has been moved
                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    // Comapare the current position to the initial position
                    Vector2 diff = touch.position - initialTouchPos;

                    if (
                        diff.x > touchThreshold
                        || diff.x < -touchThreshold
                        || diff.y > touchThreshold
                        || diff.y < -touchThreshold
                    )
                    {
                        if (isDragging)
                        {
                            if (currentItem != null && currentItem.isSelected)
                            {
                                isSelected = false;
                                selectionManager.Unselect("both");
                                selectionManager.Select("only");
                            }

                            // Drag the item around
                            Drag();
                        }
                        else
                        {
                            // Detect draggable item if the touch began from an item
                            if (!touchBeganOutsideItem)
                            {
                                DetectDraggableItem(
                                    Camera.main.ScreenToWorldPoint(initialTouchPos)
                                );
                            }
                        }
                    }
                }

                // See when the touch has been ended
                if (touch.phase == TouchPhase.Ended)
                {
                    // Reset this check
                    touchBeganOutsideItem = false;

                    if (isDragging)
                    {
                        // Drop the item
                        Drop();
                    }
                    else
                    {
                        // Select the item
                        selectionManager.SelectItem(worldPos);
                    }
                }
            }
        }
    }

    public void EnableInteractions()
    {
        if (!interactionsEnabled && previousInteractionsEnabled)
        {
            interactionsEnabled = true;
            previousInteractionsEnabled = false;
        }
    }

    public void DisableInteractions()
    {
        if (interactionsEnabled)
        {
            interactionsEnabled = false;
            previousInteractionsEnabled = true;
        }
    }

    //////// DRAGGING ////////

    void DetectDraggableItem(Vector3 initPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            initPos,
            Vector2.zero,
            Mathf.Infinity,
            LayerMask.GetMask("Item")
        );

        // Check if raycast hit anything
        if (hit.collider != null)
        {
            Item item = hit.transform.gameObject.GetComponent<Item>();

            // Check if gameobject is a item and isn't empty and if it isn't a crate or locked
            if (item != null && item.state != Types.State.Crate && item.state != Types.State.Locker)
            {
                if (currentItem != null && currentItem.isSelected)
                {
                    selectionManager.Unselect("both");
                }

                // Set current item
                currentItem = item;

                // Show selected item's info in the info box
                selectionManager.Select("info");

                // Start dragging the item
                StartDragging();
            }
        }
    }

    void StartDragging()
    {
        // Starting to drag
        isDragging = true;
        currentItem.Dragging();

        // Save currentItem's parent
        initialTile = currentItem.transform.parent.gameObject;

        // Remove currentItem's parent
        currentItem.transform.parent = null;

        // Set the drag overlay's background image to the dragging item's sprite
        dragOverlay.style.backgroundImage = new StyleBackground(currentItem.sprite);

        // Set the item's layer to Dragging
        currentItem.gameObject.layer = LayerMask.NameToLayer("ItemDragging");

        // Set the item's initial position
        initialPos = new Vector2(
            currentItem.transform.position.x,
            currentItem.transform.position.y
        );

        GetBoardData();
    }

    void Drag()
    {
        // Set the dragging item's new position
        currentItem.transform.position = new Vector3(
            worldPos.x,
            worldPos.y,
            currentItem.transform.position.z
        );

        // Get position on the UI from the scene
        Vector2 newUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
            root.panel,
            currentItem.transform.position,
            Camera.main
        );

        // Move the drag overlay
        dragOverlay.style.left = newUIPos.x - (dragOverlay.resolvedStyle.width / 2);
        dragOverlay.style.top = newUIPos.y - (dragOverlay.resolvedStyle.width / 2);

        // Show the drag overlay in UI if hidden
        if (dragOverlay.resolvedStyle.visibility == Visibility.Hidden)
        {
            dragOverlay.style.visibility = Visibility.Visible;
        }
    }

    void Drop()
    {
        // Stopping dragging
        isDragging = false;
        currentItem.Dropped();

        // Hide the drag overlay in UI
        dragOverlay.style.visibility = Visibility.Hidden;

        // Check what needs to happen when we drop the item
        CheckItemDropAction();
    }

    void CheckItemDropAction()
    {
        //////// Check for the storage button ////////

        // Get dragged position on the UI
        Vector2 newUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
            root.panel,
            currentItem.transform.position,
            Camera.main
        );

        var pickedElement = root.panel.Pick(newUIPos);

        // Check if the element under the position is the one we need
        if (pickedElement != null && pickedElement.name == "StorageButton")
        {
            CheckInventoryButton();

            return;
        }

        //////// Check for the tile ////////

        RaycastHit2D tileHit = Physics2D.Raycast(
            worldPos,
            Vector2.zero,
            10,
            LayerMask.GetMask("Tile")
        );

        // Check if a tile is found
        if (tileHit.collider != null)
        {
            GameObject tile = tileHit.transform.gameObject;

            // Check if the tile is empty
            if (tile.transform.childCount == 0)
            {
                // No child found, move the item to this tile
                MoveItem(tile);

                return;
            }
        }

        //////// Check for the item ////////

        RaycastHit2D itemHit = Physics2D.Raycast(
            worldPos,
            Vector2.zero,
            10,
            LayerMask.GetMask("Item")
        );

        // Check if an item is found
        if (itemHit.collider != null)
        {
            Item otherItem = itemHit.transform.gameObject.GetComponent<Item>();

            // Check if it can be dragged
            if (otherItem != null)
            {
                // Check if objects should be merged of swapped
                if (
                    otherItem.group == currentItem.group
                    && otherItem.level == currentItem.level
                    && !otherItem.isMaxLavel
                )
                {
                    if (otherItem.state != Types.State.Crate)
                    {
                        Merge(otherItem);

                        return;
                    }
                }
                else
                {
                    if (
                        otherItem.state != Types.State.Crate
                        && otherItem.state != Types.State.Locker
                    )
                    {
                        Swap(otherItem);

                        return;
                    }
                }
            }
        }

        //////// If there is nothing to do, move the item back to it's initial position ////////

        MoveBack();
    }

    void MoveItem(GameObject tile)
    {
        // Move the item to the tile
        currentItem.MoveToPos(tile.transform.position, moveSpeed * 10);

        // Set tile as item's new parent
        currentItem.transform.parent = tile.transform;

        // Set the item's layer back to Item
        currentItem.gameObject.layer = LayerMask.NameToLayer("Item");

        // Set board data
        SetBoardData(initialTile, tile);

        // Select item
        selectionManager.Select("both");

        CancelUndo();
    }

    void Merge(Item otherItem)
    {
        // Save the other item's data in memory
        GameObject otherTile = otherItem.transform.parent.gameObject;
        Vector3 initialScale = otherItem.transform.localScale;
        Vector3 initialPos = otherItem.transform.position;

        // Calc new item name
        Item item = otherItem;

        // Get item data for the next item
        Types.Type type = otherItem.type;
        Types.Group group = otherItem.group;
        Types.GenGroup genGroup = otherItem.genGroup;
        string spriteName = otherItem.nextSpriteName;

        currentItem.transform.parent = null;
        otherItem.transform.parent = null;

        Item tempCurrentItem = currentItem;
        currentItem = null;

        // Reduce the two items
        tempCurrentItem.ScaleToSize(Vector2.zero, scaleSpeed, true);
        otherItem.GetComponent<Item>().ScaleToSize(Vector2.zero, scaleSpeed, true);

        // Get the prefab that's one level heigher
        if (type == Types.Type.Default)
        {
            currentItem = itemHandler.CreateItem(
                otherTile,
                initializeBoard.tileSize,
                group,
                spriteName
            );
        }
        else if (type == Types.Type.Gen)
        {
            currentItem = itemHandler.CreateGenerator(
                otherTile,
                initializeBoard.tileSize,
                genGroup,
                spriteName
            );
        }

        // Play merge audio
        soundManager.PlaySFX("Merge");

        // Unlock the item
        dataManager.UnlockItem(currentItem.sprite.name);

        // Set the new item's layer to ItemBusy
        currentItem.gameObject.layer = LayerMask.NameToLayer("ItemBusy");

        // Reduce and then enlarge the new item
        currentItem.transform.localScale = new Vector3(0, 0, currentItem.transform.localScale.z);

        MergeBoardData(initialTile, otherTile, currentItem);

        callback += MergeBackCallback;

        currentItem.ScaleToSize(initialScale, scaleSpeed, false, callback);

        CancelUndo();

        CheckForCrate(otherTile);
    }

    void MergeBackCallback()
    {
        // Select item
        selectionManager.Select("both", false);
    }

    void Swap(Item otherItem)
    {
        // Save the other item's data in memory
        GameObject otherTile = otherItem.transform.parent.gameObject;

        // Set the current item's pos and parent
        currentItem.MoveToPos(otherTile.transform.position, moveSpeed);
        currentItem.transform.parent = otherTile.transform;

        // Set the other item's pos and parent
        otherItem.MoveToPos(initialTile.transform.position, moveSpeed);
        otherItem.transform.parent = initialTile.transform;

        // Set the item's layer back to Item
        currentItem.gameObject.layer = LayerMask.NameToLayer("Item");

        // Set board data
        SwapBoardData(initialTile, otherTile);

        // Select item
        selectionManager.Select("both");

        CancelUndo();
    }

    void CheckInventoryButton()
    {
        // TODO - Check if there is room in the inventory and if so, store the item in it, or else, move the item back to it's intialPosition
        Debug.Log("Checking inventory!"); // TODO - Remove this
        MoveBack();
    }

    void MoveBack()
    {
        callback += MoveBackCallback;

        // Reset item to its initial position
        currentItem.GetComponent<Item>().MoveToPos(initialPos, moveSpeed, callback);

        currentItem.transform.parent = initialTile.transform;

        // Set the item's layer back to Item
        currentItem.gameObject.layer = LayerMask.NameToLayer("Item");

        // Set board data
        SetBoardData(initialTile, initialTile);
    }

    void MoveBackCallback()
    {
        // Select item
        selectionManager.Select("both");
    }

    void CheckForCrate(GameObject tile)
    {
        //Vector2Int loc = GetBoardLocation(tile);

        /*if (loc.x == 0)
        {
            if (GameData.boardData[1, 0].state == Types.State.Crate)
            {
                GameData.boardData[1, 0].state = Types.State.Locker;

                FindItemAndOpenCrate(1);
            }
        }*/

        // Check for the first
        /*if (tileOrder == 0)
        {
            if (GameData.boardData[1, 0].state == Types.State.Crate)
            {
                GameData.boardData[1, 0].state = Types.State.Locker;

                FindItemAndOpenCrate(1);
            }
        }*/

        // Check for the last
        /*if (tileOrder == GameData.ITEM_COUNT)
        {
            if (GameData.boardData[GameData.ITEM_COUNT - 1, 0].state == Types.State.Crate)
            {
                GameData.boardData[GameData.ITEM_COUNT - 1, 0].state = Types.State.Locker;

                FindItemAndOpenCrate(GameData.ITEM_COUNT - 1);
            }
        }*/

        // Check for in between verticaly
        /* if (tileOrder >= 1 && tileOrder <= GameData.ITEM_COUNT - 1)
         {
             if (GameData.boardData[tileOrder + 1, 0].state == Types.State.Crate)
             {
                 GameData.boardData[tileOrder + 1, 0].state = Types.State.Locker;
 
                 FindItemAndOpenCrate(tileOrder - 1);
             }
 
             if (GameData.boardData[tileOrder - 1, 0].state == Types.State.Crate)
             {
                 GameData.boardData[tileOrder - 1, 0].state = Types.State.Locker;
 
                 FindItemAndOpenCrate(tileOrder - 1);
             }
         }*/

        // Check for in between horizontaly
        /* if (tileOrder >= GameData.HEIGHT && tileOrder <= GameData.ITEM_COUNT - GameData.HEIGHT)
         {
             if (GameData.boardData[tileOrder - GameData.HEIGHT, 0].state == Types.State.Crate)
             {
                 GameData.boardData[tileOrder - GameData.HEIGHT, 0].state = Types.State.Locker;
 
                 FindItemAndOpenCrate(tileOrder - GameData.HEIGHT);
             }
 
             if (GameData.boardData[tileOrder + GameData.HEIGHT, 0].state == Types.State.Crate)
             {
                 GameData.boardData[tileOrder + GameData.HEIGHT, 0].state = Types.State.Locker;
 
                 FindItemAndOpenCrate(tileOrder + GameData.HEIGHT);
             }
         }*/

        ///////////////////////////
    }

    //////// OTHER ////////

    void GetBoardData()
    {
        Vector2Int loc = GetBoardLocation(initialTile);

        // Get item
        boardItem = GameData.boardData[loc.x, loc.y];
        Debug.Log(GameData.boardData[loc.x, loc.y].order);
    }

    void SetBoardData(GameObject oldTile, GameObject newTile)
    {
        Vector2Int loc = GetBoardLocation(oldTile);

        // Clear old item
        Types.Board tempItem = GameData.boardData[loc.x, loc.y];
        Debug.Log(tempItem.order);

        GameData.boardData[loc.x, loc.y] = new Types.Board
        {
            order = tempItem.order,
            loc = tempItem.loc
        };

        // Set new item
        loc = GetBoardLocation(newTile);

        GameData.boardData[loc.x, loc.y] = boardItem;

        // Save the board to disk
        dataManager.SaveBoard();
    }

    void SwapBoardData(GameObject oldTile, GameObject newTile)
    {
        // Get tile orders

        Vector2Int oldLoc = GetBoardLocation(oldTile);
        Vector2Int newLoc = GetBoardLocation(newTile);

        // Save items
        Types.Board oldItem = GameData.boardData[oldLoc.x, oldLoc.y];
        Types.Board newItem = GameData.boardData[newLoc.x, newLoc.y];

        // Clear items
        Types.Board tempItem = GameData.boardData[oldLoc.x, oldLoc.y];
        Types.Board tempItemB = GameData.boardData[newLoc.x, newLoc.y];

        GameData.boardData[oldLoc.x, oldLoc.y] = new Types.Board
        {
            order = tempItem.order,
            loc = tempItem.loc
        };
        GameData.boardData[newLoc.x, newLoc.y] = new Types.Board
        {
            order = tempItemB.order,
            loc = tempItemB.loc
        };

        // Set items
        GameData.boardData[oldLoc.x, oldLoc.y] = newItem;
        GameData.boardData[newLoc.x, newLoc.y] = oldItem;

        // Save the board to disk
        dataManager.SaveBoard();
    }

    void MergeBoardData(GameObject oldTile, GameObject newTile, Item newItem)
    {
        // Clear old items
        Vector2Int loc = GetBoardLocation(oldTile);

        Types.Board tempItem = GameData.boardData[loc.x, loc.y];

        GameData.boardData[loc.x, loc.y] = new Types.Board
        {
            order = tempItem.order,
            loc = tempItem.loc
        };

        // Set new item
        loc = GetBoardLocation(newTile);
        GameData.boardData[loc.x, loc.y] = new Types.Board
        {
            sprite = newItem.sprite,
            group = newItem.group,
            state = newItem.state,
            crate = int.Parse(
                newItem.crateSprite.name[(newItem.crateSprite.name.LastIndexOf('e') + 1)..]
            )
        };

        // Save the board to disk
        dataManager.SaveBoard();
    }

    Vector2Int GetBoardLocation(GameObject tile)
    {
        Vector2Int loc = new Vector2Int(0, 0);

        int order = int.Parse(tile.gameObject.name[(tile.gameObject.name.LastIndexOf('e') + 1)..]);

        /* Debug.Log("_________________________________________");
         Debug.Log("_________________________________________");
         Debug.Log("_________________________________________");*/

        foreach (Types.Board boardItem in GameData.boardData)
        {
            //Debug.Log(boardItem.order);
            if (boardItem.order == order)
            {
                loc = new Vector2Int(boardItem.loc.x, boardItem.loc.y);
                break;
            }
        }

        return loc;
    }

    void FindItemAndOpenCrate(int order)
    {
        GameObject foundItem = initializeBoard.boardTiles.transform
            .GetChild(order)
            .GetChild(0)
            .gameObject;

        foundItem.GetComponent<Item>().OpenCrate();
    }

    //////// INFO ACTION ////////

    public void OpenItem(Item item, int amount, bool open)
    {
        if (item.name == currentItem.name)
        {
            if (GameData.UpdateGems(-amount))
            {
                if (open)
                {
                    currentItem.OpenCrate();
                    OpenCrateCallback(currentItem);
                }
                else
                {
                    currentItem.UnlockLock();
                    OpenLockCallback(currentItem);
                }
            }
        }
    }

    void OpenCrateCallback(Item item)
    {
        GameObject itemTile = item.transform.parent.gameObject;

        Vector2Int loc = GetBoardLocation(itemTile);

        if (GameData.boardData[loc.x, loc.y].state == Types.State.Crate)
        {
            GameData.boardData[loc.x, loc.y].state = Types.State.Locker;

            dataManager.SaveBoard();
        }
    }

    void OpenLockCallback(Item item)
    {
        GameObject itemTile = item.transform.parent.gameObject;

        Vector2Int loc = GetBoardLocation(itemTile);

        if (GameData.boardData[loc.x, loc.y].state == Types.State.Locker)
        {
            GameData.boardData[loc.x, loc.y].state = Types.State.Default;

            dataManager.SaveBoard();
        }
    }

    public void RemoveItem(Item item, int amount = 0)
    {
        if (item.name == currentItem.name)
        {
            canUndo = true;

            if (amount > 0)
            {
                GameData.UpdateGold(amount);

                sellUndoAmount = amount;
            }

            undoItem = currentItem;
            undoTile = undoItem.transform.parent.gameObject;
            undoScale = undoItem.transform.localScale;

            Vector2Int loc = GetBoardLocation(undoTile);

            undoBoardItem = GameData.boardData[loc.x, loc.y];

            undoItem.transform.parent = null;

            undoItem.ScaleToSize(Vector2.zero, scaleSpeed, false);

            GameData.boardData[loc.x, loc.y] = new Types.Board();
        }
    }

    public void UndoLastStep()
    {
        if (canUndo)
        {
            currentItem = undoItem;

            currentItem.transform.parent = undoTile.transform;

            undoItem.ScaleToSize(undoScale, scaleSpeed, false);

            Vector2Int loc = GetBoardLocation(undoTile);

            GameData.boardData[loc.x, loc.y] = undoBoardItem;

            selectionManager.SelectItemAfterUndo();

            if (sellUndoAmount > 0)
            {
                GameData.UpdateGold(-sellUndoAmount);
                sellUndoAmount = 0;
            }

            CancelUndo();
        }
    }

    void CancelUndo()
    {
        canUndo = false;

        undoItem = null;
        undoBoardItem = null;
        undoTile = null;
        undoScale = Vector2.zero;
    }
}
