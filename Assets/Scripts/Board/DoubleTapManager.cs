using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Locale;

public class DoubleTapManager : MonoBehaviour
{
    // Variables
    public float moveSpeed = 14f;
    public float scaleSpeed = 8f;

    // References
    private BoardInteractions interactions;
    private BoardManager boardManager;
    private BoardPopup boardPopup;
    private BoardIndication boardIndication;

    // Instances
    private GameData gameData;
    private I18n LOCALE;
    private EnergyMenu energyMenu;
    private ValuePop valuePop;

    private Action callback;

    private GameObject itemParent;

    void Start()
    {
        // Cache
        interactions = GetComponent<BoardInteractions>();
        boardManager = GetComponent<BoardManager>();
        boardPopup = GetComponent<BoardPopup>();
        boardIndication = GetComponent<BoardIndication>();

        // Cache instances
        gameData = GameData.Instance;
        LOCALE = I18n.Instance;
        energyMenu = GameRefs.Instance.energyMenu;
        valuePop = GameRefs.Instance.valuePop;
    }

    public bool CheckForDoubleTaps()
    {
        switch (interactions.currentItem.type)
        {
            case Types.Type.Gen:
                DoubleTappedGenerator();
                return true;

            case Types.Type.Coll:
                DoubleTappedCollectable();
                return true;

            default:

                return false;
        }
    }

    void DoubleTappedGenerator()
    {
        if (interactions.currentItem.creates.Length > 0)
        {
            // Check if we have enough enrgy to generate an item
            if (gameData.energy >= 1)
            {
                GameObject tile = interactions.currentItem.transform.parent.gameObject;

                Vector2Int tileLoc = boardManager.GetBoardLocation(0, tile);

                List<Types.BoardEmpty> emptyBoard = boardManager.GetEmptyBoardItems(tileLoc);

                // Check if the board is full
                if (emptyBoard.Count > 0)
                {
                    emptyBoard.Sort((p1, p2) => p1.distance.CompareTo(p2.distance));

                    boardIndication.StopPossibleMergeCheck();

                    SelectRadnomGroupAndItem(emptyBoard[0], tile.transform.position);
                }
                else
                {
                    boardPopup.AddPop(
                        LOCALE.Get("pop_board_full"),
                        interactions.currentItem.transform.position,
                        true,
                        "Buzz"
                    );
                }
            }
            else
            {
                energyMenu.Open();
            }
        }
    }

    void DoubleTappedCollectable()
    {
        valuePop.PopExperience(
            interactions.currentItem.level,
            "Experience",
            interactions.currentItem.transform.position
        );

        callback = RemoveCollectableFromTheBoard;

        itemParent = interactions.currentItem.transform.parent.gameObject;

        interactions.currentItem.ScaleToSize(Vector2.zero, scaleSpeed, true, callback);
    }

    void RemoveCollectableFromTheBoard()
    {
        boardManager.RemoveBoardData(itemParent);

        itemParent = null;
    }

    void SelectRadnomGroupAndItem(Types.BoardEmpty emptyBoard, Vector2 initialPosition)
    {
        Types.Creates[] creates = interactions.currentItem.creates;

        System.Random random = new System.Random();
        double diceRoll = random.NextDouble();
        double cumulative = 0.0;

        Types.Group selectedGroup = new Types.Group();

        // Randomly select a group of items to choose from
        for (int i = 0; i < creates.Length; i++)
        {
            cumulative += creates[i].chance / 100;

            if (diceRoll < cumulative)
            {
                selectedGroup = creates[i].group;
                break;
            }
        }

        // TODO - Randomly create an item from the selected group

        // Create item from selected group
        for (int i = 0; i < gameData.itemsData.Length; i++)
        {
            if (gameData.itemsData[i].group == selectedGroup)
            {
                boardManager.CreateItemOnEmptyTile(
                    gameData.itemsData[i].content[0],
                    emptyBoard,
                    initialPosition,
                    true,
                    true
                );
            }
        }
    }
}
