using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class BoardManager : MonoBehaviour
    {
        // Variables
        public float moveSpeed = 14f;
        public float altMoveSpeed = 14f;
        public float scaleSpeed = 8f;
        public float altScaleSpeed = 8f;
        public int experienceThreshold = 4;
        public float crateSoundTimeout = 1f;

        [ReadOnly]
        public bool boardSet = false;

        [Header("Bubbles")]
        public int minBubbleLevel = 6;
        public int bubbleChance = 30; // In %
        public int bubbleCount = 1000;
        public int bubblePopTimeout = 60;

        [HideInInspector]
        public GameObject boardTiles;

        private Coroutine crateCoroutine;
        private float crateTimeout = 0f;

        // References
        private BoardInitialization boardInitialization;
        private BoardInteractions interactions;
        private BoardSelection boardSelection;
        private DataManager dataManager;
        private SoundManager soundManager;
        private GameData gameData;
        private ItemHandler itemHandler;
        private ValuePop valuePop;
        private ErrorManager errorManager;
        private TimeManager timeManager;
        private TutorialManager tutorialManager;

        void Start()
        {
            // Cache
            boardTiles = transform.GetChild(0).gameObject;
            boardInitialization = GetComponent<BoardInitialization>();
            interactions = GetComponent<BoardInteractions>();
            boardSelection = GetComponent<BoardSelection>();
            dataManager = DataManager.Instance;
            soundManager = SoundManager.Instance;
            gameData = GameData.Instance;
            itemHandler = dataManager.GetComponent<ItemHandler>();
            valuePop = GameRefs.Instance.valuePop;
            errorManager = ErrorManager.Instance;
            timeManager = GameRefs.Instance.timeManager;
            tutorialManager = GameRefs.Instance.tutorialManager;
        }

        /////// GET BOARD DATA ////////

        // Get the item's order from the board by its location
        public int GetBoardOrder(int checkX, int checkY)
        {
            int count = 0;

            for (int x = 0; x < GameData.WIDTH; x++)
            {
                for (int y = 0; y < GameData.HEIGHT; y++)
                {
                    if (x == checkX && y == checkY)
                    {
                        return count;
                    }

                    count++;
                }
            }

            return 0;
        }

        // Get the item's location from the board by its tile's order
        public Vector2Int GetBoardLocation(int order, GameObject tile = null)
        {
            Vector2Int loc = new(0, 0);

            if (tile != null)
            {
                int index = tile.gameObject.name.LastIndexOf('e') + 1;
                if (index >= 0 && index < tile.gameObject.name.Length)
                {
                    if (int.TryParse(tile.gameObject.name.Substring(index), out int parsedOrder))
                    {
                        order = parsedOrder;
                    }
                    else
                    {
                        // ERROR
                        errorManager.ThrowWarning(Types.ErrorType.Code, "BoardManager", "Failed to parse order from tile name.");
                        return loc;
                    }
                }
                else
                {
                    // ERROR
                    errorManager.ThrowWarning(Types.ErrorType.Code, "BoardManager", "Invalid tile name format.");
                    return loc;
                }
            }

            int count = 0;

            for (int x = 0; x < gameData.boardData.GetLength(0); x++)
            {
                for (int y = 0; y < gameData.boardData.GetLength(1); y++)
                {
                    if (order == count)
                    {
                        {
                            loc = new Vector2Int(x, y);
                            return loc;
                        }
                    }

                    count++;
                }
            }

            return loc;
        }

        public int GetBoardOrderFromTile(GameObject tile)
        {
            Vector2Int loc = GetBoardLocation(0, tile);

            return GetBoardOrder(loc.x, loc.y);
        }

        public Vector2 GetTileItemPosByLoc(int checkX, int checkY)
        {
            int order = GetBoardOrder(checkX, checkY);

            return GetTileItemPosByOrder(order);
        }

        public Vector2 GetTileItemPosByOrder(int order)
        {
            Transform tileTransform = boardTiles.transform.GetChild(order);

            if (tileTransform != null)
            {
                return tileTransform.position;
            }
            else
            {
                // ERROR
                errorManager.ThrowWarning(Types.ErrorType.Code, "BoardManager", "Failed to retrieve tile transform.");
                return Vector2.zero;
            }
        }

        public Vector2 GetTileItemPosBySpriteName(string spriteName)
        {
            int count = 0;

            for (int x = 0; x < GameData.WIDTH; x++)
            {
                for (int y = 0; y < GameData.HEIGHT; y++)
                {
                    if (gameData.boardData[x, y].sprite != null && gameData.boardData[x, y].sprite.name == spriteName)
                    {
                        return GetTileItemPosByOrder(count);
                    }

                    count++;
                }
            }

            // ERROR
            errorManager.ThrowWarning(Types.ErrorType.Code, GetType().ToString(), "Board item with sprite name \'" + spriteName + "\' not found!");

            return default;
        }

        public Vector2 GetTileItemPosById(string id)
        {
            for (int x = 0; x < gameData.boardData.GetLength(0); x++)
            {
                for (int y = 0; y < gameData.boardData.GetLength(1); y++)
                {
                    if (gameData.boardData[x, y].id == id)
                    {
                        return GetTileItemPosByLoc(x, y);
                    }
                }
            }

            // ERROR
            errorManager.ThrowWarning(Types.ErrorType.Code, "BoardManager", "Item with ID " + id + " not found on the board.");
            return Vector2.zero;
        }

        /////// SET BOARD DATA ////////

        public void SwapBoardData(GameObject oldTile, GameObject newTile)
        {
            // Get tile locations
            Vector2Int oldLoc = GetBoardLocation(0, oldTile);
            Vector2Int newLoc = GetBoardLocation(0, newTile);

            // Save items
            Types.Tile oldItem = gameData.boardData[oldLoc.x, oldLoc.y];
            Types.Tile newItem = gameData.boardData[newLoc.x, newLoc.y];

            Types.Tile temp = gameData.boardData[oldLoc.x, oldLoc.y];
            gameData.boardData[oldLoc.x, oldLoc.y] = gameData.boardData[newLoc.x, newLoc.y];
            gameData.boardData[newLoc.x, newLoc.y] = temp;

            // Set items
            gameData.boardData[oldLoc.x, oldLoc.y] = new()
            {
                sprite = newItem.sprite,
                type = newItem.type,
                group = newItem.group,
                genGroup = newItem.genGroup,
                collGroup = newItem.collGroup,
                chestGroup = newItem.chestGroup,
                state = newItem.state,
                crate = newItem.crate,
                order = oldItem.order,
                gemPopped = newItem.gemPopped,
                isCompleted = newItem.isCompleted,
                timerOn = newItem.timerOn,
                id = newItem.id,
            };

            gameData.boardData[newLoc.x, newLoc.y] = new()
            {
                sprite = oldItem.sprite,
                type = oldItem.type,
                group = oldItem.group,
                genGroup = oldItem.genGroup,
                collGroup = oldItem.collGroup,
                chestGroup = oldItem.chestGroup,
                state = oldItem.state,
                crate = oldItem.crate,
                order = newItem.order,
                gemPopped = newItem.gemPopped,
                isCompleted = newItem.isCompleted,
                timerOn = oldItem.timerOn,
                id = oldItem.id,
            };

            // Save the board to disk
            dataManager.SaveBoard();
        }

        public void MergeBoardData(GameObject oldTile, GameObject newTile, Item newItem)
        {
            // Get tile locations
            Vector2Int oldLoc = GetBoardLocation(0, oldTile);
            Vector2Int newLoc = GetBoardLocation(0, newTile);

            int oldOrder = gameData.boardData[oldLoc.x, oldLoc.y].order;
            int newOrder = gameData.boardData[newLoc.x, newLoc.y].order;

            // Clear old items (preserve the order)
            gameData.boardData[oldLoc.x, oldLoc.y] = new() { order = oldOrder };

            // Set new item
            gameData.boardData[newLoc.x, newLoc.y] = new()
            {
                sprite = newItem.sprite,
                type = newItem.type,
                group = newItem.group,
                genGroup = newItem.genGroup,
                collGroup = newItem.collGroup,
                chestGroup = newItem.chestGroup,
                state = newItem.state,
                crate = newItem.crate,
                order = newOrder,
                gemPopped = newItem.gemPopped,
                isCompleted = newItem.isCompleted,
                timerOn = newItem.timerOn,
                id = newItem.id,
            };

            // Save the board to disk
            dataManager.SaveBoard();

            // Give experience if it's the first time unlocking it and its level is higher than experienceThreshold (default 4)
            if (newItem.type != Types.Type.Coll && newItem.level >= (experienceThreshold + 1))
            {
                CreateCollectable();
            }
        }

        public void RemoveBoardData(GameObject oldTile)
        {
            // Get tile locations
            Vector2Int oldLoc = GetBoardLocation(0, oldTile);

            int oldOrder = gameData.boardData[oldLoc.x, oldLoc.y].order;

            // Clear old items
            gameData.boardData[oldLoc.x, oldLoc.y] = new Types.Tile { order = oldOrder };

            // Save the board to disk
            dataManager.SaveBoard();
        }

        public void ToggleTimerOnItem(int order, bool enable)
        {
            if (order < 0 || order >= boardTiles.transform.childCount)
            {
                // ERROR
                errorManager.ThrowWarning(Types.ErrorType.Code, "BoardManager", "Invalid order provided.");
                return;
            }

            Transform tileTransform = boardTiles.transform.GetChild(order);

            if (tileTransform != null)
            {
                Item itemComponent = tileTransform.GetComponentInChildren<Item>();

                if (itemComponent != null)
                {
                    itemComponent.ToggleTimer(enable);
                }
                else
                {
                    // ERROR
                    errorManager.ThrowWarning(Types.ErrorType.Code, "BoardManager", "Item component not found on tile.");
                }
            }
            else
            {
                // ERROR
                errorManager.ThrowWarning(Types.ErrorType.Code, "BoardManager", "Tile transform not found.");
            }
        }

        /////// CRATE ////////

        public void CheckForCrate(GameObject tile)
        {
            // Check if the item's neighbors are crates

            Vector2Int loc = GetBoardLocation(0, tile);

            if (loc.x < (GameData.WIDTH - 1))
            {
                FindItemAndOpenCrate(loc.x + 1, loc.y);
            }

            if (loc.x > 0)
            {
                FindItemAndOpenCrate(loc.x - 1, loc.y);
            }

            if (loc.y < (GameData.HEIGHT - 1))
            {
                FindItemAndOpenCrate(loc.x, loc.y + 1);
            }

            if (loc.y > 0)
            {
                FindItemAndOpenCrate(loc.x, loc.y - 1);
            }
        }

        void FindItemAndOpenCrate(int x, int y)
        {
            // Find the the crate on the board and open it
            if (gameData.boardData[x, y].state == Types.State.Crate)
            {
                gameData.boardData[x, y].state = Types.State.Locker;

                // Play crate opening audio
                if (crateTimeout == 0)
                {
                    if (crateCoroutine != null)
                    {
                        StopCoroutine(crateCoroutine);
                        crateCoroutine = null;
                    }

                    soundManager.PlaySound(Types.SoundType.OpenCrate);

                    crateTimeout = crateSoundTimeout;

                    crateCoroutine = StartCoroutine(CrateTimeout());
                }

                int order = GetBoardOrder(x, y);

                Transform foundTile = boardTiles.transform.GetChild(order);

                if (foundTile.childCount > 0)
                {
                    GameObject foundItem = foundTile.GetChild(0).gameObject;

                    foundItem.GetComponent<Item>().OpenCrate();
                }

                dataManager.SaveBoard();
            }
        }

        IEnumerator CrateTimeout()
        {
            WaitForSeconds wait = new(0.1f);

            while (crateTimeout > 0)
            {
                yield return wait;

                crateTimeout -= 0.1f;
            }
        }

        public Vector2 UnlockAndGetItemPos(string spriteName)
        {
            int count = 0;
            Vector2 pos = new();

            for (int x = 0; x < GameData.WIDTH; x++)
            {
                for (int y = 0; y < GameData.HEIGHT; y++)
                {
                    if (gameData.boardData[x, y].sprite != null && gameData.boardData[x, y].sprite.name == spriteName)
                    {
                        Transform tileTransform = boardTiles.transform.GetChild(count);

                        if (tileTransform != null)
                        {
                            pos = tileTransform.position;

                            Transform itemTransform = tileTransform.GetChild(0);

                            if (itemTransform != null)
                            {
                                if (itemTransform.TryGetComponent(out Item item))
                                {
                                    item.UnlockLock(0.05f, false);

                                    // Play unlocking audio
                                    soundManager.PlaySound(Types.SoundType.UnlockLock);

                                    interactions.OpenLockCallback(item);
                                }
                                else
                                {
                                    // ERROR
                                    errorManager.ThrowWarning(Types.ErrorType.Code, GetType().ToString(), "Failed to retrieve item.");
                                    return default;
                                }
                            }
                            else
                            {
                                // ERROR
                                errorManager.ThrowWarning(Types.ErrorType.Code, GetType().ToString(), "Failed to retrieve item transform.");
                                return default;
                            }
                        }
                        else
                        {
                            // ERROR
                            errorManager.ThrowWarning(Types.ErrorType.Code, GetType().ToString(), "Failed to retrieve tile transform.");
                            return default;
                        }
                    }

                    count++;
                }
            }

            return pos;
        }

        /////// BUBBLE ////////

        public void CheckForBubble(Item item)
        {
            if (PlayerPrefs.HasKey("tutorialFinished") && item.level >= minBubbleLevel && item.type == Types.Type.Item)
            {
                float a = 0;
                float b = 0;

                float single = 100f / (float)bubbleCount;

                for (int i = 0; i < bubbleCount; i++)
                {
                    float randomNum = UnityEngine.Random.Range(0, 101);

                    if (randomNum <= bubbleChance)
                    {
                        a += 1f;
                    }
                    else
                    {
                        b += 1f;
                    }
                }

                if ((a * single) < bubbleChance)
                {
                    GameObject tile = item.transform.parent.gameObject;

                    Vector2Int tileLoc = GetBoardLocation(0, tile);

                    List<Types.TileEmpty> emptyBoard = GetEmptyTileItems(tileLoc);

                    // Check if the board is full
                    if (emptyBoard.Count > 0)
                    {
                        emptyBoard.Sort((p1, p2) => p1.distance.CompareTo(p2.distance));

                        for (int i = 0; i < gameData.itemsData.Length; i++)
                        {
                            if (gameData.itemsData[i].group == item.group)
                            {
                                for (int j = 0; j < gameData.itemsData[i].content.Length; j++)
                                {
                                    if (j == (item.level - 1))
                                    {
                                        CreateItemOnEmptyTile(
                                            gameData.itemsData[i].content[j],
                                            emptyBoard[0],
                                            item.transform.position,
                                            true,
                                            true,
                                            null,
                                            null,
                                            Types.State.Bubble
                                        );

                                        break;
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }

        public void RemoveBubble(string id)
        {
            int count = 0;

            for (int x = 0; x < gameData.boardData.GetLength(0); x++)
            {
                for (int y = 0; y < gameData.boardData.GetLength(1); y++)
                {
                    if (gameData.boardData[x, y].id == id)
                    {
                        Transform tile = boardTiles.transform.GetChild(count);

                        Item bubbleItem = null;

                        if (tile != null)
                        {
                            bubbleItem = tile.GetChild(0).GetComponent<Item>();
                        }

                        if (bubbleItem != null)
                        {
                            int itemLevel = bubbleItem.level;

                            Vector2 position = bubbleItem.transform.position;

                            boardSelection.Unselect(Types.SelectType.None);

                            Types.TileEmpty tileEmpty = new()
                            {
                                order = count,
                                loc = new(x, y),
                                distance = 0f
                            };

                            bubbleItem.ScaleToSize(Vector2.zero, scaleSpeed, true, () =>
                            {
                                gameData.boardData[x, y] = new();

                                for (int i = 0; i < gameData.collectablesData.Length; i++)
                                {
                                    if (gameData.collectablesData[i].collGroup == Types.CollGroup.Gold)
                                    {
                                        for (int j = 0; j < gameData.collectablesData[i].content.Length; j++)
                                        {
                                            if (itemLevel >= minBubbleLevel && j < minBubbleLevel)
                                            {
                                                CreateItemOnEmptyTile(
                                                    gameData.collectablesData[i].content[j],
                                                    tileEmpty,
                                                    position,
                                                    false,
                                                    false,
                                                    null,
                                                    null,
                                                    Types.State.Default
                                                );

                                                break;
                                            }
                                        }

                                        break;
                                    }
                                }
                            });
                        }

                        return;
                    }

                    count++;
                }
            }
        }

        /////// CREATE ITEM ON THE BOARD ////////
        public void CreateCollectable()
        {
            GameObject tile = interactions.currentItem.transform.parent.gameObject;

            Vector2Int tileLoc = GetBoardLocation(0, tile);

            List<Types.TileEmpty> emptyBoard = GetEmptyTileItems(tileLoc);

            // Check if the board is full
            if (emptyBoard.Count > 0)
            {
                emptyBoard.Sort((p1, p2) => p1.distance.CompareTo(p2.distance));

                // Create item from selected group
                for (int i = 0; i < gameData.collectablesData.Length; i++)
                {
                    if (gameData.collectablesData[i].collGroup == Types.CollGroup.Experience)
                    {
                        CreateItemOnEmptyTile(
                            gameData.collectablesData[i].content[0],
                            emptyBoard[0],
                            tile.transform.position
                        );

                        return;
                    }
                }
            }
            else
            {
                valuePop.PopValue(1, Types.CollGroup.Experience, tile.transform.position);
            }
        }

        public void CreateItemOnEmptyTile(
            Types.ItemData itemData,
            Types.TileEmpty emptyBoard,
            Vector2 initialPosition,
            bool canUnlock = true,
            bool useEnergy = false,
            Types.Inventory inventoryItem = null,
            Action<Vector2> callBack = null,
            Types.State newState = Types.State.Default
        )
        {
            GameObject emptyTile = boardTiles.transform.GetChild(emptyBoard.order).gameObject;

            Types.Tile tileItem;

            if (inventoryItem != null && itemData == null)
            {
                tileItem = new()
                {
                    sprite = inventoryItem.sprite,
                    type = inventoryItem.type,
                    group = inventoryItem.group,
                    genGroup = inventoryItem.genGroup,
                    chestGroup = inventoryItem.chestGroup,
                    gemPopped = inventoryItem.gemPopped,
                    timerOn = inventoryItem.timerOn,
                    isCompleted = inventoryItem.isCompleted,
                    id = inventoryItem.id
                };
            }
            else
            {
                tileItem = new()
                {
                    sprite = itemData.sprite,
                    type = itemData.type,
                    group = itemData.group,
                    state = newState,
                    genGroup = itemData.genGroup,
                    collGroup = itemData.collGroup,
                    chestGroup = itemData.chestGroup,
                    gemPopped = itemData.gemPopped,
                    id = ""
                };
            }

            if (tileItem.id == "")
            {
                tileItem.id = Guid.NewGuid().ToString();
            }

            // Create the item on the board
            Item newItem = itemHandler.CreateItem(emptyTile, boardInitialization.tileSize, tileItem);

            Vector2 tempScale = new(
                newItem.transform.localScale.x,
                newItem.transform.localScale.y
            );

            newItem.transform.GetChild(3).GetComponent<SpriteRenderer>().sortingOrder = 2;
            newItem.gameObject.layer = LayerMask.NameToLayer("ItemDragging");

            // Play generating audio
            soundManager.PlaySound(Types.SoundType.Generate);

            newItem.transform.position = initialPosition;

            newItem.transform.localScale = Vector2.zero;

            newItem.MoveAndScale(emptyTile.transform.position, tempScale, altMoveSpeed, altScaleSpeed, () =>
            {
                callBack?.Invoke(newItem.transform.position);

                if (newState == Types.State.Bubble)
                {
                    timeManager.AddTimer(Types.TimerType.Bubble, Types.NotificationType.Chest, newItem.itemName, newItem.id, newItem.transform.position, bubblePopTimeout);
                }
            });

            Vector2Int boardLoc = Vector2Int.zero;

            gameData.boardData[emptyBoard.loc.x, emptyBoard.loc.y] = new()
            {
                sprite = newItem.sprite,
                type = newItem.type,
                group = newItem.group,
                genGroup = newItem.genGroup,
                collGroup = newItem.collGroup,
                chestGroup = newItem.chestGroup,
                gemPopped = newItem.gemPopped,
                timerOn = inventoryItem != null ? inventoryItem.timerOn : false,
                isCompleted = inventoryItem != null ? inventoryItem.isCompleted : false,
                id = newItem.id,
                state = newItem.state,
                crate = 0,
                order = emptyBoard.order
            };

            if (canUnlock)
            {
                dataManager.UnlockItem(
                    newItem.sprite.name,
                    newItem.type,
                    newItem.group,
                    newItem.genGroup,
                    newItem.collGroup,
                    newItem.chestGroup
                );
            }

            if (useEnergy)
            {
                gameData.UpdateValue(-1, Types.CollGroup.Energy, false, true);
            }

            dataManager.SaveBoard();
        }

        public List<Types.TileEmpty> GetEmptyTileItems(Vector2Int tileLoc, bool useTileLoc = true)
        {
            List<Types.TileEmpty> emptyBoard = new();

            for (int x = 0; x < gameData.boardData.GetLength(0); x++)
            {
                for (int y = 0; y < gameData.boardData.GetLength(1); y++)
                {
                    if (gameData.boardData[x, y].sprite == null)
                    {
                        emptyBoard.Add(
                            new Types.TileEmpty
                            {
                                order = gameData.boardData[x, y].order,
                                loc = GetBoardLocation(gameData.boardData[x, y].order),
                                distance = useTileLoc
                                    ? CalculateDistance(tileLoc.x, tileLoc.y, x, y)
                                    : 0
                            }
                        );
                    }
                }
            }

            return emptyBoard;
        }

        public bool IsThereAnEmptyBoardSpace()
        {
            List<Types.TileEmpty> emptyBoard = GetEmptyTileItems(Vector2Int.zero, false);

            return emptyBoard.Count > 0;
        }

        public void RemoveItemFromChest(Item item)
        {
            if (item.chestItems > 1)
            {
                item.chestItems -= 1;

                Vector2Int loc = GetBoardLocation(0, item.transform.parent.gameObject);

                gameData.boardData[loc.x, loc.y].chestItems -= 1;

                dataManager.SaveBoard();
            }
            else
            {
                interactions.RemoveItem(item, 0, false);
            }
        }

        // Calculate the distance between two points in a 2d array
        float CalculateDistance(int currentX, int currentY, int otherX, int otherY)
        {
            float distance = Mathf.Sqrt(
                (currentX - otherX) * (currentX - otherX) + (currentY - otherY) * (currentY - otherY)
            );

            return distance;
        }

        public void SetCompletedItems()
        {
            for (int x = 0; x < gameData.boardData.GetLength(0); x++)
            {
                for (int y = 0; y < gameData.boardData.GetLength(1); y++)
                {
                    int order = GetBoardOrder(x, y);

                    Transform foundTile = boardTiles.transform.GetChild(order);

                    if (foundTile.childCount > 0)
                    {
                        foundTile.GetChild(0).GetComponent<Item>().isCompleted = gameData.boardData[x, y].isCompleted;
                    }
                }
            }
        }
    }
}