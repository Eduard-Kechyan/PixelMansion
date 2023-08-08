using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    // Names
    [ReadOnly]
    public string itemName = "";

    [HideInInspector]
    public string itemLevelName = "";

    [HideInInspector]
    public string nextSpriteName;

    [HideInInspector]
    public string nextName = "";

    [HideInInspector]
    public string id = "";

    // Bools
    public bool isSelected;
    public bool isCompleted;
    public bool isDragging;
    public bool isPlaying;
    public bool isMaxLavel;
    public bool isIndicating;
    public bool hasLevel;

    public int level = 1;
    public int generatesAt = 0;

    public Types.Type type = Types.Type.Item;
    public ItemTypes.Group group;
    public ItemTypes.GenGroup genGroup;
    public Types.CollGroup collGroup;
    public Types.ChestGroup chestGroup;
    public Types.State state = Types.State.Default;
    public Types.Creates[] creates;
    public ItemTypes.GenGroup[] parents;

    // Timer and Chest
    public bool chestLocked; // TODO - Check this
    public bool timerOn; // TODO - Check this
    public bool hasTimer;
    public DateTime startTime;
    public int seconds;
    private Coroutine chestCoroutine;
    public int chestItems;
    public bool chestItemsSet;
    public bool gemPoped = false;

    [HideInInspector]
    public Sprite sprite = null;

    [HideInInspector]
    public Sprite crateSprite = null;

    private float moveSpeed;
    private float scaleSpeed;
    [HideInInspector]
    public bool isMoving = false;
    private bool isScaling = false;
    private bool isMovingAndScaling = false;
    private bool destroy;
    public bool useOverlay = false;
    private Vector2 position;
    private Vector2 scale;
    private Animation anim;
    private Action callback;

    private GameObject selectionChild;
    public GameObject crateChild;
    private GameObject lockerChild;
    private GameObject bubbleChild;
    public GameObject itemChild;
    private GameObject completionChild;

    void Start()
    {
        // Cache animation
        anim = GetComponent<Animation>();

        // Cache children
        selectionChild = transform.GetChild(0).gameObject; // Selection
        crateChild = transform.GetChild(1).gameObject; // Crate
        lockerChild = transform.GetChild(2).gameObject; // Locker
        bubbleChild = transform.GetChild(3).gameObject; // Bubble
        itemChild = transform.GetChild(4).gameObject; // Item
        completionChild = transform.GetChild(5).gameObject; // Completion

        CheckChildren();

        GenerateRandomId();

        SetItemInitial();
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                position,
                moveSpeed * Time.deltaTime
            );

            if (transform.position.x == position.x && transform.position.y == position.y)
            {
                position = Vector2.zero;
                isMoving = false;

                callback?.Invoke(); // Null propagation
            }

            return;
        }

        if (isScaling)              
        {
            transform.localScale = Vector2.MoveTowards(
                transform.localScale,
                scale,
                scaleSpeed * Time.deltaTime
            );

            if (Vector2.Distance(transform.localScale, scale) < 0.01f)
            {
                scale = Vector2.zero;
                isScaling = false;

                callback?.Invoke(); // Null propagation

                if (destroy)
                {
                    destroy = false;
                    Destroy(gameObject);
                }
                else
                {
                    gameObject.layer = LayerMask.NameToLayer("Item");
                }
            }

            return;
        }

        if (isMovingAndScaling)
        {
            transform.localScale = Vector2.MoveTowards(
                transform.localScale,
                scale,
                scaleSpeed * Time.deltaTime
            );

            if (Vector2.Distance(transform.localScale, scale) < 0.3f)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    position,
                    moveSpeed * Time.deltaTime
                );

                if (
                    Vector2.Distance(transform.localScale, scale) < 0.01f
                    && Mathf.Abs(transform.position.x - position.x) < 0.03f
                    && Mathf.Abs(transform.position.y - position.y) < 0.03f
                )
                {
                    transform.position = new Vector2(position.x, position.y);

                    position = Vector2.zero;
                    scale = Vector2.zero;
                    isMovingAndScaling = false;

                    itemChild.GetComponent<SpriteRenderer>().sortingOrder = 1;
                    gameObject.layer = LayerMask.NameToLayer("Item");
                }
            }

            return;
        }

        if (isPlaying && !anim.IsPlaying("ItemSelect"))
        {
            isPlaying = false;
        }
    }

    void SetItemInitial()
    {
        if (hasLevel)
        {
            if (isMaxLavel)
            {
                itemLevelName = itemName + " (Max Level " + level + ")";
            }
            else
            {
                itemLevelName = itemName + " (Level " + level + ")";
            }
        }
        else
        {
            itemLevelName = itemName;
        }

        switch (type)
        {
            case Types.Type.Item:
                nextSpriteName = group + "Item" + (level + 1);
                break;
            case Types.Type.Gen:
                nextSpriteName = genGroup + "Gen" + (level + 1);
                break;
            case Types.Type.Coll:
                nextSpriteName = collGroup + "Coll" + (level + 1);
                break;
            case Types.Type.Chest:
                nextSpriteName = chestGroup + "Chest" + (level + 1);
                break;
            default:
                ErrorManager.Instance.Throw(Types.ErrorType.Code, "Wrong type: " + type);
                break;
        }

        itemChild.GetComponent<SpriteRenderer>().sprite = sprite;
        crateChild.GetComponent<SpriteRenderer>().sprite = crateSprite;

        CheckChildren();
    }

    void GenerateRandomId()
    {
        Guid guid = Guid.NewGuid();

        id = guid.ToString();
    }

    void CheckChildren()
    {
        if (state == Types.State.Crate)
        {
            crateChild.SetActive(true);
            itemChild.SetActive(false);
            bubbleChild.SetActive(false);
        }

        if (state == Types.State.Locker)
        {
            lockerChild.SetActive(true);
            crateChild.SetActive(false);
            itemChild.SetActive(true);
            bubbleChild.SetActive(false);
        }

        if (state == Types.State.Default)
        {
            lockerChild.SetActive(false);
            crateChild.SetActive(false);
            itemChild.SetActive(true);
            bubbleChild.SetActive(false);
        }

        if (state == Types.State.Bubble)
        {
            lockerChild.SetActive(false);
            crateChild.SetActive(false);
            itemChild.SetActive(false);
            bubbleChild.SetActive(true);
        }

        if (isCompleted)
        {
            completionChild.SetActive(true);
        }
        else
        {
            completionChild.SetActive(false);
        }
    }

    public void OpenCrate()
    {
        if (state == Types.State.Crate)
        {
            state = Types.State.Locker;

            CheckChildren();
        }
    }

    public void UnlockLock()
    {
        if (state == Types.State.Locker)
        {
            state = Types.State.Default;

            CheckChildren();
        }
    }

    public void PopBubble()
    {
        if (state == Types.State.Bubble)
        {
            state = Types.State.Default;

            CheckChildren();
        }
    }

    public void UnlockChest()
    {
        if (chestLocked)
        {
            chestLocked = false;

            // TODO - Start the timer
            timerOn = true;

            chestCoroutine = Glob.SetTimeout(() =>
            {
                SpeedUpChest();
            }, 5f);
        }
    }

    public void SpeedUpChest()
    {
        // TODO - Stop the timer
        timerOn = false;

        Glob.StopTimeout(chestCoroutine);
    }

    public void Dragging()
    {
        isDragging = true;

        if (isCompleted)
        {
            completionChild.SetActive(false);
        }
    }

    public void Dropped()
    {
        isDragging = false;

        if (isCompleted)
        {
            completionChild.SetActive(true);
        }
    }

    public void IncreaseSortingOrder()
    {
        itemChild.GetComponent<SpriteRenderer>().sortingOrder = 2;
    }

    public void DecreaseSortingOrder()
    {
        itemChild.GetComponent<SpriteRenderer>().sortingOrder = 1;
    }

    public void Select(float newSpeed, bool animate = true)
    {
        isSelected = true;

        selectionChild.SetActive(true);

        if (animate)
        {
            Animate(newSpeed);
        }
    }

    public void Unselect()
    {
        isSelected = false;

        selectionChild.SetActive(false);

        StopAnimate();
    }

    public void Animate(float newSpeed, bool indicating = false)
    {
        anim["ItemSelect"].speed = newSpeed;
        anim.Play("ItemSelect");

        if (indicating)
        {
            isIndicating = true;
        }

        isPlaying = true;
    }

    public void StopAnimate(bool indicating = false)
    {
        isPlaying = false;

        if (indicating)
        {
            isIndicating = false;
        }

        // These 4 steps rewind and stop the animator
        // Just stopping or rewinding doesn't seem to work
        anim.Rewind();
        anim.Play();
        anim.Sample();
        anim.Stop();
    }

    public void MoveToPos(Vector2 newPos, float newSpeed, Action newCallback = null)
    {
        isMoving = true;
        position = newPos;
        moveSpeed = newSpeed;
        callback = newCallback;
    }

    public void ScaleToSize(
        Vector2 newScale,
        float newSpeed,
        bool destroyItem,
        Action newCallback = null
    )
    {
        isScaling = true;
        scale = newScale;
        scaleSpeed = newSpeed;
        destroy = destroyItem;
        callback = newCallback;

        if (destroyItem)
        {
            gameObject.transform.parent = null;
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            gameObject.layer = LayerMask.NameToLayer("ItemBusy");
        }
    }

    public void MoveAndScale(
        Vector2 newPos,
        Vector2 newScale,
        float newMoveSpeed,
        float newScaleSpeed
    )
    {
        isMovingAndScaling = true;
        position = newPos;
        scale = newScale;
        moveSpeed = newMoveSpeed;
        scaleSpeed = newScaleSpeed;
    }
}
