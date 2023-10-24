using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "Generators", menuName = "ScriptableObject/Generators")]
    public class Generators : ScriptableObject
    {
        public Types.Gen[] content;

        void OnValidate()
        {
            for (int i = 0; i < content.Length; i++)
            {
                content[i].name = content[i].genGroup.ToString();

                for (int j = 0; j < content[i].creates.Length; j++)
                {
                    content[i].creates[j].name = content[i].creates[j].group.ToString();
                }

                content[i].coolDown.minutes = content[i].coolDown.seconds / 60;
            }
        }
    }
}
