using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class ChangeFurniture : MonoBehaviour
    {
        // Variables
        public int spriteOrder = -1;

        [Header("Flash")]
        public float flashSpeed = 1f;
        public float flashDelay = 0.8f;
        public float maxFlash = 0.5f;

        [Header("States")]
        [ReadOnly]
        public bool isOld = true;
        [ReadOnly]
        private bool isSelected = false;

        [Header("Sprites")]
        public Sprite[] sprites = new Sprite[3];
        public Sprite[] optionSprites = new Sprite[3];

        private Sprite oldSprite;

        // Overlay
        private bool flashUp = true;
        private float flashDelayTemp = 0f;

        // References
        private Selectable selectable;
        private SpriteRenderer spriteRenderer;
        private NavMeshManager navMeshManager;

        void Start()
        {
            // Cache
            selectable = GetComponent<Selectable>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            navMeshManager = NavMeshManager.Instance;

            if (isOld)
            {
                oldSprite = spriteRenderer.sprite;
                selectable.order = spriteRenderer.sortingOrder;
            }

            SetPositionZ();

            enabled = false;
        }

        void Update()
        {
            HandleOverlay();
        }

        void SetPositionZ()
        {
            int parentLayerOrder = SortingLayer.GetLayerValueFromName(transform.parent.transform.parent.gameObject.GetComponent<RoomHandler>().roomSortingLayer);

            int z = parentLayerOrder + spriteRenderer.sortingOrder + 3; // 3 is for this gameObjects' order in it's parent

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

            // Set the sprite
            spriteRenderer.sprite = sprites[spriteOrder];
        }

        public void SetSprites(int order, bool alt = false)
        {
            // Set the sprite order
            spriteOrder = order;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (alt)
            {
                CheckNavAreas(true);
            }

            // Set the sprite
            spriteRenderer.sprite = sprites[order];
        }

        public void Cancel(int order)
        {
            if (isOld)
            {
                // Reset the sprite to the old one
                spriteRenderer.sprite = oldSprite;
            }
            else
            {
                // Reset the sprite to the new one
                spriteRenderer.sprite = sprites[order];
            }
        }

        public void Confirm()
        {
            // Check if we are confirming for the first time
            if (isOld && spriteOrder > -1)
            {
                // Reset the sprite to the old one
                spriteRenderer.sprite = oldSprite;

                spriteRenderer.sprite = sprites[spriteOrder];

                if (Settings.Instance.vibrationOn && (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer))
                {
                    Handheld.Vibrate();
                }

                // Reset overlay flashing
                ResetOverlay();

                CheckNavAreas();

                isOld = false;
            }
        }

        void CheckNavAreas(bool alt = false)
        {
            if (spriteOrder > 0)
            {
                // Nav areas
                if (transform.childCount == 3)
                {
                    bool found = false;

                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Transform navArea = transform.GetChild(i);

                        if (navArea != null && i == spriteOrder)
                        {
                            navArea.gameObject.SetActive(true);

                            found = true;
                        }
                        else
                        {
                            navArea.gameObject.SetActive(false);
                        }
                    }

                    if (found && !alt)
                    {
                        navMeshManager.Bake();
                    }
                }

                // Colliders
                PolygonCollider2D[] colliders = gameObject.GetComponents<PolygonCollider2D>();

                if (colliders.Length == 3)
                {
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        if (i == spriteOrder)
                        {
                            colliders[i].enabled = true;
                        }
                        else
                        {
                            colliders[i].enabled = false;
                        }
                    }
                }
            }
        }

        //// OVERLAY ////
        void HandleOverlay()
        {
            // Flash if selected and
            if (isSelected)
            {
                float flashAmount = spriteRenderer.material.GetFloat("_FlashAmount");
                float newFlashAmount = 0f;

                if (flashAmount == maxFlash)
                {
                    flashDelayTemp = flashDelay;

                    flashUp = false;
                }

                if (flashAmount == 0)
                {
                    flashUp = true;
                }

                if (flashUp)
                {
                    flashDelayTemp -= flashSpeed * Time.deltaTime;

                    if (flashDelayTemp <= 0)
                    {
                        newFlashAmount = Mathf.MoveTowards
                        (
                            flashAmount,
                            maxFlash,
                            flashSpeed * Time.deltaTime
                        );
                    }
                }
                else
                {
                    newFlashAmount = Mathf.MoveTowards
                    (
                        flashAmount,
                        0,
                        flashSpeed * Time.deltaTime
                    );
                }

                spriteRenderer.material.SetFloat("_FlashAmount", newFlashAmount);
            }
        }

        void ResetOverlay()
        {
            spriteRenderer.material.SetFloat("_FlashAmount", 0);
        }
    }
}