using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class BoardViewer : MonoBehaviour
    {
#if UNITY_EDITOR
        // Variables
        public GameObject tile;
        public GameObject item;
        public float tileWidth = 24f;
        public float bottomOffset = 20f;

        [Header("UI")]
        public Vector2 countLabelOffset;
        public Vector2 countIndexOffset;
        public Vector2 stateBoxOffset;
        public Vector2 countButtonOffset;
        public Sprite stateCrateBoxSprite;
        public Sprite stateLockerBoxSprite;
        public Sprite stateBubbleBoxSprite;

        [HideInInspector]
        public BoardManager.Tile[] boardData;

        private float singlePixelWidth;
        private float screenUnitWidth;
        private float boardHalfWidth;
        private float boardHalfHeight;

        private GameObject board;
        private GameObject tiles;
        private float tileSize;

        // Classes
        class ButtonData
        {
            public Button button;
            public bool isTop;
        }

        // References
        private Camera cam;
        private BoardItemMenu boardItemMenu;

        // UI
        private VisualElement root;
        private ButtonData[] buttons = new ButtonData[7 * 9];

        void Start()
        {
            // Cache
            cam = Camera.main;
            boardItemMenu = GetComponent<BoardItemMenu>();

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            // Set the gameObject
            board = gameObject;
            tiles = transform.GetChild(0).gameObject;

            // Cache the preferences
            singlePixelWidth = cam.pixelWidth / GameData.GAME_PIXEL_WIDTH;
            screenUnitWidth =
                cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10)).x * 2;

            root.RegisterCallback<GeometryChangedEvent>(Init);
        }

        void Init(GeometryChangedEvent evt)
        {
            root.UnregisterCallback<GeometryChangedEvent>(Init);

            StartCoroutine(WaitForItemMenu());
        }

        IEnumerator WaitForItemMenu()
        {
            while (!boardItemMenu.ready)
            {
                yield return null;
            }

            boardData = boardItemMenu.ConvertInitialItemsToBoard(boardItemMenu.initialItems.text);

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

                    bool itemCreated = false;

                    Vector2 uiPos = RuntimePanelUtils.CameraTransformWorldToPanel(
                      root.panel,
                      newTile.transform.position,
                      cam
                    );

                    // Create item
                    if (boardData[count] != null && boardData[count].sprite != null)
                    {
                        GameObject newItem = Instantiate(item, newTile.transform.position, Quaternion.identity);

                        newItem.GetComponent<SpriteRenderer>().sprite = boardData[count].sprite;

                        newItem.transform.SetParent(newTile.transform);

                        newItem.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);

                        itemCreated = true;
                    }

                    CreateUIButton(count, uiPos, boardData[count].state);

                    CreateUILabels(count, x, y, uiPos, boardData[count].state, itemCreated);

                    // Increase count for the next loop
                    count++;
                }
            }
        }

        void CreateUILabels(int count, int indexX, int indexY, Vector2 uiPos, Item.State state, bool useState)
        {
            Sprite stateBoxSprite = null;

            if (useState)
            {
                if (state == Item.State.Crate)
                {
                    stateBoxSprite = stateCrateBoxSprite;
                }
                else if (state == Item.State.Locker)
                {
                    stateBoxSprite = stateLockerBoxSprite;
                }
                else if (state == Item.State.Bubble)
                {
                    stateBoxSprite = stateBubbleBoxSprite;
                }
            }

            Label newCountLabel = new() { name = "Count" + count, text = count.ToString() };
            Label newIndexLabel = new() { name = "Index" + count, text = indexX + "." + indexY };
            VisualElement newStateBox = new() { name = "State" + count };

            newCountLabel.AddToClassList("count_label");
            newIndexLabel.AddToClassList("index_label");
            newStateBox.AddToClassList("state_box");

            newStateBox.style.backgroundImage = new StyleBackground(stateBoxSprite);

            newCountLabel.style.top = uiPos.y + countLabelOffset.y;
            newCountLabel.style.left = uiPos.x + countLabelOffset.x;

            newIndexLabel.style.top = uiPos.y + countIndexOffset.y;
            newIndexLabel.style.left = uiPos.x + countIndexOffset.x;

            newStateBox.style.top = uiPos.y + stateBoxOffset.y;
            newStateBox.style.left = uiPos.x + stateBoxOffset.x;

            root.Insert(0, newCountLabel);
            root.Insert(0, newIndexLabel);
            root.Insert(0, newStateBox);
        }

        void CreateUIButton(int count, Vector2 uiPos, Item.State state)
        {
            Button newItemButton = new() { name = "Button" + count, text = "" };

            newItemButton.AddToClassList("board_item_button");

            newItemButton.style.top = uiPos.y + countButtonOffset.y;
            newItemButton.style.left = uiPos.x + countButtonOffset.x;

            root.Insert(0, newItemButton);

            string order = count.ToString();

            bool isTop = uiPos.y > root.resolvedStyle.height / 2;

            buttons[count] = new()
            {
                button = newItemButton,
                isTop = isTop
            };

            newItemButton.clicked += () => boardItemMenu.OpenItemMenu(order, isTop, state);
        }

        public void SelectButton(int order)
        {
            buttons[order].button.AddToClassList("board_item_button_selected");
        }

        public void DeSelectButton(int order)
        {
            buttons[order].button.RemoveFromClassList("board_item_button_selected");
        }

        public void UpdateBoardItemSprite(int order)
        {
            Transform newTile = tiles.transform.GetChild(order);

            if (newTile.childCount > 0)
            {
                newTile.GetChild(0).GetComponent<SpriteRenderer>().sprite = boardData[order].sprite;
            }
            else
            {
                if (boardData[order] != null && boardData[order].sprite != null)
                {
                    GameObject newItem = Instantiate(item, newTile.transform.position, Quaternion.identity);

                    newItem.GetComponent<SpriteRenderer>().sprite = boardData[order].sprite;

                    newItem.transform.SetParent(newTile.transform);

                    newItem.transform.localScale = new Vector3(1, 1, 1);
                }
            }
        }

        public void UpdateStateBoxSprite(int order, Item.State state, bool useState = true)
        {
            Sprite stateBoxSprite = null;

            if (useState)
            {
                if (state == Item.State.Crate)
                {
                    stateBoxSprite = stateCrateBoxSprite;
                }
                else if (state == Item.State.Locker)
                {
                    stateBoxSprite = stateLockerBoxSprite;
                }
                else if (state == Item.State.Bubble)
                {
                    stateBoxSprite = stateBubbleBoxSprite;
                }
            }

            root.Q<VisualElement>("State" + order).style.backgroundImage = new StyleBackground(stateBoxSprite);

            root.Q<Button>("Button" + order).clicked += () => boardItemMenu.OpenItemMenu(order.ToString(), buttons[order].isTop, state);
        }
#endif
    }
}
