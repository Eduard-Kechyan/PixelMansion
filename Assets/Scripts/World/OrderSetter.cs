using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderSetter : MonoBehaviour
{
    public Type type = Type.Single;
    [SortingLayer]
    public string sortingLayer;
    public bool set = false;

    [HideInInspector]
    public bool isNavArea;

    [Condition("isNavArea", true)]
    public bool show = false;

    public enum Type
    {
        Single,
        Self,
        NavArea,
        Room
    }

    private List<string> roomParts = new List<string>();
    
#if UNITY_EDITOR
    void OnValidate()
    {
        if (set)
        {
            set = false;
        }

        isNavArea = type == Type.NavArea;

        if (type == Type.Room)
        {
            roomParts.Add("WallLeft");
            roomParts.Add("WallRight");
            roomParts.Add("Floor");
        }

        ToggleNavArea();

        SetSpriteOrders();
    }
#endif

    void ToggleNavArea()
    {
        if (type == Type.NavArea)
        {
            Transform walkable = transform.GetChild(0);
            Transform notWalkable = transform.GetChild(1);

            for (int i = 0; i < walkable.childCount; i++)
            {
                walkable.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = 0;
                walkable.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = show ? sortingLayer : "Default";
            }

            for (int i = 0; i < notWalkable.childCount; i++)
            {
                notWalkable.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = 1;
                walkable.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = show ? sortingLayer : "Default";
            }
        }
    }

    public void SetSpriteOrders()
    {
        int order = (int)transform.localPosition.z;

        switch (type)
        {
            case Type.Room:
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (roomParts.Contains(transform.GetChild(i).name))
                    {
                        Transform firstChild = transform.GetChild(i);

                        for (int j = 0; j < firstChild.childCount; j++)
                        {
                            Transform secondChild = firstChild.GetChild(j);

                            if (secondChild.name == "Overlay")
                            {
                                secondChild.GetComponent<SpriteRenderer>().sortingOrder = order + 2;
                                secondChild.GetComponent<SpriteRenderer>().sortingLayerName = sortingLayer;
                            }
                            else if (secondChild.name == "LockedOverlay")
                            {
                                secondChild.GetComponent<SpriteRenderer>().sortingLayerName = sortingLayer;
                            }
                            else
                            {
                                secondChild.GetComponent<SpriteRenderer>().sortingOrder = order;
                                secondChild.GetComponent<SpriteRenderer>().sortingLayerName = sortingLayer;

                                if (secondChild.childCount > 0)
                                {
                                    // Main walls
                                    for (int k = 0; k < secondChild.childCount; k++)
                                    {
                                        secondChild.GetChild(k).GetComponent<SpriteRenderer>().sortingOrder = order + 1;
                                        secondChild.GetChild(k).GetComponent<SpriteRenderer>().sortingLayerName = sortingLayer;
                                    }
                                }
                            }
                        }
                    }
                    else if (transform.GetChild(i).name == "Furniture")
                    {
                        for (int j = 0; j < transform.GetChild(i).childCount; j++)
                        {
                            transform.GetChild(i).GetChild(j).GetComponent<SpriteRenderer>().sortingLayerName = sortingLayer;

                            if (transform.GetChild(i).GetChild(j).childCount > 0)
                            {
                                transform.GetChild(i).GetChild(j).GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName = sortingLayer;

                                transform.GetChild(i).GetChild(j).GetChild(0).GetComponent<SpriteRenderer>().sortingOrder =
                                transform.GetChild(i).GetChild(j).GetComponent<SpriteRenderer>().sortingOrder + 1;
                            }
                        }
                    }
                }

                break;

            case Type.Single:
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).name.Contains("End") || transform.GetChild(i).name.Contains("WindowFrame"))
                    {
                        // Wall ends
                        transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = order + 1;
                        transform.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = sortingLayer;
                    }
                    else
                    {
                        transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = order;
                        transform.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = sortingLayer;
                    }
                }

                break;

            case Type.Self:
                GetComponent<SpriteRenderer>().sortingOrder = order;
                GetComponent<SpriteRenderer>().sortingLayerName = sortingLayer;

                break;
        }
    }
}
