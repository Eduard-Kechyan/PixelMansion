using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class TestBoardInteractions : MonoBehaviour
    {
        /* // Variables
         public bool interactionsEnabled = true; // Is the board currently interactable
         public float moveSpeed = 16f; // How fast should the item move
         public float scaleSpeed = 8f; // How fast should the item resize
         public float touchThreshold = 20f; // How far can the finger be moved before starting to drag
         public float radius = 0.5f;
         public float radiusAlt = 0.5f;
         public float crateBreakSpeed = 0.05f;
         public bool isDragging = false; // Are we currently dragging
         public bool isSelected = false; // Have we currently selected something
         public Item currentItem;

         [HideInInspector]
         public GameObject tempTile;

         // Undo
         [HideInInspector]
         public bool canUndo = false;
         private Item undoItem;
         private BoardManager.Tile undoTileItem = new();
         private GameObject undoTile;
         private Vector2 undoScale;
         private int sellUndoAmount = 0;

         private GameObject initialTile;
         private bool touchBeganOutsideItem = false;
         private bool previousInteractionsEnabled = true;
         private Action callback;

         // References
         private GameRefs gameRefs;
         private BoardSelection boardSelection;
         private TestBoardManager boardManager;
         private TestBoardInit boardInitialization;
         private BoardIndication boardIndication;
         private PopupManager popupManager;
         private InventoryMenu inventoryMenu;
         private DataManager dataManager;
         private GameData gameData;
         private ItemHandler itemHandler;
         private SoundManager soundManager;
         private I18n LOCALE;
         private ClockManager clockManager;
         private TimeManager timeManager;
         private PointerHandler pointerHandler;

         // UI
         private VisualElement root;
         private VisualElement dragOverlay;

         // Positions
         private Vector3 worldPos;
         private Vector2 initialPos;
         private Vector2 initialTouchPos = Vector2.zero;

         private void Start()
         {
             // Cache
             gameRefs = GameRefs.Instance;
             boardInitialization = GetComponent<TestBoardInit>();
             boardSelection = GetComponent<BoardSelection>();
             boardManager = GetComponent<TestBoardManager>();
             boardIndication = GetComponent<BoardIndication>();
             popupManager = PopupManager.Instance;
             inventoryMenu = gameRefs.inventoryMenu;
             soundManager = SoundManager.Instance;
             dataManager = DataManager.Instance;
             gameData = GameData.Instance;
             itemHandler = dataManager.GetComponent<ItemHandler>();
             LOCALE = I18n.Instance;
             clockManager = gameRefs.clockManager;
             timeManager = gameRefs.timeManager;
             pointerHandler = gameRefs.pointerHandler;

             // Cache root and dragOverlay
             root = gameRefs.mergeUIDoc.rootVisualElement;
             dragOverlay = root.Q<VisualElement>("DragOverlay");

             // Drag overlay shouldn't be pickable
             dragOverlay.pickingMode = PickingMode.Ignore;
         }

         void OnEnable()
         {
             // Subscribe to events
             DataManager.OnBoardSaveUndo += CancelUndo;
         }

         void OnDestroy()
         {
             // Unsubscribe from events
             DataManager.OnBoardSaveUndo -= CancelUndo;
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
                             else
                             {
                                 tempTile = hit.transform.gameObject;
                             }
                         }
                     }

                     // See when the touch has been moved
                     if (Input.GetTouch(0).phase == TouchPhase.Moved)
                     {
                         // Compare the current position to the initial position
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
                                     boardSelection.Unselect(BoardSelection.SelectType.Both);
                                     boardSelection.Select(BoardSelection.SelectType.Only);
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
                             boardSelection.SelectItem(worldPos);
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

         //// DRAGGING ////

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

                 // Check if game object is an item and isn't empty and if it isn't a crate or locked
                 if (item != null && item.state != Item.State.Crate && item.state != Item.State.Locker)
                 {
                     CancelUndo();

                     if (currentItem != null && currentItem.isSelected)
                     {
                         boardSelection.Unselect(BoardSelection.SelectType.Both);
                     }

                     // Set current item
                     currentItem = item;

                     // Show selected item's info in the info box
                     boardSelection.Select(BoardSelection.SelectType.Info);

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

             // Hide the clock
             if (currentItem.timerOn)
             {
                 clockManager.HideClock(currentItem.id);
             }

             // Set the item's initial position
             initialPos = new Vector2(
                 currentItem.transform.position.x,
                 currentItem.transform.position.y
             );

             pointerHandler.HidePointer();
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
             //// Check for the inventory button ////
             if (inventoryMenu.CheckInventoryButton(currentItem, initialTile, scaleSpeed))
             {
                 return;
             }

             //// Check for the tile ////

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

             //// Check for the item ////

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
                     bool sameGroup = false;

                     switch (otherItem.type)
                     {
                         case Item.Type.Item:
                             if (otherItem.group == currentItem.group)
                             {
                                 sameGroup = true;
                             }
                             break;
                         case Item.Type.Gen:
                             if (otherItem.genGroup == currentItem.genGroup)
                             {
                                 sameGroup = true;
                             }
                             break;
                         case Item.Type.Coll:
                             if (otherItem.collGroup == currentItem.collGroup)
                             {
                                 sameGroup = true;
                             }
                             break;
                         case Item.Type.Chest:
                             if (otherItem.chestGroup == currentItem.chestGroup)
                             {
                                 sameGroup = true;
                             }
                             break;
                         default:
                             // ERROR
                             ErrorManager.Instance.Throw(ErrorManager.ErrorType.Code, "BoardInteractions.cs -> CheckItemDropAction()", "Wrong type: " + otherItem.type);
                             break;
                     }

                     if (
                         sameGroup
                         && otherItem.type == currentItem.type
                         && otherItem.level == currentItem.level
                         && otherItem.state != Item.State.Bubble
                         && currentItem.state != Item.State.Bubble
                     )
                     {
                         if (!otherItem.isMaxLevel && otherItem.state != Item.State.Crate)
                         {
                             Merge(otherItem);

                             return;
                         }
                         else
                         {
                             // FIX - Check if we need this
                             popupManager.Pop(LOCALE.Get("pop_max_level"), otherItem.transform.position, SoundManager.SoundType.None, true);
                         }
                     }
                     else
                     {
                         if (
                             otherItem.state != Item.State.Crate
                             && otherItem.state != Item.State.Locker
                         )
                         {
                             Swap(otherItem);

                             return;
                         }
                     }
                 }
             }

             //// If there is nothing to do, move the item back to it's initial position ////

             MoveBack();

             pointerHandler.ShowPointer();
         }

         void MoveItem(GameObject tile)
         {
             // Move the item to the tile
             currentItem.MoveToPos(tile.transform.position, moveSpeed * 10);

             // Set tile as item's new parent
             currentItem.transform.parent = tile.transform;

             // Set the item's layer back to Item
             currentItem.gameObject.layer = LayerMask.NameToLayer("Item");

             // Move the clock
             if (currentItem.timerOn)
             {
                 clockManager.MoveClock(tile.transform.position, currentItem.id);
             }

             // Set board data
             boardManager.SwapBoardData(initialTile, tile);

             // Select item
             boardSelection.Select(BoardSelection.SelectType.Both);

             pointerHandler.ShowPointer();
         }

         void Merge(Item otherItem)
         {
             // Save the other item's data in memory
             GameObject otherTile = otherItem.transform.parent.gameObject;
             Vector3 initialScale = otherItem.transform.localScale;
             Vector3 initialPos = otherItem.transform.position;

             pointerHandler.CheckMerge(currentItem.sprite);

             // Calc new item name
             Item item = otherItem;
             BoardManager.Tile tileItem = new()
             {
                 sprite = item.sprite,
                 type = item.type,
                 group = item.group,
                 genGroup = item.genGroup,
                 collGroup = item.collGroup,
                 chestGroup = item.chestGroup,
                 gemPopped = item.gemPopped,
             };

             string spriteName = otherItem.nextSpriteName;
             bool isLocked = otherItem.state == Item.State.Locker;

             currentItem.transform.parent = null;
             otherItem.transform.parent = null;

             Item tempCurrentItem = currentItem;
             currentItem = null;

             // Reduce the two items
             tempCurrentItem.ScaleToSize(Vector2.zero, scaleSpeed, true);
             otherItem.GetComponent<Item>().ScaleToSize(Vector2.zero, scaleSpeed, true);

             // Get the prefab that's one level higher
             currentItem = itemHandler.CreateItem(
                 otherTile,
                 boardInitialization.tileSize,
                 tileItem,
                 spriteName
             );

             if (isLocked)
             {
                 // Play unlocking audio
                 soundManager.PlaySound(SoundManager.SoundType.UnlockLock);
             }
             else
             {
                 // Play merge audio
                 soundManager.PlaySound(SoundManager.SoundType.Merge);
             }

             if (currentItem.type != Item.Type.Coll)
             {
                 // Unlock the item
                 dataManager.UnlockItem(
                     currentItem.sprite.name,
                     currentItem.type,
                     currentItem.group,
                     currentItem.genGroup,
                     currentItem.collGroup,
                     currentItem.chestGroup
                 );
             }

             // Set the new item's layer to ItemBusy
             currentItem.gameObject.layer = LayerMask.NameToLayer("ItemBusy");

             // Reduce and then enlarge the new item
             currentItem.transform.localScale = new Vector3(0, 0, currentItem.transform.localScale.z);

             boardManager.MergeBoardData(initialTile, otherTile, currentItem);

             callback += MergeBackCallback;

             currentItem.ScaleToSize(initialScale, scaleSpeed, false, callback);

             boardManager.CheckForCrate(otherTile);

             boardManager.CheckForBubble(currentItem);
         }

         void MergeBackCallback()
         {
             // Select item
             boardSelection.Select(BoardSelection.SelectType.Both, false);
         }

         void Swap(Item otherItem)
         {
             // Save the other item's data in 
             GameObject otherTile = otherItem.transform.parent.gameObject;

             // Set the current item's pos and parent
             currentItem.MoveToPos(otherTile.transform.position, moveSpeed);
             currentItem.transform.parent = otherTile.transform;

             // Set the other item's pos and parent
             otherItem.MoveToPos(initialTile.transform.position, moveSpeed);
             otherItem.transform.parent = initialTile.transform;

             // Set the item's layer back to Item
             currentItem.gameObject.layer = LayerMask.NameToLayer("Item");

             // Move the clock
             if (currentItem.timerOn)
             {
                 clockManager.MoveClock(otherTile.transform.position, currentItem.id);
             }

             // Set board data
             boardManager.SwapBoardData(initialTile, otherTile);

             // Select item
             boardSelection.Select(BoardSelection.SelectType.Both);

             pointerHandler.ShowPointer();
         }

         void MoveBack()
         {
             callback += MoveBackCallback;

             // Show the drag overlay in UI
             dragOverlay.style.visibility = Visibility.Visible;

             // Reset item to its initial position
             currentItem.GetComponent<Item>().MoveToPos(initialPos, moveSpeed, callback);

             StartCoroutine(MoveBackOverlay(currentItem));

             currentItem.transform.parent = initialTile.transform;

             // Set the item's layer back to Item
             currentItem.gameObject.layer = LayerMask.NameToLayer("Item");
         }

         void MoveBackCallback()
         {
             // Select item
             boardSelection.Select(BoardSelection.SelectType.Both);
         }

         IEnumerator MoveBackOverlay(Item item)
         {
             while (item.isMoving)
             {
                 // Get position on the UI from the scene
                 Vector2 newUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
                     root.panel,
                     currentItem.transform.position,
                     Camera.main
                 );

                 // Move the drag overlay
                 dragOverlay.style.left = newUIPos.x - (dragOverlay.resolvedStyle.width / 2);
                 dragOverlay.style.top = newUIPos.y - (dragOverlay.resolvedStyle.width / 2);

                 yield return null;
             }

             // Show the drag overlay in UI
             if (!isDragging)
             {
                 dragOverlay.style.visibility = Visibility.Hidden;
             }

             yield return null;
         }

         //// INFO ACTION ////

         public void OpenItem(Item item, int amount, Item.State state)
         {
             if (item.name == currentItem.name)
             {
                 if (gameData.UpdateValue(-amount, Item.CollGroup.Gems, false, true))
                 {
                     switch (state)
                     {
                         case Item.State.Crate:
                             OpenCrateCallback(currentItem);

                             currentItem.OpenCrate(crateBreakSpeed);

                             // Play crate opening audio
                             soundManager.PlaySound(SoundManager.SoundType.OpenCrate);
                             break;

                         case Item.State.Bubble:
                             PopBubbleCallback(currentItem);

                             currentItem.PopBubble();

                             timeManager.RemoveTimer(currentItem.id);

                             // Play crate opening audio
                             soundManager.PlaySound(SoundManager.SoundType.None, "PopBubble" + UnityEngine.Random.Range(0, 3));
                             break;

                         default:
                             OpenLockCallback(currentItem);

                             currentItem.UnlockLock();

                             // Play unlocking audio
                             soundManager.PlaySound(SoundManager.SoundType.UnlockLock);
                             break;
                     }
                 }
             }
         }

         public void UnlockChest(Item item)
         {
             if (item.name == currentItem.name && item.type == Item.Type.Chest)
             {
                 // Play unlocking audio
                 soundManager.PlaySound(SoundManager.SoundType.UnlockLock);

                 int seconds = 1800; // 30 minutes

                 if (item.level >= 2 && item.level <= 4)
                 {
                     seconds *= item.level;
                 }

                 timeManager.AddTimer(TimeManager.TimerType.Item, NotificsManager.NotificationType.Chest, item.name, item.id, item.transform.position, seconds);
             }
         }

         public void SpeedUpItem(Item item, int amount)
         {
             if (item.name == currentItem.name)
             {
                 if (gameData.UpdateValue(-amount, Item.CollGroup.Gems, false, true))
                 {
                     // FIX - Remove switch statement

                     switch (currentItem.type)
                     {
                         case Item.Type.Chest:
                             // Play speeding up audio
                             soundManager.PlaySound(SoundManager.SoundType.Generate);

                             timeManager.RemoveTimer(item.id);
                             break;
                         case Item.Type.Gen:
                             // Play speeding up audio
                             soundManager.PlaySound(SoundManager.SoundType.Generate);

                             timeManager.RemoveTimer(item.id);
                             break;

                         default:
                             Debug.Log("????");
                             break;
                     }
                 }
             }
         }

         void OpenCrateCallback(Item item)
         {
             GameObject itemTile = item.transform.parent.gameObject;

             Vector2Int loc = boardManager.GetBoardLocation(0, itemTile);

             if (gameData.boardData[loc.x, loc.y].state == Item.State.Crate)
             {
                 gameData.boardData[loc.x, loc.y].state = Item.State.Locker;

                 dataManager.SaveBoard();
             }
         }

         public void OpenLockCallback(Item item)
         {
             GameObject itemTile = item.transform.parent.gameObject;

             Vector2Int loc = boardManager.GetBoardLocation(0, itemTile);

             if (gameData.boardData[loc.x, loc.y].state == Item.State.Locker)
             {
                 gameData.boardData[loc.x, loc.y].state = Item.State.Default;

                 dataManager.SaveBoard();
             }
         }

         void PopBubbleCallback(Item item)
         {
             GameObject itemTile = item.transform.parent.gameObject;

             Vector2Int loc = boardManager.GetBoardLocation(0, itemTile);

             if (gameData.boardData[loc.x, loc.y].state == Item.State.Bubble)
             {
                 gameData.boardData[loc.x, loc.y].state = Item.State.Default;

                 dataManager.SaveBoard();
             }
         }

         public void RemoveItem(Item item, int amount = 0, bool canUndoPre = true)
         {
             if (item.name == currentItem.name)
             {
                 boardIndication.CheckIfShouldStop(item);

                 if (canUndoPre)
                 {
                     CancelUndo();

                     canUndo = true;

                     if (amount > 0)
                     {
                         gameData.UpdateValue(amount, Item.CollGroup.Gold, false, true);

                         sellUndoAmount = amount;
                     }

                     undoItem = currentItem;
                     undoTile = undoItem.transform.parent.gameObject;
                     undoScale = undoItem.transform.localScale;

                     currentItem = null;

                     Vector2Int loc = boardManager.GetBoardLocation(0, undoTile);

                     undoTileItem = gameData.boardData[loc.x, loc.y];

                     undoItem.transform.parent = null;

                     undoItem.ScaleToSize(Vector2.zero, scaleSpeed, false);

                     gameData.boardData[loc.x, loc.y] = new BoardManager.Tile { order = undoTileItem.order };
                 }
                 else
                 {
                     Vector2Int loc = boardManager.GetBoardLocation(0, currentItem.transform.parent.gameObject);

                     BoardManager.Tile removeTileItem = gameData.boardData[loc.x, loc.y];

                     currentItem.transform.parent = null;

                     boardSelection.Unselect(BoardSelection.SelectType.Info);

                     currentItem.ScaleToSize(Vector2.zero, scaleSpeed, true);

                     currentItem = null;

                     gameData.boardData[loc.x, loc.y] = new BoardManager.Tile { order = removeTileItem.order };
                 }

                 dataManager.SaveBoard(true, false);
             }
         }

         public void UndoLastStep()
         {
             if (canUndo)
             {
                 currentItem = undoItem;
                 currentItem.transform.parent = undoTile.transform;

                 currentItem.ScaleToSize(undoScale, scaleSpeed, false, () =>
                 {
                     CancelUndo();
                 });

                 Vector2Int loc = boardManager.GetBoardLocation(0, undoTile);

                 gameData.boardData[loc.x, loc.y] = new BoardManager.Tile
                 {
                     sprite = undoTileItem.sprite,
                     group = undoTileItem.group,
                     state = undoTileItem.state,
                     crate = undoTileItem.crate,
                     order = undoTileItem.order,
                 };

                 boardSelection.SelectItemAfterUndo();

                 if (sellUndoAmount > 0)
                 {
                     gameData.UpdateValue(-sellUndoAmount, Item.CollGroup.Gold, false, true);
                     sellUndoAmount = 0;
                 }

                 dataManager.SaveBoard(true, false);
             }
         }

         public void CancelUndo(bool unselect = false)
         {
             if (canUndo)
             {
                 canUndo = false;

                 if (undoItem != null)
                 {
                     undoItem = null;
                     undoTileItem = null;
                     undoTile = null;
                     undoScale = Vector2.zero;
                 }

                 if (unselect)
                 {
                     boardSelection.UnselectUndo();
                 }
             }
         }*/
    }
}
