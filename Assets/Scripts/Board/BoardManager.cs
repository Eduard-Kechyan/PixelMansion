using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [HideInInspector]
    public GameObject boardTiles;

    private DataManager dataManager;
    private GameData gameData;
    private SoundManager soundManager;

    void Start()
    {
        boardTiles = transform.GetChild(0).gameObject;

        dataManager = DataManager.Instance;

        soundManager = SoundManager.Instance;

        gameData = GameData.Instance;
    }

    /////// GET BOARD DATA ////////

    int GetBoardOrder(int checkX, int checkY)
    {
        // Get the item's order from the board by its location

        int count = 0;

        for (int x = 0; x < gameData.boardData.GetLength(0); x++)
        {
            for (int y = 0; y < gameData.boardData.GetLength(1); y++)
            {
                if (x == checkX && y == checkY)
                {
                    return count;
                }

                count++;
            }
        }

        return 0;
    }

    public Vector2Int GetBoardLocation(int order, GameObject tile = null)
    {
        // Get the item's location from the board by its tile's order

        Vector2Int loc = new Vector2Int(0, 0);

        if (tile != null)
        {
            order = int.Parse(tile.gameObject.name[(tile.gameObject.name.LastIndexOf('e') + 1)..]);
        }

        int count = 0;

        for (int x = 0; x < gameData.boardData.GetLength(0); x++)
        {
            for (int y = 0; y < gameData.boardData.GetLength(1); y++)
            {
                if (order == count)
                {
                    {
                        loc = new Vector2Int(x, y);
                        return loc;
                    }
                }

                count++;
            }
        }

        return loc;
    }

    /////// SET BOARD DATA ////////

    public void SwapBoardData(GameObject oldTile, GameObject newTile)
    {
        // Get tile locations
        Vector2Int oldLoc = GetBoardLocation(0, oldTile);
        Vector2Int newLoc = GetBoardLocation(0, newTile);

        // Save items
        Types.Board oldItem = gameData.boardData[oldLoc.x, oldLoc.y];
        Types.Board newItem = gameData.boardData[newLoc.x, newLoc.y];

        // Set items
        gameData.boardData[oldLoc.x, oldLoc.y] = new Types.Board
        {
            sprite = newItem.sprite,
            type = newItem.type,
            group = newItem.group,
            genGroup = newItem.genGroup,
            state = newItem.state,
            crate = newItem.crate,
            order = oldItem.order,
        };

        gameData.boardData[newLoc.x, newLoc.y] = new Types.Board
        {
            sprite = oldItem.sprite,
            type = oldItem.type,
            group = oldItem.group,
            genGroup = oldItem.genGroup,
            state = oldItem.state,
            crate = oldItem.crate,
            order = newItem.order,
        };

        // Save the board to disk
        dataManager.SaveBoard();
    }

    public void MergeBoardData(GameObject oldTile, GameObject newTile, Item newItem)
    {
        // Get tile locations
        Vector2Int oldLoc = GetBoardLocation(0, oldTile);
        Vector2Int newLoc = GetBoardLocation(0, newTile);

        int oldOrder = gameData.boardData[oldLoc.x, oldLoc.y].order;
        int newOrder = gameData.boardData[newLoc.x, newLoc.y].order;

        // Clear old items
        gameData.boardData[oldLoc.x, oldLoc.y] = new Types.Board { order = oldOrder };

        // Set new item
        gameData.boardData[newLoc.x, newLoc.y] = new Types.Board
        {
            sprite = newItem.sprite,
            group = newItem.group,
            state = newItem.state,
            crate = int.Parse(
                newItem.crateSprite.name[(newItem.crateSprite.name.LastIndexOf('e') + 1)..]
            ),
            order = newOrder
        };

        // Save the board to disk
        dataManager.SaveBoard();
    }

    /////// CRATE ////////

    public void CheckForCrate(GameObject tile)
    {
        // Check if the item's neighbors are crates

        Vector2Int loc = GetBoardLocation(0, tile);

        if (loc.x < (GameData.WIDTH - 1))
        {
            FindItemAndOpenCrate(loc.x + 1, loc.y);
        }

        if (loc.x > 0)
        {
            FindItemAndOpenCrate(loc.x - 1, loc.y);
        }

        if (loc.y < (GameData.HEIGHT - 1))
        {
            FindItemAndOpenCrate(loc.x, loc.y + 1);
        }

        if (loc.y > 0)
        {
            FindItemAndOpenCrate(loc.x, loc.y - 1);
        }
    }

    void FindItemAndOpenCrate(int x, int y)
    {
        // Find the the crate on the board and open it
        if (gameData.boardData[x, y].state == Types.State.Crate)
        {
            gameData.boardData[x, y].state = Types.State.Locker;

            // Play crate opening audio
            soundManager.PlaySFX("OpenCrate", 0.3f);

            int order = GetBoardOrder(x, y);

            Transform foundTile = boardTiles.transform.GetChild(order);

            if (foundTile.childCount > 0)
            {
                GameObject foundItem = boardTiles.transform.GetChild(order).GetChild(0).gameObject;

                foundItem.GetComponent<Item>().OpenCrate();
            }

            dataManager.SaveBoard();
        }
    }
}
