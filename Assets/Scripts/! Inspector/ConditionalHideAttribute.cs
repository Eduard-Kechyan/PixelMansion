using UnityEngine;
using System;
using System.Collections;

//Original version of the ConditionAttribute created by Brecht Lecluyse (www.brechtos.com)
//Modified by: -

namespace Merge
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ConditionAttribute : PropertyAttribute
{
    public string FirstCondition = "";
    public string SecondCondition = "";
    public string[] FirstConditions = new string[] { };
    public bool[] FirstConditionInverseBools = new bool[] { };
    public bool HideInInspector = false;
    public bool Inverse = false;
    public bool UseOrLogic = false;

    public bool InverseCondition1 = false;
    public bool InverseCondition2 = false;


	// Use this for initialization
    public ConditionAttribute(string FirstCondition)
    {
        this.FirstCondition = FirstCondition;
        this.HideInInspector = false;
        this.Inverse = false;
    }

    public ConditionAttribute(string FirstCondition, bool hideInInspector)
    {
        this.FirstCondition = FirstCondition;
        this.HideInInspector = hideInInspector;
        this.Inverse = false;
    }

    public ConditionAttribute(string FirstCondition, bool hideInInspector, bool inverse)
    {
        this.FirstCondition = FirstCondition;
        this.HideInInspector = hideInInspector;
        this.Inverse = inverse;
    }

    public ConditionAttribute(bool hideInInspector = false)
    {
        this.FirstCondition = "";
        this.HideInInspector = hideInInspector;
        this.Inverse = false;
    }

    public ConditionAttribute(string[] FirstConditions,bool[] FirstConditionInverseBools, bool hideInInspector, bool inverse)
    {
        this.FirstConditions = FirstConditions;
        this.FirstConditionInverseBools = FirstConditionInverseBools;
        this.HideInInspector = hideInInspector;
        this.Inverse = inverse;
    }

    public ConditionAttribute(string[] FirstConditions, bool hideInInspector, bool inverse)
    {
        this.FirstConditions = FirstConditions;        
        this.HideInInspector = hideInInspector;
        this.Inverse = inverse;
    }

}
}