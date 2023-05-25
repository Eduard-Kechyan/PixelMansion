using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharOrderer : MonoBehaviour
{
    // Variables
    public int sortingOrderOffset = 7;

    private SpriteRenderer spriteRenderer;
    private string roomName = "";

    void Start()
    {
        // Cache
        spriteRenderer = GetComponent<SpriteRenderer>();

        CheckArea();
    }

    public void CheckArea()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.zero,
            Mathf.Infinity,
            LayerMask.GetMask("Room")
        );

        if (hit || hit.collider != null)
        {
            if (hit.transform.name != roomName)
            {
                int order = (int)hit.transform.position.z;

                spriteRenderer.sortingOrder = order + sortingOrderOffset;

                roomName = hit.transform.name;
            }
        }
    }
}
