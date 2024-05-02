using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "ShopData", menuName = "ScriptableObject/ShopData")]
    public class ShopData : ScriptableObject
    {
        public ShopMenu.ShopItemsContent[] dailyContent;
        public ShopMenu.ShopItemsContent[] itemsContent;
        public ShopMenu.ShopValuesContent[] gemsContent;
        public ShopMenu.ShopValuesContent[] goldContent;
    }
}