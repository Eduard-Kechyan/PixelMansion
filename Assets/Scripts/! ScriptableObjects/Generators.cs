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

                float createsFullPercent = 0;

                for (int j = 0; j < content[i].creates.Length; j++)
                {
                    content[i].creates[j].name = content[i].creates[j].group.ToString();

                    createsFullPercent += content[i].creates[j].chance;
                }

                if (createsFullPercent > 100)
                {
                    Debug.LogWarning(content[i].name +" created percent is too big.");
                }
                else if (createsFullPercent < 100)
                {
                    Debug.LogWarning(content[i].name + " created percent is too small.");
                }

                content[i].coolDown.minutes = content[i].coolDown.seconds / 60;
            }
        }
    }
}
