using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "TasksData", menuName = "ScriptableObject/TasksData", order = 7)]
    public class TasksData : ScriptableObject
    {
        public Types.Task[] tasks;
    }
}
