using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class BonusManager : MonoBehaviour
    {
        // References
        private MergeUI mergeUI;
        private PopupManager popupManager;
        private DataManager dataManager;
        private UIButtons uiButtons;
        private BoardManager boardManager;

        // Enums
        public class Bonus
        {
            public Sprite sprite;
            public Item.Type type;
            public Item.Group group;
            public Item.GenGroup genGroup;
            public Item.ChestGroup chestGroup;
        }

        public class BonusJson
        {
            public string sprite;
            public string type;
            public string group;
            public string genGroup;
            public string chestGroup;
        }


        // Instances
        private GameData gameData;
        private I18n LOCALE;

        void Start()
        {
            // References
            mergeUI = GetComponent<MergeUI>();
            popupManager = PopupManager.Instance;
            dataManager = DataManager.Instance;
            uiButtons = GameData.Instance.GetComponent<UIButtons>();
            boardManager = GameRefs.Instance.boardManager;

            // Cache instances
            gameData = GameData.Instance;
            LOCALE = I18n.Instance;
        }

        public void GetBonus()
        {
            List<BoardManager.TileEmpty> emptyBoard = boardManager.GetEmptyTileItems(Vector2Int.zero, false);

            // Check if the board is full
            if (emptyBoard.Count > 0)
            {
                Bonus latestBonus = gameData.GetAndRemoveLatestBonus();

                emptyBoard.Sort((p1, p2) => p1.distance.CompareTo(p2.distance));

                BoardManager.ItemData itemData = new()
                {
                    sprite = latestBonus.sprite,
                    type = latestBonus.type,
                    group = latestBonus.group,
                    genGroup = latestBonus.genGroup,
                    chestGroup = latestBonus.chestGroup,
                    collGroup = Item.CollGroup.Experience,
                    gemPopped = false
                };

                boardManager.CreateItemOnEmptyTile(
                    itemData,
                    emptyBoard[0],
                    uiButtons.mergeBonusButtonPos,
                    false
                );

                dataManager.SaveBonus();
            }
            else
            {
                popupManager.Pop(
                    LOCALE.Get("pop_board_full"),
                    uiButtons.mergeBonusButtonPos,
                    SoundManager.SoundType.Buzz,
                    true
                );
            }
        }
    }
}