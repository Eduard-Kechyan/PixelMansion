using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "InitialItems", menuName = "ScriptableObject/InitialItems")]
    public class InitialItems : ScriptableObject
    {
        public Types.Tile[] content = new Types.Tile[GameData.ITEM_COUNT];
    }
}