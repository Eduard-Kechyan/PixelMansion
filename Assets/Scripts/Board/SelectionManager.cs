using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class SelectionManager : MonoBehaviour
    {
        // Variables
        public float selectSpeed = 1.8f;

        // References
        private BoardInteractions interactions;
        private DoubleTapManager doubleTapManager;

        // Instances
        private GameplayUI gameplayUI;
        private InfoBox infoBox;

        void Start()
        {
            // Cache
            interactions = GetComponent<BoardInteractions>();
            doubleTapManager = GetComponent<DoubleTapManager>();

            // Cache instances
            gameplayUI = GameRefs.Instance.gameplayUI;
            infoBox = gameplayUI.GetComponent<InfoBox>();
        }

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
                        doubleTapManager.CheckForDoubleTaps();
                    }

                    //item.gameObject.layer == LayerMask.NameToLayer("Item")
                    if (item.isIndicating || !item.isPlaying) // TODO - This was "!item.isPlaying", check if it's good now
                    {
                        // Unselect other items if they exist
                        Unselect("both");

                        // Set current item
                        interactions.currentItem = item;

                        // Select the item
                        interactions.isSelected = true;
                        Select("both");
                    }
                }
            }

            // Check if we should unselect
            CheckUnselect(worldPos);
        }

        public void SelectItemAfterUndo()
        {
            // Unselect other items if they exist
            Unselect("both");

            // Select the item
            interactions.isSelected = true;
            Select("both");
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
                Unselect("both");
            }
        }

        // Selection
        public void Select(string type, bool animate = true)
        {
            if (interactions.currentItem != null)
            {
                if (type == "info")
                {
                    infoBox.Select(interactions.currentItem);
                }
                else
                {
                    interactions.currentItem.isSelected = true;

                    if (type != "only")
                    {
                        interactions.currentItem.Select(selectSpeed, animate);
                        infoBox.Select(interactions.currentItem);
                    }
                }
            }
        }

        public void Unselect(string type)
        {
            if (type == "info")
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

        public void UnselectAlt()
        {
            interactions.isSelected = false;

            if (interactions.currentItem != null)
            {
                interactions.currentItem.isSelected = false;

                interactions.currentItem.Unselect();
            }
        }

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