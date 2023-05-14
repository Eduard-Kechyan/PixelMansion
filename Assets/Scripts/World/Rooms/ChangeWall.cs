using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeWall : MonoBehaviour
{
    // Variables
    public Side side = Side.Left;
    [HideInInspector]
    public bool isRight;
    public float changeSpeed = 0.1f;
    public AudioClip changeSound;
    [Condition("isRight", true)]
    public Color shadowColor;
    [Condition("isRight", true)]
    public bool unFlip;

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
    public Sprite[] windowSpritesLeft = new Sprite[3];
    public Sprite[] windowSpritesRight = new Sprite[3];
    public Sprite[] doorFrameSpritesLeft = new Sprite[3];
    public Sprite[] doorFrameSpritesRight = new Sprite[3];
    public Sprite[] optionSprites = new Sprite[3];

    private List<SpriteRenderer> chunks = new List<SpriteRenderer>();
    private SpriteRenderer overlay;

    private bool alphaUp = true;
    private float alphaDelayTemp = 0f;

    // Enums
    public enum Side
    {
        Left,
        Right
    }

    // References
    private Selectable selectable;

    void Start()
    {
        // Cache
        selectable = GetComponent<Selectable>();

        GetChunks();
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

    void OnValidate()
    {
        isRight = side == Side.Right;
    }

    void GetChunks()
    {
        if (transform.childCount > 0)
        {
            overlay = transform.GetChild(0).GetComponent<SpriteRenderer>();

            // Start at 1 since 0 is the overlay
            for (int i = 1; i < transform.childCount; i++)
            {
                for (int j = 0; j < transform.GetChild(i).childCount; j++)
                {
                    SpriteRenderer newChild = transform.GetChild(i).GetChild(j).GetComponent<SpriteRenderer>();

                    // Set side's shadow color if the wall is on the right
                    if (side == Side.Right)
                    {
                        newChild.color = shadowColor;
                    }

                    // Set the sorting order to the parent
                    if (i == 0)
                    {
                        selectable.order = newChild.sortingOrder;
                    }

                    // Check if we shoul't flip the sprites
                    if (unFlip)
                    {
                        newChild.transform.localScale = new Vector3(1, newChild.transform.localScale.y, newChild.transform.localScale.z);
                    }

                    chunks.Add(newChild);
                }
            }

            chunks.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
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

    public void Change(int order, bool flip = true)
    {
        if (isChanging)
        {
            StopCoroutine("ChangeSprites");
        }

        StartCoroutine(ChangeSprites(order, flip));

        if (!newSprites)
        {
            newSprites = true;
        }
    }

    IEnumerator ChangeSprites(int order, bool flip = true)
    {
        isChanging = true;

        for (int i = 0; i < chunks.Count; i++)
        {
            chunks[i].material.SetFloat("_FlashAmount", 1);

            // Check if we shoul flip the sprites
            if (flip && chunks[i].transform.localScale.x == 1)
            {
                chunks[i].transform.localScale = new Vector3(-1, chunks[i].transform.localScale.y, chunks[i].transform.localScale.z);
            }

            if (!flip && chunks[i].transform.localScale.x == -1)
            {
                chunks[i].transform.localScale = new Vector3(1, chunks[i].transform.localScale.y, chunks[i].transform.localScale.z);
            }

            SoundManager.Instance.PlaySound("", 0, changeSound);

            chunks[i].sprite = ConvertOrder(order, chunks[i].name);

            spriteOrder = order;

            yield return new WaitForSeconds(changeSpeed);

            chunks[i].material.SetFloat("_FlashAmount", 0);
        }

        isChanging = false;
    }

    Sprite ConvertOrder(int order, string chunkName)
    {
        if (chunkName.Contains("Window"))
        {
            if (chunkName.Contains("Left"))
            {
                return windowSpritesLeft[order];
            }
            else
            {
                return windowSpritesRight[order];
            }
        }

        if (chunkName.Contains("DoorFrame"))
        {
            if (chunkName.Contains("Left"))
            {
                return doorFrameSpritesLeft[order];
            }
            else
            {
                return doorFrameSpritesRight[order];
            }
        }

        return sprites[order];
    }
}
