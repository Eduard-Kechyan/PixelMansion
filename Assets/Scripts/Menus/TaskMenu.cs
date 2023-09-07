using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class TaskMenu : MonoBehaviour
    {
        private TemplateContainer taskGroupPrefab;
        private TemplateContainer taskPrefab;
        private TemplateContainer taskNeedPrefab;

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

            AddTasks();

            // Open menu
            menuUI.OpenMenu(taskMenu, title);
        }

        void AddTasks()
        {
            taskScrollView.Clear();

            if (gameData.taskGroupsData.Count > 0 && gameData.tasksData.Count > 0)
            {
                for (int i = 0; i < gameData.taskGroupsData.Count; i++)
                {
                    var newTaskGroup = taskGroupPrefab;

                    newTaskGroup.Q<Label>("GroupTitle").text = LOCALE.Get(
                        "task_group_" + gameData.taskGroupsData[i].id
                    );

                    newTaskGroup.Q<VisualElement>("Image").style.backgroundImage =
                        new StyleBackground(
                            gameData.GetTaskSprite("TaskGroup" + gameData.taskGroupsData[i].id)
                        );

                    newTaskGroup.Q<Label>("Desc").text = LOCALE.Get(
                        "task_group_" + gameData.taskGroupsData[i].id + "_desc"
                    );

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

                            string taskId = gameData.tasksData[j].id;

                            playButton.clicked += () => ShowTask(taskId);

                            playButton.text = LOCALE.Get("task_button_play");

                            for (int k = 0; k < gameData.tasksData[j].needs.Length; k++)
                            {
                                var newTaskNeed = taskNeedPrefab;

                                newTaskNeed.Q<VisualElement>("Image").style.backgroundImage =
                                    new StyleBackground(gameData.tasksData[j].needs[k].sprite);

                                newTaskNeed.Q<Label>("Count").text =
                                    0 + "/" + gameData.tasksData[j].needs[k].count;

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

        void ShowTask(string id)
        {
            Debug.Log(id);
        }
    }
}
