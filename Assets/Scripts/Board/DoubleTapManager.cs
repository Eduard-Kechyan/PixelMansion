using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class DoubleTapManager : MonoBehaviour
    {
        // Variables
        public float moveSpeed = 14f;
        public float scaleSpeed = 8f;
        public ClockManager clockManager;
        public TimeManager timeManager;

        [Tooltip("Will automatically adjust")]
        public AnimationCurve[] randomGoldCurves;
        [Tooltip("Will automatically adjust")]
        public AnimationCurve[] generationCurves;

        [Tooltip("Lower values, higher chance. Default is 8")]
        public int gemChance = 8;

        // References
        private BoardInteractions interactions;
        private BoardManager boardManager;
        private PopupManager popupManager;
        private BoardIndication boardIndication;
        private DataManager dataManager;
        private GameData gameData;
        private I18n LOCALE;
        private EnergyMenu energyMenu;
        private ValuePop valuePop;
        private LevelMenu levelMenu;

        private GameObject itemParent;

        void Start()
        {
            // Cache
            interactions = GetComponent<BoardInteractions>();
            boardManager = GetComponent<BoardManager>();
            popupManager = PopupManager.Instance;
            boardIndication = GetComponent<BoardIndication>();
            dataManager = DataManager.Instance;
            levelMenu = GameRefs.Instance.levelMenu;
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

                case Types.Type.Chest:
                    DoubleTappedChest();
                    return true;

                default:

                    return false;
            }
        }

        void DoubleTappedGenerator()
        {
            if (interactions.currentItem.creates.Length > 0 && interactions.currentItem.level >= interactions.currentItem.generatesAt && !interactions.currentItem.timerOn)
            {
                // Check if we have enough energy to generate an item
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

                        SelectRandomGroupAndItem(emptyBoard[0], tile.transform.position);

                        timeManager.CheckCoolDown(interactions.currentItem);
                    }
                    else
                    {
                        popupManager.Pop(
                            LOCALE.Get("pop_board_full"),
                            interactions.currentItem.transform.position,
                            "Buzz",
                            true
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
            if (levelMenu != null && interactions.currentItem.collGroup == Types.CollGroup.Experience)
            {
                levelMenu.isRewarding = true;
            }

            valuePop.PopValue(
                interactions.currentItem.level,
                interactions.currentItem.collGroup,
                interactions.currentItem.transform.position,
                true,
                false,
                () =>
                {
                    levelMenu.isRewarding = false;
                }
            );

            itemParent = interactions.currentItem.transform.parent.gameObject;

            interactions.currentItem.ScaleToSize(Vector2.zero, scaleSpeed, true, RemoveCollectableFromTheBoard);
        }

        void DoubleTappedChest()
        {
            if (!interactions.currentItem.chestLocked && !interactions.currentItem.timerOn)
            {
                // Check if we have enough energy to generate an item
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

                        SelectRandomItemFromChest(emptyBoard[0], tile.transform.position, interactions.currentItem.chestItems == 1);
                    }
                    else
                    {
                        popupManager.Pop(
                            LOCALE.Get("pop_board_full"),
                            interactions.currentItem.transform.position,
                            "Buzz",
                            true
                        );
                    }
                }
                else
                {
                    energyMenu.Open();
                }
            }
        }

        void SelectRandomGroupAndItem(Types.BoardEmpty emptyBoard, Vector2 initialPosition)
        {
            Types.Creates[] creates = interactions.currentItem.creates;

            System.Random random = new();
            double diceRoll = random.NextDouble();
            double cumulative = 0.0;

            ItemTypes.Group selectedGroup = new();

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

            int selectedItem;

            // Randomly create an item from the selected group
            generationCurves[interactions.currentItem.level - interactions.currentItem.generatesAt].keys[^1].value = dataManager.GetGroupCount(selectedGroup);

            selectedItem = Mathf.FloorToInt(Glob.CalcCurvedChances(generationCurves[interactions.currentItem.level - interactions.currentItem.generatesAt]));

            //Debug.Log(selectedItem);

            // Create item from selected group
            for (int i = 0; i < gameData.itemsData.Length; i++)
            {
                if (gameData.itemsData[i].group == selectedGroup)
                {
                    boardManager.CreateItemOnEmptyTile(
                        gameData.itemsData[i].content[selectedItem],
                        emptyBoard,
                        initialPosition,
                        true,
                        true
                    );
                }
            }
        }

        void RemoveCollectableFromTheBoard()
        {
            boardManager.RemoveBoardData(itemParent);

            itemParent = null;
        }

        void SelectRandomItemFromChest(Types.BoardEmpty emptyBoard, Vector2 initialPosition, bool last = false)
        {
            if (interactions.currentItem.chestGroup == Types.ChestGroup.Energy)
            {
                int energyCollCount = 0;
                int randomEnergyOrder;

                Types.Item energyItems = new Types.Item();

                for (int i = 0; i < gameData.collectablesData.Length; i++)
                {
                    if (gameData.collectablesData[i].collGroup == Types.CollGroup.Energy)
                    {
                        energyItems = gameData.collectablesData[i];

                        energyCollCount = gameData.collectablesData[i].content.Length;
                    }
                }

                randomGoldCurves[interactions.currentItem.level - 1].keys[^1].value = energyCollCount;

                randomEnergyOrder = Mathf.FloorToInt(Glob.CalcCurvedChances(randomGoldCurves[interactions.currentItem.level - 1]));

                boardManager.RemoveItemForChest(interactions.currentItem);

                boardManager.CreateItemOnEmptyTile(
                    energyItems.content[randomEnergyOrder],
                    emptyBoard,
                    initialPosition,
                    true,
                    true
                );
            }
            else if (interactions.currentItem.chestGroup == Types.ChestGroup.Piggy)
            {
                int randomCollTypeOrder = Random.Range(0, gemChance - interactions.currentItem.level);
                int collCount = 0;
                int randomCollOrder = 0;

                Types.Item collItems = new Types.Item();

                // Make sure at least once a gem is created
                if (last && !interactions.currentItem.gemPopped)
                {
                    randomCollTypeOrder = gemChance - interactions.currentItem.level - 1;
                }

                if (randomCollTypeOrder == gemChance - interactions.currentItem.level - 1)
                {
                    // Get random gem
                    for (int i = 0; i < gameData.collectablesData.Length; i++)
                    {
                        if (gameData.collectablesData[i].collGroup == Types.CollGroup.Gems)
                        {
                            collItems = gameData.collectablesData[i];

                            interactions.currentItem.gemPopped = true;
                        }
                    }
                }
                else
                {
                    // Get random gold
                    for (int i = 0; i < gameData.collectablesData.Length; i++)
                    {
                        if (gameData.collectablesData[i].collGroup == Types.CollGroup.Gold)
                        {
                            collItems = gameData.collectablesData[i];

                            collCount = gameData.collectablesData[i].content.Length;
                        }
                    }

                    randomGoldCurves[interactions.currentItem.level - 1].keys[^1].value = collCount;

                    randomCollOrder = Mathf.FloorToInt(Glob.CalcCurvedChances(randomGoldCurves[interactions.currentItem.level - 1]));
                }

                boardManager.RemoveItemForChest(interactions.currentItem);

                boardManager.CreateItemOnEmptyTile(
                    collItems.content[randomCollOrder],
                    emptyBoard,
                    initialPosition,
                    true,
                    true
                );
            }
            else
            {
                //interactions.currentItem.chestItems--;
                // TODO - Create items on the board
            }
        }
    }
}