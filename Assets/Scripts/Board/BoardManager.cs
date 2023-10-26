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
        public float scaleSpeed = 8f;
        public int experienceThreshold = 4;

        [ReadOnly]
        public bool boardSet = false;

        [Header("Bubbles")]
        public int minBubbleLevel = 4; // TODO - Change this to 6 or higher
        public int bubbleChance = 30; // In %
        public int bubbleCount = 1000;
        public bool popBubbles = false;

        [HideInInspector]
        public GameObject boardTiles;

        // References
        private InitializeBoard initializeBoard;
        private BoardInteractions interactions;
        private DataManager dataManager;
        private SoundManager soundManager;
        private GameData gameData;
        private ItemHandler itemHandler;
        private ValuePop valuePop;

        void Start()
        {
            // Cache
            boardTiles = transform.GetChild(0).gameObject;
            initializeBoard = GetComponent<InitializeBoard>();
            interactions = GetComponent<BoardInteractions>();
            dataManager = DataManager.Instance;
            soundManager = SoundManager.Instance;
            gameData = GameData.Instance;
            itemHandler = dataManager.GetComponent<ItemHandler>();
            valuePop = GameRefs.Instance.valuePop;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (popBubbles)
            {
                popBubbles = false;

                CheckForBubble();
            }
        }
#endif

        /////// GET BOARD DATA ////////

        public int GetBoardOrder(int checkX, int checkY)
        {
            // Get the item's order from the board by its location

            int count = 0;

            for (int x = 0; x < gameData.boardData.GetLength(0); x++)
            {
                for (int y = 0; y < gameData.boardData.GetLength(1); y++)
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

        public Vector2Int GetBoardLocation(int order, GameObject tile = null)
        {
            // Get the item's location from the board by its tile's order

            Vector2Int loc = new(0, 0);

            if (tile != null)
            {
                order = int.Parse(tile.gameObject.name[(tile.gameObject.name.LastIndexOf('e') + 1)..]);
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

        public Vector2 GetBoardItemPosByLoc(int checkX, int checkY)
        {
            for (int i = 0; i < boardTiles.transform.childCount; i++)
            {
                if (i == GetBoardOrder(checkX, checkY))
                {
                    return boardTiles.transform.GetChild(i).position;
                }
            }

            return Vector2.zero;
        }

        public Vector2 GetBoardItemPosById(string id)
        {
            for (int x = 0; x < gameData.boardData.GetLength(0); x++)
            {
                for (int y = 0; y < gameData.boardData.GetLength(1); y++)
                {
                    if (gameData.boardData[x, y].id == id)
                    {
                        return GetBoardItemPosByLoc(x, y);
                    }
                }
            }

            return Vector2.zero;
        }

        /////// SET BOARD DATA ////////

        public void SwapBoardData(GameObject oldTile, GameObject newTile)
        {
            // Get tile locations
            Vector2Int oldLoc = GetBoardLocation(0, oldTile);
            Vector2Int newLoc = GetBoardLocation(0, newTile);

            // Save items
            Types.Board oldItem = gameData.boardData[oldLoc.x, oldLoc.y];
            Types.Board newItem = gameData.boardData[newLoc.x, newLoc.y];

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

            // Clear old items
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
            gameData.boardData[oldLoc.x, oldLoc.y] = new Types.Board { order = oldOrder };

            // Save the board to disk
            dataManager.SaveBoard();
        }

        public void ToggleTimerOnItem(int order, bool enable)
        {
            boardTiles.transform.GetChild(order).GetChild(0).GetComponent<Item>().ToggleTimer(enable);
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
                soundManager.PlaySound("OpenCrate");

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

        /////// BUBBLE ////////

        public void CheckForBubble(Item item = null)
        {
            float a = 0;
            float b = 0;

            float single = 100f / (float)bubbleCount;

            // Check if it's a high level item
            /* if (item.level >= minBubbleLevel)
             {
                 float randomNum = UnityEngine.Random.Range(0, 101);

                 if (randomNum <= bubbleChance)
                 {
                     a++;
                 }else{
                     b++;
                 }
             }*/

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

            //  Debug.Log(a * single + ":" + b * single);


            // TODO - Add timer for bubble
        }

        /////// CREATE ITEM ON THE BOARD ////////
        public void CreateCollectable()
        {
            GameObject tile = interactions.currentItem.transform.parent.gameObject;

            Vector2Int tileLoc = GetBoardLocation(0, tile);

            List<Types.BoardEmpty> emptyBoard = GetEmptyBoardItems(tileLoc);

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
            Types.BoardEmpty emptyBoard,
            Vector2 initialPosition,
            bool canUnlock = true,
            bool useEnergy = false,
            Types.Inventory inventoryItem = null,
            Action<Vector2> callBack = null
        )
        {
            GameObject emptyTile = boardTiles.transform.GetChild(emptyBoard.order).gameObject;

            Types.Board boardItem;

            if (inventoryItem != null && itemData == null)
            {
                boardItem = new()
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
                boardItem = new()
                {
                    sprite = itemData.sprite,
                    type = itemData.type,
                    group = itemData.group,
                    genGroup = itemData.genGroup,
                    collGroup = itemData.collGroup,
                    chestGroup = itemData.chestGroup,
                    gemPopped = itemData.gemPopped,
                    id = ""
                };
            }

            if (boardItem.id == "")
            {
                boardItem.id = Guid.NewGuid().ToString();
            }

            // Create the item on the board
            Item newItem = itemHandler.CreateItem(emptyTile, initializeBoard.tileSize, boardItem);

            Vector2 tempScale = new(
                newItem.transform.localScale.x,
                newItem.transform.localScale.y
            );

            newItem.transform.GetChild(3).GetComponent<SpriteRenderer>().sortingOrder = 2;
            newItem.gameObject.layer = LayerMask.NameToLayer("ItemDragging");

            // Play generating audio
            soundManager.PlaySound("Generate");

            newItem.transform.position = initialPosition;

            newItem.transform.localScale = Vector2.zero;

            newItem.MoveAndScale(emptyTile.transform.position, tempScale, moveSpeed, scaleSpeed, () =>
            {
                if (callBack != null)
                {
                    callBack(newItem.transform.position);
                }
            });

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

        public List<Types.BoardEmpty> GetEmptyBoardItems(Vector2Int tileLoc, bool useTileLoc = true)
        {
            List<Types.BoardEmpty> emptyBoard = new();

            for (int x = 0; x < gameData.boardData.GetLength(0); x++)
            {
                for (int y = 0; y < gameData.boardData.GetLength(1); y++)
                {
                    if (gameData.boardData[x, y].sprite == null)
                    {
                        emptyBoard.Add(
                            new Types.BoardEmpty
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
            List<Types.BoardEmpty> emptyBoard = GetEmptyBoardItems(Vector2Int.zero, false);

            return emptyBoard.Count > 0;
        }

        public void RemoveItemForChest(Item item)
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