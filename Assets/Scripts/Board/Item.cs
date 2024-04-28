using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
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
        public bool isReadyPlaying;
        public bool isMaxLevel;
        public bool isIndicating;
        public bool hasLevel;

        public int level = 1;
        public int generatesAtLevel = 0;

        public Types.Type type = Types.Type.Item;
        public ItemTypes.Group group;
        public ItemTypes.GenGroup genGroup;
        public Types.CollGroup collGroup;
        public Types.ChestGroup chestGroup;
        public Types.State state = Types.State.Default;
        public Types.Creates[] creates;
        public Types.ParentData[] parents;

        // Chest
        public bool chestOpen = false;
        public int chestItems;
        public bool chestItemsSet;
        public bool gemPopped = false;

        // Timer
        public bool timerOn;
        public Types.CoolDown coolDown;

        [HideInInspector]
        public Sprite sprite = null;

        [HideInInspector]
        public int crate = 0;

        private bool loaded = false;

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
        private Action callback;

        // Events
        public delegate void OnInitializedEvent();
        public event OnInitializedEvent OnInitialized;

        // References
        private Animation anim;
        private Animation animReady;

        private GameObject selectionChild;
        public GameObject crateChild;
        private GameObject lockerChild;
        private GameObject bubbleChild;
        public GameObject readyChild;
        public GameObject readyAltChild;
        private GameObject completionChild;
        public GameObject itemChild;

        private SpriteRenderer crateSpriteRenderer;
        private SpriteRenderer lockSpriteRenderer;
        private SpriteRenderer bubbleSpriteRenderer;

        void Start()
        {
            // Cache animation
            anim = GetComponent<Animation>();

            // Cache children
            // TODO - Improve this child getting using placeholder (PH)
            selectionChild = transform.GetChild(0).gameObject; // Selection
            crateChild = transform.GetChild(1).gameObject; // Crate (In SetCrateSprite() too
            lockerChild = transform.GetChild(2).gameObject; // Locker
            bubbleChild = transform.GetChild(3).gameObject; // Bubble
            readyChild = transform.GetChild(4).gameObject; // Ready
            completionChild = transform.GetChild(6).gameObject; // Completion
            itemChild = transform.GetChild(7).gameObject; // Item

            if (type == Types.Type.Gen && level >= generatesAtLevel)
            {
                readyChild.SetActive(true);

                animReady = readyChild.GetComponent<Animation>();

                animReady.enabled = true;

                readyChild.SetActive(false);
            }

            if (type == Types.Type.Chest && chestGroup == Types.ChestGroup.Item)
            {
                readyAltChild = transform.GetChild(5).gameObject; // Ready Chest

                readyAltChild.SetActive(true);
            }

            if (state == Types.State.Crate)
            {
                crateSpriteRenderer = crateChild.GetComponent<SpriteRenderer>();
                lockSpriteRenderer = lockerChild.GetComponent<SpriteRenderer>();
            }
            else if (state == Types.State.Locker)
            {
                lockSpriteRenderer = lockerChild.GetComponent<SpriteRenderer>();
            }
            else if (state == Types.State.Bubble)
            {
                bubbleSpriteRenderer = bubbleChild.GetComponent<SpriteRenderer>();
            }

            OnInitialized?.Invoke();

            SetItemInitial();

            ToggleTimer(timerOn, true);
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

                        callback?.Invoke(); // Null propagation
                    }
                }

                return;
            }
        }

        void SetItemInitial()
        {
            if (hasLevel)
            {
                if (isMaxLevel)
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
                    // ERROR
                    ErrorManager.Instance.Throw(Types.ErrorType.Code, "Item.cs -> SetItemInitial()", "Wrong type: " + type);
                    break;
            }

            itemChild.GetComponent<SpriteRenderer>().sprite = sprite;

            CheckChildren();

            loaded = true;
        }

        void CheckChildren()
        {
            switch (state)
            {
                case Types.State.Default:
                    lockerChild.SetActive(false);
                    crateChild.SetActive(false);
                    itemChild.SetActive(true); //
                    bubbleChild.SetActive(false);

                    if (type == Types.Type.Gen && level >= generatesAtLevel)
                    {
                        if (timerOn)
                        {
                            readyChild.SetActive(false);
                        }
                        else
                        {
                            readyChild.SetActive(true); //
                        }
                    }

                    if (type == Types.Type.Chest && chestGroup == Types.ChestGroup.Item)
                    {
                        if (timerOn)
                        {
                            readyAltChild.SetActive(false);
                        }
                        else
                        {
                            if (!chestOpen)
                            {
                                readyAltChild.SetActive(true); //
                            }
                        }
                    }
                    break;
                case Types.State.Crate:
                    lockerChild.SetActive(false);
                    crateChild.SetActive(true); //
                    itemChild.SetActive(false);
                    bubbleChild.SetActive(false);
                    break;
                case Types.State.Locker:
                    lockerChild.SetActive(true); //
                    crateChild.SetActive(false);
                    itemChild.SetActive(true); //
                    bubbleChild.SetActive(false);
                    break;
                case Types.State.Bubble:
                    lockerChild.SetActive(false);
                    crateChild.SetActive(false);
                    itemChild.SetActive(true); //
                    bubbleChild.SetActive(true); //
                    break;
            }

            if (isCompleted)
            {
                completionChild.SetActive(true); //
            }
            else
            {
                completionChild.SetActive(false);
            }
        }

        public void ToggleTimer(bool enable, bool initial = false)
        {
            timerOn = enable;

            if (state == Types.State.Default && type == Types.Type.Gen && level >= generatesAtLevel)
            {
                if (timerOn)
                {
                    StopAnimateReady();
                }
                else
                {
                    AnimateReady();
                }
            }

            if (!chestOpen && type == Types.Type.Chest && chestGroup == Types.ChestGroup.Item && !enable && !initial)
            {
                chestOpen = true;
            }

            CheckChildren();
        }

        public void OpenCrate(float crateBreakSpeed = 0.05f, float newSpeed = 1f)
        {
            if (state == Types.State.Crate)
            {
                state = Types.State.Locker;

                itemChild.transform.localScale = Vector2.zero;
                itemChild.SetActive(true);

                lockerChild.transform.localScale = Vector2.zero;
                lockerChild.SetActive(true);

                StartCoroutine(WaitForCrateToBreakAnimation(crateBreakSpeed, newSpeed));
            }
        }

        IEnumerator WaitForCrateToBreakAnimation(float crateBreakSpeed, float newSpeed)
        {
            GameRefs.SpriteArray[] crateBreakSprites = GameRefs.Instance.crateBreakSprites;

            WaitForSeconds wait = new(crateBreakSpeed);

            for (int i = 0; i < crateBreakSprites.Length; i++)
            {
                crateSpriteRenderer.sprite = crateBreakSprites[crate].content[i];

                yield return wait;
            }

            anim["PostCrateBreak"].speed = newSpeed;
            anim.Play("PostCrateBreak");

            isPlaying = true;

            while (!anim.IsPlaying("PostCrateBreak"))
            {
                yield return null;
            }

            isPlaying = false;

            CheckChildren();
        }

        public void SetCrateSprite(Sprite newSprite)
        {
            if (crateChild == null)
            {
                crateChild = transform.GetChild(1).gameObject; // Crate
            }

            crateChild.GetComponent<SpriteRenderer>().sprite = newSprite;
        }

        public void UnlockLock(float lockOpenSpeed = 0.05f, bool wait = true)
        {
            if (state == Types.State.Locker)
            {
                state = Types.State.Default;

                if (wait)
                {
                    StartCoroutine(WaitForLockToOpenAnimation(lockOpenSpeed));
                }
                else
                {
                    CheckChildren();
                }
            }
        }

        IEnumerator WaitForLockToOpenAnimation(float lockOpenSpeed)
        {
            Sprite[] lockOpenSprites = GameRefs.Instance.lockOpenSprites;

            WaitForSeconds wait = new(lockOpenSpeed);

            for (int i = 0; i < lockOpenSprites.Length; i++)
            {
                lockSpriteRenderer.sprite = lockOpenSprites[i];

                yield return wait;
            }

            CheckChildren();
        }

        public void PopBubble(bool destroy = false, float bubblePopSpeed = 0.05f)
        {
            if (state == Types.State.Bubble)
            {
                state = Types.State.Default;

                StartCoroutine(WaitForBubbleToPopAnimation(destroy, bubblePopSpeed));
            }
        }

        IEnumerator WaitForBubbleToPopAnimation(bool destroy, float bubblePopSpeed)
        {
            Sprite[] bubblePopSprites = GameRefs.Instance.bubblePopSprites;

            WaitForSeconds wait = new(bubblePopSpeed);

            for (int i = 0; i < bubblePopSprites.Length; i++)
            {
                bubbleSpriteRenderer.sprite = bubblePopSprites[i];

                yield return wait;
            }

            if (destroy)
            {
                Destroy(gameObject);
            }
            else
            {
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

            if (type == Types.Type.Gen && level >= generatesAtLevel && isReadyPlaying)
            {
                StopAnimateReady();
            }

            if (type == Types.Type.Chest && chestGroup == Types.ChestGroup.Item)
            {
                readyAltChild.SetActive(false);
            }
        }

        public void Dropped()
        {
            isDragging = false;

            if (isCompleted)
            {
                completionChild.SetActive(true);
            }

            if (state == Types.State.Default && type == Types.Type.Gen && level >= generatesAtLevel && !timerOn && !isReadyPlaying)
            {
                AnimateReady();
            }

            if (type == Types.Type.Chest && chestGroup == Types.ChestGroup.Item && !chestOpen && !timerOn)
            {
                readyAltChild.SetActive(true);
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

            if (loaded)
            {
                selectionChild.SetActive(false);

                StopAnimate();
            }
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

            StartCoroutine(WaitForItemSelectAnimation());
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

            StopCoroutine(WaitForItemSelectAnimation());
        }

        void AnimateReady()
        {
            readyChild.SetActive(true);

            isReadyPlaying = true;

            animReady["GenReady"].speed = GameRefs.Instance.readySpeed;
            animReady.Play("GenReady");
        }

        void StopAnimateReady()
        {
            isReadyPlaying = false;

            animReady.Rewind();
            animReady.Play();
            animReady.Sample();
            animReady.Stop();

            readyChild.SetActive(false);
        }

        IEnumerator WaitForItemSelectAnimation()
        {
            while (!anim.IsPlaying("ItemSelect"))
            {
                yield return null;
            }

            isPlaying = false;
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
            float newScaleSpeed,
            Action newCallback = null
        )
        {
            isMovingAndScaling = true;
            position = newPos;
            scale = newScale;
            moveSpeed = newMoveSpeed;
            scaleSpeed = newScaleSpeed;
            callback = newCallback;
        }
    }
}