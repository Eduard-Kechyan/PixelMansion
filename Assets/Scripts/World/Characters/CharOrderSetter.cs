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
            Collider2D hit = Physics2D.OverlapCircle(transform.position, radius, LayerMask.GetMask("Room"));

            if (hit.transform.name != currentRoomName)
            {
                OrderSetter roomOrderSetter = hit.transform.GetComponent<OrderSetter>();

                if (roomOrderSetter != null)
                {
                    mainSpriteRenderer.sortingLayerName = roomOrderSetter.sortingLayer;
                    shadowSpriteRenderer.sortingLayerName = roomOrderSetter.sortingLayer;

                    currentRoomName = hit.transform.name;
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

        public void FadeIn()
        {
            StopCoroutine(FadeOutOvertime());

            StartCoroutine(FadeInOvertime());
        }

        public void FadeOut()
        {
            StopCoroutine(FadeInOvertime());

            StartCoroutine(FadeOutOvertime());
        }

        IEnumerator FadeInOvertime()
        {
            while (mainSpriteRenderer.color.a < 1f)
            {
                mainSpriteRenderer.color = new Color(1f, 1f, 1f, mainSpriteRenderer.color.a + 0.1f);
                shadowSpriteRenderer.color = new Color(1f, 1f, 1f, shadowSpriteRenderer.color.a + 0.1f);

                yield return new WaitForSeconds(0.03f);
            }
        }

        IEnumerator FadeOutOvertime()
        {
            while (mainSpriteRenderer.color.a > 0f)
            {
                mainSpriteRenderer.color = new Color(1f, 1f, 1f, mainSpriteRenderer.color.a - 0.1f);
                shadowSpriteRenderer.color = new Color(1f, 1f, 1f, shadowSpriteRenderer.color.a - 0.1f);

                yield return new WaitForSeconds(0.03f);
            }
        }
    }
}