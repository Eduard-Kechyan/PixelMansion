using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class ChangeWall : MonoBehaviour, IChanger
    {
        // Variables
        public Side side = Side.Left;
        [HideInInspector]
        public bool isRight;
        public float changeSpeed = 0.1f;
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
        [SerializeField]
        private bool isChanging = false;
        [ReadOnly]
        private bool isSelected = false;

        [HideInInspector]
        public bool loaded = false;

        // Sprites
        private readonly List<Chunk> chunks = new();
        private readonly List<Sprite> oldChunkSprites = new();

        // Enums
        public enum WallType
        {
            Default,
            DoorLeft,
            DoorRight,
            WindowLeft,
            WindowRight
        }

        // Classes
        [Serializable]
        public class Chunk
        {
            public SpriteRenderer spriteRenderer;
            public WallType type;
        }

        // Overlay
        private readonly List<SpriteRenderer> overlays = new();
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

            StartCoroutine(WaitForSelectableTo());
        }

        IEnumerator WaitForSelectableTo()
        {
            while (!selectable.loaded)
            {
                yield return null;
            }

            loaded = true;
            enabled = false;
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
                List<SpriteRenderer> tempOldChunks = new();

                // Start at 1 since 0 is the overlay
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform wallItem = transform.GetChild(i);

                    for (int j = 0; j < wallItem.childCount; j++)
                    {
                        Transform wallItemChunk = wallItem.GetChild(j);

                        SpriteRenderer newChild = wallItemChunk.GetComponent<SpriteRenderer>();

                        if (j >= (wallItem.childCount == 2 ? 1 : 2)) // Placeholder
                        {
                            // Set side's shadow color if the wall is on the right
                            if (side == Side.Right)
                            {
                                newChild.color = shadowColor;
                            }

                            // Set the sorting order to the parent
                            /*  if (i == 0)
                              {
                                  //selectable.order = newChild.sortingOrder;
                              }*/

                            // Check if we shouldn't flip the sprites
                            if (unFlip)
                            {
                                newChild.transform.localScale = new Vector3(1, newChild.transform.localScale.y, newChild.transform.localScale.z);
                            }

                            WallType newType = WallType.Default;

                            if (wallItemChunk.name.Contains("Door"))
                            {
                                if (wallItemChunk.name.Contains("Left"))
                                {
                                    newType = WallType.DoorLeft;
                                }
                                else
                                {
                                    newType = WallType.DoorRight;
                                }
                            }
                            else if (wallItemChunk.name.Contains("Window"))
                            {
                                if (wallItemChunk.name.Contains("Left"))
                                {
                                    newType = WallType.WindowLeft;
                                }
                                else
                                {
                                    newType = WallType.WindowRight;
                                }
                            }

                            chunks.Add(new()
                            {
                                spriteRenderer = newChild,
                                type = newType
                            });

                            if (selectable.isOld)
                            {
                                tempOldChunks.Add(newChild);
                            }
                        }
                        else // Overlay
                        {
                            overlays.Add(newChild);
                        }
                    }
                }


                chunks.Sort((a, b) => a.spriteRenderer.transform.position.x.CompareTo(b.spriteRenderer.transform.position.x));

                if (selectable.isOld)
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

        //// SELECT ////
        public void Select(bool select = true)
        {
            isSelected = select;

            enabled = select;

            if (select)
            {
                SetInitial();
            }
            else
            {
                ResetOverlays();
            }
        }

        //// SPRITES ////
        public void SetInitial()
        {
            // Set the sprite order to the first one
            if (selectable.spriteOrder == -1)
            {
                selectable.spriteOrder = 0;
            }

            Sprite spriteToUse = selectable.GetWallSprite(selectable.spriteOrder);
            Sprite spriteToUseDoorLeft = selectable.GetWallSprite(selectable.spriteOrder, true);
            Sprite spriteToUseDoorRight = selectable.GetWallSprite(selectable.spriteOrder, true, false, true);
            Sprite spriteToUseWindowLeft = selectable.GetWallSprite(selectable.spriteOrder, false, true);
            Sprite spriteToUseWindowRight = selectable.GetWallSprite(selectable.spriteOrder, false, true, true);

            // Set the sprites
            for (int i = 0; i < chunks.Count; i++)
            {
                switch (chunks[i].type)
                {
                    case WallType.DoorLeft:
                        chunks[i].spriteRenderer.sprite = spriteToUseDoorLeft;
                        break;
                    case WallType.DoorRight:
                        chunks[i].spriteRenderer.sprite = spriteToUseDoorRight;
                        break;
                    case WallType.WindowLeft:
                        chunks[i].spriteRenderer.sprite = spriteToUseWindowLeft;
                        break;
                    case WallType.WindowRight:
                        chunks[i].spriteRenderer.sprite = spriteToUseWindowRight;
                        break;
                    default: // WallType.Default
                        chunks[i].spriteRenderer.sprite = spriteToUse;
                        break;
                }
            }
        }

        public void SetSprites(int order, bool alt = false)
        {
            // Stop changing if we are changing
            if (isChanging)
            {
                StopCoroutine("ChangeWallChunks");
            }

            if (selectable == null)
            {
                selectable = GetComponent<Selectable>();
            }

            // Set the sprite order
            selectable.spriteOrder = order;

            if (alt)
            {
                GetChunks();
            }

            Sprite spriteToUse = selectable.GetWallSprite(selectable.spriteOrder);
            Sprite spriteToUseDoorLeft = selectable.GetWallSprite(selectable.spriteOrder, true);
            Sprite spriteToUseDoorRight = selectable.GetWallSprite(selectable.spriteOrder, true, false, true);
            Sprite spriteToUseWindowLeft = selectable.GetWallSprite(selectable.spriteOrder, false, true);
            Sprite spriteToUseWindowRight = selectable.GetWallSprite(selectable.spriteOrder, false, true, true);

            // Set the sprites
            for (int i = 0; i < chunks.Count; i++)
            {
                switch (chunks[i].type)
                {
                    case WallType.DoorLeft:
                        chunks[i].spriteRenderer.sprite = spriteToUseDoorLeft;
                        break;
                    case WallType.DoorRight:
                        chunks[i].spriteRenderer.sprite = spriteToUseDoorRight;
                        break;
                    case WallType.WindowLeft:
                        chunks[i].spriteRenderer.sprite = spriteToUseWindowLeft;
                        break;
                    case WallType.WindowRight:
                        chunks[i].spriteRenderer.sprite = spriteToUseWindowRight;
                        break;
                    default: // WallType.Default
                        chunks[i].spriteRenderer.sprite = spriteToUse;
                        break;
                }
            }
        }

        public void Cancel(int order, Action callback = null)
        {
            // Stop changing if we are changing
            if (isChanging)
            {
                StopCoroutine("ChangeWallChunks");
            }

            // Reset the sprites to the old ones
            if (selectable.isOld)
            {
                // Reset the sprites to the old ones
                for (int i = 0; i < chunks.Count; i++)
                {
                    chunks[i].spriteRenderer.sprite = oldChunkSprites[i];
                }
            }
            else
            {
                Sprite spriteToUse = selectable.GetWallSprite(selectable.spriteOrder);
                Sprite spriteToUseDoorLeft = selectable.GetWallSprite(selectable.spriteOrder, true);
                Sprite spriteToUseDoorRight = selectable.GetWallSprite(selectable.spriteOrder, true, false, true);
                Sprite spriteToUseWindowLeft = selectable.GetWallSprite(selectable.spriteOrder, false, true);
                Sprite spriteToUseWindowRight = selectable.GetWallSprite(selectable.spriteOrder, false, true, true);

                // Reset the sprites to the new ones
                for (int i = 0; i < chunks.Count; i++)
                {
                    switch (chunks[i].type)
                    {
                        case WallType.DoorLeft:
                            chunks[i].spriteRenderer.sprite = spriteToUseDoorLeft;
                            break;
                        case WallType.DoorRight:
                            chunks[i].spriteRenderer.sprite = spriteToUseDoorRight;
                            break;
                        case WallType.WindowLeft:
                            chunks[i].spriteRenderer.sprite = spriteToUseWindowLeft;
                            break;
                        case WallType.WindowRight:
                            chunks[i].spriteRenderer.sprite = spriteToUseWindowRight;
                            break;
                        default: // WallType.Default
                            chunks[i].spriteRenderer.sprite = spriteToUse;
                            break;
                    }
                }
            }

            callback?.Invoke();
        }

        public void Confirm(Action callback = null)
        {
            // Stop changing if we are changing
            if (isChanging)
            {
                StopCoroutine("ChangeWallChunks");
            }

            // Check if we are confirming for the first time
            if (selectable.isOld)
            {
                // Reset the sprites to the old ones
                for (int i = 0; i < chunks.Count; i++)
                {
                    for (int j = 0; j < chunks.Count; j++)
                    {
                        chunks[i].spriteRenderer.sprite = oldChunkSprites[i];
                    }
                }

                StartCoroutine(ChangeWallChunks());

                if (Settings.Instance.vibrationOn && (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer))
                {
                    Handheld.Vibrate();
                }

                // Reset overlay flashing
                ResetOverlays();

                selectable.isOld = false;

                callback?.Invoke();
            }
            else
            {
                callback?.Invoke();
            }
        }

        IEnumerator ChangeWallChunks()
        {
            isChanging = true;
            Glob.taskLoading = true;

            Sprite spriteToUse = selectable.GetWallSprite(selectable.spriteOrder);
            Sprite spriteToUseDoorLeft = selectable.GetWallSprite(selectable.spriteOrder, true);
            Sprite spriteToUseDoorRight = selectable.GetWallSprite(selectable.spriteOrder, true, false, true);
            Sprite spriteToUseWindowLeft = selectable.GetWallSprite(selectable.spriteOrder, false, true);
            Sprite spriteToUseWindowRight = selectable.GetWallSprite(selectable.spriteOrder, false, true, true);

            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].spriteRenderer.material.SetFloat("_FlashAmount", 1);

                SoundManager.Instance.PlaySound(SoundManager.SoundType.Generate);

                switch (chunks[i].type)
                {
                    case WallType.DoorLeft:
                        chunks[i].spriteRenderer.sprite = spriteToUseDoorLeft;
                        break;
                    case WallType.DoorRight:
                        chunks[i].spriteRenderer.sprite = spriteToUseDoorRight;
                        break;
                    case WallType.WindowLeft:
                        chunks[i].spriteRenderer.sprite = spriteToUseWindowLeft;
                        break;
                    case WallType.WindowRight:
                        chunks[i].spriteRenderer.sprite = spriteToUseWindowRight;
                        break;
                    default: // WallType.Default
                        chunks[i].spriteRenderer.sprite = spriteToUse;
                        break;
                }

                yield return new WaitForSeconds(changeSpeed);

                chunks[i].spriteRenderer.material.SetFloat("_FlashAmount", 0);
            }

            isChanging = false;
            Glob.taskLoading = false;
        }

        //// OVERLAY ////
        void HandleOverlay()
        {
            // Flash if selected and isn't changing the tiles
            if (isSelected && !isChanging)
            {
                float alphaAmount = overlays[0].color.a;
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

                Color newOverlayColor = new Color(1, 1, 1, newAlphaAmount);

                for (int i = 0; i < overlays.Count; i++)
                {
                    overlays[i].color = newOverlayColor;
                }
            }
        }

        void ResetOverlays()
        {
            Color newOverlayColor = new Color(1, 1, 1, 0);

            for (int i = 0; i < overlays.Count; i++)
            {
                overlays[i].color = newOverlayColor;
            }
        }
    }
}