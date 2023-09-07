using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class HubGameUI : MonoBehaviour
    {
        // Variables
        public Sprite taskIconSprite;
        public ProgressManager progressManager;

        private List<TaskIconData> taskIconsData = new();

        // References
        private Camera cam;

        // UI
        private VisualElement root;
        private VisualElement taskIconsContainer;

        private class TaskIconData
        {
            public Button taskIconButton;
            public GameObject gameObject;
        };

        void Start()
        {
            // Cache
            cam = Camera.main;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            taskIconsContainer = root.Q<VisualElement>("TaskIconsContainer");

            //progressManager.CheckInitialData();
        }

        void Update()
        {
            UpdateTaskIconsPos();
        }

        public void ClearTaskIcons()
        {
            taskIconsContainer.Clear();
        }

        public void AddTaskIcon(GameObject newGameObject, Types.Task taskData)
        {
            Button newTaskIconButton = new();

            newTaskIconButton.AddToClassList("task_icon_button");
            newTaskIconButton.AddToClassList("button_active");

            newTaskIconButton.clicked += () => TaskIconClicked(taskData.id);

            taskIconsContainer.Add(newTaskIconButton);
            taskIconsData.Add(
                new() { taskIconButton = newTaskIconButton, gameObject = newGameObject }
            );

            StartCoroutine(ShowTaskIconButton(newTaskIconButton));
        }

        IEnumerator ShowTaskIconButton(Button newTaskIconButton)
        {
            yield return new WaitForSeconds(0.1f);

            newTaskIconButton.style.opacity = 1;
            newTaskIconButton.style.visibility = Visibility.Visible;
        }

        void TaskIconClicked(string id)
        {
            Debug.Log(id);
        }

        void UpdateTaskIconsPos()
        {
            if (taskIconsData.Count > 0)
            {
                Vector2 newUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
                    root.panel,
                    taskIconsData[0].gameObject.transform.position,
                    cam
                );

                for (int i = 0; i < taskIconsData.Count; i++)
                {
                    Button taskIconButton = taskIconsData[i].taskIconButton;

                    taskIconButton.style.top =
                        newUIPos.y - (taskIconButton.resolvedStyle.height / 2);
                    taskIconButton.style.left =
                        newUIPos.x - (taskIconButton.resolvedStyle.width / 2);
                }
            }
        }
    }
}
