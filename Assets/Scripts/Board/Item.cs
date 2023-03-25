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

    // Bools
    public bool isSelected;
    public bool isCompleted;
    public bool isDragging;
    public bool isPlaying;
    public bool isMaxLavel;
    public bool hasLevel;

    public int level = 1;

    public Types.Group group;
    public Types.GenGroup genGroup;
    public Types.Creates[] creates;

    public Types.GenGroup[] parents;

    public Types.State state = Types.State.Default;

    public Types.Type type = Types.Type.Default;

    [HideInInspector]
    public Sprite sprite = null;

    [HideInInspector]
    public Sprite crateSprite = null;

    private float moveSpeed;
    private float scaleSpeed;
    private bool isMoving = false;
    private bool isScaling = false;
    private bool destroy;
    private Vector2 position;
    private Vector2 scale;
    private Animation anim;
    private Action callback;

    private GameObject selectionChild;
    private GameObject crateChild;
    private GameObject lockerChild;
    public GameObject itemChild;
    private GameObject completionChild;

    void Start()
    {
        // Cache animation
        anim = GetComponent<Animation>();

        // Cache children
        selectionChild = transform.Find("Selection").gameObject;
        crateChild = transform.Find("Crate").gameObject;
        lockerChild = transform.Find("Locker").gameObject;
        itemChild = transform.Find("Item").gameObject;
        completionChild = transform.Find("Completion").gameObject;

        CheckChildren();

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

        if (isPlaying && !anim.IsPlaying("ItemSelect"))
        {
            isPlaying = false;
        }
    }

    void SetItemInitial()
    {
        if (hasLevel)
        {
            itemLevelName = itemName + " (Level " + level + ")";
        }

        if (type == Types.Type.Default)
        {
            nextSpriteName = group + "Item" + (level + 1);
        }
        else if (type == Types.Type.Gen)
        {
            nextSpriteName = genGroup + "Gen" + (level + 1);
        }

        itemChild.GetComponent<SpriteRenderer>().sprite = sprite;
        crateChild.GetComponent<SpriteRenderer>().sprite = crateSprite;

        CheckChildren();
    }

    void CheckChildren()
    {
        if (state == Types.State.Crate)
        {
            crateChild.SetActive(true);
            itemChild.SetActive(false);
        }

        if (state == Types.State.Locker)
        {
            lockerChild.SetActive(true);
            crateChild.SetActive(false);
            itemChild.SetActive(true);
        }

        if (state == Types.State.Default)
        {
            lockerChild.SetActive(false);
            crateChild.SetActive(false);
            itemChild.SetActive(true);
        }

        if (isCompleted)
        {
            completionChild.SetActive(true);
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

    public void Select(float newSpeed, bool animate = true)
    {
        isSelected = true;

        selectionChild.SetActive(true);

        if (animate)
        {
            anim["ItemSelect"].speed = newSpeed;
            anim.Play("ItemSelect");

            isPlaying = true;
        }
    }

    public void Unselect()
    {
        isSelected = false;

        selectionChild.SetActive(false);
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
}
