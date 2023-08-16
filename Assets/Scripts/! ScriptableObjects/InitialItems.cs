using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "InitialItems", menuName = "ScriptableObject/InitialItems", order = 0)]
public class InitialItems : ScriptableObject
{
    public Types.Board[] content = new Types.Board[GameData.ITEM_COUNT];
}
}