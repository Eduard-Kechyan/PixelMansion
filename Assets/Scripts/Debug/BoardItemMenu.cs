using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json;

namespace Merge
{
    public class BoardItemMenu : MonoBehaviour
    {
#if UNITY_EDITOR
        // Variables
        public Generators generators;
        public Items items;
        public Chests chests;
        public Colls colls;

        [HideInInspector]
        public bool ready = false;
        [HideInInspector]
        public int orderToChange = 0;
        private BoardManager.ItemData selectedItemData;
        private Item.State selectedState = Item.State.Crate;

        // Item Menu
        private bool itemMenuOpen = false;
        private bool closing = false;

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
        private BoardViewer boardViewer;

        // UI
        private VisualElement root;

        private VisualElement itemMenu;
        private ScrollView itemScrollView;
        private Label selectedItemLabel;
        private EnumField itemEnumFieldTop;
        private EnumField itemEnumFieldBottom;
        private Button acceptButton;
        private Button cancelButton;
        private Button clearButton;

        void Start()
        {
            // Cache
            addressableManager = GetComponent<AddressableManager>();
            boardViewer = GetComponent<BoardViewer>();

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            itemMenu = root.Q<VisualElement>("ItemMenu");
            itemScrollView = itemMenu.Q<ScrollView>("ItemScrollView");
            selectedItemLabel = itemMenu.Q<Label>("SelectedItemLabel");
            itemEnumFieldTop = itemMenu.Q<EnumField>("ItemEnumFieldTop");
            itemEnumFieldBottom = itemMenu.Q<EnumField>("ItemEnumFieldBottom");
            acceptButton = itemMenu.Q<Button>("AcceptButton");
            cancelButton = itemMenu.Q<Button>("CancelButton");
            clearButton = itemMenu.Q<Button>("ClearButton");

            // UI taps
            acceptButton.clicked += () => AcceptItem();
            cancelButton.clicked += () => CloseMenu();
            clearButton.clicked += () => Clear();

            // Init
            itemEnumFieldTop.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                HandleItemEnumField(evt);
            });
            itemEnumFieldBottom.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                HandleItemEnumField(evt);
            });

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

            ready = true;

            AddItemsToScrollView();
        }

        public void OpenItemMenu(string orderString, bool isTop, Item.State state)
        {
            if (!itemMenuOpen || !closing)
            {
                orderToChange = int.Parse(orderString);

                boardViewer.SelectButton(orderToChange);

                itemEnumFieldTop.value = state;
                itemEnumFieldBottom.value = state;
                selectedState = state;

                if (isTop)
                {
                    itemMenu.AddToClassList("item_menu_top");
                }
                else
                {
                    itemMenu.RemoveFromClassList("item_menu_top");
                }

                selectedItemLabel.style.visibility = Visibility.Hidden;
                selectedItemLabel.style.opacity = 0;

                // Open menu
                itemMenu.style.display = DisplayStyle.Flex;
                itemMenu.style.opacity = 1;

                itemMenuOpen = true;
            }
        }

        void AddItemsToScrollView()
        {
            if (itemScrollView.childCount == 0)
            {
                // Coll
                Label collLabel = new() { name = "CollLabel", text = "Colls:" };
                collLabel.AddToClassList("item_menu_label");
                itemScrollView.Add(collLabel);

                Item.CollGroup lastCollGroup = Item.CollGroup.Energy;

                for (int i = 0; i < colls.content.Length; i++)
                {
                    if (lastCollGroup != colls.content[i].collGroup)
                    {
                        lastCollGroup = colls.content[i].collGroup;

                        Label newCollGroupLabel = new() { name = "CollGroupLabel" + i, text = lastCollGroup + ":" };
                        newCollGroupLabel.AddToClassList("item_menu_label");
                        newCollGroupLabel.AddToClassList("item_menu_sub_label");

                        itemScrollView.Add(newCollGroupLabel);
                    }

                    for (int j = 0; j < colls.content[i].content.Length; j++)
                    {
                        Button newCollButton = new() { name = "CollButton" + i + "." + j, text = "" };
                        newCollButton.AddToClassList("button");
                        newCollButton.AddToClassList("item_menu_item");

                        BoardManager.ItemData newItemData = colls.content[i].content[j];

                        newItemData.type = Item.Type.Coll;

                        newCollButton.style.backgroundImage = new StyleBackground(GetSprite(colls.content[i].content[j].sprite.name, newItemData.type));

                        itemScrollView.Add(newCollButton);

                        newCollButton.clicked += () => SelectItem(newItemData);
                    }
                }

                // Chest
                Label chestLabel = new() { name = "ChestLabel", text = "Chests:" };
                chestLabel.AddToClassList("item_menu_label");
                itemScrollView.Add(chestLabel);

                Item.ChestGroup lastChestGroup = Item.ChestGroup.Energy;

                for (int i = 0; i < chests.content.Length; i++)
                {
                    if (lastChestGroup != chests.content[i].chestGroup)
                    {
                        lastChestGroup = chests.content[i].chestGroup;

                        Label newChestGroupLabel = new() { name = "ChestGroupLabel" + i, text = lastChestGroup + ":" };
                        newChestGroupLabel.AddToClassList("item_menu_sub_label");

                        itemScrollView.Add(newChestGroupLabel);
                    }

                    for (int j = 0; j < chests.content[i].content.Length; j++)
                    {
                        Button newChestButton = new() { name = "ChestButton" + i + "." + j, text = "" };
                        newChestButton.AddToClassList("button");
                        newChestButton.AddToClassList("item_menu_item");

                        BoardManager.ItemData newItemData = chests.content[i].content[j];

                        newItemData.type = Item.Type.Chest;

                        newChestButton.style.backgroundImage = new StyleBackground(GetSprite(chests.content[i].content[j].sprite.name, newItemData.type));

                        itemScrollView.Add(newChestButton);


                        newChestButton.clicked += () => SelectItem(newItemData);
                    }
                }

                // Generators
                Label genLabel = new() { name = "GenLabel", text = "Generators:" };
                genLabel.AddToClassList("item_menu_label");
                itemScrollView.Add(genLabel);

                Item.GenGroup lastGenGroup = Item.GenGroup.Toolbox;

                for (int i = 0; i < generators.content.Length; i++)
                {
                    if (lastGenGroup != generators.content[i].genGroup)
                    {
                        lastGenGroup = generators.content[i].genGroup;

                        Label newGenGroupLabel = new() { name = "GenGroupLabel" + i, text = lastGenGroup + ":" };
                        newGenGroupLabel.AddToClassList("item_menu_sub_label");

                        itemScrollView.Add(newGenGroupLabel);
                    }

                    for (int j = 0; j < generators.content[i].content.Length; j++)
                    {
                        Button newGenButton = new() { name = "GenButton" + i + "." + j, text = "" };
                        newGenButton.AddToClassList("button");
                        newGenButton.AddToClassList("item_menu_item");

                        BoardManager.ItemData newItemData = generators.content[i].content[j];

                        newItemData.type = Item.Type.Gen;

                        newGenButton.style.backgroundImage = new StyleBackground(GetSprite(generators.content[i].content[j].sprite.name, newItemData.type));

                        itemScrollView.Add(newGenButton);

                        newGenButton.clicked += () => SelectItem(newItemData);
                    }
                }

                // Items
                Label itemLabel = new() { name = "ItemLabel", text = "Items:" };
                itemLabel.AddToClassList("item_menu_label");
                itemScrollView.Add(itemLabel);

                Item.Group lastGroup = Item.Group.Gloves;

                for (int i = 0; i < items.content.Length; i++)
                {
                    if (lastGroup != items.content[i].group)
                    {
                        lastGroup = items.content[i].group;

                        Label newGroupLabel = new() { name = "GroupLabel" + i, text = lastGroup + ":" };
                        newGroupLabel.AddToClassList("item_menu_sub_label");

                        itemScrollView.Add(newGroupLabel);
                    }

                    for (int j = 0; j < items.content[i].content.Length; j++)
                    {
                        Button newItemButton = new() { name = "ItemButton" + i + "." + j, text = "" };
                        newItemButton.AddToClassList("button");
                        newItemButton.AddToClassList("item_menu_item");

                        BoardManager.ItemData newItemData = items.content[i].content[j];

                        newItemData.type = Item.Type.Item;

                        newItemButton.style.backgroundImage = new StyleBackground(GetSprite(items.content[i].content[j].sprite.name, newItemData.type));

                        itemScrollView.Add(newItemButton);

                        newItemButton.clicked += () => SelectItem(newItemData);
                    }
                }
            }
        }

        void SelectItem(BoardManager.ItemData itemData)
        {
            selectedItemData = itemData;

            selectedItemLabel.text = itemData.sprite.name;

            selectedItemLabel.style.visibility = Visibility.Visible;
            selectedItemLabel.style.opacity = 1;

            Debug.Log(itemData);
        }

        void Clear()
        {
            closing = true;

            boardViewer.boardData[orderToChange].sprite = null;
            boardViewer.boardData[orderToChange].type = Item.Type.Item;
            boardViewer.boardData[orderToChange].group = Item.Group.Tools;
            boardViewer.boardData[orderToChange].genGroup = Item.GenGroup.Toolbox;
            boardViewer.boardData[orderToChange].chestGroup = Item.ChestGroup.Item;
            boardViewer.boardData[orderToChange].collGroup = Item.CollGroup.Experience;
            boardViewer.boardData[orderToChange].state = Item.State.Crate;

            boardViewer.SaveBoardData(() =>
            {
                CloseMenu();
            });
        }

        void AcceptItem()
        {
            closing = true;

            bool changed = false;

            if (selectedItemData != null)
            {
                boardViewer.boardData[orderToChange].sprite = selectedItemData.sprite;
                boardViewer.boardData[orderToChange].type = selectedItemData.type;
                boardViewer.boardData[orderToChange].group = selectedItemData.group;
                boardViewer.boardData[orderToChange].genGroup = selectedItemData.genGroup;
                boardViewer.boardData[orderToChange].chestGroup = selectedItemData.chestGroup;
                boardViewer.boardData[orderToChange].collGroup = selectedItemData.collGroup;

                changed = true;
            }

            if (boardViewer.boardData[orderToChange].state != selectedState)
            {
                boardViewer.boardData[orderToChange].state = selectedState;

                changed = true;
            }

            if (changed)
            {
                boardViewer.SaveBoardData(() =>
                {
                    CloseMenu();
                });
            }
            else
            {
                CloseMenu();
            }
        }

        void CloseMenu()
        {
            itemMenu.style.display = DisplayStyle.None;
            itemMenu.style.opacity = 0;

            boardViewer.DeSelectButton(orderToChange);

            StartCoroutine(ClearDataAfterClosing());
        }

        IEnumerator ClearDataAfterClosing()
        {
            selectedItemLabel.style.visibility = Visibility.Hidden;
            selectedItemLabel.style.opacity = 0;

            yield return new WaitForSeconds(0.2f);

            itemMenuOpen = false;

            selectedItemData = null;

            selectedItemLabel.text = "";

            selectedState = Item.State.Crate;

            closing = false;
        }

        void HandleItemEnumField(ChangeEvent<string> evt)
        {
            Item.State newState = Glob.ParseEnum<Item.State>(evt.newValue);

            itemEnumFieldTop.value = newState;
            itemEnumFieldBottom.value = newState;
            selectedState = newState;
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

        public string ConvertBoardToInitialItems(BoardManager.Tile[] boardData)
        {
            InitialItemData[] initialItemDataJson = new InitialItemData[boardData.Length];

            for (int i = 0; i < boardData.Length; i++)
            {
                InitialItemData newInitialItemJson = new()
                {
                    sprite = boardData[i].sprite == null ? "" : boardData[i].sprite.name,
                    state = boardData[i].state.ToString(),
                    type = boardData[i].type.ToString(),
                    group = boardData[i].group.ToString(),
                    genGroup = boardData[i].genGroup.ToString(),
                    collGroup = boardData[i].collGroup.ToString(),
                    chestGroup = boardData[i].chestGroup.ToString(),
                };

                initialItemDataJson[i] = newInitialItemJson;
            }

            return JsonConvert.SerializeObject(initialItemDataJson);
        }
#endif
    }
}
