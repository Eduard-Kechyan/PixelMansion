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
        public TutorialManager tutorialManager;

        [HideInInspector]
        public bool isLoaded = false;

        // References
        private GameData gameData;
        private DataManager dataManager;
        private Selector selector;
        private ProgressManager progressManager;
        private RemoveFilth removeFilth;
        private CameraMotion cameraMotion;
        private CameraPinch cameraPinch;
        private NoteDotHandler noteDotHandler;
        private UIButtons uiButtons;

        void Start()
        {
            // Cache
            gameData = GameData.Instance;
            dataManager = DataManager.Instance;
            progressManager = GetComponent<ProgressManager>();
            removeFilth = GetComponent<RemoveFilth>();
            cameraMotion = Camera.main.GetComponent<CameraMotion>();
            cameraPinch = Camera.main.GetComponent<CameraPinch>();
            noteDotHandler = GameRefs.Instance.noteDotHandler;
            uiButtons = gameData.GetComponent<UIButtons>();

            // The world data manager is only attached in the world scene
            if (worldDataManager != null)
            {
                selector = worldDataManager.GetComponent<Selector>();
            }

            isLoaded = true;
        }

        void OnEnable()
        {
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
        public void TryToCompleteTask(string groupId, string taskId, Action callback = null)
        {
            Transform taskRef = null;
            Vector2 taskRefPos = Vector2.zero;
            Types.TaskRefType taskRefType = Types.TaskRefType.Area;
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

                            taskRefType = gameData.tasksData[i].tasks[j].taskRefType;

                            taskRefPos = worldDataManager.GetColliderCenter(taskRef, taskRefType);

                            isLastStep = gameData.tasksData[i].tasks[j].id == "Last";

                            break;
                        }
                    }

                    break;
                }
            }

            // Move the camera to the item we want to change
            if (taskRefType == Types.TaskRefType.Filth)
            {
                cameraMotion.MoveTo(taskRefPos, 250, () =>
                {
                    removeFilth.Remove(taskRef, () =>
                    {
                        // If successfully changed, give the rewards and complete the task

                        worldDataManager.SetFilth(taskRef);

                        callback?.Invoke();

                        Glob.SetTimeout(() =>
                        {
                            HandleRewards(rewards, groupId, taskId);
                        }, 0.35f);

                        TaskCompleted(groupId, taskId);
                    });
                });
                cameraPinch.isResetting = true;
            }
            else
            {
                cameraMotion.MoveTo(taskRefPos, 250);
                cameraPinch.isResetting = true;

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

                        callback?.Invoke();

                        Glob.SetTimeout(() =>
                        {
                            HandleRewards(rewards, groupId, taskId);
                        }, 0.35f);

                        TaskCompleted(groupId, taskId);
                    });
                }
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
                            gameData.boardData[x, y] = new Types.Tile { order = oldOrder };

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
        void HandleRewards(Types.TaskItem[] rewards, string groupId, string taskId)
        {
            bool nextIsConvo = progressManager.CheckIfNextIsConvo(groupId, taskId);

            for (int k = 0; k < rewards.Length; k++)
            {
                if (rewards[k].type == Types.Type.Coll)
                {
                    if (nextIsConvo)
                    {
                        gameData.UpdateValue(rewards[k].amount, rewards[k].collGroup, false, true);
                    }
                    else
                    {
                        valuePop.PopValue(rewards[k].amount, rewards[k].collGroup, uiButtons.worldTaskButtonPos, false);
                    }
                }
                else
                {
                    AddItemToBonus(rewards[k], nextIsConvo);
                }
            }
        }

        // Handle Items, Generators and Chests
        IEnumerator AddItemToBonus(Types.TaskItem reward, bool nextIsConvo = false)
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

            if (nextIsConvo)
            {
                gameData.AddToBonus(newItem);
            }
            else
            {
                Vector2 initialPosition;
                Vector2 buttonPosition;

                if (noteDotHandler.isWorld)
                {
                    initialPosition = uiButtons.worldTaskButtonPos;
                    buttonPosition = uiButtons.worldPlayButtonPos;
                }
                else
                {
                    initialPosition = uiButtons.mergeTaskButtonPos;
                    buttonPosition = uiButtons.mergeBonusButtonPos;
                }

                valuePop.PopBonus(newItem, initialPosition, buttonPosition);
            }
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
                if (noteDotHandler.isWorld && completedCount == noteDotHandler.taskNoteDotAmount)
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
            dataManager.SaveBoard(false, false);
            dataManager.SaveInventory();
            dataManager.SaveTasks();

            // Set board items to completed in the merge scene
            if (completedAtLeastOnItem && boardManager != null)
            {
                if (Application.isEditor)
                {
                    StopCoroutine(WaitForBoardInitialization());

                    StartCoroutine(WaitForBoardInitialization());
                }
                else
                {
                    boardManager.SetCompletedItems();
                }
            }

            CheckTaskNoteDot();
        }

        IEnumerator WaitForBoardInitialization()
        {
            while (!boardManager.boardSet)
            {
                yield return null;
            }

            boardManager.SetCompletedItems();
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

        // After tapping the complete button on the task in the merge scene, 
        // send the data to the world scene and try to complete the scene here
        public void CheckIfThereIsATaskToComplete(Action callback = null, bool isFromMerge = false)
        {
            if (PlayerPrefs.HasKey("taskToComplete"))
            {
                string[] splitTaskData = PlayerPrefs.GetString("taskToComplete").Split("|");

                TryToCompleteTask(splitTaskData[0], splitTaskData[1]);

                PlayerPrefs.DeleteKey("taskToComplete");

                PlayerPrefs.Save();
            }

            if (PlayerPrefs.HasKey("waitForTaskChange") && tutorialManager != null)
            {
                if (isFromMerge)
                {
                    tutorialManager.NextStep();
                }
                else
                {
                    StartCoroutine(WaitForTaskChangingToFinish());
                }
            }

            callback?.Invoke();
        }

        IEnumerator WaitForTaskChangingToFinish()
        {
            yield return new WaitForSeconds(0.3f);

            while (Glob.taskLoading)
            {
                yield return null;
            }

            PlayerPrefs.DeleteKey("waitForTaskChange");

            PlayerPrefs.Save();

            tutorialManager.NextStep();
        }
    }
}
