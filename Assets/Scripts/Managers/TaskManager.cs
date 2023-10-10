using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// TODO - Every time we want to get the task or task group we loop through it,
// TODO - It would be more performant to save them in a variable and use them when needed

namespace Merge
{
    public class TaskManager : MonoBehaviour
    {
        // Variables
        public TasksData tasksData;
        public WorldDataManager worldDataManager;
        public BoardManager boardManager;
        public ValuePop valuePop;

        [HideInInspector]
        public bool isLoaded = false;

        // References
        private GameData gameData;
        private DataManager dataManager;
        private Selector selector;
        private ProgressManager progressManager;
        private CameraMotion cameraMotion;
        private NoteDotHandler noteDotHandler;
        private HubUI hubUI;
        private GameplayUI gameplayUI;
        private UIButtons uiButtons;

        void Start()
        {
            // Cache
            gameData = GameData.Instance;
            dataManager = DataManager.Instance;
            progressManager = GetComponent<ProgressManager>();
            cameraMotion = Camera.main.GetComponent<CameraMotion>();
            noteDotHandler = GameRefs.Instance.noteDotHandler;
            hubUI = GameRefs.Instance.hubUI;
            gameplayUI = GameRefs.Instance.gameplayUI;
            uiButtons = gameData.GetComponent<UIButtons>();

            // The world data manager is only attached in the hub scene
            if (worldDataManager != null)
            {
                selector = worldDataManager.GetComponent<Selector>();
            }

            isLoaded = true;

            // Subscribe to events
            DataManager.BoardSaveEventAction += CheckBoardAndInventoryForTasks;
        }

        void OnDestroy()
        {
            // Unsubscribe from events
            DataManager.BoardSaveEventAction -= CheckBoardAndInventoryForTasks;
        }

        //// Tasks ////

        // Add a new task to the task group
        // Or add a new task group if it doesn't exists, then add the task to it
        void AddTask(string groupId, string taskId)
        {
            Types.Task newTask = null;

            // Get task data with the given id
            for (int i = 0; i < tasksData.taskGroups.Length; i++)
            {
                if (tasksData.taskGroups[i].id == groupId)
                {
                    newTask = tasksData.taskGroups[i].tasks.FirstOrDefault(i => i.id == taskId);

                    break;
                }
            }

            if (newTask != null)
            {
                bool addNewGroup = true;

                // Check if task group exists and add the new task to it
                for (int i = 0; i < gameData.tasksData.Count; i++)
                {
                    if (gameData.tasksData[i].id == groupId)
                    {
                        // Add the task to the task group
                        gameData.tasksData[i].tasks.Add(newTask);

                        // Task group does exists
                        addNewGroup = false;

                        break;
                    }
                }

                // If the task group doesn't exists, add a new task group and add the new task to it
                if (addNewGroup)
                {
                    // Initialize the new task group
                    Types.TaskGroup newTaskGroup = new()
                    {
                        id = groupId,
                        tasks = new List<Types.Task>(),
                        total = CountTaskGroupTotal(groupId)
                    };

                    // Add the task to the task group
                    newTaskGroup.tasks.Add(newTask);

                    // Add the task group to the task data
                    gameData.tasksData.Add(newTaskGroup);
                }

                // Save data to disk
                dataManager.SaveTasks();

                CheckBoardAndInventoryForTasks();
            }
            else
            {
                Debug.LogWarning("Task not found!");
            }
        }

        // Remove task from the task group
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

            AddTaskIdToFinishedTasks(groupId, taskId);

            // Save changes to disk
            dataManager.SaveTasks();
            dataManager.SaveFinishedTasks();
        }

        // Remove task group from the task data
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

        // Count the total amount of tasks in the given task group
        int CountTaskGroupTotal(string groupId)
        {
            int count = 0;

            for (int i = 0; i < tasksData.taskGroups.Length; i++)
            {
                if (tasksData.taskGroups[i].id == groupId)
                {
                    // We don't need the last task
                    count = tasksData.taskGroups[i].tasks.Count - 1;
                }
            }

            return count;
        }

        // Try to add a new task if it doesn't already exist
        public void TryToAddTask(string groupId, string taskId)
        {
            bool addNewGroup = true;

            for (int i = 0; i < gameData.tasksData.Count; i++)
            {
                if (gameData.tasksData[i].id == groupId)
                {
                    if (!gameData.tasksData[i].tasks.Exists(i => i.id == taskId) && !gameData.finishedTasks.Exists(i => i.groupId == groupId && i.taskId == taskId))
                    {
                        AddTask(groupId, taskId);
                    }

                    addNewGroup = false;

                    break;
                }
            }

            if (addNewGroup)
            {
                AddTask(groupId, taskId);
            }
        }

        void AddTaskIdToFinishedTasks(string groupId, string taskId)
        {
            if (!gameData.finishedTasks.Exists(i => i.groupId == groupId && i.taskId == taskId))
            {
                gameData.finishedTasks.Add(new Types.FinishedTask() { groupId = groupId, taskId = taskId, });
            }
        }

        //// Completion ////

        // Trying to complete the task
        public void TryToCompleteTask(string groupId, string taskId)
        {
            Transform taskRef = null;
            Vector2 taskRefPos = Vector2.zero;
            bool isLastStep = false;
            Types.TaskItem[] rewards = null;

            // Get the task item form the world
            for (int i = 0; i < gameData.tasksData.Count; i++)
            {
                if (gameData.tasksData[i].id == groupId)
                {
                    for (int j = 0; j < gameData.tasksData[i].tasks.Count; j++)
                    {
                        if (gameData.tasksData[i].tasks[j].id == taskId)
                        {
                            rewards = gameData.tasksData[i].tasks[j].rewards;

                            taskRef = worldDataManager.GetWorldItem(groupId, gameData.tasksData[i].tasks[j]);

                            taskRefPos = worldDataManager.GetColliderCenter(taskRef, gameData.tasksData[i].tasks[j].taskRefType);

                            isLastStep = gameData.tasksData[i].tasks[j].id == "Last";

                            break;
                        }
                    }

                    break;
                }
            }

            // Move the camera to the item we want to change
            cameraMotion.MoveTo(taskRefPos, 250);

            if (isLastStep)
            {
                TaskCompleted(groupId, taskId, true);
            }
            else
            {
                // Select the item
                selector.SelectAlt(taskRef.GetComponent<Selectable>(), () =>
                {
                    // If successfully changed, give the rewards and complete the task 

                    Glob.SetTimeout(() =>
                    {
                        HandleRewards(rewards);
                    }, 0.35f);

                    TaskCompleted(groupId, taskId);
                });
            }
        }

        // Complete the task
        void TaskCompleted(string groupId, string taskId, bool isLastStep = false)
        {
            // Increase the number of tasks completed in the task group
            for (int i = 0; i < gameData.tasksData.Count; i++)
            {
                if (gameData.tasksData[i].id == groupId)
                {
                    gameData.tasksData[i].completed += 1;

                    break;
                }
            }

            if (isLastStep)
            {
                progressManager.CheckNextArea(groupId);

                RemoveTaskGroup(groupId);

                CheckBoardAndInventoryForTasks();
            }
            else
            {
                RemoveNeedsFromBoardAndInventory(groupId, taskId, () =>
                {
                    RemoveTask(groupId, taskId);

                    // Save data to disk
                    dataManager.SaveBoard();
                    dataManager.SaveInventory();

                    // Check the next ids
                    progressManager.CheckNextIds(groupId, taskId);
                });
            }
        }

        // Remove the task needs from the board and the inventory after completing the task
        void RemoveNeedsFromBoardAndInventory(string groupId, string taskId, Action callback)
        {
            List<Types.TaskItem> needs = new();

            // Get all the needs from the task
            // If the need amount is more than 0, then add the needs multiple times
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

                            // Create a new board data using the old older
                            gameData.boardData[x, y] = new Types.Board { order = oldOrder };

                            // Nullify the needs
                            needs[i] = new Types.TaskItem();
                        }
                    }
                }
            }

            // Remove needs from the inventory, if there are any left
            for (int i = 0; i < needs.Count; i++)
            {
                // Check if the needs isn't null
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

            callback();
        }

        //// Rewards ////

        // Get the rewards and handle them as needed
        void HandleRewards(Types.TaskItem[] rewards)
        {
            for (int k = 0; k < rewards.Length; k++)
            {
                if (rewards[k].type == Types.Type.Coll)
                {
                    valuePop.PopValue(rewards[k].amount, rewards[k].collGroup, uiButtons.hubTaskButtonPos, false);
                }
                else
                {
                    AddItemToBonus(rewards[k]);
                }
            }
        }

        // Handle Items, Generators and Chests
        IEnumerator AddItemToBonus(Types.TaskItem reward)
        {
            yield return new WaitForSeconds(0.5f);

            Item newItem = new()
            {
                sprite = reward.sprite,
                type = reward.type,
                group = reward.group,
                genGroup = reward.genGroup,
                chestGroup = reward.chestGroup
            };

            Vector2 initialPosition;
            Vector2 buttonPosition;

            if (noteDotHandler.isHub)
            {
                initialPosition = uiButtons.hubTaskButtonPos;
                buttonPosition = uiButtons.hubPlayButtonPos;
            }
            else
            {
                initialPosition = uiButtons.gameplayTaskButtonPos;
                buttonPosition = uiButtons.gameplayBonusButtonPos;
            }

            valuePop.PopBonus(newItem, initialPosition, buttonPosition, true);
        }

        //// Checks ////

        // Check if any task is ready to be completed and enable the task button note dot
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

            if (noteDotHandler != null)
            {
                if (noteDotHandler.isHub && completedCount == noteDotHandler.taskNoteDotAmount)
                {
                    return;
                }

                noteDotHandler.ToggleButtonNoteDot("task", completedCount > 0, completedCount, completedCount > noteDotHandler.taskNoteDotAmount);
            }
        }

        // Check if any task is ready to be completed
        // This function runs every time the board data is saved
        public void CheckBoardAndInventoryForTasks()
        {
            // Clear board items from being completed
            for (int x = 0; x < gameData.boardData.GetLength(0); x++)
            {
                for (int y = 0; y < gameData.boardData.GetLength(1); y++)
                {
                    if (gameData.boardData[x, y].isCompleted)
                    {
                        gameData.boardData[x, y].isCompleted = false;
                    }
                }
            }

            // Clear inventory items from being completed
            for (int i = 0; i < gameData.inventoryData.Count; i++)
            {
                if (gameData.inventoryData[i].isCompleted)
                {
                    gameData.inventoryData[i].isCompleted = false;
                }
            }

            bool completedAtLeastOnItem = false;

            for (int i = 0; i < gameData.tasksData.Count; i++)
            {
                for (int j = 0; j < gameData.tasksData[i].tasks.Count; j++)
                {
                    int needsCompleted = 0;

                    for (int k = 0; k < gameData.tasksData[i].tasks[j].needs.Length; k++)
                    {
                        // Check if need is completed
                        gameData.tasksData[i].tasks[j].needs[k].completed = CheckNeedCompleted(gameData.tasksData[i].tasks[j].needs[k]);

                        if (gameData.tasksData[i].tasks[j].needs[k].amount == gameData.tasksData[i].tasks[j].needs[k].completed)
                        {
                            needsCompleted++;
                        }

                        // Check if there is at least one item completed
                        if (!completedAtLeastOnItem && gameData.tasksData[i].tasks[j].needs[k].completed > 0)
                        {
                            completedAtLeastOnItem = true;
                        }
                    }

                    // Set the task's needs completed count
                    gameData.tasksData[i].tasks[j].completed = needsCompleted;
                }
            }

            // Save data to disk
            dataManager.SaveBoard(false);
            dataManager.SaveInventory();
            dataManager.SaveTasks();

            // Set board items to completed in the gameplay scene
            if (completedAtLeastOnItem && boardManager != null)
            {
                boardManager.SetCompletedItems();
            }

            CheckTaskNoteDot();
        }

        // Check if a task need exists on the board or in the inventory and get its amount
        int CheckNeedCompleted(Types.TaskItem taskNeed)
        {
            int amount = 0;

            // Count ready items on the board
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
                        // If found, set the board item to completed
                        gameData.boardData[x, y].isCompleted = true;

                        amount++;
                    }
                }
            }

            // Count ready items in the inventory
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
                    // If found, set the inventory item to completed
                    gameData.inventoryData[i].isCompleted = true;

                    amount++;
                }
            }

            // We only need the max amount of ready items, so make sure the amount doesn't surpass the needs count
            if (amount > taskNeed.amount)
            {
                amount = taskNeed.amount;
            }

            return amount;
        }

        // After tapping the complete button on the task in the gameplay scene, 
        // send the data to the hub scene and try to complete the scene here
        public void CheckIfThereIsATaskToComplete(Action callback = null)
        {
            if (Glob.taskToComplete != "")
            {
                string[] splitTaskData = Glob.taskToComplete.Split("|");

                TryToCompleteTask(splitTaskData[0], splitTaskData[1]);

                Glob.taskToComplete = "";
            }

            if (callback != null)
            {
                callback();
            }
        }
    }
}
