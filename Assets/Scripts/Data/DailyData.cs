using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class DailyData : MonoBehaviour
    {
        // Variables
        public ShopData shopData;

        [HideInInspector]
        public bool dataSet = false;
        [HideInInspector]
        public int dailyDate = DateTime.UtcNow.Day;
        [HideInInspector]
        public bool dailyItem0 = false;
        [HideInInspector]
        public bool dailyItem1 = false;
        [HideInInspector]
        public ShopMenu.ShopItemsContent[] dailyContent;

        // References
        private DataManager dataManager;
        private DataConverter dataConverter;

        void Start()
        {
            // Cache
            dataManager = GetComponent<DataManager>();
            dataConverter = GetComponent<DataConverter>();

            StartCoroutine(WaitForData());
        }

        IEnumerator WaitForData()
        {
            while (!dataManager.loaded)
            {
                yield return null;
            }

            CheckDailyItems();
        }

        void CheckDailyItems()
        {
            dailyDate = DateTime.UtcNow.Day;

            if (PlayerPrefs.HasKey("dailyDate"))
            {
                if (dailyDate == PlayerPrefs.GetInt("dailyDate"))
                {
                    // The same day
                    GetDailyContent();

                    dailyItem0 = PlayerPrefs.GetInt("dailyItem0") == 1;
                    dailyItem1 = PlayerPrefs.GetInt("dailyItem1") == 1;
                }
                else
                {
                    // A new day
                    SetDailyContent();

                    dailyItem0 = false;
                    dailyItem1 = false;
                }
            }
            else
            {
                // First day
                PlayerPrefs.SetInt("dailyDate", DateTime.UtcNow.Day);
                PlayerPrefs.SetInt("dailyItem0", 0);
                PlayerPrefs.SetInt("dailyItem1", 0);
                PlayerPrefs.Save();

                SetDailyContent();
            }
        }

        void SetDailyContent()
        {
            dailyContent = new ShopMenu.ShopItemsContent[2];

            dailyContent[0] = shopData.dailyContent[0];
            dailyContent[1] = shopData.dailyContent[UnityEngine.Random.Range(1, 3)];

            PlayerPrefs.SetString("dailyContent", dataConverter.ConvertShopItemContentToJson(dailyContent));
            PlayerPrefs.Save();

            dataSet = true;
        }

        void GetDailyContent()
        {
            dailyContent = dataConverter.ConvertShopItemContentFromJson(PlayerPrefs.GetString("dailyContent"));

            dataSet = true;
        }

        public int CheckBoughtItem(string nameOrder, int total)
        {
            // FIX - Handle this
            int order = int.Parse(nameOrder);

            return 0;
        }

        public void SetBoughtItem(string nameOrder)
        {
            // FIX - Handle this
            int order = int.Parse(nameOrder);
        }
    }
}