using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Locale;

public class BonusManager : MonoBehaviour
{
    // Variables
    public BoardManager boardManager;
    public BoardPopup boardPopup;

    // References
    private GameplayUI gameplayUI;

    // Instances
    private GameData gameData;
    private I18n LOCALE;

    void Start()
    {
        // References
        gameplayUI = GetComponent<GameplayUI>();

        // Cache instances
        gameData = GameData.Instance;
        LOCALE = I18n.Instance;
    }

    public void GetBonus()
    {
        List<Types.BoardEmpty> emptyBoard = boardManager.GetEmptyBoardItems(Vector2Int.zero, false);

        // Check if the board is full
        if (emptyBoard.Count > 0)
        {
            Types.Bonus latestBonus = gameData.GetAndRemoveLatestBonus();

            emptyBoard.Sort((p1, p2) => p1.distance.CompareTo(p2.distance));

            Types.ItemsData boardItem = new Types.ItemsData
            {
                sprite = latestBonus.sprite,
                type = latestBonus.type,
                group = latestBonus.group,
                genGroup = latestBonus.genGroup,
                chestGroup = latestBonus.chestGroup,
                collGroup = Types.CollGroup.Experience,
            };

            boardManager.CreateItemOnEmptyTile(
                boardItem,
                emptyBoard[0],
                gameplayUI.bonusButtonPosition,
                false
            );
        }
        else
        {
            boardPopup.AddPop(
                LOCALE.Get("pop_board_full"),
                gameplayUI.bonusButtonPosition,
                true,
                "Buzz"
            );
        }
    }
}
