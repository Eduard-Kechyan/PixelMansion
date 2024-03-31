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
        public AnimationCurve generationCurve;

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
            if (interactions.currentItem.creates.Length > 0 && interactions.currentItem.level >= interactions.currentItem.generatesAtLevel && !interactions.currentItem.timerOn)
            {
                // Check if we have enough energy to generate an item
                if (gameData.energy >= 1)
                {
                    GameObject tile = interactions.currentItem.transform.parent.gameObject;

                    Vector2Int tileLoc = boardManager.GetBoardLocation(0, tile);

                    List<Types.TileEmpty> emptyTile = boardManager.GetEmptyTileItems(tileLoc);

                    // Check if the board is full
                    if (emptyTile.Count > 0)
                    {
                        emptyTile.Sort((p1, p2) => p1.distance.CompareTo(p2.distance));

                        boardIndication.StopPossibleMergeCheck();

                        SelectRandomGroupAndItem(emptyTile[0], tile.transform.position);

                        timeManager.CheckCoolDown(interactions.currentItem);
                    }
                    else
                    {
                        popupManager.Pop(
                            LOCALE.Get("pop_board_full"),
                            interactions.currentItem.transform.position,
                            Types.SoundType.Buzz,
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
            if (!interactions.currentItem.timerOn)
            {
                if (interactions.currentItem.chestGroup == Types.ChestGroup.Item && !interactions.currentItem.chestOpen)
                {
                    return;
                }

                GameObject tile = interactions.currentItem.transform.parent.gameObject;

                Vector2Int tileLoc = boardManager.GetBoardLocation(0, tile);

                List<Types.TileEmpty> emptyTile = boardManager.GetEmptyTileItems(tileLoc);

                // Check if the board is full
                if (emptyTile.Count > 0)
                {
                    emptyTile.Sort((p1, p2) => p1.distance.CompareTo(p2.distance));

                    boardIndication.StopPossibleMergeCheck();

                    SelectRandomItemFromChest(emptyTile[0], tile.transform.position, interactions.currentItem.chestItems == 1);
                }
                else
                {
                    popupManager.Pop(
                        LOCALE.Get("pop_board_full"),
                        interactions.currentItem.transform.position,
                        Types.SoundType.Buzz,
                        true
                    );
                }
            }
        }

        void SelectRandomGroupAndItem(Types.TileEmpty emptyTile, Vector2 initialPosition)
        {
            Types.Creates[] creates = interactions.currentItem.creates;

            System.Random random = new();
            double diceRoll = random.NextDouble();
            double cumulative = 0.0;

            ItemTypes.Group selectedGroup = new();
            int maxLevel = 0;
            bool canIncreaseMaxLevel = false;

            int selectedItem = 0;

            // Randomly select a group of items to choose from
            for (int i = 0; i < creates.Length; i++)
            {
                cumulative += creates[i].chance / 100;

                if (diceRoll < cumulative)
                {
                    selectedGroup = creates[i].group;
                    maxLevel = creates[i].maxLevel;
                    canIncreaseMaxLevel = creates[i].canIncreaseMaxLevel;

                    break;
                }
            }

            // If "canIncreaseMaxLevel" is false, generate only the first item in the group
            // Else generate according to the generators level
            if (canIncreaseMaxLevel)
            {
                int generateAtLevel = interactions.currentItem.level - interactions.currentItem.generatesAtLevel;

                int groupItemsCount = dataManager.GetGroupItemsCount(selectedGroup);

                int currentItemsCount = maxLevel + generateAtLevel;

                if (currentItemsCount > groupItemsCount)
                {
                    currentItemsCount = groupItemsCount;
                }

                if (currentItemsCount < 1)
                {
                    currentItemsCount = 1;
                }

                // Randomly select an item from the selected group
                selectedItem = Mathf.FloorToInt(Glob.CalcCurvedChances(generationCurve) * currentItemsCount);

                if (selectedItem < 0)
                {
                    selectedItem = 0;
                }
            }

            // Create item from selected group
            for (int i = 0; i < gameData.itemsData.Length; i++)
            {
                if (gameData.itemsData[i].group == selectedGroup)
                {
                    if (selectedItem >= gameData.itemsData[i].content.Length)
                    {
                        selectedItem = gameData.itemsData[i].content.Length - 1;
                    }

                    boardManager.CreateItemOnEmptyTile(
                        gameData.itemsData[i].content[selectedItem],
                        emptyTile,
                        initialPosition,
                        true,
                        true
                    );

                    break;
                }
            }
        }

        void RemoveCollectableFromTheBoard()
        {
            boardManager.RemoveBoardData(itemParent);

            itemParent = null;
        }

        void SelectRandomItemFromChest(Types.TileEmpty emptyTile, Vector2 initialPosition, bool last = false)
        {
            if (interactions.currentItem.chestGroup == Types.ChestGroup.Energy)
            {
                int energyCollCount = 0;
                int randomEnergyOrder;

                Types.Item energyItems = new();

                for (int i = 0; i < gameData.collectablesData.Length; i++)
                {
                    if (gameData.collectablesData[i].collGroup == Types.CollGroup.Energy)
                    {
                        energyItems = gameData.collectablesData[i];

                        energyCollCount = gameData.collectablesData[i].content.Length;

                        break;
                    }
                }

                randomGoldCurves[interactions.currentItem.level - 1].keys[^1].value = energyCollCount;

                randomEnergyOrder = Mathf.FloorToInt(Glob.CalcCurvedChances(randomGoldCurves[interactions.currentItem.level - 1]));

                boardManager.RemoveItemFromChest(interactions.currentItem);

                boardManager.CreateItemOnEmptyTile(
                    energyItems.content[randomEnergyOrder],
                    emptyTile,
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

                Types.Item collItems = new();

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

                            break;
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

                            break;
                        }
                    }

                    randomGoldCurves[interactions.currentItem.level - 1].keys[^1].value = collCount;

                    randomCollOrder = Mathf.FloorToInt(Glob.CalcCurvedChances(randomGoldCurves[interactions.currentItem.level - 1]));
                }

                boardManager.RemoveItemFromChest(interactions.currentItem);

                boardManager.CreateItemOnEmptyTile(
                    collItems.content[randomCollOrder],
                    emptyTile,
                    initialPosition,
                    true,
                    true
                );
            }
            else // Types.ChestGroup.Items
            {
                Types.Creates[] creates = interactions.currentItem.creates;

                System.Random random = new();
                double diceRoll = random.NextDouble();
                double cumulative = 0.0;

                string selectedSpriteName = "";
                Types.Type selectedType = new();
                ItemTypes.GenGroup selectedGenGroup = new();
                Types.CollGroup selectedCollGroup = new();

                // Randomly select a group of items to choose from
                for (int i = 0; i < creates.Length; i++)
                {
                    cumulative += creates[i].chance / 100;

                    if (diceRoll < cumulative)
                    {
                        selectedSpriteName = creates[i].sprite.name;
                        selectedType = creates[i].type;
                        selectedGenGroup = creates[i].genGroup;
                        selectedCollGroup = creates[i].collGroup;

                        break;
                    }
                }

                boardManager.RemoveItemFromChest(interactions.currentItem);

                // Create item from selected group
                if (selectedType == Types.Type.Gen)
                {
                    for (int i = 0; i < gameData.generatorsData.Length; i++)
                    {
                        if (gameData.generatorsData[i].genGroup == selectedGenGroup)
                        {
                            for (int j = 0; j < gameData.generatorsData[i].content.Length; j++)
                            {
                                if (gameData.generatorsData[i].content[j].sprite.name == selectedSpriteName)
                                {
                                    boardManager.CreateItemOnEmptyTile(
                                    gameData.generatorsData[i].content[j],
                                    emptyTile,
                                    initialPosition,
                                    true,
                                    true
                                    );

                                    break;
                                }
                            }

                            break;
                        }
                    }
                }
                else //Types.Type.Coll
                {
                    for (int i = 0; i < gameData.collectablesData.Length; i++)
                    {
                        if (gameData.collectablesData[i].collGroup == selectedCollGroup)
                        {
                            for (int j = 0; j < gameData.collectablesData[i].content.Length; j++)
                            {
                                if (gameData.collectablesData[i].content[j].sprite.name == selectedSpriteName)
                                {
                                    boardManager.CreateItemOnEmptyTile(
                                        gameData.collectablesData[i].content[j],
                                        emptyTile,
                                        initialPosition,
                                        true,
                                        true
                                    );

                                    break;
                                }
                            }

                            break;
                        }
                    }
                }
            }
        }
    }
}