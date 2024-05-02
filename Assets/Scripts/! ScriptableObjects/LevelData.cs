using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObject/LevelData")]
    public class LevelData : ScriptableObject
    {
        public ShopMenu.ShopItemsContent[] levelRewardContent;
        public ShopMenu.ShopItemsContent[] levelTenRewardContent;
    }
}