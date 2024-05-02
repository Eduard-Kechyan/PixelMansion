using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "InitialItems", menuName = "ScriptableObject/InitialItems")]
    public class InitialItems : ScriptableObject
    {
        public BoardManager.Tile[] content = new BoardManager.Tile[GameData.ITEM_COUNT];
    }
}