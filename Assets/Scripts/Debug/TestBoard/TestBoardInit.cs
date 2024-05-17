using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class TestBoardInit : MonoBehaviour
    {
        // Variables
        public const int TILE_WIDTH = 24;

        [HideInInspector]
        public BoardManager.Tile[] boardData;

        // References
        private TestBoardManager testBoardManager;

        // UI
        private VisualElement root;
        private VisualElement board;

        void Start()
        {
            // Cache
            testBoardManager = GetComponent<TestBoardManager>();

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;
            board = root.Q<VisualElement>("Board");

            board.Clear();

            root.RegisterCallback<GeometryChangedEvent>(Init);
        }

        void Init(GeometryChangedEvent evt)
        {
            root.UnregisterCallback<GeometryChangedEvent>(Init);

            StartCoroutine(WaitForBoardManager());
        }

        IEnumerator WaitForBoardManager()
        {
            while (!testBoardManager.ready)
            {
                yield return null;
            }

            boardData = testBoardManager.ConvertInitialItemsToBoard(testBoardManager.initialItems.text);

            SetBoard();
        }

        void SetBoard()
        {
            for (int i = 0; i < boardData.Length; i++)
            {
                VisualElement newBoardTileButton = new() { name = "Tile" + i };
                VisualElement newBoardItem = new() { name = "Item" };
                VisualElement newBoardOverlay = new() { name = "Overlay" };
                VisualElement newBoardReady = new() { name = "Ready" };
                VisualElement newBoardCompleted = new() { name = "Completed" };

                newBoardTileButton.pickingMode = PickingMode.Position;
                newBoardItem.pickingMode = PickingMode.Ignore;
                newBoardOverlay.pickingMode = PickingMode.Ignore;
                newBoardReady.pickingMode = PickingMode.Ignore;
                newBoardCompleted.pickingMode = PickingMode.Ignore;

                newBoardTileButton.AddToClassList("board_tile");
                newBoardItem.AddToClassList("board_tile_item");
                newBoardReady.AddToClassList("board_tile_ready");
                newBoardCompleted.AddToClassList("board_tile_completed");

                if (boardData[i].state == Item.State.Crate)
                {
                    newBoardItem.style.backgroundImage = new StyleBackground(testBoardManager.crates[boardData[i].crate]);
                }
                else
                {
                    newBoardItem.style.backgroundImage = new StyleBackground(boardData[i].sprite);

                    if (boardData[i].state == Item.State.Locker)
                    {
                        newBoardOverlay.AddToClassList("board_tile_locker");
                        newBoardOverlay.style.display = DisplayStyle.Flex;
                    }
                    else if (boardData[i].state == Item.State.Bubble)
                    {
                        newBoardOverlay.AddToClassList("board_tile_bubble");
                        newBoardOverlay.style.display = DisplayStyle.Flex;
                    }

                    if (boardData[i].isCompleted)
                    {
                        newBoardCompleted.style.display = DisplayStyle.Flex;
                    }
                }

                newBoardTileButton.Add(newBoardItem);
                newBoardTileButton.Add(newBoardOverlay);
                newBoardTileButton.Add(newBoardReady);
                newBoardTileButton.Add(newBoardCompleted);

                board.Add(newBoardTileButton);
            }
        }
    }
}
