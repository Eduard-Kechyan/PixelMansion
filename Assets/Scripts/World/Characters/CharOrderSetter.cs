using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class CharOrderSetter : MonoBehaviour
    {
        // Variables
        public int radius;
        public int sortingOrderOffset = 7;
        public Sprite shadowIdleSprite;
        public Sprite shadowWalkingSprite;

        private SpriteRenderer mainSpriteRenderer;
        private SpriteRenderer shadowSpriteRenderer;
        [HideInInspector]
        public string currentRoomName = "";

        void Start()
        {
            // Cache
            mainSpriteRenderer = GetComponent<SpriteRenderer>();
            shadowSpriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>(); // 0 is shadow

            shadowSpriteRenderer.sortingOrder = mainSpriteRenderer.sortingOrder - 1;

            CheckArea();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        public void CheckArea()
        {
            Collider2D[] hits = new Collider2D[2];

            int numColliders = Physics2D.OverlapCircleNonAlloc(transform.position, radius, hits, LayerMask.GetMask("Room"));

            for (int i = 0; i < numColliders; i++)
            {
                if (hits[i].transform.name != currentRoomName)
                {
                    OrderSetter roomOrderSetter = hits[i].transform.GetComponent<OrderSetter>();

                    if (roomOrderSetter != null)
                    {
                        mainSpriteRenderer.sortingLayerName = roomOrderSetter.sortingLayer;
                        shadowSpriteRenderer.sortingLayerName = roomOrderSetter.sortingLayer;

                        currentRoomName = hits[i].transform.name;
                    }
                }
            }
        }

        public void SetShadow(bool isIdle = true)
        {
            if (isIdle)
            {
                shadowSpriteRenderer.sprite = shadowIdleSprite;
            }
            else
            {
                shadowSpriteRenderer.sprite = shadowWalkingSprite;
            }
        }
    }
}