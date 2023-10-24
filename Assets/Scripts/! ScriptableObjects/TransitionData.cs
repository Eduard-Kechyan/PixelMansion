using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
[CreateAssetMenu(fileName = "TransitionData", menuName = "ScriptableObject/TransitionData")]
public class TransitionData : ScriptableObject
{
    [ReadOnly]
    public int backgroundColor = -1;
}
}