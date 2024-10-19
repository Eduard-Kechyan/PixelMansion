using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class ChangeFloor : MonoBehaviour, IChanger
    {
        // Variables
        public float changeSpeed = 0.1f;

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

        public int tempSpriteOrder = -1;

        // Tiles
        private readonly List<List<SpriteRenderer>> tiles = new();
        private readonly List<List<Sprite>> oldTilesSprites = new();

        // Overlays
        private readonly List<SpriteRenderer> overlays = new();
        private bool alphaUp = true;
        private float alphaDelayTemp = 0f;

        private bool tilesSet = false;

        // References
        private Selectable selectable;

        void Start()
        {
            // Cache
            selectable = GetComponent<Selectable>();

            if (!tilesSet)
            {
                GetTiles();
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

        void GetTiles()
        {
            if (transform.childCount > 0)
            {
                List<Transform> preTiles = new();

                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform tile = transform.GetChild(i);

                    preTiles.Add(tile);

                    overlays.Add(tile.GetChild(0).GetComponent<SpriteRenderer>());
                }

                //tiles.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));
                preTiles.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

                int count = 0;

                for (int i = 0; i < preTiles.Count; i++)
                {
                    // First entry
                    if (i == 0)
                    {
                        SpriteRenderer newSpriteRenderer = preTiles[i].GetComponent<SpriteRenderer>();

                        List<SpriteRenderer> tempTile = new() { newSpriteRenderer };

                        // Set the sorting order to the parent
                        //selectable.order = tempTiles[0].sortingOrder;

                        tiles.Add(tempTile);

                        if (selectable.isOld)
                        {
                            List<Sprite> tempTileSprites = new() { newSpriteRenderer.sprite };

                            oldTilesSprites.Add(tempTileSprites);
                        }
                    }
                    else
                    {
                        if (preTiles[i].transform.position.x == preTiles[i - 1].transform.position.x)
                        {
                            SpriteRenderer newSpriteRenderer = preTiles[i].GetComponent<SpriteRenderer>();

                            tiles[count].Add(newSpriteRenderer);

                            if (selectable.isOld)
                            {
                                oldTilesSprites[count].Add(newSpriteRenderer.sprite);
                            }
                        }
                        else
                        {
                            SpriteRenderer newSpriteRenderer = preTiles[i].GetComponent<SpriteRenderer>();

                            List<SpriteRenderer> tempTiles = new() { newSpriteRenderer };

                            tiles.Add(tempTiles);

                            if (selectable.isOld)
                            {
                                List<Sprite> tempTileSprites = new() { newSpriteRenderer.sprite };

                                oldTilesSprites.Add(tempTileSprites);
                            }

                            count++;
                        }
                    }
                }
            }

            tilesSet = true;
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
            if (tempSpriteOrder == -1)
            {
                tempSpriteOrder = 0;
            }

            Sprite spriteToUse = selectable.GetSprite(tempSpriteOrder);

            // Set the sprites
            for (int i = 0; i < tiles.Count; i++)
            {
                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].sprite = spriteToUse;
                }
            }
        }

        public void SetSprites(int order, bool alt = false)
        {
            // Stop changing if we are changing
            if (isChanging)
            {
                StopCoroutine(ChangeFloorTiles());
            }

            tempSpriteOrder = order;

            if (alt)
            {
                GetTiles();
            }

            Sprite spriteToUse = selectable.GetSprite(order);

            // Set the sprites
            for (int i = 0; i < tiles.Count; i++)
            {
                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].sprite = spriteToUse;
                }
            }
        }

        public void Cancel(int order, Action callback = null)
        {
            // Stop changing if we are changing
            if (isChanging)
            {
                StopCoroutine(ChangeFloorTiles());
            }

            if (selectable.isOld)
            {
                tempSpriteOrder = 0;

                // Reset the sprites to the old ones
                for (int i = 0; i < tiles.Count; i++)
                {
                    for (int j = 0; j < tiles[i].Count; j++)
                    {
                        tiles[i][j].sprite = oldTilesSprites[i][j];
                    }
                }
            }
            else
            {
                tempSpriteOrder = order;

                Sprite spriteToUse = selectable.GetSprite(order);

                // Reset the sprites to the new ones
                for (int i = 0; i < tiles.Count; i++)
                {
                    for (int j = 0; j < tiles[i].Count; j++)
                    {
                        tiles[i][j].sprite = spriteToUse;
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
                StopCoroutine(ChangeFloorTiles());
            }

            // Check if we are confirming for the first time
            if (selectable.isOld && tempSpriteOrder > -1)
            {
                // Reset the sprites to the old ones
                for (int i = 0; i < tiles.Count; i++)
                {
                    for (int j = 0; j < tiles[i].Count; j++)
                    {
                        tiles[i][j].sprite = oldTilesSprites[i][j];
                    }
                }

                StartCoroutine(ChangeFloorTiles());

                if (Settings.Instance.vibrationOn && (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer))
                {
                    Handheld.Vibrate();
                }

                // Reset overlay flashing
                ResetOverlays();

                selectable.isOld = false;
            }

            selectable.spriteOrder = tempSpriteOrder;

            callback?.Invoke();
        }

        IEnumerator ChangeFloorTiles()
        {
            isChanging = true;
            Glob.taskLoading = true;
            Sprite spriteToUse = selectable.GetSprite(tempSpriteOrder);

            for (int i = 0; i < tiles.Count; i++)
            {
                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].material.SetFloat("_FlashAmount", 1);

                    SoundManager.Instance.PlaySound(SoundManager.SoundType.Generate);

                    tiles[i][j].sprite = spriteToUse;
                }

                yield return new WaitForSeconds(changeSpeed);

                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].material.SetFloat("_FlashAmount", 0);
                }
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