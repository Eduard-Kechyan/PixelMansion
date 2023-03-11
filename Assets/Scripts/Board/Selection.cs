using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Selection : MonoBehaviour
{
    public UIDocument uiDoc;
    public float selectSpeed = 1.8f;

    private BoardInteractions interactions;
    private InfoBox infoBox;

    void Start()
    {
        // Cache boardInteractions
        interactions = GetComponent<BoardInteractions>();

        // Cache the infoBox
        infoBox = uiDoc.GetComponent<InfoBox>();
    }

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

                interactions.currentItem?.Unselect(); // Null propagation
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
}
