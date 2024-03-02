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
        public bool dailyItem1 = false;
        [HideInInspector]
        public bool dailyItem2 = false;
        [HideInInspector]
        public Types.ShopItemsContent[] dailyContent;

        // References
        private DataConverter dataConverter;

        void Start()
        {
            // Cache
            dataConverter = GetComponent<DataConverter>();

            StartCoroutine(WaitForData());
        }

        IEnumerator WaitForData()
        {
            while (!DataManager.Instance.loaded)
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
                    // A different day
                    GetDailyContent();
                    dailyItem1 = PlayerPrefs.GetInt("dailyItem1") == 1;
                    dailyItem2 = PlayerPrefs.GetInt("dailyItem2") == 1;
                }
                else
                {
                    // A new day
                    SetDailyContent();
                    dailyItem1 = false;
                    dailyItem2 = false;
                }
            }
            else
            {
                // First day
                PlayerPrefs.SetInt("dailyDate", DateTime.UtcNow.Day);
                PlayerPrefs.SetInt("dailyItem1", 0);
                PlayerPrefs.SetInt("dailyItem2", 0);
                PlayerPrefs.Save();
                SetDailyContent();
            }
        }

        void SetDailyContent()
        {
            dailyContent = new Types.ShopItemsContent[2];

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

        public int GetLeftCount(string nameOrder, int total, Types.ShopItemType shopItemType)
        {
            // FIX - Handle this
            int order = int.Parse(nameOrder);
            return 0;
        }

        public void SetBoughtItem(string nameOrder, Types.ShopItemType shopItemType)
        {
            // FIX - Handle this
            int order = int.Parse(nameOrder);


        }
    }
}