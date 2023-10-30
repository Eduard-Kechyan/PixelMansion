using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "Chests", menuName = "ScriptableObject/Chests")]
    public class Chests : ScriptableObject
    {
        public Types.Chest[] content;

        void OnValidate()
        {
            for (int i = 0; i < content.Length; i++)
            {
                content[i].name = content[i].chestGroup.ToString();

                for (int j = 0; j < content[i].creates.Length; j++)
                {
                    switch (content[i].creates[j].type)
                    {
                        case Types.Type.Item:
                            content[i].creates[j].name = content[i].creates[j].group.ToString();
                            break;
                        case Types.Type.Gen:
                            content[i].creates[j].name = content[i].creates[j].genGroup.ToString();
                            break;
                        case Types.Type.Coll:
                            content[i].creates[j].name = content[i].creates[j].collGroup.ToString();
                            break;
                    }
                }
            }
        }
    }
}
