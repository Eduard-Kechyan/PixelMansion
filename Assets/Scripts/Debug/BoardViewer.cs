using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class BoardViewer : MonoBehaviour
    {
        // Variables
        public GameObject tile;
        public GameObject item;
        public InitialItems initialItems;
        public float tileWidth = 24f;
        public float bottomOffset = 20f;

        private float singlePixelWidth;
        private float screenUnitWidth;
        private float boardHalfWidth;
        private float boardHalfHeight;

        private GameObject board;
        private GameObject tiles;
        private Types.Board[,] boardData;
        private float tileSize;

        // References
        private Camera cam;

        void Start()
        {
            // Cache
            cam = Camera.main;

            boardData = ConvertArrayToBoard(initialItems.content);

            // Set the gameObject
            board = gameObject;
            tiles = transform.GetChild(0).gameObject;

            // Cache the preferences
            singlePixelWidth = cam.pixelWidth / GameData.GAME_PIXEL_WIDTH;
            screenUnitWidth =
                cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10)).x * 2;

            SetBoard();
        }

        void SetBoard()
        {
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
                - ((screenUnitHeight / gamePixelHeight) * bottomOffset);

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

                    newTile.transform.SetParent(tiles.transform);

                    // Create item
                    if (boardData[x, y] != null && boardData[x, y].sprite != null)
                    {
                        GameObject newItem = Instantiate(item, newTile.transform.position, Quaternion.identity);

                        newItem.GetComponent<SpriteRenderer>().sprite = boardData[x, y].sprite;

                        newItem.transform.SetParent(newTile.transform);

                        newItem.transform.localScale = new Vector3(1, 1, 1);

                    }

                    // Increase count for the next loop
                    count++;
                }
            }
        }

        public Types.Board[,] ConvertArrayToBoard(Types.Board[] boardArray)
        {
            Types.Board[,] newBoardData = new Types.Board[GameData.WIDTH, GameData.HEIGHT];

            int count = 0;

            for (int i = 0; i < GameData.WIDTH; i++)
            {
                for (int j = 0; j < GameData.HEIGHT; j++)
                {
                    newBoardData[i, j] = new Types.Board
                    {
                        sprite = boardArray[count].sprite,
                        type = boardArray[count].type,
                        group = boardArray[count].group,
                        genGroup = boardArray[count].genGroup,
                        collGroup = boardArray[count].collGroup,
                        chestGroup = boardArray[count].chestGroup,
                        state = boardArray[count].state,
                        crate = boardArray[count].crate,
                        order = count,
                        chestItems = boardArray[count].chestItems,
                        chestItemsSet = boardArray[count].chestItemsSet,
                        chestOpen = boardArray[count].chestOpen,
                        generatesAtLevel = boardArray[count].generatesAtLevel,
                        id = boardArray[count].id,
                        gemPopped = boardArray[count].gemPopped,
                        isCompleted = boardArray[count].isCompleted,
                        timerOn = boardArray[count].timerOn,
                    };

                    count++;
                }
            }

            return newBoardData;
        }
    }
}
