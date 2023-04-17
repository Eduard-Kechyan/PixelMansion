using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Locale;

public class BonusManager : MonoBehaviour
{
    public BoardManager boardManager;

    [HideInInspector]
    public Vector2 bonusButtonPosition;

    private GameData gameData;
    private GamePlayButtons gamePlayButtons;
    private BoardPopup boardPopup;

    private I18n LOCALE = I18n.Instance;

    void Start()
    {
        gameData = GameData.Instance;
        gamePlayButtons= GetComponent<GamePlayButtons>();
        boardPopup = boardManager.GetComponent<BoardPopup>();
    }

    public void CalcBonusButtonPosition()
    {
        // Calculate the button position on the screen and the world space
        float singlePixelWidth = Camera.main.pixelWidth / GameData.GAME_PIXEL_WIDTH;

        Vector2 bonusButtonScreenPosition = new Vector2(
            singlePixelWidth
                * (
                    gamePlayButtons.root.worldBound.width
                    - gamePlayButtons.bonusButton.worldBound.center.x
                ),
            singlePixelWidth
                * (
                    gamePlayButtons.root.worldBound.height
                    - gamePlayButtons.bonusButton.worldBound.center.y
                )
        );

        bonusButtonPosition = Camera.main.ScreenToWorldPoint(bonusButtonScreenPosition);
    }

    public void GetBonus()
    {
        List<Types.BoardEmpty> emptyBoard = boardManager.GetEmptyBoardItems(Vector2Int.zero, false);

        // Check if the board is full
        if (emptyBoard.Count > 0)
        {
            Types.Bonus latestBonus = gameData.GetAndRemoveLatestBonus();

            emptyBoard.Sort((p1, p2) => p1.distance.CompareTo(p2.distance));

            if (latestBonus.type == Types.Type.Item)
            {
                boardManager.CreateItemOnEmptyTile(
                    latestBonus.sprite.name,
                    latestBonus.group,
                    emptyBoard[0],
                    bonusButtonPosition,
                    false
                );
            }
            else if (latestBonus.type == Types.Type.Gen)
            {
                boardManager.CreateGenOnEmptyTile(
                    latestBonus.sprite.name,
                    latestBonus.genGroup,
                    emptyBoard[0],
                    bonusButtonPosition
                );
            }
        }
        else
        {
            boardPopup.AddPop(LOCALE.Get("pop_board_full"), bonusButtonPosition, true, "Buzz");
        }
    }
}
