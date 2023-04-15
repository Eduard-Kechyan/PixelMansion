using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Locale;

public class DoubleTapManager : MonoBehaviour
{
    public float moveSpeed = 14f;
    public float scaleSpeed = 8f;

    private BoardInteractions interactions;
    private BoardManager boardManager;
    private GameData gameData;
    private BoardPopup boardPopup;
    private EnergyMenu energyMenu;
    private ValuePop valuePop;

    private I18n LOCALE = I18n.Instance;

    void Start()
    {
        // Cache
        interactions = GetComponent<BoardInteractions>();
        boardManager = GetComponent<BoardManager>();
        boardPopup = GetComponent<BoardPopup>();
        gameData = GameData.Instance;
        energyMenu = MenuManager.Instance.GetComponent<EnergyMenu>();
        valuePop = MenuManager.Instance.GetComponent<ValuePop>();
    }

    public void DoubleTapped()
    {
        // Check for generator
        if (
            interactions.currentItem.type == Types.Type.Gen
            && interactions.currentItem.creates.Length > 0
            && gameData.energy >= 1
        )
        {
            DoubleTappedGenerator();
        }
        else if (interactions.currentItem.type == Types.Type.Coll)
        {
            valuePop.PopExperience(interactions.currentItem.level, "Experience", interactions.currentItem.transform.position);
        }
    }

    void DoubleTappedGenerator()
    {

        GameObject tile = interactions.currentItem.transform.parent.gameObject;

        Vector2Int tileLoc = boardManager.GetBoardLocation(0, tile);

        List<Types.BoardEmpty> emptyBoard = boardManager.GetEmptyBoardItems(tileLoc);

        // Check if the board is full
        if (emptyBoard.Count > 0)
        {
            // Check if we have any energy left
            if (gameData.energy > 0)
            {
                emptyBoard.Sort((p1, p2) => p1.distance.CompareTo(p2.distance));

                SelectRadnomGroupAndItem(emptyBoard[0], tile);
            }
            else
            {
                energyMenu.Open();
            }
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

    void SelectRadnomGroupAndItem(Types.BoardEmpty emptyBoard, GameObject tile)
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
                    gameData.itemsData[i].content[0].sprite.name,
                    gameData.itemsData[i].content[0].group,
                    emptyBoard,
                    tile
                );
            }
        }
    }

}
