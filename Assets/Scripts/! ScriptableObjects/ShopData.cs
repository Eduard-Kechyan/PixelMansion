using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "ShopData", menuName = "ScriptableObject/ShopData")]
    public class ShopData : ScriptableObject
    {
        public Types.ShopItemsContent[] dailyContent;
        public Types.ShopItemsContent[] itemsContent;
        public Types.ShopValuesContent[] gemsContent;
        public Types.ShopValuesContent[] goldContent;
    }
}