using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
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

#if UNITY_EDITOR
        void OnValidate()
        {
            if (set)
            {
                set = false;
            }

            isNavArea = type == Type.NavArea;

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
                SpriteRenderer walkableRenderer = walkable.GetComponent<SpriteRenderer>();
                SpriteRenderer notWalkableRenderer = notWalkable.GetComponent<SpriteRenderer>();

                for (int i = 0; i < walkable.childCount; i++)
                {
                    walkableRenderer.sortingOrder = 0;
                    walkableRenderer.sortingLayerName = show ? sortingLayer : "Default";
                }

                for (int i = 0; i < notWalkable.childCount; i++)
                {
                    notWalkableRenderer.sortingOrder = 1;
                    notWalkableRenderer.sortingLayerName = show ? sortingLayer : "Default";
                }
            }
        }

        public void SetSpriteOrders()
        {
            switch (type)
            {
                case Type.Room:
                    // Set room sprite orders
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        // Set position z
                        int positionZ = SortingLayer.GetLayerValueFromName(sortingLayer);

                        transform.position = new Vector3(transform.position.x, transform.position.y, positionZ);

                        // Set sorting order
                        Transform roomPart = transform.GetChild(i);

                        if (roomPart.name == "Floor")
                        {
                            // Set position z
                            roomPart.transform.position = new Vector3(roomPart.transform.position.x, roomPart.transform.position.y, positionZ + 0.3f);

                            // Set sorting order
                            Transform floorTiles = roomPart;

                            for (int j = 0; j < floorTiles.childCount; j++)
                            {
                                Transform floorTileTransform = floorTiles.GetChild(j);
                                SpriteRenderer floorTileSingle = floorTileTransform.GetComponent<SpriteRenderer>();
                                SpriteRenderer floorOverlay = floorTileTransform.GetChild(0).GetComponent<SpriteRenderer>();

                                floorTileSingle.sortingOrder = 0;
                                floorTileSingle.sortingLayerName = sortingLayer;

                                floorOverlay.sortingOrder = 1;
                                floorOverlay.sortingLayerName = sortingLayer;
                            }
                        }
                        else if (roomPart.name.Contains("Wall"))
                        {
                            // Set position z
                            roomPart.transform.position = new Vector3(
                                roomPart.transform.position.x,
                                roomPart.transform.position.y,
                                positionZ + (roomPart.name.Contains("Right") ? 0.2f : 0.1f)
                            );

                            // Set sorting order
                            Transform wallChunks = roomPart;

                            for (int j = 0; j < wallChunks.childCount; j++)
                            {
                                Transform wallChunkTransform = wallChunks.GetChild(j);
                                SpriteRenderer wallChunkSingle = wallChunkTransform.GetComponent<SpriteRenderer>();

                                wallChunkSingle.sortingOrder = 0;
                                wallChunkSingle.sortingLayerName = sortingLayer;

                                for (int k = 0; k < wallChunkTransform.childCount; k++)
                                {
                                    if (k >= (wallChunkTransform.childCount == 2 ? 1 : 2)) // Placeholder
                                    {
                                        SpriteRenderer wallPlaceholder = wallChunkTransform.GetChild(k).GetComponent<SpriteRenderer>();
                                        wallPlaceholder.sortingOrder = 1;
                                        wallPlaceholder.sortingLayerName = sortingLayer;
                                    }
                                    else // Overlay
                                    {
                                        SpriteRenderer wallOverlay = wallChunkTransform.GetChild(k).GetComponent<SpriteRenderer>();
                                        wallOverlay.sortingOrder = 2;
                                        wallOverlay.sortingLayerName = sortingLayer;
                                    }
                                }
                            }
                        }
                        else if (roomPart.name == "Furniture")
                        {
                            // Set position z
                            roomPart.transform.position = new Vector3(roomPart.transform.position.x, roomPart.transform.position.y, positionZ + 0.4f);

                            // Set sorting order
                            for (int j = 0; j < roomPart.childCount; j++)
                            {
                                SpriteRenderer furniturePart = roomPart.GetChild(j).GetComponent<SpriteRenderer>();

                                furniturePart.sortingOrder = 3;
                                furniturePart.sortingLayerName = sortingLayer;
                            }
                        }
                        /* else
                         {
                             // TODO - Handle items here
                             Debug.Log("Room part Items in't implemented yet!");
                         }*/
                    }

                    break;

                case Type.Single:
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Transform singleChild = transform.GetChild(i);
                        SpriteRenderer singleChildRenderer = singleChild.GetComponent<SpriteRenderer>();

                        if (singleChild.name.Contains("End") || singleChild.name.Contains("Corner") || singleChild.name.Contains("WindowFrame"))
                        {
                            // Wall ends
                            singleChildRenderer.sortingOrder = 1;
                            singleChildRenderer.sortingLayerName = sortingLayer;
                        }
                        else
                        {
                            singleChildRenderer.sortingOrder = 0;
                            singleChildRenderer.sortingLayerName = sortingLayer;
                        }
                    }

                    break;

                case Type.Self:
                    SpriteRenderer self = GetComponent<SpriteRenderer>();

                    self.sortingOrder = 0;
                    self.sortingLayerName = sortingLayer;

                    break;
            }
        }
    }
}