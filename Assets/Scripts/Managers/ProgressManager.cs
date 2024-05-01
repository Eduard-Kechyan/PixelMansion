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
        private GameRefs gameRefs;
        private WorldDataManager worldDataManager;
        private ConvoUIHandler convoUIHandler;
        private TutorialManager tutorialManager;
        private TaskManager taskManager;
        private GameData gameData;
        private WorldUI worldUI;
        private ValuesUI valuesUI;
        private CharMain charMain;
        private CloudSave cloudSave;
        private ErrorManager errorManager;
        private ValuePop valuePop;

        void Start()
        {
            // Cache
            gameRefs = GameRefs.Instance;
            worldDataManager = gameRefs.worldDataManager;
            convoUIHandler = gameRefs.convoUIHandler;
            tutorialManager = gameRefs.tutorialManager;
            gameData = GameData.Instance;
            worldUI = gameRefs.worldUI;
            valuesUI = gameRefs.valuesUI;
            charMain = CharMain.Instance;
            errorManager = ErrorManager.Instance;
            valuePop = gameRefs.valuePop;

            if (taskManager == null)
            {
                taskManager = GetComponent<TaskManager>();
            }

            if (cloudSave == null)
            {
                cloudSave = Services.Instance.GetComponent<CloudSave>();
            }

            Glob.SetTimeout(() =>
            {
                if (!settingInitial)
                {
                    taskManager.CheckTaskNoteDot();
                }
            }, 0.3f);

            CheckForProgressSteps();
        }

        public void SetInitialData(int order = 0, bool last = false)
        {
            settingInitial = true;

            taskManager = GetComponent<TaskManager>();
            cloudSave = Services.Instance.GetComponent<CloudSave>();

            if (last)
            {
                PlayerPrefs.SetInt("initialTaskDataSet", 1);

                cloudSave.SetUnsavedData("initialTaskDataSet", 1);
            }
            else if (!initialSet)
            {
                if (GameData.Instance.tasksData.Count == 0 && !PlayerPrefs.HasKey("initialTaskDataSet"))
                {
                    taskManager.TryToAddTask(progressData.areas[0].id, progressData.areas[0].steps[order].id);
                }

                taskManager.CheckTaskNoteDot();
            }

            initialSet = true;
            settingInitial = false;
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

                    if (progressData.areas[i].steps[length].stepType == Types.StepType.Last || progressData.areas[i].steps[length].stepType == Types.StepType.PreMansion)
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
                    // Add the area and its first and second step
                    HandleNextStepType(progressData.areas[i].steps[0].stepType, areaId, progressData.areas[i].steps[0].id);
                    HandleNextStepType(progressData.areas[i].steps[1].stepType, areaId, progressData.areas[i].steps[1].id);

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
                                bool shouldShowUI = true;

                                for (int k = 0; k < progressData.areas[i].steps[j].nextIds.Length; k++)
                                {
                                    if (!HandleNextStep(groupId, progressData.areas[i].steps[j].nextIds[k], progressData.areas[i].steps) || (tutorialManager != null && tutorialManager.CheckIfNextStepIsConvo()))
                                    {
                                        shouldShowUI = false;
                                    }
                                }

                                if (shouldShowUI)
                                {
                                    worldUI.OpenUI();

                                    valuesUI.OpenUI();

                                    charMain.Show();
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
        bool HandleNextStep(string groupId, string nextStepId, Types.Step[] steps)
        {
            bool shouldShowUI = true;

            for (int i = 0; i < steps.Length; i++)
            {
                if (steps[i].id == nextStepId)
                {
                    bool addNewTask = true;

                    // Check for the last step
                    if (steps[i].stepType == Types.StepType.Last || steps[i].stepType == Types.StepType.PreMansion)
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
                        if (shouldShowUI && steps[i].stepType == Types.StepType.Convo)
                        {
                            shouldShowUI = false;
                        }

                        HandleNextStepType(steps[i].stepType, groupId, steps[i].id);
                    }

                    break;
                }
            }

            return shouldShowUI;
        }

        // TODO - This funciton isn't being used anymore, check if we need to keep it? 
        public bool CheckIfNextIsConvo(string groupId, string stepId)
        {
            bool nextIsConvo = false;

            for (int i = 0; i < progressData.areas.Length; i++)
            {
                if (progressData.areas[i].id == groupId)
                {
                    for (int j = 0; j < progressData.areas[i].steps.Length; j++)
                    {
                        if (progressData.areas[i].steps[j].id == stepId && progressData.areas[i].steps[j].nextIds.Length > 0)
                        {
                            for (int k = 0; k < progressData.areas[i].steps[j].nextIds.Length; k++)
                            {
                                if (!nextIsConvo)
                                {
                                    for (int l = 0; l < progressData.areas[i].steps.Length; l++)
                                    {
                                        if (progressData.areas[i].steps[l].id == progressData.areas[i].steps[j].nextIds[k])
                                        {
                                            nextIsConvo = true;

                                            break;
                                        }
                                    }

                                    break;
                                }
                            }

                            break;
                        }
                    }

                    break;
                }
            }

            return nextIsConvo;
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
                case Types.StepType.Convo:
                    if (worldDataManager != null)
                    {
                        SetProgressStep(stepType, stepId);

                        Glob.WaitForSelectable(() =>
                        {
                            StartCoroutine(WaitForPop(() =>
                            {
                                convoUIHandler.Converse(stepId);
                            }));
                        });
                    }
                    else
                    {
                        Debug.LogWarning("convoUIHandler is null");
                    }
                    break;
                default:
                    taskManager.TryToAddTask(areaId, stepId);
                    break;
            }
        }

        IEnumerator WaitForPop(Action callback)
        {
            while (valuePop.popping || valuePop.mightPop)
            {
                yield return null;
            }

            callback();
        }

        void CheckForProgressSteps()
        {
            if (PlayerPrefs.HasKey("progressStep"))
            {
                Types.ProgressStep oldProgressStep = JsonConvert.DeserializeObject<Types.ProgressStep>(PlayerPrefs.GetString("progressStep"));

                switch (Glob.ParseEnum<Types.StepType>(oldProgressStep.stepType))
                {
                    case Types.StepType.Convo:
                        if (worldDataManager != null)
                        {
                            Glob.SetTimeout(() =>
                            {
                                Glob.WaitForSelectable(() =>
                                {
                                    StartCoroutine(WaitForPop(() =>
                                    {
                                        convoUIHandler.Converse(oldProgressStep.id);
                                    }));
                                });
                            }, 0.2f);
                        }
                        break;
                    default:
                        Debug.LogWarning("Types.StepType " + oldProgressStep.stepType + " is not being properly handled!");
                        break;
                }
            }
        }

        void SetProgressStep(Types.StepType stepType, string stepId)
        {
            Types.ProgressStep newProgressStep = new()
            {
                stepType = stepType.ToString(),
                id = stepId,
            };

            string progressStepString = JsonConvert.SerializeObject(newProgressStep);

            PlayerPrefs.SetString("progressStep", progressStepString);

            PlayerPrefs.Save();

            cloudSave.SaveDataAsync("progressStep", progressStepString);
        }
    }
}
