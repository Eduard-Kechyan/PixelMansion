using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    public class ClockManager : MonoBehaviour
    {
        // Variables
        public BoardManager boardManager;
        public GameObject clockPanel;
        public GameObject clockObject;
        public Vector2 offset = new(48f, -48f);

        [Serializable]
        public class ClockData
        {
            public string id;
            public GameObject clockItem;
            public Image clockAmountImage;
            public DateTime startTime;
            public int seconds;
        }

        private List<ClockData> clocks = new();

        private bool canCheck = false;

        // References
        private GameData gameData;
        private DataManager dataManager;
        private Camera cam;

        void Start()
        {
            // Cache
            gameData = GameData.Instance;
            dataManager = DataManager.Instance;
            cam = Camera.main;
        }

        public void AddClock(Vector2 position, string id, DateTime startTime, int seconds = 0)
        {
            GameObject newClockItem = Instantiate(clockObject, Vector2.zero, Quaternion.identity);

            newClockItem.transform.SetParent(clockPanel.transform, false);

            SetPosition(newClockItem, position);

            clocks.Add(new()
            {
                id = id,
                clockItem = newClockItem,
                clockAmountImage = newClockItem.transform.GetChild(0).GetComponent<Image>(),
                startTime = startTime,
                seconds = seconds
            });

            // canCheck = true;
        }

        public void RemoveClock(string id)
        {
            int index = 0;

            for (int i = 0; i < clocks.Count; i++)
            {
                if (clocks[i].id == id)
                {
                    index = i;

                    clocks[i].clockItem.SetActive(false);

                    break;
                }
            }

            clocks.RemoveAt(index);

            /*   if (clocks.Count == 0)
               {
                   canCheck = false;
               }*/
        }

        public void HideClock(string id)
        {
            for (int i = 0; i < clocks.Count; i++)
            {
                if (clocks[i].id == id)
                {
                    clocks[i].clockItem.SetActive(false);

                    break;
                }
            }
        }

        public void MoveClock(Vector2 newPosition, string id)
        {
            for (int i = 0; i < clocks.Count; i++)
            {
                if (clocks[i].id == id)
                {
                    clocks[i].clockItem.SetActive(true);

                    SetPosition(clocks[i].clockItem, newPosition);

                    break;
                }
            }
        }

        public void SetFillAmount(string id, DateTime endTime)
        {
            for (int i = 0; i < clocks.Count; i++)
            {
                if (clocks[i].id == id)
                {
                    float fillAmount = CalcFillAmount(clocks[i].startTime, endTime, clocks[i].seconds);

                    clocks[i].clockAmountImage.fillAmount = fillAmount;
                }
            }
        }

        void SetPosition(GameObject clockItem, Vector2 position)
        {
            Vector2 uiPos = cam.WorldToViewportPoint(position);

            RectTransform newClockItemRectTransform = clockItem.GetComponent<RectTransform>();

            newClockItemRectTransform.anchorMin = uiPos;
            newClockItemRectTransform.anchorMax = uiPos;

            newClockItemRectTransform.anchoredPosition = offset;
        }

        float CalcFillAmount(DateTime startTime, DateTime endTime, int seconds)
        {
            double diffSeconds = (endTime - startTime).TotalSeconds;

            float singleFillAmount = 1f / seconds;

            double currentFillAmount = 0.01d + (singleFillAmount * diffSeconds);

            return (float)currentFillAmount;
        }
    }
}
