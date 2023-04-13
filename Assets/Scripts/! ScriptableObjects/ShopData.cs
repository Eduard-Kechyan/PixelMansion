using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopData", menuName = "ScriptableObject/ShopData", order = 3)]
public class ShopData : ScriptableObject
{
    public Types.ShopItemsContent[] dailyContent;
    public Types.ShopItemsContent[] itemsContent;
    public Types.ShopValuesContent[] gemsContent;
    public Types.ShopValuesContent[] goldContent;
}
