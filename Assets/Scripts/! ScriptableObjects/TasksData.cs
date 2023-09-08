using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "TasksData", menuName = "ScriptableObject/TasksData", order = 7)]
    public class TasksData : ScriptableObject
    {
        public Types.Task[] tasks;

        void OnValidate()
        {
            for (int i = 0; i < tasks.Length; i++)
            {
                for (int j = 0; j < tasks[i].needs.Length; j++)
                {
                    if (tasks[i].needs[j].amount == 0)
                    {
                        tasks[i].needs[j].amount = 1;
                    }
                }

                for (int j = 0; j < tasks[i].rewards.Length; j++)
                {
                    if (tasks[i].rewards[j].amount == 0)
                    {
                        tasks[i].rewards[j].amount = 1;
                    }
                }
            }
        }
    }
}
