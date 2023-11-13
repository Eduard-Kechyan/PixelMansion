using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class ChangeFloor : MonoBehaviour
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
        public bool isOld = true;
        [ReadOnly]
        [SerializeField]
        private bool isChanging = false;
        [ReadOnly]
        private bool isSelected = false;

        [Header("Sprites")]
        public Sprite[] sprites = new Sprite[3];
        public Sprite[] optionSprites = new Sprite[3];

        // Tiles
        private readonly List<List<SpriteRenderer>> tiles = new();
        private readonly List<List<Sprite>> oldTilesSprites = new();

        // Overlay
        private SpriteRenderer overlay;
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

            SetPositionZ();

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
                overlay = transform.GetChild(0).GetComponent<SpriteRenderer>();

                List<Transform> preTiles = new();

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
                        SpriteRenderer newSpriteRenderer = preTiles[i].GetComponent<SpriteRenderer>();

                        List<SpriteRenderer> tempTiles = new(){
                        newSpriteRenderer
                    };

                        // Set the sorting order to the parent
                        //selectable.order = tempTiles[0].sortingOrder;

                        tiles.Add(tempTiles);

                        if (isOld)
                        {
                            List<Sprite> tempTileSprites = new(){
                            newSpriteRenderer.sprite
                        };

                            oldTilesSprites.Add(tempTileSprites);
                        }
                    }
                    else
                    {
                        if (preTiles[i].transform.position.x == preTiles[i - 1].transform.position.x)
                        {
                            SpriteRenderer newSpriteRenderer = preTiles[i].GetComponent<SpriteRenderer>();

                            tiles[count].Add(newSpriteRenderer);

                            if (isOld)
                            {
                                oldTilesSprites[count].Add(newSpriteRenderer.sprite);
                            }
                        }
                        else
                        {
                            SpriteRenderer newSpriteRenderer = preTiles[i].GetComponent<SpriteRenderer>();

                            List<SpriteRenderer> tempTiles = new(){
                        newSpriteRenderer
                    };

                            tiles.Add(tempTiles);

                            if (isOld)
                            {
                                List<Sprite> tempTileSprites = new(){
                                newSpriteRenderer.sprite
                            };

                                oldTilesSprites.Add(tempTileSprites);
                            }

                            count++;
                        }
                    }
                }
            }

            tilesSet = true;
        }

        void SetPositionZ()
        {
            int parentLayerOrder = SortingLayer.GetLayerValueFromName(transform.parent.gameObject.GetComponent<RoomHandler>().roomSortingLayer);

            int z = parentLayerOrder + 2; // 2 is for this gameObjects' order in it's parent

            transform.position = new Vector3(transform.position.x, transform.position.y, z);
        }

        //// SELECT ////
        public void Select()
        {
            isSelected = true;

            enabled = true;

            SetInitial();
        }

        public void Unselect()
        {
            isSelected = false;

            enabled = false;

            // Reset overlay flashing
            ResetOverlay();
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
                    tiles[i][j].sprite = sprites[spriteOrder];
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
                    tiles[i][j].sprite = sprites[order];
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

            if (isOld)
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
                        tiles[i][j].sprite = sprites[order];
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
            if (isOld && spriteOrder > -1)
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
                ResetOverlay();

                isOld = false;
            }
        }

        IEnumerator ChangeFloorTiles()
        {
            isChanging = true;
            Glob.selectableIsChanging = true;

            for (int i = 0; i < tiles.Count; i++)
            {
                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].material.SetFloat("_FlashAmount", 1);

                    SoundManager.Instance.PlaySound(Types.SoundType.Generate);

                    tiles[i][j].sprite = sprites[spriteOrder];
                }

                yield return new WaitForSeconds(changeSpeed);

                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].material.SetFloat("_FlashAmount", 0);
                }
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
}