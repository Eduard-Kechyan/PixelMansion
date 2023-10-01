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
        public ProgressData progressData;
        public CurrentProgress currentProgress;

        [Serializable]
        public class CurrentProgress
        {
            [ReadOnly]
            public string currentAreaId;

            [ReadOnly]
            public int currentStepOrder;

            [ReadOnly]
            public string currentStepContentId;
        }

        private bool initialSet = false;
        private bool settingInitial = false;

        // References
        private TaskManager taskManager;

        void Start()
        {
            // Cache
            taskManager = GetComponent<TaskManager>();

            Glob.SetTimeout(() =>
            {
                if (!settingInitial)
                {
                    taskManager.CheckTaskNoteDot();
                }
            }, 0.3f);
        }

        public void CheckInitialData()
        {
            settingInitial = true;

            if (!initialSet)
            {
                if (PlayerPrefs.HasKey("currentProgress"))
                {
                    currentProgress = JsonConvert.DeserializeObject<CurrentProgress>(
                        PlayerPrefs.GetString("currentProgress")
                    );
                }
                else
                {
                    if (progressData.areas.Length > 0)
                    {
                        currentProgress.currentAreaId = progressData.areas[0].id;
                    }

                    if (progressData.areas[0].steps.Length > 0)
                    {
                        currentProgress.currentStepOrder = progressData.areas[0].steps[0].order;
                    }

                    if (progressData.areas[0].steps[0].content.Length > 0)
                    {
                        currentProgress.currentStepContentId = progressData.areas[0].steps[0].content[0].id;
                    }

                    PlayerPrefs.SetString(
                        "currentProgress",
                        JsonConvert.SerializeObject(currentProgress)
                    );
                }


                // TODO - Remove this line
                if (GameData.Instance.tasksData.Count == 0 && !PlayerPrefs.HasKey("TempTaskDataSet"))
                {
                    taskManager.AddTaskGroup(progressData.areas[0].id);
                    taskManager.AddTask(
                        currentProgress.currentStepContentId = progressData.areas[0].steps[
                            0
                        ].content[0].id,
                        progressData.areas[0].id
                    );

                    PlayerPrefs.SetInt("TempTaskDataSet", 1);
                }

                taskManager.CheckTaskNoteDot();

                initialSet = true;
                settingInitial = false;
            }
        }

        public bool CheckNextTaskIds(string groupId, string taskId)
        {
            bool hasNext = true;

            for (int i = 0; i < progressData.areas.Length; i++)
            {
                if (progressData.areas[i].id == groupId)
                {
                    if (progressData.areas[i].steps.Length > 0)
                    {
                        for (int j = 0; j < progressData.areas[i].steps.Length; j++)
                        {
                            if (Array.Exists(progressData.areas[i].steps[j].nextIds, id => id == taskId))
                            {
                                for (int k = 0; k < progressData.areas[i].steps[j].nextIds.Length; k++)
                                {
                                    taskManager.AddTaskGroup(groupId);
                                    taskManager.AddTask(
                                        progressData.areas[i].steps[j].nextIds[k],
                                        groupId
                                    );
                                }
                            }
                        }
                    }
                    else
                    {
                        hasNext = false;
                    }
                }
            }

            return hasNext;
        }

        public void CheckNextTaskGroupIds(string groupId)
        {
            bool hasNext = true;

            for (int i = 0; i < progressData.areas.Length; i++)
            {
                if (progressData.areas[i].id == groupId)
                {
                    if (progressData.areas[i].steps.Length > 0)
                    {
                        for (int j = 0; j < progressData.areas[i].steps.Length; j++)
                        {
                            
                        }
                    }
                    else
                    {
                        hasNext = false;
                    }
                }
            }

            //return hasNext;
        }
    }
}
