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

        void OnValidate()
        {
            for (int i = 0; i < goldContent.Length; i++)
            {
                string typeString = goldContent[i].type.ToString();

                goldContent[i].id = typeString + "_" + i;

                goldContent[i].name = typeString + " " + goldContent[i].amount;

                goldContent[i].desc = "Buy " + goldContent[i].amount + " " + typeString;
            }

            for (int i = 0; i < gemsContent.Length; i++)
            {
                string typeString = gemsContent[i].type.ToString();

                gemsContent[i].id = typeString + "_" + i;

                gemsContent[i].name = typeString + " " + gemsContent[i].amount;

                gemsContent[i].desc = "Buy " + gemsContent[i].amount + " " + typeString;
            }
        }
    }
}