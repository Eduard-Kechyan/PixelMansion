using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class InitializeBoard : MonoBehaviour
    {
        // Variables
        public GameObject tile;
        public float tileWidth = 24f;
        public float tileSize;

        // References
        private BoardManager boardManager;
        private DataManager dataManager;
        private GameData gameData;
        private ItemHandler itemHandler;
        private GameplayUI gamePlayUI;

        private GameObject board;
        private float singlePixelWidth;
        private float screenUnitWidth;
        private float boardHalfWidth;
        private float boardHalfHeight;
        private SafeAreaHandler safeAreaHandler;
        private Camera cam;
        private VisualElement root;

        private bool set;
        private bool readyToInitialize;

        void Start()
        {
            // Cache
            cam = Camera.main;
            boardManager = GetComponent<BoardManager>();

            // Cache instances
            dataManager = DataManager.Instance;
            gameData = GameData.Instance;
            itemHandler = dataManager.GetComponent<ItemHandler>();
            gamePlayUI = GameRefs.Instance.gamePlayUI;
            safeAreaHandler = gamePlayUI.GetComponent<SafeAreaHandler>();

            // UI
            root = gamePlayUI.GetComponent<UIDocument>().rootVisualElement;

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
        }

        void Update()
        {
            if (dataManager.loaded && !set && readyToInitialize)
            {
                set = true;

                // Calculate board scale
                SetBoard();

                // Stop running the update function
                enabled = false;
            }
        }

        void SetBoard()
        {
            // Ready the board

            // Get board sprite width
            float boardWidth = board.GetComponent<SpriteRenderer>().sprite.rect.width;

            // Get the size of the board relative to the in game units
            float boardPixelWidth = singlePixelWidth * boardWidth;

            // Calc final width
            float boardWidthInUnits = (screenUnitWidth / cam.pixelWidth) * boardPixelWidth;

            // Set board scale
            board.transform.localScale = new Vector3(
                boardWidthInUnits,
                boardWidthInUnits,
                board.transform.localScale.z
            );

            // Set board vertical position
            float screenUnitHeight =
                cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10)).y * 2;
            float gamePixelOnScreen = cam.pixelWidth / GameData.GAME_PIXEL_WIDTH;
            float gamePixelHeight = (cam.pixelHeight / gamePixelOnScreen);

            float boardPosX =
                (screenUnitHeight / 2)
                - ((screenUnitHeight / gamePixelHeight) * safeAreaHandler.GetBottomOffset());

            board.transform.position = new Vector3(
                board.transform.position.x,
                -boardPosX + ((board.transform.localScale.x * 1.28f) / 2),
                board.transform.position.z
            );

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

        void CreateBoard()
        {
            // Loop the items to the board

            int count = 0;

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
                    GameObject newTile = Instantiate(tile, pos, tile.transform.rotation);

                    newTile.transform.localScale = new Vector3(
                        tileSize,
                        tileSize,
                        newTile.transform.localScale.y
                    );

                    newTile.gameObject.name = "Tile" + count;

                    newTile.transform.parent = boardManager.boardTiles.transform;

                    // Create item
                    Types.Board boardItem = gameData.boardData[x, y];

                    if (gameData.boardData[x, y].id == "")
                    {
                        gameData.boardData[x, y].id = Guid.NewGuid().ToString();
                    }

                    if (boardItem != null && boardItem.sprite != null)
                    {
                        Item newItem = itemHandler.CreateItem(newTile, tileSize, boardItem);
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
                    }

                    // Increase count for the next loop
                    count++;
                }
            }

            dataManager.SaveBoard(false, false);

            boardManager.boardSet = true;
        }
    }
}