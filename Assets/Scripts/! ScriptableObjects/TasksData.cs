using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "TasksData", menuName = "ScriptableObject/TasksData")]
    public class TasksData : ScriptableObject
    {
        public Types.TaskGroup[] taskGroups;

        void OnValidate()
        {
            for (int i = 0; i < taskGroups.Length; i++)
            {
                taskGroups[i].name =  taskGroups[i].id;

                for (int j = 0; j < taskGroups[i].tasks.Count; j++)
                {
                    taskGroups[i].tasks[j].name = taskGroups[i].tasks[j].id;

                    for (int k = 0; k < taskGroups[i].tasks[j].needs.Length; k++)
                    {
                        if (taskGroups[i].tasks[j].needs[k].amount == 0)
                        {
                            taskGroups[i].tasks[j].needs[k].amount = 1;
                        }
                    }

                    for (int k = 0; k < taskGroups[i].tasks[j].rewards.Length; k++)
                    {
                        if (taskGroups[i].tasks[j].rewards[k].amount == 0)
                        {
                            taskGroups[i].tasks[j].rewards[k].amount = 1;
                        }
                    }
                }
            }
        }
    }
}
