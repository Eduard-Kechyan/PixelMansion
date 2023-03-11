using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Items", menuName = "ScriptableObject/Items", order = 1)]
public class Items : ScriptableObject
{
    public Types.Items[] content;
}
