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
        public int spriteOrder = -1;

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
            if (spriteOrder == -1)
            {
                spriteOrder = 0;
            }

            // Set the sprites
            for (int i = 0; i < tiles.Count; i++)
            {
                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].sprite = selectable.GetSprite(spriteOrder);
                }
            }
        }

        public void SetSprites(int order, bool alt = false)
        {
            // Stop changing if we are changing
            if (isChanging)
            {
                StopCoroutine("ChangeFloorTiles");
            }

            // Set the sprite order
            spriteOrder = order;

            if (alt)
            {
                GetTiles();
            }

            // Set the sprites
            for (int i = 0; i < tiles.Count; i++)
            {
                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].sprite = selectable.GetSprite(spriteOrder);
                }
            }
        }

        public void Cancel(int order)
        {
            // Stop changing if we are changing
            if (isChanging)
            {
                StopCoroutine("ChangeFloorTiles");
            }

            // Reset the sprite order
            spriteOrder = order;

            if (selectable.isOld)
            {
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
                // Reset the sprites to the new ones
                for (int i = 0; i < tiles.Count; i++)
                {
                    for (int j = 0; j < tiles[i].Count; j++)
                    {
                        tiles[i][j].sprite = selectable.GetSprite(spriteOrder);
                    }
                }
            }
        }

        public void Confirm()
        {
            // Stop changing if we are changing
            if (isChanging)
            {
                StopCoroutine("ChangeFloorTiles");
            }

            // Check if we are confirming for the first time
            if (selectable.isOld && spriteOrder > -1)
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
        }

        IEnumerator ChangeFloorTiles()
        {
            isChanging = true;
            Glob.taskLoading = true;

            for (int i = 0; i < tiles.Count; i++)
            {
                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].material.SetFloat("_FlashAmount", 1);

                    SoundManager.Instance.PlaySound(Types.SoundType.Generate);

                    tiles[i][j].sprite = selectable.GetSprite(spriteOrder);
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