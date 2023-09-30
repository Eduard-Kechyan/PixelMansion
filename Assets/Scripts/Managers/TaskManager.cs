using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Merge
{
    public class TaskManager : MonoBehaviour
    {
        // Variables
        public TasksData tasksData;
        public Selector selector;
        public WorldDataManager worldDataManager;

        // References
        private GameData gameData;
        private DataManager dataManager;
        private HubUI hubUi;
        private GameplayUI gameplayUI;
        private TaskMenu taskMenu;

        private void Start()
        {
            // Initialize references to other managers and UI elements
            gameData = GameData.Instance;
            dataManager = DataManager.Instance;
            hubUi = GameRefs.Instance.hubUI;
            gameplayUI = GameRefs.Instance.gameplayUI;
            taskMenu = GameRefs.Instance.taskMenu;

            CheckIfThereIsATaskToComplete();

            DataManager.boardSaveEvent += CheckBoardAndInventoryForTasks;
        }

        private void OnDestroy()
        {
            DataManager.boardSaveEvent -= CheckBoardAndInventoryForTasks;
        }

        //// Task Groups ////

        public void AddTaskGroup(string areaId)
        {
            if (!TaskGroupExists(areaId))
            {
                int totalTasks = 0;

                for (int i = 0; i < tasksData.tasks.Length; i++)
                {
                    if (tasksData.tasks[i].groupId == areaId)
                    {
                        totalTasks++;
                    }
                }

                // Create a new task group and add it to the list
                var newTaskGroup = new Types.TaskGroup { id = areaId, completed = 0, total = totalTasks };

                gameData.taskGroupsData.Add(newTaskGroup);

                // Save changes to disk
                dataManager.SaveTasks();
            }
        }

        public void RemoveTaskGroup(string areaId)
        {
            // Remove task group logic here
        }

        private bool TaskGroupExists(string areaId)
        {
            // Check if a task group with the given area ID already exists
            return gameData.taskGroupsData.Exists(taskGroup => taskGroup.id == areaId);
        }

        //// Tasks ////

        public void AddTask(string taskId, string newGroupId)
        {
            if (TaskGroupExists(newGroupId) && !TaskExists(taskId))
            {
                // Find the task by ID
                var newTask = FindTaskById(taskId);

                if (newTask != null)
                {
                    // Assign the task to a new group
                    newTask.groupId = newGroupId;

                    // Add the task to the list of tasks
                    gameData.tasksData.Add(newTask);

                    // Save changes to disk
                    dataManager.SaveTasks();
                }
                else
                {
                    Debug.LogWarning(
                        $"No Task found with id: {taskId}, with Task Group id: {newGroupId}"
                    );
                }

                // Update task icons in the world
                // CheckWorldTaskIcons();

                CheckBoardAndInventoryForTasks();
            }
        }

        void RemoveTask(string taskId)
        {
            for (int i = 0; i < gameData.tasksData.Count; i++)
            {
                if (gameData.tasksData[i].id == taskId)
                {
                    gameData.tasksData.Remove(gameData.tasksData[i]);

                    break;
                }
            }

            // Save changes to disk
            dataManager.SaveTasks();
        }

        public Types.TaskGroup TaskCompleted(string groupId, string taskId)
        {
            Types.TaskGroup taskGroupData = new();

            for (int i = 0; i < gameData.taskGroupsData.Count; i++)
            {
                if (gameData.taskGroupsData[i].id == groupId)
                {
                    gameData.taskGroupsData[i].completed += 1;

                    taskGroupData = gameData.taskGroupsData[i];

                    break;
                }
            }

            RemoveTask(taskId);

            return taskGroupData;
        }

        private Types.Task FindTaskById(string taskId)
        {
            // Find a task by its ID from the tasks data
            return tasksData.tasks.FirstOrDefault(task => task.id == taskId);
        }

        private bool TaskExists(string taskId)
        {
            // Check if a task with the given ID already exists
            return gameData.tasksData.Exists(task => task.id == taskId);
        }

        //// Completion ////

        // Check if any task is ready and enable the task button note dot
        public void CheckTaskNoteDot()
        {
            bool hasAtLeastOneReady = false;

            for (int i = 0; i < gameData.tasksData.Count; i++)
            {
                if (gameData.tasksData[i].needs.Length == gameData.tasksData[i].completed)
                {
                    hasAtLeastOneReady = true;

                    break;
                }
            }

            if (hubUi != null)
            {
                hubUi.ToggleButtonNoteDot("task", hasAtLeastOneReady);
            }
            else
            {
                gameplayUI.ToggleButtonNoteDot("task", hasAtLeastOneReady);
            }
        }

        // Check if any task is ready to be completed
        public void CheckBoardAndInventoryForTasks()
        {
            //newBoardData.state == Types.State.Default && newBoardData.type == Types.Type.Item
            for (int i = 0; i < gameData.taskGroupsData.Count; i++)
            {
                for (int j = 0; j < gameData.tasksData.Count; j++)
                {
                    int needsCompleted = 0;

                    // Set task needs completed count
                    if (gameData.tasksData[j].groupId == gameData.taskGroupsData[i].id)
                    {
                        for (int k = 0; k < gameData.tasksData[j].needs.Length; k++)
                        {
                            gameData.tasksData[j].needs[k].completed = CheckNeedCompleted(gameData.tasksData[j].needs[k]);

                            if (gameData.tasksData[j].needs[k].amount == gameData.tasksData[j].needs[k].completed)
                            {
                                needsCompleted++;
                            }
                        }
                    }

                    // Set task completed count
                    gameData.tasksData[j].completed = needsCompleted;
                }
            }

            dataManager.SaveTasks();

            CheckTaskNoteDot();
        }

        // Check if task needs exists on the board or in the inventory
        int CheckNeedCompleted(Types.TaskItem taskNeed)
        {
            int count = 0;

            // Count on the board
            for (int x = 0; x < gameData.boardData.GetLength(0); x++)
            {
                for (int y = 0; y < gameData.boardData.GetLength(1); y++)
                {
                    if (
                        gameData.boardData[x, y].sprite == taskNeed.sprite
                        && gameData.boardData[x, y].type == taskNeed.type
                        && gameData.boardData[x, y].group == taskNeed.group
                        && gameData.boardData[x, y].genGroup == taskNeed.genGroup
                        && gameData.boardData[x, y].collGroup == taskNeed.collGroup
                        && gameData.boardData[x, y].chestGroup == taskNeed.chestGroup
                        && count < taskNeed.amount
                    )
                    {
                        count++;
                    }

                    if (count == taskNeed.amount)
                    {
                        break;
                    }
                }
            }

            if (count < taskNeed.amount)
            {
                // Count in the inventory
                for (int i = 0; i < gameData.inventoryData.Count; i++)
                {
                    if (
                        gameData.inventoryData[i].sprite == taskNeed.sprite
                        && gameData.inventoryData[i].type == taskNeed.type
                        && gameData.inventoryData[i].group == taskNeed.group
                        && gameData.inventoryData[i].genGroup == taskNeed.genGroup
                        && gameData.inventoryData[i].chestGroup == taskNeed.chestGroup
                        && count < taskNeed.amount
                    )
                    {
                        count++;
                    }

                    if (count == taskNeed.amount)
                    {
                        break;
                    }
                }
            }
            else if (count > taskNeed.amount)
            {
                count = taskNeed.amount;
            }

            return count;
        }

        void CheckIfThereIsATaskToComplete()
        {
            if (Glob.taskToComplete != "")
            {
                string[] splitTaskData = Glob.taskToComplete.Split("|");

                if (splitTaskData.Length == 3)
                {
                    TryToCompleteTask(splitTaskData[0], splitTaskData[1], splitTaskData[2]);
                }

                Glob.taskToComplete = "";
            }
        }

        public void TryToCompleteTask(string groupId, string taskId, string indexString)
        {
            Debug.Log(groupId);
            Debug.Log(taskId);
            Debug.Log(indexString);

            Selectable taskRef = new();

            for (int i = 0; i < tasksData.tasks.Length; i++)
            {
                if (tasksData.tasks[i].id == taskId)
                {
                    taskRef = worldDataManager.GetWorldItem(tasksData.tasks[i]).GetComponent<Selectable>();
                }
            }

            selector.SelectAlt(taskRef, () =>
            {
                Debug.Log("Confirmed!");
            }, () =>
            {
                Debug.Log("Canceled!");
            });

            //Types.TaskGroup taskGroupData = TaskCompleted(groupId, taskId);
            // taskMenu.EndTask(groupId, indexString,taskGroupData);
        }
    }
}
