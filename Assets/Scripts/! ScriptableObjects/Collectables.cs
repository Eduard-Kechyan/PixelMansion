using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Collectables", menuName = "ScriptableObject/Collectables", order = 3)]
public class Collectables : ScriptableObject
{
    public Types.Collectables[] content;
}
