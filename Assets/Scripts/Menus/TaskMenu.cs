using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class TaskMenu : MonoBehaviour
    {
        public SceneLoader sceneLoader;
        public TaskManager taskManager;

        private TemplateContainer taskGroupPrefab;
        private TemplateContainer taskPrefab;
        private TemplateContainer taskNeedPrefab;

        [Serializable]
        private class CompletedNeed
        {
            public string sprite;
            public int amount;
        }

        // References
        private MenuUI menuUI;
        private InfoMenu infoMenu;
        private I18n LOCALE;
        private GameData gameData;
        private ItemHandler itemHandler;

        // UI
        private VisualElement root;
        private VisualElement taskMenu;
        private ScrollView taskScrollView;

        void Start()
        {
            // Cache
            menuUI = GetComponent<MenuUI>();
            infoMenu = GetComponent<InfoMenu>();
            itemHandler = DataManager.Instance.GetComponent<ItemHandler>();
            LOCALE = I18n.Instance;
            gameData = GameData.Instance;

            // Cache UI
            root = GetComponent<UIDocument>().rootVisualElement;

            taskMenu = root.Q<VisualElement>("TaskMenu");

            taskScrollView = root.Q<ScrollView>("TaskScrollView");

            taskGroupPrefab = Resources.Load<VisualTreeAsset>("Uxml/TaskGroup").CloneTree();
            taskPrefab = Resources.Load<VisualTreeAsset>("Uxml/Task").CloneTree();
            taskNeedPrefab = Resources.Load<VisualTreeAsset>("Uxml/TaskNeed").CloneTree();

            Init();
        }

        void Init()
        {
            // Make sure the menu is closed
            taskMenu.style.display = DisplayStyle.None;
            taskMenu.style.opacity = 0;
        }

        public void Open()
        {
            // Set the title
            string title = LOCALE.Get("task_menu_title");

            SetTasks();

            // Open menu
            menuUI.OpenMenu(taskMenu, title);
        }

        void SetTasks()
        {
            taskScrollView.Clear();

            if (gameData.taskGroupsData.Count > 0 && gameData.tasksData.Count > 0)
            {
                for (int i = 0; i < gameData.taskGroupsData.Count; i++)
                {
                    var newTaskGroup = taskGroupPrefab;

                    newTaskGroup.name = "TaskGroup" + i;

                    newTaskGroup.Q<Label>("GroupTitle").text = LOCALE.Get("task_group_" + gameData.taskGroupsData[i].id);

                    newTaskGroup.Q<VisualElement>("Image").style.backgroundImage = new StyleBackground(
                        gameData.GetTaskSprite("TaskGroup" + gameData.taskGroupsData[i].id)
                    );

                    newTaskGroup.Q<Label>("Desc").text = LOCALE.Get(
                        "task_group_" + gameData.taskGroupsData[i].id + "_desc"
                    );

                    int percentComplete = Mathf.RoundToInt((100 / gameData.taskGroupsData[i].total) * gameData.taskGroupsData[i].completed);

                    newTaskGroup.Q<VisualElement>("Fill").style.width = percentComplete;
                    newTaskGroup.Q<Label>("FillLabel").text = percentComplete + "%";

                    for (int j = 0; j < gameData.tasksData.Count; j++)
                    {
                        if (gameData.tasksData[j].groupId == gameData.taskGroupsData[i].id)
                        {
                            var newTask = taskPrefab;

                            newTask.Q<Label>("TaskTitle").text = LOCALE.Get(
                                "task_"
                                    + gameData.taskGroupsData[i].id
                                    + "_"
                                    + gameData.tasksData[j].id
                            );

                            Button playButton = newTask.Q<Button>("PlayButton");

                            string groupId = gameData.tasksData[j].groupId;
                            string taskId = gameData.tasksData[j].id;

                            List<CompletedNeed> completedNeeds = CheckIfTaskIsCompleted(
                                gameData.tasksData[j]
                            );

                            if (completedNeeds.Count == gameData.tasksData[j].needs.Length)
                            {
                                string indexString = i.ToString();

                                playButton.clicked += () => FinishTask(groupId, taskId, indexString);

                                playButton.text = LOCALE.Get("task_button_complete");
                            }
                            else
                            {
                                playButton.clicked += () => sceneLoader.Load(2);

                                playButton.text = LOCALE.Get("task_button_play");
                            }

                            for (int k = 0; k < gameData.tasksData[j].needs.Length; k++)
                            {
                                var newTaskNeed = taskNeedPrefab;

                                string amount = 0 + "/" + gameData.tasksData[j].needs[k].amount;

                                for (int l = 0; l < completedNeeds.Count; l++)
                                {
                                    if (
                                        completedNeeds[l].sprite
                                        == gameData.tasksData[j].needs[k].sprite.name
                                    )
                                    {
                                        if (
                                            completedNeeds[l].amount
                                            >= gameData.tasksData[j].needs[k].amount
                                        )
                                        {
                                            amount =
                                                completedNeeds[l].amount
                                                + "/"
                                                + gameData.tasksData[j].needs[k].amount;

                                            newTaskNeed
                                                .Q<VisualElement>("Image")
                                                .style.backgroundColor = Glob.colorGreen;
                                        }
                                        else
                                        {
                                            amount =
                                                completedNeeds[l].amount
                                                + "/"
                                                + gameData.tasksData[j].needs[k].amount;
                                        }
                                    }
                                }

                                newTaskNeed.Q<Label>("Count").text = amount;

                                newTaskNeed.Q<VisualElement>("Image").style.backgroundImage =
                                    new StyleBackground(gameData.tasksData[j].needs[k].sprite);

                                Types.ShopItemsContent taskNeedItem =
                                    new()
                                    {
                                        type = gameData.tasksData[j].needs[k].type,
                                        group = gameData.tasksData[j].needs[k].group,
                                        genGroup = gameData.tasksData[j].needs[k].genGroup,
                                        chestGroup = gameData.tasksData[j].needs[k].chestGroup,
                                        sprite = gameData.tasksData[j].needs[k].sprite,
                                    };

                                newTaskNeed.Q<Button>("InfoButton").clicked += () =>
                                    infoMenu.Open(itemHandler.CreateItemTemp(taskNeedItem));

                                newTask.Q<VisualElement>("TaskNeeds").Add(newTaskNeed);
                            }

                            newTaskGroup.Q<VisualElement>("TasksContainer").Add(newTask);
                        }
                    }

                    taskScrollView.Add(newTaskGroup);
                }
            }
            else
            {
                Label emptyTasksLabel = new() { text = "No tasks found!" };

                emptyTasksLabel.AddToClassList("menu_label");

                taskScrollView.Add(emptyTasksLabel);
            }
        }

        List<CompletedNeed> CheckIfTaskIsCompleted(Types.Task taskData)
        {
            List<CompletedNeed> completedNeeds = new();

            for (int i = 0; i < taskData.needs.Length; i++)
            {
                int count = 0;

                for (int x = 0; x < gameData.boardData.GetLength(0); x++)
                {
                    for (int y = 0; y < gameData.boardData.GetLength(1); y++)
                    {
                        if (
                            gameData.boardData[x, y].sprite == taskData.needs[i].sprite
                            && gameData.boardData[x, y].type == taskData.needs[i].type
                            && gameData.boardData[x, y].group == taskData.needs[i].group
                            && gameData.boardData[x, y].genGroup == taskData.needs[i].genGroup
                            && gameData.boardData[x, y].collGroup == taskData.needs[i].collGroup
                            && gameData.boardData[x, y].chestGroup == taskData.needs[i].chestGroup
                        )
                        {
                            count++;
                        }
                    }
                }

                if (count > 0)
                {
                    completedNeeds.Add(
                        new CompletedNeed
                        {
                            sprite = taskData.needs[i].sprite.name,
                            amount = taskData.needs[i].amount
                        }
                    );
                }
            }

            return completedNeeds;
        }

        void FinishTask(string groupId, string id, string indexString)
        {
            Types.TaskGroup taskGroupData = taskManager.TaskCompleted(groupId, id);

            VisualElement taskGroup = taskScrollView.Q<VisualElement>(groupId + indexString);

            int percentComplete = Mathf.RoundToInt((100 / taskGroupData.total) * taskGroupData.completed + 1);

            taskGroup.Q<VisualElement>("Fill").style.width = percentComplete;
            taskGroup.Q<Label>("FillLabel").text = percentComplete + "%";

            Debug.Log(id + " - " + indexString);
        }
    }
}
