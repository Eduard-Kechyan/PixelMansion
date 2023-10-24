using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class BonusManager : MonoBehaviour
    {
        // Variables
        public BoardManager boardManager;

        // References
        private GameplayUI gameplayUI;
        private PopupManager popupManager;
        private DataManager dataManager;
        private UIButtons uiButtons;

        // Instances
        private GameData gameData;
        private I18n LOCALE;

        void Start()
        {
            // References
            gameplayUI = GetComponent<GameplayUI>();
            popupManager = PopupManager.Instance;
            dataManager = DataManager.Instance;
            uiButtons = GameData.Instance.GetComponent<UIButtons>();

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

                Types.ItemData itemData = new()
                {
                    sprite = latestBonus.sprite,
                    type = latestBonus.type,
                    group = latestBonus.group,
                    genGroup = latestBonus.genGroup,
                    chestGroup = latestBonus.chestGroup,
                    collGroup = Types.CollGroup.Experience,
                    gemPopped = false
                };

                boardManager.CreateItemOnEmptyTile(
                    itemData,
                    emptyBoard[0],
                    uiButtons.gameplayBonusButtonPos,
                    false
                );

                dataManager.SaveBonus();
            }
            else
            {
                popupManager.Pop(
                    LOCALE.Get("pop_board_full"),
                    uiButtons.gameplayBonusButtonPos,
                    "Buzz",
                    true
                );
            }
        }
    }
}