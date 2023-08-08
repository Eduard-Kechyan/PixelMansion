using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObject/LevelData", order = 5)]
public class LevelData : ScriptableObject
{
    public Types.ShopItemsContent[] levelRewardContent;
    public Types.ShopItemsContent[] levelTenRewardContent;
}
