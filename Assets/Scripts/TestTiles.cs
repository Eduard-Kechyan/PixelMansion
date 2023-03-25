using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestTiles : MonoBehaviour
{
    public GameObject tile;
    public float tileWidth = 140f;

    public static TestTiles Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void InitializeTestTiles()
    {
        for (int x = 0; x < GameData.WIDTH; x++)
        {
            for (int y = 0; y < GameData.HEIGHT; y++)
            {
                // Calculate tile's positionx
                Vector3 pos = new Vector3(
                    (tileWidth * x) - (540 - tileWidth),
                    -(tileWidth * y) + (540 - tileWidth),
                    0
                );

                // Create tile
                GameObject newTile = Instantiate(tile, pos, tile.transform.rotation);

                newTile.transform.GetChild(0).GetComponent<Text>().text = GameData.testTiles[
                    x,
                    y
                ].order.ToString();

                newTile.transform.SetParent(transform, false);
            }
        }

        CheckTestTiles();
    }

    void CheckTestTiles()
    {
        Types.TestTileDistance foundTile;

        Types.TestTile exampleTile = new Types.TestTile
        {
            order = 3,
            x = 0,
            y = 3
        };

        List<Types.TestTileDistance> sortedTestTiles = new List<Types.TestTileDistance>();

        foreach (Types.TestTile testTile in GameData.testTiles)
        {
            if (exampleTile.order != testTile.order)
            {
                sortedTestTiles.Add(
                new Types.TestTileDistance
                {
                    order = testTile.order,
                    distance = CalculateDistance(
                        exampleTile.x,
                        exampleTile.y,
                        testTile.x,
                        testTile.y
                    ),
                }
            );
            }
        }

        sortedTestTiles.Sort((p1, p2) => p1.distance.CompareTo(p2.distance));

        foundTile = sortedTestTiles[0];

        for (int i = 0; i < sortedTestTiles.Count; i++)
        {
            Debug.Log(
                "Order:" + sortedTestTiles[i].order + " Distance: " + sortedTestTiles[i].distance
            );
        }

        Debug.Log(foundTile.order);

        /////////////////////
        /*GameObject test = transform.GetChild(exampleTile.order).gameObject;

        Debug.Log(test.transform.GetChild(0).GetComponent<Text>().text);
        Debug.Log(GameData.testTiles[exampleTile.x, exampleTile.y].order);*/
    }

    float CalculateDistance(int currentX, int currentY, int otherX, int otherY)
    {
        float distance = Mathf.Sqrt(
            (currentX - otherX) * (currentX - otherX) + (currentY - otherY) * (currentY - otherY)
        );

        return distance;
    }
}
