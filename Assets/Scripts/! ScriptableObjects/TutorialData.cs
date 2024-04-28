using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "TutorialData", menuName = "ScriptableObject/TutorialData")]
    public class TutorialData : ScriptableObject
    {
        public Types.TutorialStep[] steps;

        void OnValidate()
        {
            for (int i = 0; i < steps.Length; i++)
            {
                if (steps[i].type == Types.TutorialStepType.Task)
                {
                    steps[i].name = steps[i].scene.ToString() + " " + steps[i].type.ToString() + " " + steps[i].taskType.ToString() + " " + steps[i].id;
                }
                else
                {
                    steps[i].name = steps[i].scene.ToString() + " " + steps[i].type.ToString() + " " + steps[i].id;

                    steps[i].taskOrder = 0;
                }
            }
        }
    }
}
