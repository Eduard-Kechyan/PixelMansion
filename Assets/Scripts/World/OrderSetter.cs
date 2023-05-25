using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderSetter : MonoBehaviour
{
    public Type type = Type.Single;
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
        Room,
        Furniture
    }

    void OnValidate()
    {
        if (set)
        {
            set = false;
        }

        isNavArea = type == Type.NavArea;

        CheckType();

        ToggleNavArea();

        SetSpriteOrders();
    }

    void CheckType()
    {
        if (GetComponent<RoomHandler>() != null)
        {
            type = Type.Room;
        }
    }

    void ToggleNavArea()
    {
        if (type == Type.NavArea)
        {
            int order;

            if (show)
            {
                order = 1000;
            }
            else
            {
                order = -100;
            }

            Transform walkable = transform.GetChild(0);
            Transform notWalkable = transform.GetChild(1);

            for (int i = 0; i < walkable.childCount; i++)
            {
                walkable.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = order;
            }

            for (int i = 0; i < notWalkable.childCount; i++)
            {
                notWalkable.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = order + 1;
            }
        }
    }

    public void SetSpriteOrders()
    {
        int order = (int)transform.position.z;

        switch (type)
        {
            case Type.Room:
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform firstChild = transform.GetChild(i);

                    for (int j = 0; j < firstChild.childCount; j++)
                    {
                        Transform secondChild = firstChild.GetChild(j);

                        if (j == 0)
                        {
                            // Overlay
                            secondChild.GetComponent<SpriteRenderer>().sortingOrder = order + 2;
                        }
                        else
                        {
                            secondChild.GetComponent<SpriteRenderer>().sortingOrder = order;

                            if (secondChild.childCount > 0)
                            {
                                // Main walls
                                for (int k = 0; k < secondChild.childCount; k++)
                                {
                                    secondChild.GetChild(k).GetComponent<SpriteRenderer>().sortingOrder = order + 1;
                                }
                            }
                        }
                    }
                }

                break;

            case Type.Furniture:

                break;

            case Type.Single:
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).name.Contains("End"))
                    {
                        // Wall ends
                        transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = order + 1;
                    }
                    else
                    {
                        transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = order;
                    }
                }

                break;

            case Type.Self:
                GetComponent<SpriteRenderer>().sortingOrder = order;

                break;
        }
    }
}
