using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class BoardInitialization : MonoBehaviour
    {
        // Variables
        public GameObject tilePrefab;
        public float tileWidth = 24f;
        public float tileSize;

        private GameObject board;

        private float singlePixelWidth;
        private float screenUnitWidth;
        private float boardHalfWidth;
        private float boardHalfHeight;

        [HideInInspector]
        public int itemsTotal;
        [HideInInspector]
        public int itemsInitialized;
        [HideInInspector]
        public bool set;
        private bool readyToInitialize;
        private List<Item> items = new();

        // References
        private BoardManager boardManager;
        private DataManager dataManager;
        public TaskManager taskManager;
        private GameData gameData;
        private ItemHandler itemHandler;
        private MergeUI mergeUI;
        private SafeAreaHandler safeAreaHandler;
        private Camera cam;

        // UI
        private VisualElement root;

        void Start()
        {
            // Cache
            boardManager = GetComponent<BoardManager>();
            dataManager = DataManager.Instance;
            taskManager = GameRefs.Instance.taskManager;
            gameData = GameData.Instance;
            itemHandler = dataManager.GetComponent<ItemHandler>();
            mergeUI = GameRefs.Instance.mergeUI;
            safeAreaHandler = mergeUI.GetComponent<SafeAreaHandler>();
            cam = Camera.main;

            // UI
            root = mergeUI.GetComponent<UIDocument>().rootVisualElement;

            // Subscribe to GeometryChangedEvent to know when UI geometry is ready
            root.RegisterCallback<GeometryChangedEvent>((evt) =>
            {
                readyToInitialize = true;
            });

            // Set the gameObject
            board = gameObject;

            // Cache the preferences
            singlePixelWidth = cam.pixelWidth / GameData.GAME_PIXEL_WIDTH;
            screenUnitWidth =
                cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10)).x * 2;

            StartCoroutine(WaitForDataAndUI());
        }

        IEnumerator WaitForDataAndUI()
        {
            // Check if data is loaded and if the board is not yet set up
            while (!dataManager.loaded || !readyToInitialize)
            {
                yield return null;
            }

            set = true;

            // Pre calculate board
            SetBoard();
        }

        void SetBoard()
        {
            SetBoardScale();

            float boardPosX = SetBoardPosition();

            // Get board half
            boardHalfWidth =
    (board.transform.localScale.x / 2) - ((board.transform.localScale.x / 174) * 3);
            boardHalfHeight =
                ((board.transform.localScale.x) / 2)
                - ((board.transform.localScale.x / 174) * 3)
                - (boardPosX - ((board.transform.localScale.x * 1.28f) / 2));

            // Calculate tile size
            // Get the size of the item relative to the in game units
            float tilePixelWidth = singlePixelWidth * tileWidth;

            // Calc final width and height
            tileSize = (screenUnitWidth / cam.pixelWidth) * tilePixelWidth;

            CreateBoard();
        }

        void SetBoardScale()
        {
            SpriteRenderer boardSpriteRenderer = board.GetComponent<SpriteRenderer>();

            if (boardSpriteRenderer == null)
            {
                Debug.LogWarning("Board sprite renderer not found.");
                return;
            }

            // Get board sprite width
            float boardWidth = boardSpriteRenderer.sprite.rect.width;

            // Get the size of the board relative to the in-game units
            float boardPixelWidth = singlePixelWidth * boardWidth;

            // Calculate final width
            float boardWidthInUnits = (screenUnitWidth / cam.pixelWidth) * boardPixelWidth;

            // Set board scale
            board.transform.localScale = new Vector3(
                boardWidthInUnits,
                boardWidthInUnits,
                board.transform.localScale.z
            );
        }

        float SetBoardPosition()
        {
            float screenUnitHeight = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10)).y * 2;
            float gamePixelOnScreen = cam.pixelWidth / GameData.GAME_PIXEL_WIDTH;
            float gamePixelHeight = cam.pixelHeight / gamePixelOnScreen;

            float boardPosX = (screenUnitHeight / 2) - ((screenUnitHeight / gamePixelHeight) * safeAreaHandler.GetBottomOffset());

            float boardOffsetX = ((board.transform.localScale.x * 1.28f) / 2);

            board.transform.position = new Vector3(board.transform.position.x, -boardPosX + boardOffsetX, board.transform.position.z);

            return boardPosX;
        }

        void CreateBoard()
        {
            int count = 0;
            bool initial = false;

            // Loop the items to the board
            for (int x = 0; x < GameData.WIDTH; x++)
            {
                for (int y = 0; y < GameData.HEIGHT; y++)
                {
                    // Calculate tile's position
                    Vector3 pos = new(
                        (tileSize * x) - (boardHalfWidth - (tileSize / 2)),
                        -(tileSize * y) + (boardHalfHeight + (tileSize / 2)),
                        0
                    );

                    // Create tile
                    GameObject newTile = Instantiate(tilePrefab, pos, tilePrefab.transform.rotation);

                    newTile.transform.localScale = new Vector3(
                        tileSize,
                        tileSize,
                        newTile.transform.localScale.y
                    );

                    newTile.gameObject.name = "Tile" + count;

                    newTile.transform.parent = boardManager.boardTiles.transform;

                    // Create item
                    BoardManager.Tile tileItem = gameData.boardData[x, y];

                    if (gameData.boardData[x, y].id == "")
                    {
                        gameData.boardData[x, y].id = Guid.NewGuid().ToString();

                        initial = true;
                    }

                    if (tileItem != null && tileItem.sprite != null)
                    {
                        Item newItem = itemHandler.CreateItem(newTile, tileSize, tileItem);

                        if (newItem)
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

                        newItem.OnInitialized += IncreaseInitializedItemCount;

                        items.Add(newItem);

                        itemsTotal++;
                    }

                    // Increase count for the next loop
                    count++;
                }
            }

            if (initial)
            {
                dataManager.SaveBoard(false, false);
            }

            StartCoroutine(WaitForItemsToInitialize());
        }

        void IncreaseInitializedItemCount()
        {
            itemsInitialized++;
        }

        IEnumerator WaitForItemsToInitialize()
        {
            while (itemsTotal != itemsInitialized)
            {
                yield return null;
            }

            boardManager.boardSet = true;

            if (taskManager != null && taskManager.enabled)
            {
                StartCoroutine(CheckIfThereIsATaskToComplete());
            }

            // Unsubscribe from events and clear references
            for (int i = 0; i < items.Count; i++)
            {
                items[i].OnInitialized -= IncreaseInitializedItemCount;
            }

            items.Clear();
            items = null;
        }

        // Check if there is a tasks to complete
        IEnumerator CheckIfThereIsATaskToComplete()
        {
            while (!taskManager.isLoaded)
            {
                yield return null;
            }

            taskManager.CheckIfThereIsATaskToComplete(null, true);
        }
    }
}