using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Merge
{
    public class ProgressManager : MonoBehaviour
    {
        // Variables
        public TasksData tasksData;
        public ProgressData progressData;

        private bool initialSet = false;
        private bool settingInitial = false;

        // References
        private TaskManager taskManager;
        private GameData gameData;

        void Start()
        {
            // Cache
            taskManager = GetComponent<TaskManager>();
            gameData = GameData.Instance;

            Glob.SetTimeout(() =>
            {
                if (!settingInitial)
                {
                    taskManager.CheckTaskNoteDot();
                }
            }, 0.3f);

            // Subscribe to events
            DataManager.checkProgressEvent += CheckInitialData;
        }

        void OnDestroy()
        {
            // Unsubscribe from events
            DataManager.checkProgressEvent -= CheckInitialData;
        }

        // Dummy function for testing
        // TODO - Remove this
        public void CheckInitialData()
        {
            settingInitial = true;

            if (!initialSet)
            {
                if (GameData.Instance.tasksData.Count == 0 && !PlayerPrefs.HasKey("TempTaskDataSet"))
                {
                    taskManager.TryToAddTask(
                        progressData.areas[0].id,
                        progressData.areas[0].steps[0].id
                    );

                    PlayerPrefs.SetInt("TempTaskDataSet", 1);
                }

                taskManager.CheckTaskNoteDot();

                initialSet = true;
                settingInitial = false;
            }
        }

        //// Next Area ////

        // Check for the next area of the last step
        public void CheckNextArea(string groupId)
        {
            for (int i = 0; i < progressData.areas.Length; i++)
            {
                if (progressData.areas[i].id == groupId)
                {
                    // To get the last step
                    int length = progressData.areas[i].steps.Length - 1;

                    if (progressData.areas[i].steps[length].id == "Last")
                    {
                        // Check if the step even has next ids
                        if (progressData.areas[i].steps[length].nextIds.Length > 0)
                        {
                            for (int k = 0; k < progressData.areas[i].steps[length].nextIds.Length; k++)
                            {
                                HandleNextArea(progressData.areas[i].steps[length].nextIds[k]);
                            }
                        }
                    }

                    break;
                }
            }
        }

        // Call next area functions
        void HandleNextArea(string areaId)
        {
            for (int i = 0; i < progressData.areas.Length; i++)
            {
                if (progressData.areas[i].id == areaId)
                {
                    // Add the area and its first task that is a step
                    for (int j = 0; j < progressData.areas[i].steps.Length; j++)
                    {
                        if (progressData.areas[i].steps[j].stepType == Types.StepType.Task)
                        {
                            taskManager.TryToAddTask(areaId, progressData.areas[i].steps[j].id);

                            break;
                        }
                    }


                    break;
                }
            }
        }

        //// Next Step ////

        // Check for the next steps of the step
        public void CheckNextIds(string groupId, string stepId)
        {
            for (int i = 0; i < progressData.areas.Length; i++)
            {
                if (progressData.areas[i].id == groupId)
                {
                    for (int j = 0; j < progressData.areas[i].steps.Length; j++)
                    {
                        if (progressData.areas[i].steps[j].id == stepId)
                        {
                            // Check if the step even has next ids
                            if (progressData.areas[i].steps[j].nextIds.Length > 0)
                            {
                                for (int k = 0; k < progressData.areas[i].steps[j].nextIds.Length; k++)
                                {
                                    HandleNextStep(groupId, progressData.areas[i].steps[j].nextIds[k], progressData.areas[i].steps);
                                }
                            }

                            break;
                        }
                    }

                    break;
                }
            }
        }

        // Check the next step's type
        void HandleNextStep(string groupId, string nextStepId, Types.Step[] steps)
        {
            for (int i = 0; i < steps.Length; i++)
            {
                if (steps[i].id == nextStepId)
                {
                    bool addNewTask = true;

                    // Check for the last step
                    if (nextStepId == "Last")
                    {
                        addNewTask = CheckLastRequirements(groupId);
                    }
                    // Check for requirements
                    else if (steps[i].requiredIds.Length > 0)
                    {
                        int count = 0;


                        for (int j = 0; j < steps[i].requiredIds.Length; j++)
                        {
                            for (int k = 0; k < gameData.finishedTasks.Count; k++)
                            {
                                if (groupId == gameData.finishedTasks[k].groupId && steps[i].requiredIds[j] == gameData.finishedTasks[k].taskId)
                                {
                                    count++;
                                }
                            }
                        }

                        if (count < steps[i].requiredIds.Length)
                        {
                            addNewTask = false;
                        }
                    }

                    if (addNewTask)
                    {
                        HandleNextStepType(steps[i].stepType, groupId, steps[i].id);
                    }

                    break;
                }
            }
        }

        bool CheckLastRequirements(string groupId)
        {
            int taskCount = 0;
            int finishedCount = 0;

            for (int i = 0; i < tasksData.taskGroups.Length; i++)
            {
                if (tasksData.taskGroups[i].id == groupId)
                {
                    // Don't use the last task
                    for (int j = 0; j < tasksData.taskGroups[i].tasks.Count - 1; j++)
                    {
                        taskCount++;

                        for (int k = 0; k < gameData.finishedTasks.Count; k++)
                        {
                            if (groupId == gameData.finishedTasks[k].groupId && tasksData.taskGroups[i].tasks[j].id == gameData.finishedTasks[k].taskId)
                            {
                                finishedCount++;
                            }
                        }
                    }

                    break;
                }
            }

            return taskCount == finishedCount;
        }

        // Check witch functions to call for the next step
        void HandleNextStepType(Types.StepType stepType, string areaId, string stepId)
        {
            switch (stepType)
            {
                case Types.StepType.Task:
                    taskManager.TryToAddTask(areaId, stepId);
                    break;
                case Types.StepType.Conversation:
                    Debug.Log("Handle conversation here!");
                    break;
                case Types.StepType.RoomUnlocking:
                    Debug.Log("Handle room unlocking here!");
                    break;
            }
        }
    }
}
