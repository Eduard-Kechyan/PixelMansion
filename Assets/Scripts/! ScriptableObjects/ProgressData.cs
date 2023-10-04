using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "ProgressData", menuName = "ScriptableObject/ProgressData", order = 6)]
    public class ProgressData : ScriptableObject
    {
        public bool check;
        public Types.Area[] areas;

        void OnValidate()
        {
            if (check)
            {
                check = false;
            }

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
                            areas[i].steps[j].name = areas[i].steps[j].stepType.ToString() + " " + areas[i].steps[j].id;

                            if (areas[i].steps[j].id == "")
                            {
                                Debug.LogWarning("Step " + j + " in Area " + i + " doesn't have an id. In ProgressData.cs");
                            }

                            for (int k = 0; k < areas[i].steps[j].requiredIds.Length; k++)
                            {
                                if (areas[i].steps[j].requiredIds[k] == "")
                                {
                                    Debug.LogWarning("Required " + k + " in Step " + j + " in Area " + i + " doesn't have an id. In ProgressData.cs");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
