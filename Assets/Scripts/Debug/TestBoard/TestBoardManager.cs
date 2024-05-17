using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Merge
{
    public class TestBoardManager : MonoBehaviour
    {
        // Variables
        public Generators generators;
        public Items items;
        public Chests chests;
        public Colls colls;
        public List<Sprite> crates;

        [HideInInspector]
        public bool ready = false;

        [HideInInspector]
        public TextAsset initialItems;

        private string initialItemsPath = "";

        // Sprites
        private Sprite[] itemSprites;
        private Sprite[] generatorSprites;
        private Sprite[] chestSprites;
        private Sprite[] collectableSprites;

        // Classes
        [Serializable]
        public class InitialItemData
        {
            public string sprite;
            public string type;
            public string state;
            public string group;
            public string genGroup;
            public string collGroup;
            public string chestGroup;
        }

        // References
        private AddressableManager addressableManager;

        void Start()
        {
            // Cache
            addressableManager = GetComponent<AddressableManager>();

            StartCoroutine(WaitForInitialization());
        }

        IEnumerator WaitForInitialization()
        {
            while (!addressableManager.initialized)
            {
                yield return null;
            }

            Init();
        }

        async void Init()
        {
            // Get sprites
            itemSprites = await addressableManager.LoadAssetAllArrayAsync<Sprite>("items");
            generatorSprites = await addressableManager.LoadAssetAllArrayAsync<Sprite>("generators");
            collectableSprites = await addressableManager.LoadAssetAllArrayAsync<Sprite>("collectables");
            chestSprites = await addressableManager.LoadAssetAllArrayAsync<Sprite>("chests");

            // Get Initial Items
            string prePath = "Assets/Addressables/Data/InitialItemsSkip.json";

            initialItems = await addressableManager.LoadAssetAsync<TextAsset>(prePath);

            initialItemsPath = Application.dataPath.Replace("/Assets", "/") + prePath;

            ready = true;
        }

        Sprite GetSprite(string spriteName, Item.Type type)
        {
            if (type == Item.Type.Coll)
            {
                for (int i = 0; i < collectableSprites.Length; i++)
                {
                    if (collectableSprites[i].name == spriteName)
                    {
                        return collectableSprites[i];
                    }
                }
            }
            else if (type == Item.Type.Chest)
            {
                for (int i = 0; i < chestSprites.Length; i++)
                {
                    if (chestSprites[i].name == spriteName)
                    {
                        return chestSprites[i];
                    }
                }
            }
            else if (type == Item.Type.Gen)
            {
                for (int i = 0; i < generatorSprites.Length; i++)
                {
                    if (generatorSprites[i].name == spriteName)
                    {
                        return generatorSprites[i];
                    }
                }
            }
            else
            {
                for (int i = 0; i < itemSprites.Length; i++)
                {
                    if (itemSprites[i].name == spriteName)
                    {
                        return itemSprites[i];
                    }
                }
            }

            Debug.Log(type);
            Debug.Log(spriteName);

            Debug.Log("SPRITE IS NULL");

            return default;
        }

        public BoardManager.Tile[] ConvertInitialItemsToBoard(string initialJson)
        {
            InitialItemData[] initialItemDataJson = JsonConvert.DeserializeObject<InitialItemData[]>(initialJson);

            BoardManager.Tile[] boardData = new BoardManager.Tile[initialItemDataJson.Length];

            for (int i = 0; i < initialItemDataJson.Length; i++)
            {
                Item.Type newType = Glob.ParseEnum<Item.Type>(initialItemDataJson[i].type);

                BoardManager.Tile newBoardData = new()
                {
                    sprite = initialItemDataJson[i].sprite == "" ? null : GetSprite(initialItemDataJson[i].sprite, newType),
                    state = Glob.ParseEnum<Item.State>(initialItemDataJson[i].state),
                    type = newType,
                    group = Glob.ParseEnum<Item.Group>(initialItemDataJson[i].group),
                    genGroup = Glob.ParseEnum<Item.GenGroup>(initialItemDataJson[i].genGroup),
                    collGroup = Glob.ParseEnum<Item.CollGroup>(initialItemDataJson[i].collGroup),
                    chestGroup = Glob.ParseEnum<Item.ChestGroup>(initialItemDataJson[i].chestGroup),
                };

                boardData[i] = newBoardData;
            }

            return boardData;
        }
    }
}
