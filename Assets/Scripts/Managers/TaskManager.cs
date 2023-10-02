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

        private Types.TaskGroup[] initialTaskData = null;

        // References
        private GameData gameData;
        private DataManager dataManager;
        private HubUI hubUI;
        private GameplayUI gameplayUI;
        private TaskMenu taskMenu;
        private Selector selector;
        private ProgressManager progressManager;
        private CameraMotion cameraMotion;

        void Awake()
        {
            initialTaskData = tasksData.taskGroups;
        }

        void Start()
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

            CheckIfThereIsATaskToComplete();

            DataManager.boardSaveEvent += CheckBoardAndInventoryForTasks;
        }

        void OnDestroy()
        {
            DataManager.boardSaveEvent -= CheckBoardAndInventoryForTasks;
        }

        //// Tasks ////

        public void AddTask(string groupId, string taskId)
        {
            Types.Task newTask = initialTaskData.FirstOrDefault(i => i.id == groupId).tasks.FirstOrDefault(i => i.id == taskId);

            bool addNewGroup = true;

            // Check if task group exists and add the new task to it
            for (int i = 0; i < gameData.tasksData.Count; i++)
            {
                if (gameData.tasksData[i].id == groupId)
                {
                    gameData.tasksData[i].tasks.Add(newTask);

                    addNewGroup = false;

                    break;
                }
            }

            // Add a new task group and add the new task to it
            if (addNewGroup)
            {
                Types.TaskGroup newTaskGroup = new()
                {
                    id = groupId,
                    tasks= new List<Types.Task>()
                };

                newTaskGroup.tasks.Add(newTask);

                gameData.tasksData.Add(newTaskGroup);
            }

            // Save data to disk
            dataManager.SaveTasks();

            CheckBoardAndInventoryForTasks();
        }

        void RemoveTask(string groupId, string taskId)
        {
            for (int i = 0; i < gameData.tasksData.Count; i++)
            {
                if (gameData.tasksData[i].id == groupId)
                {
                    for (int j = 0; j < gameData.tasksData[i].tasks.Count; j++)
                    {
                        if (gameData.tasksData[i].tasks[j].id == taskId)
                        {
                            gameData.tasksData[i].tasks.RemoveAt(j);

                            break;
                        }
                    }

                    break;
                }
            }

            // Save changes to disk
            dataManager.SaveTasks();
        }

        void RemoveTaskGroup(string groupId)
        {
            for (int i = 0; i < gameData.tasksData.Count; i++)
            {
                if (gameData.tasksData[i].id == groupId)
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
            for (int i = 0; i < gameData.tasksData.Count; i++)
            {
                if (gameData.tasksData[i].id == groupId)
                {
                    gameData.tasksData[i].completed += 1;

                    break;
                }
            }

            //bool isLastTask = progressManager.CheckNextTaskIds(groupId, taskId);

            // TODO - Check this
            /* if (!progressManager.CheckNextTaskIds(groupId, taskId))
             {
                 progressManager.CheckNextTaskGroupIds(groupId);
             }*/

            RemoveNeedsFromBoardAndInventory(groupId, taskId);

            RemoveTask(groupId, taskId);
        }

        //// Completion ////

        // Check if any task is ready and enable the task button note dot
        public void CheckTaskNoteDot()
        {
            int completedCount = 0;

            for (int i = 0; i < gameData.tasksData.Count; i++)
            {
                for (int j = 0; j < gameData.tasksData[i].tasks.Count; j++)
                {
                    if (gameData.tasksData[i].tasks[j].needs.Length == gameData.tasksData[i].tasks[j].completed)
                    {
                        completedCount++;
                    }
                }
            }

            if (hubUI != null)
            {
                hubUI.ToggleButtonNoteDot("task", completedCount > 0, completedCount.ToString());
            }
            else
            {
                gameplayUI.ToggleButtonNoteDot("task", completedCount > 0, completedCount.ToString());
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

            // If there is at least on item completed
            bool completedAtLeastOnItem = false;

            for (int i = 0; i < gameData.tasksData.Count; i++)
            {
                for (int j = 0; j < gameData.tasksData[i].tasks.Count; j++)
                {
                    int needsCompleted = 0;

                    for (int k = 0; k < gameData.tasksData[i].tasks[j].needs.Length; k++)
                    {
                        // Check needs completed
                        gameData.tasksData[i].tasks[j].needs[k].completed = CheckNeedCompleted(gameData.tasksData[i].tasks[j].needs[k]);

                        if (gameData.tasksData[i].tasks[j].needs[k].amount == gameData.tasksData[i].tasks[j].needs[k].completed)
                        {
                            needsCompleted++;
                        }

                        if (!completedAtLeastOnItem && gameData.tasksData[i].tasks[j].needs[k].completed > 0)
                        {
                            completedAtLeastOnItem = true;
                        }
                    }

                    // Set needs completed count
                    gameData.tasksData[i].tasks[j].completed = needsCompleted;
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

            for (int i = 0; i < gameData.tasksData.Count; i++)
            {
                if (gameData.tasksData[i].id == groupId)
                {
                    for (int j = 0; j < gameData.tasksData[i].tasks.Count; j++)
                    {
                        if (gameData.tasksData[i].tasks[j].id == taskId)
                        {

                            taskRef = worldDataManager.GetWorldItem(groupId, gameData.tasksData[i].tasks[j]);

                            break;
                        }
                    }

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

        void RemoveNeedsFromBoardAndInventory(string groupId, string taskId)
        {
            List<Types.TaskItem> needs = new();

            // Get needs
            for (int i = 0; i < gameData.tasksData.Count; i++)
            {
                if (gameData.tasksData[i].id == groupId)
                {
                    for (int j = 0; j < gameData.tasksData[i].tasks.Count; j++)
                    {
                        if (gameData.tasksData[i].tasks[j].id == taskId)
                        {
                            for (int k = 0; k < gameData.tasksData[i].tasks[j].needs.Length; k++)
                            {
                                for (int l = 0; l < gameData.tasksData[i].tasks[j].needs[k].amount; l++)
                                {
                                    needs.Add(gameData.tasksData[i].tasks[j].needs[k]);
                                }
                            }

                            break;
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
