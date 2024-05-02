using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "Items", menuName = "ScriptableObject/Items")]
    public class Items : ScriptableObject
    {
        public BoardManager.TypeItem[] content;

        void OnValidate()
        {
            for (int i = 0; i < content.Length; i++)
            {
                switch (content[i].type)
                {
                    case Item.Type.Item:
                        content[i].name = content[i].group.ToString();
                        break;
                    case Item.Type.Gen:
                        content[i].name = content[i].genGroup.ToString();
                        break;
                    case Item.Type.Coll:
                        content[i].name = content[i].collGroup.ToString();
                        break;
                    case Item.Type.Chest:
                        content[i].name = content[i].chestGroup.ToString();
                        break;
                }

                for (int j = 0; j < content[i].parents.Length; j++)
                {
                    content[i].parents[j].name = content[i].parents[j].type.ToString() + " " + (content[i].parents[j].type == Item.Type.Gen
                    ? content[i].parents[j].genGroup.ToString()
                    : content[i].parents[j].chestGroup.ToString());
                }
            }
        }
    }
}