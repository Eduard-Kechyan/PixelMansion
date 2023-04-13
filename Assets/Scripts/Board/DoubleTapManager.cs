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
    private InitializeBoard initializeBoard;
    private DataManager dataManager;
    private ItemHandler itemHandler;
    private GameData gameData;
    private SoundManager soundManager;
    private BoardPopup boardPopup;
    private EnergyMenu energyMenu;

    private I18n LOCALE = I18n.Instance;

    void Start()
    {
        // Cache
        interactions = GetComponent<BoardInteractions>();
        initializeBoard = GetComponent<InitializeBoard>();
        boardManager = GetComponent<BoardManager>();
        boardPopup = GetComponent<BoardPopup>();

        dataManager = DataManager.Instance;
        gameData = GameData.Instance;
        soundManager = SoundManager.Instance;
        itemHandler = dataManager.GetComponent<ItemHandler>();
        energyMenu = MenuManager.Instance.GetComponent<EnergyMenu>();
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
            List<Types.BoardEmpty> distances = new List<Types.BoardEmpty>();

            GameObject tile = interactions.currentItem.transform.parent.gameObject;

            Vector2Int tileLoc = boardManager.GetBoardLocation(0, tile);

            // Get all empty items from the board data
            for (int x = 0; x < gameData.boardData.GetLength(0); x++)
            {
                for (int y = 0; y < gameData.boardData.GetLength(1); y++)
                {
                    if (gameData.boardData[x, y].sprite == null)
                    {
                        distances.Add(
                            new Types.BoardEmpty
                            {
                                order = gameData.boardData[x, y].order,
                                loc = boardManager.GetBoardLocation(gameData.boardData[x, y].order),
                                distance = CalculateDistance(tileLoc.x, tileLoc.y, x, y),
                            }
                        );
                    }
                }
            }

            // Check if the board is full
            if (distances.Count > 0)
            {
                // Check if we have any energy left
                if (gameData.energy > 0)
                {
                    distances.Sort((p1, p2) => p1.distance.CompareTo(p2.distance));

                    SelectRadnomGroupAndItem(distances[0], tile);
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
                CreateItemOnEmptyTile(
                    gameData.itemsData[i].content[0].sprite.name,
                    gameData.itemsData[i].content[0].group,
                    emptyBoard,
                    tile
                );
            }
        }
    }

    void CreateItemOnEmptyTile(
        string spriteName,
        Types.Group group,
        Types.BoardEmpty emptyBoard,
        GameObject tile
    )
    {
        GameObject emptyTile = boardManager.boardTiles.transform
            .GetChild(emptyBoard.order)
            .gameObject;

        // Create the item on the board
        Item newItem = itemHandler.CreateItem(
            emptyTile,
            initializeBoard.tileSize,
            group,
            spriteName
        );

        Vector2 tempScale = new Vector2(
            newItem.transform.localScale.x,
            newItem.transform.localScale.y
        );

        newItem.transform.GetChild(3).GetComponent<SpriteRenderer>().sortingOrder = 2;
        newItem.gameObject.layer = LayerMask.NameToLayer("ItemDragging");

        // Play generating audio
        soundManager.PlaySFX("Generate", 0.3f);

        newItem.transform.position = tile.transform.position;

        newItem.transform.localScale = Vector2.zero;

        newItem.MoveAndScale(emptyTile.transform.position, tempScale, moveSpeed, scaleSpeed);

        gameData.boardData[emptyBoard.loc.x, emptyBoard.loc.y] = new Types.Board
        {
            sprite = newItem.sprite,
            type = newItem.type,
            group = newItem.group,
            genGroup = newItem.genGroup,
            state = newItem.state,
            crate = 0,
            order = emptyBoard.order
        };

        dataManager.UnlockItem(newItem.sprite.name, newItem.type, newItem.group, newItem.genGroup);

        gameData.UpdateEnergy(-1);

        dataManager.SaveBoard();
    }

    // Calculate the distance between two points in a 2d array
    float CalculateDistance(int currentX, int currentY, int otherX, int otherY)
    {
        float distance = Mathf.Sqrt(
            (currentX - otherX) * (currentX - otherX) + (currentY - otherY) * (currentY - otherY)
        );

        return distance;
    }
}
