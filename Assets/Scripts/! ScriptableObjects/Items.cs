using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "Items", menuName = "ScriptableObject/Items", order = 1)]
    public class Items : ScriptableObject
    {
        public Types.Items[] content;

        void OnValidate()
        {
            for (int i = 0; i < content.Length; i++)
            {
                switch (content[i].type)
                {
                    case Types.Type.Item:
                        content[i].name = content[i].group.ToString();
                        break;
                    case Types.Type.Gen:
                        content[i].name = content[i].genGroup.ToString();
                        break;
                    case Types.Type.Coll:
                        content[i].name = content[i].collGroup.ToString();
                        break;
                    case Types.Type.Chest:
                        content[i].name = content[i].chestGroup.ToString();
                        break;
                }
            }
        }
    }
}