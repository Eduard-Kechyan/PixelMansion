using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "Colls", menuName = "ScriptableObject/Colls")]
    public class Colls : ScriptableObject
    {
        public Types.Coll[] content;

        void OnValidate()
        {
            for (int i = 0; i < content.Length; i++)
            {
                content[i].name = content[i].collGroup.ToString();

                for (int j = 0; j < content[i].parents.Length; j++)
                {
                    content[i].parents[j].name = content[i].parents[j].type.ToString() + " " + (content[i].parents[j].type == Types.Type.Gen
                    ? content[i].parents[j].genGroup.ToString()
                    : content[i].parents[j].chestGroup.ToString());
                }
            }
        }
    }
}
