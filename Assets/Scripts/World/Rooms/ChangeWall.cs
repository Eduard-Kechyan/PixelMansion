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
    public int spriteOrder = -1;
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
    public bool isOld = true;
    [ReadOnly]
    [SerializeField]
    private bool isChanging = false;
    [ReadOnly]
    private bool isSelected = false;

    [Header("Sprites")]
    public Sprite[] sprites = new Sprite[3];
    public Sprite[] windowSpritesLeft = new Sprite[3];
    public Sprite[] windowSpritesRight = new Sprite[3];
    public Sprite[] doorFrameSpritesLeft = new Sprite[3];
    public Sprite[] doorFrameSpritesRight = new Sprite[3];
    public Sprite[] optionSprites = new Sprite[3];

    // Sprites
    private List<SpriteRenderer> chunks = new List<SpriteRenderer>();
    private List<Sprite> oldChunkSprites = new List<Sprite>();

    // Overlay
    private SpriteRenderer overlay;
    private bool alphaUp = true;
    private float alphaDelayTemp = 0f;

    private bool chunksSet = false;

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

        if (!chunksSet)
        {
            GetChunks();
        }

        SetPositionZ();
    }

    void Update()
    {
        HandleOverlay();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        isRight = side == Side.Right;
    }
#endif

    void GetChunks()
    {
        if (transform.childCount > 0)
        {
            overlay = transform.GetChild(0).GetComponent<SpriteRenderer>();

            List<SpriteRenderer> tempOldChunks = new List<SpriteRenderer>();

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

                    if (isOld)
                    {
                        tempOldChunks.Add(newChild);
                    }
                }
            }

            chunks.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

            if (isOld)
            {
                tempOldChunks.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

                for (int i = 0; i < tempOldChunks.Count; i++)
                {
                    oldChunkSprites.Add(tempOldChunks[i].sprite);
                }
            }

            chunksSet = true;
        }
    }

    void SetPositionZ()
    {
        int parentLayerOrder = SortingLayer.GetLayerValueFromName(transform.parent.gameObject.GetComponent<RoomHandler>().roomSortingLayer);

        int z = parentLayerOrder + (isRight ? 1 : 0); // 0 or 1 is for this gameObjects' order in it's parent

        transform.position = new Vector3(transform.position.x, transform.position.y, z);
    }

    //// SELECT ////
    public void Select()
    {
        isSelected = true;

        SetInitial();
    }

    public void Unselect()
    {
        isSelected = false;

        // Reset flashing
        overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, 0);
    }

    //// SPRITES ////
    public void SetInitial()
    {
        // Set the sprite order to the first one
        if (spriteOrder == -1)
        {
            spriteOrder = 0;
        }

        // Set the sprites
        for (int i = 0; i < chunks.Count; i++)
        {
            chunks[i].sprite = ConvertOrder(spriteOrder, chunks[i].name);
        }
    }

    public void SetSprites(int order, bool alt = false)
    {
        // Stop changing if we are changing
        if (isChanging)
        {
            StopCoroutine("ChangeWallChunks");
        }

        // Set the sprite order
        spriteOrder = order;

        if (alt)
        {
            GetChunks();
        }

        // Set the sprites
        for (int i = 0; i < chunks.Count; i++)
        {
            chunks[i].sprite = ConvertOrder(order, chunks[i].name);
        }
    }

    public void Cancel(int order)
    {
        // Stop changing if we are changing
        if (isChanging)
        {
            StopCoroutine("ChangeWallChunks");
        }

        // Reset the sprites to the old ones
        if (isOld)
        {
            // Reset the sprites to the old ones
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].sprite = oldChunkSprites[i];
            }
        }
        else
        {
            // Reset the sprites to the new ones
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].sprite = ConvertOrder(order, chunks[i].name);
            }
        }
    }

    public void Confirm()
    {
        // Stop changing if we are changing
        if (isChanging)
        {
            StopCoroutine("ChangeWallChunks");
        }

        // Check if we are confirming for the first time
        if (isOld)
        {
            // Reset the sprites to the old ones
            for (int i = 0; i < chunks.Count; i++)
            {
                for (int j = 0; j < chunks.Count; j++)
                {
                    chunks[i].sprite = oldChunkSprites[i];
                }
            }

            StartCoroutine(ChangeWallChunks());

            if (Settings.Instance.vibrationOn && (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer))
            {
                Handheld.Vibrate();
            }

            // Reset overlay flashing
            ResetOverlay();

            isOld = false;
        }
    }

    IEnumerator ChangeWallChunks()
    {
        isChanging = true;

        for (int i = 0; i < chunks.Count; i++)
        {
            chunks[i].material.SetFloat("_FlashAmount", 1);

            SoundManager.Instance.PlaySound("", changeSound);

            chunks[i].sprite = ConvertOrder(spriteOrder, chunks[i].name);

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

    //// OVERLAY ////
    void HandleOverlay()
    {
        // Flash if selected and isn't changing the tiles
        if (isSelected && !isChanging)
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

    void ResetOverlay()
    {
        overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, 0);
    }
}
