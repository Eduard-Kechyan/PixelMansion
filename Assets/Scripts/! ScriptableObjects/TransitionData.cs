using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TransitionData", menuName = "ScriptableObject/TransitionData", order = 3)]
public class TransitionData : ScriptableObject
{
    [ReadOnly]
    public int backgroundColor = -1;
}
