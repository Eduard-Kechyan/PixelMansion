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
        public SceneLoader sceneLoader;

        // References
        private GameData gameData;
        private DataManager dataManager;
        private ProgressManager progressManager;
        private HubGameUI hubGameUi;

        private void Start()
        {
            InitializeReferences();
        }

        private void InitializeReferences()
        {
            // Initialize references to other managers and UI elements
            gameData = GameData.Instance;
            dataManager = DataManager.Instance;
            progressManager = GetComponent<ProgressManager>();
            hubGameUi = GameRefs.Instance.hubGameUI;
        }

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
                CheckWorldTaskIcons();
            }
        }

        public void RemoveTask(string taskId)
        {
            // Remove task logic here
        }

        public Types.TaskGroup TaskCompleted(string groupId, string id)
        {
            Types.TaskGroup taskGroupData = new();

            for (int i = 0; i < gameData.taskGroupsData.Count; i++)
            {
                if (gameData.taskGroupsData[i].id == groupId)
                {
                    gameData.taskGroupsData[i].completed++;

                    taskGroupData = gameData.taskGroupsData[i];

                    break;
                }
            }

            for (int i = 0; i < gameData.tasksData.Count; i++)
            {
                if (gameData.tasksData[i].groupId == groupId && gameData.tasksData[i].id == id)
                {
                    gameData.tasksData[i].completed = true;

                    break;
                }
            }

            // Save changes to disk
            dataManager.SaveTasks();

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

        public void CheckWorldTaskIcons()
        {
            if (gameData == null)
            {
                InitializeReferences();
            }

            if (
                sceneLoader.GetSceneName() == "Hub"
                && gameData != null
                && gameData.tasksData.Count > 0
                && worldDataManager != null
            )
            {
                // Clear existing task icons in the UI
                hubGameUi.ClearTaskIcons();

                // Add task icons for tasks in the world
                foreach (var taskData in gameData.tasksData)
                {
                    hubGameUi.AddTaskIcon(worldDataManager.GetWorldItemPos(taskData), taskData);
                }
            }
        }
    }
}
