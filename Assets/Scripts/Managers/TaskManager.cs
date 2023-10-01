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
        public WorldDataManager worldDataManager;
        public BoardManager boardManager;

        // References
        private GameData gameData;
        private DataManager dataManager;
        private HubUI hubUI;
        private GameplayUI gameplayUI;
        private TaskMenu taskMenu;
        private Selector selector;
        private ProgressManager progressManager;
        private CameraMotion cameraMotion;

        private void Start()
        {
            // Initialize references to other managers and UI elements
            gameData = GameData.Instance;
            dataManager = DataManager.Instance;
            hubUI = GameRefs.Instance.hubUI;
            gameplayUI = GameRefs.Instance.gameplayUI;
            taskMenu = GameRefs.Instance.taskMenu;
            progressManager = GetComponent<ProgressManager>();
            cameraMotion = Camera.main.GetComponent<CameraMotion>();

            if (worldDataManager != null)
            {
                selector = worldDataManager.GetComponent<Selector>();
            }

            /* if (hubUI != null && PlayerPrefs.HasKey("TaskNoteDotSet"))
             {
                 hubUI.ToggleButtonNoteDot("task", true);
             }*/

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
            for (int i = 0; i < gameData.taskGroupsData.Count; i++)
            {
                if (gameData.taskGroupsData[i].id == areaId)
                {
                    gameData.taskGroupsData.RemoveAt(i);

                    break;
                }
            }

            // Save changes to disk
            dataManager.SaveTasks();
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

                /*hubUI.ToggleButtonNoteDot("task", true);
                PlayerPrefs.SetInt("TaskNoteDotSet", 1);*/

                CheckBoardAndInventoryForTasks();
            }
        }

        void RemoveTask(string taskId)
        {
            for (int i = 0; i < gameData.tasksData.Count; i++)
            {
                if (gameData.tasksData[i].id == taskId)
                {
                    gameData.tasksData.RemoveAt(i);

                    break;
                }
            }

            // Save changes to disk
            dataManager.SaveTasks();
        }

        public void TaskCompleted(string groupId, string taskId)
        {
            for (int i = 0; i < gameData.taskGroupsData.Count; i++)
            {
                if (gameData.taskGroupsData[i].id == groupId)
                {
                    gameData.taskGroupsData[i].completed += 1;

                    break;
                }
            }

            // TODO - Check this
           /* if (!progressManager.CheckNextTaskIds(groupId, taskId))
            {
                progressManager.CheckNextTaskGroupIds(groupId);
            }*/

            RemoveNeedsFromBoardAndInventory(taskId);

            RemoveTask(taskId);
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
            int hasAtLeastOneReady = 0;

            for (int i = 0; i < gameData.tasksData.Count; i++)
            {
                if (gameData.tasksData[i].needs.Length == gameData.tasksData[i].completed)
                {
                    hasAtLeastOneReady++;
                }
            }

            if (hubUI != null)
            {
                hubUI.ToggleButtonNoteDot("task", hasAtLeastOneReady > 0, hasAtLeastOneReady.ToString());
            }
            else
            {
                gameplayUI.ToggleButtonNoteDot("task", hasAtLeastOneReady > 0, hasAtLeastOneReady.ToString());
            }
        }

        // Check if any task is ready to be completed
        public void CheckBoardAndInventoryForTasks()
        {
            // Clear board items from being completed
            for (int x = 0; x < gameData.boardData.GetLength(0); x++)
            {
                for (int y = 0; y < gameData.boardData.GetLength(1); y++)
                {
                    gameData.boardData[x, y].isCompleted = false;
                }
            }

            bool completedAtLeastOnItem = false;

            // Count completed needs and set items to completed
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

                            if (!completedAtLeastOnItem && gameData.tasksData[j].needs[k].completed > 0)
                            {
                                completedAtLeastOnItem = true;
                            }
                        }
                    }

                    // Set task completed count
                    gameData.tasksData[j].completed = needsCompleted;
                }
            }

            dataManager.SaveTasks();

            dataManager.SaveBoard(false);

            if (completedAtLeastOnItem && boardManager != null)
            {
                boardManager.SetCompletedItems();
            }

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
                        && gameData.boardData[x, y].state == Types.State.Default
                        && gameData.boardData[x, y].type == taskNeed.type
                        && gameData.boardData[x, y].group == taskNeed.group
                        && gameData.boardData[x, y].genGroup == taskNeed.genGroup
                        && gameData.boardData[x, y].collGroup == taskNeed.collGroup
                        && gameData.boardData[x, y].chestGroup == taskNeed.chestGroup
                    )
                    {
                        gameData.boardData[x, y].isCompleted = true;

                        if (count < taskNeed.amount)
                        {
                            count++;
                        }
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
                    )
                    {
                        if (count < taskNeed.amount)
                        {
                            count++;
                        }
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

                TryToCompleteTask(splitTaskData[0], splitTaskData[1]);

                Glob.taskToComplete = "";
            }
        }

        public void TryToCompleteTask(string groupId, string taskId)
        {
            GameObject taskRef = new();

            for (int i = 0; i < tasksData.tasks.Length; i++)
            {
                if (tasksData.tasks[i].id == taskId)
                {
                    taskRef = worldDataManager.GetWorldItem(tasksData.tasks[i]);

                    break;
                }
            }

            if (cameraMotion != null)
            {
                cameraMotion.MoveTo(taskRef.transform.position, 250);
            }

            selector.SelectAlt(taskRef.GetComponent<Selectable>(), () =>
            {
                TaskCompleted(groupId, taskId);
            });
        }

        void RemoveNeedsFromBoardAndInventory(string taskId)
        {
            List<Types.TaskItem> needs = new();

            // Get needs
            for (int i = 0; i < gameData.tasksData.Count; i++)
            {
                if (gameData.tasksData[i].id == taskId)
                {
                    for (int j = 0; j < gameData.tasksData[i].needs.Length; j++)
                    {
                        for (int k = 0; k < gameData.tasksData[i].needs[j].amount; k++)
                        {
                            needs.Add(gameData.tasksData[i].needs[j]);
                        }
                    }

                    break;
                }
            }

            // Remove needs from the board
            for (int i = 0; i < needs.Count; i++)
            {
                for (int x = 0; x < gameData.boardData.GetLength(0); x++)
                {
                    for (int y = 0; y < gameData.boardData.GetLength(1); y++)
                    {
                        if (needs[i].sprite == gameData.boardData[x, y].sprite && needs[i].type == gameData.boardData[x, y].type && gameData.boardData[x, y].state == Types.State.Default)
                        {
                            int oldOrder = gameData.boardData[x, y].order;

                            gameData.boardData[x, y] = new Types.Board { order = oldOrder };

                            needs[i] = new Types.TaskItem();
                        }
                    }
                }
            }

            // Remove needs from board
            for (int i = 0; i < needs.Count; i++)
            {
                if (needs[i].sprite != null)
                {
                    for (int j = 0; j < gameData.inventoryData.Count; j++)
                    {
                        if (needs[i].sprite == gameData.inventoryData[j].sprite && needs[i].type == gameData.inventoryData[j].type)
                        {
                            gameData.inventoryData.RemoveAt(j);
                        }
                    }
                }
            }

            // Save data to disk
            dataManager.SaveInventory();
            dataManager.SaveBoard();
        }
    }
}
