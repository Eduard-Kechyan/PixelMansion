using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "ProgressData", menuName = "ScriptableObject/ProgressData", order = 6)]
    public class ProgressData : ScriptableObject
    {
        public Types.Area[] areas;

        void OnValidate()
        {
            if (areas.Length > 0)
            {
                for (int i = 0; i < areas.Length; i++)
                {
                    areas[i].name = areas[i].id;

                    if (areas[i].id == "")
                    {
                        Debug.LogWarning("Area " + i + " doesn't have an id. In ProgressData.cs");
                    }

                    if (areas[i].steps.Length > 0)
                    {
                        for (int j = 0; j < areas[i].steps.Length; j++)
                        {
                            areas[i].steps[j].order = j;

                            if (areas[i].steps[j].content.Length > 0)
                            {
                                for (int k = 0; k < areas[i].steps[j].content.Length; k++)
                                {
                                    areas[i].steps[j].content[k].name = areas[i].steps[j].content[k].stepType.ToString() + " " + areas[i].steps[j].content[k].id;

                                    if (areas[i].steps[j].content[k].id == "")
                                    {
                                        Debug.LogWarning("Step Content" + k + " in Step " + j + " in Area " + i + " doesn't have an id. In ProgressData.cs");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
