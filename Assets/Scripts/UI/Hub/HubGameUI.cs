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

        // UI
        private VisualElement root;
        private VisualElement taskIconsContainer;
        private List<TaskIconData> taskIconsData = new();

        private class TaskIconData
        {
            public Button taskIconButton;
            public GameObject gameObject;
        };

        void Start()
        {
            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            taskIconsContainer = root.Q<VisualElement>("TaskIconsContainer");
        }

        void Update()
        {
            UpdatePos();
        }

        public void ClearTaskIcons()
        {
            taskIconsContainer.Clear();
        }

        public void AddTaskIcon(GameObject newGameObject)
        {
            Debug.Log(newGameObject);

            Vector2 newUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
                root.panel,
                gameObject.transform.position,
                Camera.main
            );

            Debug.Log(newUIPos);

            Button newTaskIconButton = new();

            newTaskIconButton.style.top = gameObject.transform.position.y;
            newTaskIconButton.style.left = gameObject.transform.position.x;

            newTaskIconButton.AddToClassList("task_icon_button");

            taskIconsContainer.Add(newTaskIconButton);
            taskIconsData.Add(new()
            {
                taskIconButton = newTaskIconButton,
                gameObject = newGameObject
            });

            StartCoroutine(ShowTaskIconButton(newTaskIconButton));
        }

        IEnumerator ShowTaskIconButton(Button newTaskIconButton)
        {
            yield return new WaitForSeconds(0.1f);

            newTaskIconButton.style.opacity = 1;
            newTaskIconButton.style.visibility = Visibility.Visible;
        }

        void UpdatePos()
        {
            if (taskIconsData.Count > 0)
            {
                Debug.Log(taskIconsData[0].gameObject);
                for (int i = 0; i < taskIconsData.Count; i++)
                {
                    taskIconsData[i].taskIconButton.style.top = taskIconsData[i].gameObject.transform.position.y;
                    taskIconsData[i].taskIconButton.style.left = taskIconsData[i].gameObject.transform.position.x;
                }
            }
        }
    }
}