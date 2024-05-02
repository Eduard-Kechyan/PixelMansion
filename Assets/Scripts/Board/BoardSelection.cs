using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class BoardSelection : MonoBehaviour
    {
        // Variables
        public float selectSpeed = 1.8f;

        // Enums
        public enum SelectType
        {
            None,
            Both,
            Info,
            Only
        }

        // References
        private BoardInteractions interactions;
        private BoardDoubleTap boardDoubleTap;
        private MergeUI mergeUI;
        private InfoBox infoBox;

        void Start()
        {
            // Cache
            interactions = GetComponent<BoardInteractions>();
            boardDoubleTap = GetComponent<BoardDoubleTap>();
            mergeUI = GameRefs.Instance.mergeUI;
            infoBox = mergeUI.GetComponent<InfoBox>();
        }

        // Find and select the item at the given position
        public void SelectItem(Vector3 worldPos)
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

                // Check if game object is an item and isn't empty
                if (item != null)
                {
                    // See if the same exact item was double tapped
                    if (interactions.currentItem != null && interactions.currentItem.isSelected && interactions.currentItem.id == item.id)
                    {
                        boardDoubleTap.CheckForDoubleTaps();
                    }

                    //item.gameObject.layer == LayerMask.NameToLayer("Item")
                    if (item.isIndicating || !item.isPlaying) // FIX - This was "!item.isPlaying", check if it's good now
                    {
                        // Unselect other items if they exist
                        Unselect(SelectType.Both);

                        // Set current item
                        interactions.currentItem = item;

                        // Select the item
                        interactions.isSelected = true;
                        Select(SelectType.Both);
                    }
                }
            }

            // Check if we should unselect
            CheckUnselect(worldPos);
        }

        // Select the item after undoing the items from a state of being sold or removed
        public void SelectItemAfterUndo()
        {
            // Unselect other items if they exist
            Unselect(SelectType.Both);

            // Select the item
            interactions.isSelected = true;
            Select(SelectType.Both);
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
                Unselect(SelectType.Both);
            }
        }

        // Select the given item
        public void Select(SelectType selectType, bool animate = true)
        {
            if (interactions.currentItem != null)
            {
                if (selectType == SelectType.Info)
                {
                    infoBox.Select(interactions.currentItem);
                }
                else
                {
                    interactions.currentItem.isSelected = true;

                    if (selectType != SelectType.Only)
                    {
                        interactions.currentItem.Select(selectSpeed, animate);
                        infoBox.Select(interactions.currentItem);
                    }
                }
            }
        }

        // Unselect the given item
        public void Unselect(SelectType selectType)
        {
            if (selectType == SelectType.Info)
            {
                infoBox.Unselect();
            }
            else
            {
                if (interactions.currentItem != null)
                {
                    interactions.currentItem.isSelected = false;

                    interactions.currentItem.Unselect();
                }

                infoBox.Unselect();
            }
        }

        // Alternative way of unselecting the given item
        public void UnselectAlt()
        {
            interactions.isSelected = false;

            if (interactions.currentItem != null)
            {
                interactions.currentItem.isSelected = false;

                interactions.currentItem.Unselect();
            }
        }

        // Unselect the item when undoing
        public void UnselectUndo()
        {
            interactions.isSelected = false;

            if (interactions.currentItem != null)
            {
                interactions.currentItem.isSelected = false;

                interactions.currentItem.Unselect();
            }

            infoBox.Unselect();
        }
    }
}