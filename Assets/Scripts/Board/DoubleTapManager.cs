using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleTapManager : MonoBehaviour
{
    private BoardInteractions interactions;
    private InitializeBoard initializeBoard;
    private DataManager dataManager;
    private ItemHandler itemHandler;
    public GameObject boardTiles;

    void Start()
    {
        // Cache
        interactions = GetComponent<BoardInteractions>();
        initializeBoard = GetComponent<InitializeBoard>();

        dataManager = DataManager.Instance;
        itemHandler = dataManager.GetComponent<ItemHandler>();
    }

    public void DoubleTapped()
    {
        if (
            interactions.currentItem.type == Types.Type.Gen
            && interactions.currentItem.creates.Length > 0
        )
        {
            List<int> emptyTiles = new List<int>();

            for (int i = 0; i < GameData.boardData.Length; i++)
            {
               /* if (GameData.boardData[i].sprite == null)
                {
                    emptyTiles.Add(i);
                }*/
            }

            if (emptyTiles.Count > 0)
            {
                SelectRadnomGroupAndItem(emptyTiles);
            }
        }
    }

    void SelectRadnomGroupAndItem(List<int> emptyTiles)
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
        for (int i = 0; i < GameData.itemsData.Length; i++)
        {
            if (GameData.itemsData[i].group == selectedGroup)
            {
                CreateItemOnEmptyTile(
                    GameData.itemsData[i].content[0].sprite.name,
                    GameData.itemsData[i].content[0].group,
                    interactions.currentItem.transform.parent.gameObject,
                    emptyTiles
                );
            }
        }
    }

    void CreateItemOnEmptyTile(
        string spriteName,
        Types.Group group,
        GameObject parentTile,
        List<int> emptyTiles
    )
    {
        /*int tileOrder = interactions.GetTileOrder(parentTile);

        int currentDistance = 1000;
        int currentOrder = 1000;

        for (int i = 0; i < emptyTiles.Count; i++)
        {
            int newDistance = 0;

            if (emptyTiles[i] < GameData.HEIGHT)
            {
                newDistance = Mathf.Abs(emptyTiles[i] - tileOrder);
            }
            else
            {
                int offset = emptyTiles[i] / GameData.HEIGHT;

                newDistance = Mathf.Abs(emptyTiles[i] - (offset * GameData.HEIGHT) - tileOrder);
            }
            
            Debug.Log(newDistance);

            if (currentDistance > newDistance)
            {
                currentDistance = newDistance;

                currentOrder = emptyTiles[i];
            }
            else if (currentDistance == newDistance)
            {
                int[] values = new int[2];

                values[0] = currentOrder;
                values[1] = emptyTiles[i];

                System.Random r = new System.Random();

                int result = values[r.Next(values.Length)];

                if (result == emptyTiles[i])
                {
                    currentOrder = emptyTiles[i];
                }
                else if (currentOrder == 1000)
                {
                    currentOrder = emptyTiles[i];
                }
            }
        }

        GameObject emptyTile = boardTiles.transform.GetChild(currentOrder).gameObject;

        // Create the item on the board
        Item newItem = itemHandler.CreateItem(
            emptyTile,
            initializeBoard.tileSize,
            group,
            spriteName
        );

        GameData.boardData[currentOrder] = new Types.Board
        {
            sprite = newItem.sprite,
            type = newItem.type,
            group = newItem.group,
            genGroup = newItem.genGroup,
            state = newItem.state,
            crate = 0
        };

        dataManager.SaveBoard();*/
    }
}
