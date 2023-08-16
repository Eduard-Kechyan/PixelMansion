using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "Items", menuName = "ScriptableObject/Items", order = 1)]
    public class Items : ScriptableObject
    {
        public Types.Items[] content;
    }
}