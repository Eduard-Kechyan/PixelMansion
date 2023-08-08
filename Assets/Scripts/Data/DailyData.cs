using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyData : MonoBehaviour
{
    // Variables
    public ShopData shopData;

    [HideInInspector]
    public int dailyDate = DateTime.UtcNow.Day;
    [HideInInspector]
    public bool dailyItem1 = false;
    [HideInInspector]
    public bool dailyItem2 = false;
    [HideInInspector]
    public Types.ShopItemsContent[] dailyContent;

    // References
    private JsonHandler jsonHandler;

    void Start() {
        // Cache
        jsonHandler = GetComponent<JsonHandler>();

        CheckDailyItems();
    }

    void CheckDailyItems()
    {
        dailyDate = DateTime.UtcNow.Day;

        if (PlayerPrefs.HasKey("dailyDate"))
        {
            if (dailyDate == PlayerPrefs.GetInt("dailyDate"))
            {
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
            PlayerPrefs.SetInt("dailyDate", DateTime.UtcNow.Day);
            PlayerPrefs.SetInt("dailyItem1", 0);
            PlayerPrefs.SetInt("dailyItem2", 0);
            PlayerPrefs.Save();
            SetDailyContent();
        }
    }

    public void SetDailyContent()
    {
        Types.ShopItemsContent[] dailyContent = new Types.ShopItemsContent[shopData.dailyContent.Length];

        dailyContent[0] = shopData.dailyContent[0];
        dailyContent[1] = shopData.dailyContent[UnityEngine.Random.Range(1, 3)];

        PlayerPrefs.SetString("dailyContent", jsonHandler.ConvertShopItemContentToJson(dailyContent, true));
        PlayerPrefs.Save();
    }

    public void GetDailyContent()
    {
        dailyContent = jsonHandler.ConvertShopItemContentFromJson(PlayerPrefs.GetString("dailyContent"));
    }
}
