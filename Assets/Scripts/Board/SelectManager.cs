using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectManager : MonoBehaviour
{
    private BoardInteractions interactions;
    private Selection selection;

    void Start()
    {
        // Cache boardInteractions
        interactions = GetComponent<BoardInteractions>();

        // Cache selection
        selection = GetComponent<Selection>();
    }

    public bool SelectItem(Vector3 worldPos)
    {
        // Cast a ray to find an item on the current position
        RaycastHit2D hit = Physics2D.Raycast(
            worldPos,
            Vector2.zero,
            Mathf.Infinity,
            LayerMask.GetMask("Item")
        );

        // Check if raycast hit an item
        if (hit.collider != null)
        {
            Item item = hit.transform.gameObject.GetComponent<Item>();

            // Check if gameobject is an item and isn't empty
            if (item != null && !item.isPlaying)
            {
                if (item == interactions.currentItem)
                {
                    return true;
                }
                else
                {
                    // Unselect other items if they exist
                    selection.Unselect("both");

                    // Set current item
                    interactions.currentItem = item;

                    // Select the item
                    interactions.isSelected = true;
                    selection.Select("both");
                    return false;
                }
            }

            return false;
        }

        // Check if we should unselect
        CheckUnselect(worldPos);

        return false;
    }

    public void SelectItemAfterUndo()
    {
        // Unselect other items if they exist
        selection.Unselect("both");

        // Select the item
        interactions.isSelected = true;
        selection.Select("both");
    }

    void CheckUnselect(Vector3 worldPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            worldPos,
            Vector2.zero,
            Mathf.Infinity,
            LayerMask.GetMask("Tile")
        );

        // Check if raycast hit anything
        if (hit.collider != null && hit.transform.childCount == 0)
        {
            // Unselect other items if they exist
            interactions.isSelected = false;
            selection.Unselect("both");
        }
    }
}
