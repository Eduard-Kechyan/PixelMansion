using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "ValuesData", menuName = "ScriptableObject/ValuesData")]
    public class ValuesData : ScriptableObject
    {
        public int[] maxExperienceMultiplier;
        public int[] experienceMultiplier;
        public int[] energyMultiplier;
        public int[] goldMultiplier;
        public int[] gemsMultiplier;
    }
}