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
        public PointerHandler pointerHandler;

        [HideInInspector]
        public bool loadingTaskMenuButton = true;
        [HideInInspector]
        public Vector2 tempTaskButtonPos;

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
        private AddressableManager addressableManager;

        // UI
        private VisualElement root;
        private VisualElement taskMenu;
        private ScrollView taskScrollView;

        private VisualTreeAsset taskGroupPrefab;
        private VisualTreeAsset taskPrefab;
        private VisualTreeAsset taskNeedPrefab;

        async void Start()
        {
            // Cache
            menuUI = GetComponent<MenuUI>();
            infoMenu = GetComponent<InfoMenu>();
            itemHandler = DataManager.Instance.GetComponent<ItemHandler>();
            LOCALE = I18n.Instance;
            gameData = GameData.Instance;
            addressableManager = DataManager.Instance.GetComponent<AddressableManager>();

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            taskMenu = root.Q<VisualElement>("TaskMenu");

            taskScrollView = root.Q<ScrollView>("TaskScrollView");

            taskGroupPrefab = await addressableManager.LoadAssetAsync<VisualTreeAsset>("Assets/Addressables/Uxml/TaskGroup.uxml");
            taskPrefab = await addressableManager.LoadAssetAsync<VisualTreeAsset>("Assets/Addressables/Uxml/Task.uxml");
            taskNeedPrefab = await addressableManager.LoadAssetAsync<VisualTreeAsset>("Assets/Addressables/Uxml/TaskNeed.uxml");

            Init();
        }

        void Init()
        {
            // Make sure the menu is closed
            taskMenu.style.display = DisplayStyle.None;
            taskMenu.style.opacity = 0;
        }

        public void Open(Action<Vector2> callback = null)
        {
            if (menuUI.IsMenuOpen(taskMenu.name))
            {
                return;
            }

            // Set the title
            string title = LOCALE.Get("task_menu_title");

            loadingTaskMenuButton = true;
            tempTaskButtonPos = Vector2.zero;

            SetTasks();

            // Open menu
            menuUI.OpenMenu(taskMenu, title);
        }

        // Show task data
        void SetTasks()
        {
            // Clear past data
            taskScrollView.Clear();

            if (gameData.tasksData.Count > 0)
            {
                // Task groups
                for (int i = 0; i < gameData.tasksData.Count; i++)
                {
                    var newTaskGroup = taskGroupPrefab.CloneTree();

                    newTaskGroup.name = gameData.tasksData[i].id;

                    newTaskGroup.Q<Label>("GroupTitle").text = LOCALE.Get("task_group_" + gameData.tasksData[i].id);

                    newTaskGroup.Q<VisualElement>("Image").style.backgroundImage = new StyleBackground(
                        gameData.GetTaskSprite("TaskGroup" + gameData.tasksData[i].id)
                    );

                    newTaskGroup.Q<Label>("Desc").text = LOCALE.Get(
                        "task_group_" + gameData.tasksData[i].id + "_desc"
                    );

                    int percentComplete = 0;

                    if (gameData.tasksData[i].total > 0)
                    {
                        percentComplete = Mathf.CeilToInt((100f / gameData.tasksData[i].total) * gameData.tasksData[i].completed);
                    }

                    newTaskGroup.Q<VisualElement>("Fill").style.width = percentComplete;
                    newTaskGroup.Q<Label>("FillLabel").text = percentComplete + "%";

                    // Tasks
                    for (int j = 0; j < gameData.tasksData[i].tasks.Count; j++)
                    {
                        var newTask = taskPrefab.CloneTree();

                        newTask.Q<Label>("TaskTitle").text = LOCALE.Get(
                            "task_"
                                + gameData.tasksData[i].id
                                + "_"
                                + gameData.tasksData[i].tasks[j].id
                        );

                        Button playButton = newTask.Q<Button>("PlayButton");

                        string groupId = gameData.tasksData[i].id;
                        string taskId = gameData.tasksData[i].tasks[j].id;

                        // Get the first task group's first task's play button position
                        if (i == 0 && j == 0)
                        {
                            newTask.Q<Button>("PlayButton").RegisterCallback<GeometryChangedEvent>(SetTaskButtonPos);
                        }

                        // Set button
                        if (taskId == "Last")
                        {
                            newTask.Q<VisualElement>("TaskNeeds").style.display = DisplayStyle.None;

                            playButton.clicked += () =>
                            {
                                if (pointerHandler != null)
                                {
                                    pointerHandler.ButtonPress(Types.Button.TaskMenu, true, () =>
                                    {
                                        HandleCompletedTap(groupId, taskId);
                                    });
                                }
                                else
                                {
                                    HandleCompletedTap(groupId, taskId);
                                }
                            };

                            playButton.text = LOCALE.Get("task_button_finish");

                            playButton.AddToClassList("task_button_last");

                            playButton.style.unityBackgroundImageTintColor = Glob.colorGreen;

                        }
                        else if (gameData.tasksData[i].tasks[j].needs.Length == gameData.tasksData[i].tasks[j].completed)
                        {
                            playButton.clicked += () =>
                            {
                                if (pointerHandler != null)
                                {
                                    pointerHandler.ButtonPress(Types.Button.TaskMenu, true, () =>
                                    {
                                        HandleCompletedTap(groupId, taskId);
                                    });
                                }
                                else
                                {
                                    HandleCompletedTap(groupId, taskId);
                                }
                            };

                            playButton.text = LOCALE.Get("task_button_complete");

                            playButton.style.unityBackgroundImageTintColor = Glob.colorGreen;
                        }
                        else
                        {
                            playButton.text = LOCALE.Get("task_button_play");

                            playButton.style.unityBackgroundImageTintColor = Glob.colorBlue;

                            if (sceneLoader.GetScene() == Types.Scene.Hub)
                            {
                                playButton.clicked += () =>
                                {
                                    if (pointerHandler != null)
                                    {
                                        pointerHandler.ButtonPress(Types.Button.TaskMenu, true, () =>
                                        {
                                            sceneLoader.Load(Types.Scene.GamePlay);
                                        });
                                    }
                                    else
                                    {
                                        sceneLoader.Load(Types.Scene.GamePlay);
                                    }
                                };
                            }
                            else
                            {
                                playButton.style.opacity = 0.3f;
                                playButton.pickingMode = PickingMode.Ignore;
                            }
                        }

                        // Task needs
                        for (int k = 0; k < gameData.tasksData[i].tasks[j].needs.Length; k++)
                        {
                            var newTaskNeed = taskNeedPrefab.CloneTree();

                            string amount = gameData.tasksData[i].tasks[j].needs[k].completed + "/" + gameData.tasksData[i].tasks[j].needs[k].amount;

                            if (gameData.tasksData[i].tasks[j].needs[k].completed == gameData.tasksData[i].tasks[j].needs[k].amount)
                            {
                                VisualElement check = new() { name = "Check" };

                                check.AddToClassList("check");

                                newTaskNeed.Q<VisualElement>("Image").Add(check);
                            }

                            newTaskNeed.Q<Label>("Count").text = amount;

                            newTaskNeed.Q<VisualElement>("Image").style.backgroundImage =
                                new StyleBackground(gameData.tasksData[i].tasks[j].needs[k].sprite);

                            Types.ShopItemsContent taskNeedItem =
                                new()
                                {
                                    type = gameData.tasksData[i].tasks[j].needs[k].type,
                                    group = gameData.tasksData[i].tasks[j].needs[k].group,
                                    genGroup = gameData.tasksData[i].tasks[j].needs[k].genGroup,
                                    chestGroup = gameData.tasksData[i].tasks[j].needs[k].chestGroup,
                                    sprite = gameData.tasksData[i].tasks[j].needs[k].sprite,
                                };

                            newTaskNeed.Q<Button>("InfoButton").clicked += () =>
                                infoMenu.Open(itemHandler.CreateItemTemp(taskNeedItem));

                            newTask.Q<VisualElement>("TaskNeeds").Add(newTaskNeed);
                        }

                        // Add task to task group
                        newTaskGroup.Q<VisualElement>("TasksContainer").Add(newTask);
                    }

                    // Add task group to the menu
                    taskScrollView.Add(newTaskGroup);
                }
            }
            else
            {
                // There are no tasks
                Label emptyTasksLabel = new() { text = LOCALE.Get("task_empty") };

                emptyTasksLabel.AddToClassList("menu_label");

                taskScrollView.Add(emptyTasksLabel);
            }
        }

        // Check if we are on the hub scene and complete the task,
        // but if we are on the game play scene,
        // then save the task group id and task id to a static variable
        // so it can be used in the hub scene
        void HandleCompletedTap(string groupId, string taskId)
        {
            if (sceneLoader.GetScene() == Types.Scene.Hub)
            {
                taskManager.TryToCompleteTask(groupId, taskId, () =>
                {
                    StartCoroutine(WaitForTaskChangingToFinish());
                });

                menuUI.CloseMenu(taskMenu.name);
            }
            else
            {
                Glob.taskToComplete = groupId + "|" + taskId;

                sceneLoader.Load(Types.Scene.Hub);
            }
        }

        IEnumerator WaitForTaskChangingToFinish()
        {
            while (Glob.selectableIsChanging)
            {
                yield return null;
            }

            pointerHandler.ButtonPressFinish();
        }

        void SetTaskButtonPos(GeometryChangedEvent evt)
        {
            root.UnregisterCallback<GeometryChangedEvent>(SetTaskButtonPos);

            foreach (var taskGroup in taskScrollView.Children())
            {
                tempTaskButtonPos = taskGroup.Q<Button>("PlayButton").worldBound.center;
            }

            loadingTaskMenuButton = false;
        }
    }
}
