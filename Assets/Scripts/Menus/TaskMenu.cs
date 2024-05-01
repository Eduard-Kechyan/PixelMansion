using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class TaskMenu : MonoBehaviour
    {
        // Variables
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

        private Types.Menu menuType = Types.Menu.Task;

        // References
        private GameRefs gameRefs;
        private MenuUI menuUI;
        private InfoMenu infoMenu;
        private I18n LOCALE;
        private GameData gameData;
        private UIData uiData;
        private ItemHandler itemHandler;
        private AddressableManager addressableManager;
        private SceneLoader sceneLoader;
        private TaskManager taskManager;
        private PointerHandler pointerHandler;
        private TutorialManager tutorialManager;

        // UI
        private VisualElement root;
        private VisualElement content;
        private VisualElement taskScrollView;

        void Start()
        {
            // Cache
            gameRefs = GameRefs.Instance;
            menuUI = GetComponent<MenuUI>();
            infoMenu = GetComponent<InfoMenu>();
            itemHandler = DataManager.Instance.GetComponent<ItemHandler>();
            LOCALE = I18n.Instance;
            gameData = GameData.Instance;
            uiData = gameData.GetComponent<UIData>();
            addressableManager = DataManager.Instance.GetComponent<AddressableManager>();
            sceneLoader = gameRefs.sceneLoader;
            taskManager = gameRefs.taskManager;
            pointerHandler = gameRefs.pointerHandler;
            tutorialManager = gameRefs.tutorialManager;

            DataManager.Instance.CheckLoaded(() =>
            {
                // UI
                root = GetComponent<UIDocument>().rootVisualElement;
                content = uiData.GetMenuAsset(menuType);

                taskScrollView = content.Q<ScrollView>("TaskScrollView");
            });
        }

        public void Open()
        {
            // Check menu
            if (menuUI.IsMenuOpen(menuType))
            {
                return;
            }

            // Set menu content
            loadingTaskMenuButton = true;
            tempTaskButtonPos = Vector2.zero;

            SetTasks();

            // Open menu
            menuUI.OpenMenu(content, menuType);
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
                    var newTaskGroup = uiData.taskGroupPrefab.CloneTree();

                    newTaskGroup.name = gameData.tasksData[i].id;

                    newTaskGroup.Q<Label>("GroupTitle").text = LOCALE.Get("task_group_" + gameData.tasksData[i].id);

                    newTaskGroup.Q<VisualElement>("Image").style.backgroundImage = new StyleBackground(
                        gameData.GetSprite("TaskGroup" + gameData.tasksData[i].id)
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
                        var newTask = uiData.taskPrefab.CloneTree();

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
                        if (gameData.tasksData[i].tasks[j].taskRefType == Types.TaskRefType.Last || gameData.tasksData[i].tasks[j].taskRefType == Types.TaskRefType.PreMansion)
                        {
                            newTask.Q<VisualElement>("TaskNeeds").style.display = DisplayStyle.None;

                            playButton.clicked += () =>
                            {
                                if (pointerHandler != null)
                                {
                                    pointerHandler.ButtonPress(Types.Button.TaskMenu, true, () =>
                                    {
                                        HandleCompletedTap(groupId, taskId, true);
                                    });
                                }
                                else
                                {
                                    HandleCompletedTap(groupId, taskId);
                                }
                            };

                            if (gameData.tasksData[i].tasks[j].taskRefType == Types.TaskRefType.Last)
                            {
                                playButton.text = LOCALE.Get("task_button_finish");
                            }
                            else
                            {

                                playButton.text = LOCALE.Get("task_button_finish_alt");
                            }

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
                                        HandleCompletedTap(groupId, taskId, true);
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

                            if (sceneLoader.GetScene() == Types.Scene.World)
                            {
                                playButton.clicked += () =>
                                {
                                    if (pointerHandler != null)
                                    {
                                        pointerHandler.ButtonPress(Types.Button.TaskMenu, true, () =>
                                        {
                                            tutorialManager.NextStep(false, () =>
                                            {
                                                sceneLoader.Load(Types.Scene.Merge);
                                            });
                                        });
                                    }
                                    else
                                    {
                                        sceneLoader.Load(Types.Scene.Merge);
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
                            var newTaskNeed = uiData.taskNeedPrefab.CloneTree();

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

        // Check if we are on the world scene and complete the task,
        // but if we are on the merge scene,
        // then save the task group id and task id to a static variable
        // so it can be used in the world scene
        void HandleCompletedTap(string groupId, string taskId, bool wait = false)
        {
            if (sceneLoader.GetScene() == Types.Scene.World)
            {
                taskManager.TryToCompleteTask(groupId, taskId, () =>
                {
                    if (wait)
                    {
                        PlayerPrefs.SetInt("waitForTaskChange", 1);

                        PlayerPrefs.Save();

                        StartCoroutine(WaitForTaskChangingToFinish());
                    }
                });

                menuUI.CloseMenu(menuType);
            }
            else
            {
                PlayerPrefs.SetString("taskToComplete", groupId + "|" + taskId);

                PlayerPrefs.Save();

                if (wait)
                {
                    PlayerPrefs.SetInt("waitForTaskChange", 1);

                    PlayerPrefs.Save();
                }

                sceneLoader.Load(Types.Scene.World);
            }
        }

        IEnumerator WaitForTaskChangingToFinish()
        {
            while (Glob.taskLoading)
            {
                yield return null;
            }

            PlayerPrefs.DeleteKey("waitForTaskChange");

            PlayerPrefs.Save();

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
