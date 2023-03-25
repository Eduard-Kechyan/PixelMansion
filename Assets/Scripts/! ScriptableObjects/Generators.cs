using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Generators", menuName = "ScriptableObject/Generators", order = 2)]
public class Generators : ScriptableObject
{
    public Types.Generators[] content;

    void OnValidate()
    {
        if (content.Length > 0)
        {
            for (int i = 0; i < content.Length; i++)
            {
                if (content[i].creates.Length > 0)
                {
                    content[i].createsTotal = 0;

                    for (int j = 0; j < content[i].creates.Length; j++)
                    {
                        content[i].createsTotal += (int)content[i].creates[j].chance;
                    }
                }
            }
        }
    }
}
