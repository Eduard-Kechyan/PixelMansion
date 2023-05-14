using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeFloor : MonoBehaviour
{
    // Variables
    public float changeSpeed = 0.1f;
    public AudioClip changeSound;

    [Header("Flash")]
    public float alphaSpeed = 1f;
    public float alphaDelay = 0.8f;
    public float maxAlpha = 0.5f;

    [Header("States")]
    [ReadOnly]
    public bool newSprites = false;
    [ReadOnly]
    [SerializeField]
    private bool isChanging = false;
    [ReadOnly]
    private bool isSelected = false;
    [ReadOnly]
    public int spriteOrder = -1;

    [Header("Sprites")]
    public Sprite[] sprites = new Sprite[3];
    public Sprite[] optionSprites = new Sprite[3];

    private List<List<SpriteRenderer>> tiles = new List<List<SpriteRenderer>>();
    private SpriteRenderer overlay;

    private bool alphaUp = true;
    private float alphaDelayTemp = 0f;

    // References
    private Selectable selectable;

    void Start()
    {
        // Cache
        selectable = GetComponent<Selectable>();

        GetTiles();
    }

    void Update()
    {
        // Flash when selected, until unselected
        if (isSelected)
        {
            float alphaAmount = overlay.color.a;
            float newAlphaAmount = 0f;

            if (alphaAmount == maxAlpha)
            {
                alphaDelayTemp = alphaDelay;

                alphaUp = false;
            }

            if (alphaAmount == 0)
            {
                alphaUp = true;
            }

            if (alphaUp)
            {
                alphaDelayTemp -= alphaSpeed * Time.deltaTime;

                if (alphaDelayTemp <= 0)
                {
                    newAlphaAmount = Mathf.MoveTowards
                    (
                        alphaAmount,
                        maxAlpha,
                        alphaSpeed * Time.deltaTime
                    );
                }
            }
            else
            {
                newAlphaAmount = Mathf.MoveTowards
                (
                    alphaAmount,
                    0,
                    alphaSpeed * Time.deltaTime
                );
            }

            overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, newAlphaAmount);
        }
    }

    void GetTiles()
    {
        if (transform.childCount > 0)
        {
            overlay = transform.GetChild(0).GetComponent<SpriteRenderer>();

            List<Transform> preTiles = new List<Transform>();

            // Start at 1 since 0 is the overlay
            for (int i = 1; i < transform.childCount; i++)
            {
                preTiles.Add(transform.GetChild(i));
            }

            //tiles.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));
            preTiles.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

            int count = 0;

            for (int i = 0; i < transform.childCount - 1; i++)
            {
                // First entry
                if (i == 0)
                {
                    List<SpriteRenderer> tempTiles = new List<SpriteRenderer>{
                        preTiles[i].GetComponent<SpriteRenderer>()
                    };

                    // Set the sorting order to the parent
                    selectable.order = tempTiles[0].sortingOrder;

                    tiles.Add(tempTiles);
                }
                else
                {
                    if (preTiles[i].transform.position.x == preTiles[i - 1].transform.position.x)
                    {
                        tiles[count].Add(preTiles[i].GetComponent<SpriteRenderer>());
                    }
                    else
                    {
                        List<SpriteRenderer> tempTiles = new List<SpriteRenderer>{
                            preTiles[i].GetComponent<SpriteRenderer>()
                        };

                        tiles.Add(tempTiles);

                        count++;
                    }
                }
            }
        }
    }

    public void Select()
    {
        isSelected = true;
    }

    public void Unselect()
    {
        isSelected = false;

        // Reset flashing
        overlay.material.SetFloat("_FlashAmount", 0);
    }

    public void Change(int order)
    {
        if (isChanging)
        {
            StopCoroutine("ChangeSprites");
        }

        StartCoroutine(ChangeSprites(order));

        if (!newSprites)
        {
            newSprites = true;
        }
    }

    IEnumerator ChangeSprites(int order)
    {
        isChanging = true;

        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].material.SetFloat("_FlashAmount", 1);

                SoundManager.Instance.PlaySound("", 0, changeSound);

                tiles[i][j].sprite = sprites[order];

                spriteOrder = order;
            }

            yield return new WaitForSeconds(changeSpeed);

            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].material.SetFloat("_FlashAmount", 0);
            }
        }

        isChanging = false;
    }
}

