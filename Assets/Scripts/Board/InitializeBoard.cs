using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InitializeBoard : MonoBehaviour
{
    const int WIDTH = 7;
    const int HEIGHT = 9;

    public UIDocument uiDoc;
    public GameObject boardTiles;
    public GameObject tile;
    public float tileWidth = 24f;
    public float tileSize;

    private DataManager dataManager;
    private ItemHandler itemHandler;

    private GameObject board;
    private float gamePixelWidth;
    private float singlePixelWidth;
    private float screenUnitWidth;
    private float boardHalfWidth;
    private float boardHalfHeight;
    private SafeAreaHandler safeAreaHandler;
    private Camera cam;
    private VisualElement root;

    private bool set;

    void Start()
    {
        // Cache the camera
        cam = Camera.main;

        dataManager = DataManager.Instance;
        itemHandler = dataManager.GetComponent<ItemHandler>();

        // Cache the SafeAreaHandler
        safeAreaHandler = uiDoc.GetComponent<SafeAreaHandler>();

        // Cache the SafeAreaHandler
        root = uiDoc.rootVisualElement;

        // Set the gameObject
        board = gameObject;

        // Cache the preferences
        gamePixelWidth = PlayerPrefs.GetFloat("gamePixelWidth", gamePixelWidth);
        singlePixelWidth = cam.pixelWidth / gamePixelWidth;
        screenUnitWidth =
            cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10)).x * 2;
    }

    void Update()
    {
        if (dataManager.loaded && !set)
        {
            // Calculate board scale
            root.RegisterCallback<GeometryChangedEvent>(SetBoard);

            set = true;
        }
    }

    void SetBoard(GeometryChangedEvent evt)
    {
        root.UnregisterCallback<GeometryChangedEvent>(SetBoard);

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
        float gamePixelOnScreen = cam.pixelWidth / gamePixelWidth;
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

        CalcTiles();
    }

    void CalcTiles()
    {
        // Get the size of the item relative to the in game units
        float tilePixelWidth = singlePixelWidth * tileWidth;

        // Calc final width and height
        tileSize = (screenUnitWidth / cam.pixelWidth) * tilePixelWidth;

        CreateBoard();
    }

    void CreateBoard()
    {
        int count = 0;

        for (int x = 0; x < WIDTH; x++)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                // Calculate tile's position
                Vector3 pos = new Vector3(
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

                newTile.transform.parent = boardTiles.transform;

                // Create item
                if (GameData.boardData[count] != null && GameData.boardData[count].sprite != null)
                {
                    itemHandler.CreateItem(
                        pos,
                        newTile,
                        tileSize,
                        GameData.boardData[count].group,
                        GameData.boardData[count].sprite.name,
                        GameData.boardData[count].state,
                        GameData.boardData[count].crate
                    );
                }

                // Increase count for the next loop
                count++;
            }
        }
    }
}
