using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ValuesData", menuName = "ScriptableObject/ValuesData", order = 4)]
public class ValuesData : ScriptableObject
{
    public int[] maxExperienceMultiplier;
    public int[] experienceMultiplier;
    public int[] energyMultiplier;
    public int[] goldMultiplier;
    public int[] gemsMultiplier;
}