using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Generators", menuName = "ScriptableObject/Generators", order = 2)]
public class Generators : ScriptableObject
{
    public Types.Generators[] content;
}
