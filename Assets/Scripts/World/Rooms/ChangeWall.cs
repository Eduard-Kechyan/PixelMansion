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

        // Sprites
        private readonly List<SpriteRenderer> chunks = new();
        private readonly List<Sprite> oldChunkSprites = new();

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
                        SpriteRenderer newChild = wallItem.GetChild(j).GetComponent<SpriteRenderer>();

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

                            chunks.Add(newChild);

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


                chunks.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

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

            // Set the sprites
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].sprite = selectable.GetSprite(selectable.spriteOrder);
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
            selectable.spriteOrder = order;

            if (alt)
            {
                GetChunks();
            }

            // Set the sprites
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].sprite = selectable.GetSprite(order);
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
            if (selectable.isOld)
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
                    chunks[i].sprite = selectable.GetSprite(order);
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
            if (selectable.isOld)
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
                ResetOverlays();

                selectable.isOld = false;
            }
        }

        IEnumerator ChangeWallChunks()
        {
            isChanging = true;
            Glob.selectableIsChanging = true;

            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].material.SetFloat("_FlashAmount", 1);

                SoundManager.Instance.PlaySound(Types.SoundType.Generate);

                chunks[i].sprite = selectable.GetSprite(selectable.spriteOrder);

                yield return new WaitForSeconds(changeSpeed);

                chunks[i].material.SetFloat("_FlashAmount", 0);
            }

            isChanging = false;
            Glob.selectableIsChanging = false;
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