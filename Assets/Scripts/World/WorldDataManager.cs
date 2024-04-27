using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace Merge
{
    public class WorldDataManager : MonoBehaviour
    {
        // Variables
        public Transform worldRoot;
        public int roomInHierarchyOffset = 2;
        public float bakeDelay = 0.5f;

        [Header("Debug")]
        public bool canLog = false;
        public bool saveData = false;
        public bool loadData = false;
        public bool clearData = false;

        [HideInInspector]
        public bool loaded = false;

        private bool initial = true;

        // References
        private NavMeshManager navMeshManager;
        private DataManager dataManager;
        private DataConverter dataConverter;
        private GameData gameData;
        private TaskManager taskManager;

        void Start()
        {
            // Cache
            navMeshManager = NavMeshManager.Instance;
            dataManager = DataManager.Instance;
            dataConverter = dataManager.GetComponent<DataConverter>();
            gameData = GameData.Instance;
            taskManager = GameRefs.Instance.taskManager;

            StartCoroutine(WaitForData());
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (saveData)
            {
                saveData = false;

                GetDataFromRoot(true);
            }

            if (loadData)
            {
                loadData = false;

                LoadData();
            }

            if (clearData)
            {
                clearData = false;

                ClearData();
            }
        }
#endif

        IEnumerator WaitForData()
        {
            while (!dataManager.loaded)
            {
                yield return null;
            }

            Init();
        }

        void Init()
        {
            // Check world root
            if (worldRoot == null)
            {
                // ERROR
                ErrorManager.Instance.FindUsed(
                    "World Root",
                    GetType().Name
                );

                worldRoot = GameObject.Find("World").transform.Find("Root");
            }

            if (PlayerPrefs.HasKey("areaSet"))
            {
                initial = false;
            }

            if (initial)
            {
                GetDataFromRoot();

                initial = false;
            }
            else
            {
                SetDataToRoot();
            }
        }

        // Read the data to be saved from the root gameObject and it's children
        void GetDataFromRoot(bool alt = false)
        {
            for (int i = 0; i < worldRoot.childCount; i++)
            {
                Transform worldItem = worldRoot.GetChild(i);

                if (worldItem.name.Contains("Area"))
                {
                    gameData.areasData.Add(HandleArea(worldItem));
                }
            }

            LogData();

            if (initial || alt)
            {
                SaveData();
            }
        }

        // Write the saved data to the root gameObject and it's children
        void SetDataToRoot()
        {
            LoadData();

            for (int i = 0; i < gameData.areasData.Count; i++)
            {
                for (int j = 0; j < worldRoot.childCount; j++)
                {
                    Transform worldArea = worldRoot.GetChild(j);

                    if (worldArea.name == gameData.areasData[i].name)
                    {
                        // Rooms
                        if (gameData.areasData[i].isRoom)
                        {
                            if (!gameData.areasData[i].isLocked)
                            {
                                worldArea.GetComponent<RoomHandler>().UnlockAlt();

                                // Wall left
                                if (gameData.areasData[i].wallLeftOrder >= 0 && worldArea.GetChild(0).TryGetComponent(out ChangeWall changeWallLeft))
                                {
                                    changeWallLeft.SetSprites(gameData.areasData[i].wallLeftOrder, true);

                                    changeWallLeft.GetComponent<Selectable>().canBeSelected = true;
                                }

                                // Wall right
                                if (gameData.areasData[i].wallRightOrder >= 0 && worldArea.GetChild(1).TryGetComponent(out ChangeWall changeWallRight))
                                {
                                    changeWallRight.SetSprites(gameData.areasData[i].wallRightOrder, true);

                                    changeWallRight.GetComponent<Selectable>().canBeSelected = true;
                                }

                                // Floor
                                if (gameData.areasData[i].floorOrder >= 0 && worldArea.GetChild(2).TryGetComponent(out ChangeFloor changeFloor))
                                {
                                    changeFloor.SetSprites(gameData.areasData[i].floorOrder, true);

                                    changeFloor.GetComponent<Selectable>().canBeSelected = true;
                                }

                                // Furniture
                                for (int k = 0; k < gameData.areasData[i].furniture.Count; k++)
                                {
                                    Transform furniture = worldArea.GetChild(3).GetChild(k);

                                    if (furniture.name == gameData.areasData[i].furniture[k].name)
                                    {
                                        if (gameData.areasData[i].furniture[k].order >= 0 && furniture.TryGetComponent(out ChangeFurniture changeFurniture))
                                        {
                                            changeFurniture.SetSprites(gameData.areasData[i].furniture[k].order, true);

                                            changeFurniture.GetComponent<Selectable>().canBeSelected = true;
                                        }
                                    }
                                }

                                // Props
                                for (int k = 0; k < gameData.areasData[i].props.Count; k++)
                                {
                                    Transform props = worldArea.GetChild(4).GetChild(k);

                                    if (props.name == gameData.areasData[i].props[k].name)
                                    {
                                        if (gameData.areasData[i].props[k].order >= 0 && props.TryGetComponent(out ChangeProp changeProp))
                                        {
                                            changeProp.SetSprites(gameData.areasData[i].props[k].order, true);

                                            changeProp.GetComponent<Selectable>().canBeSelected = true;
                                        }
                                    }
                                }

                                // Filth
                                for (int k = 0; k < gameData.areasData[i].filth.Count; k++)
                                {
                                    Transform filth = worldArea.GetChild(5).GetChild(k);

                                    if (filth.name == gameData.areasData[i].filth[k].name)
                                    {
                                        if (gameData.areasData[i].filth[k].removed)
                                        {
                                            filth.gameObject.SetActive(false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            StartCoroutine(BakeNavMeshAfterPhysicsUpdate());
        }

        // Read the data from the given area
        WorldTypes.Area HandleArea(Transform area)
        {
            bool isRoom;
            bool isLocked;
            int wallLeftOrder = -1;
            int wallRightOrder = -1;
            int floorOrder = -1;
            List<WorldTypes.Furniture> furniture = new();
            List<WorldTypes.Prop> props = new();
            List<WorldTypes.Filth> filth = new();

            RoomHandler roomHandler = area.GetComponent<RoomHandler>();

            if (roomHandler != null)
            {
                isRoom = true;
                isLocked = roomHandler.locked;

                Selectable wallLeftSelectable = area.GetChild(0).GetComponent<Selectable>();
                Selectable wallRightSelectable = area.GetChild(1).GetComponent<Selectable>();
                Selectable floorSelectable = area.GetChild(2).GetComponent<Selectable>();

                if (!wallLeftSelectable.isOld)
                {
                    wallLeftOrder = wallLeftSelectable.spriteOrder;
                }

                if (!wallRightSelectable.isOld)
                {
                    wallRightOrder = wallRightSelectable.spriteOrder;
                }

                if (!floorSelectable.isOld)
                {
                    floorOrder = floorSelectable.spriteOrder;
                }

                // Furniture
                Transform areaFurniture = area.GetChild(3);

                for (int i = 0; i < areaFurniture.childCount; i++)
                {
                    Selectable furnitureSelectable = areaFurniture.GetChild(i).GetComponent<Selectable>();

                    WorldTypes.Furniture furnitureItem = new()
                    {
                        name = areaFurniture.GetChild(i).name,
                        order = furnitureSelectable.spriteOrder
                    };

                    furniture.Add(furnitureItem);
                }

                // Props
                Transform areaProps = area.GetChild(4);

                for (int i = 0; i < areaProps.childCount; i++)
                {
                    Selectable propSelectable = areaFurniture.GetChild(i).GetComponent<Selectable>();

                    WorldTypes.Prop propItem = new()
                    {
                        name = areaProps.GetChild(i).name,
                        order = propSelectable.spriteOrder
                    };

                    props.Add(propItem);
                }

                // Filth
                Transform areaFilth = area.GetChild(5);

                for (int i = 0; i < areaFilth.childCount; i++)
                {
                    Transform filthItemTransform = areaProps.GetChild(i);

                    WorldTypes.Filth filthItem = new()
                    {
                        name = filthItemTransform.name,
                        removed = !filthItemTransform.gameObject.activeSelf
                    };

                    filth.Add(filthItem);
                }
            }
            else
            {
                isRoom = false;
                isLocked = false;
            }

            WorldTypes.Area newArea = new()
            {
                name = area.name,
                isRoom = isRoom,
                isLocked = isLocked,
                wallLeftOrder = wallLeftOrder,
                wallRightOrder = wallRightOrder,
                floorOrder = floorOrder,
                furniture = furniture,
                props = props,
                filth = filth,
            };

            return newArea;
        }

        // Save the data to disk
        void SaveData()
        {
            string areasJson = dataConverter.ConvertAreaToJson(gameData.areasData);

            if (initial)
            {
                dataManager.SaveValue(new(){
                    {"areaSet", true},
                    {"areas", areasJson}
                });

                PlayerPrefs.SetInt("areaSet", 1);
                PlayerPrefs.Save();
            }
            else
            {
                dataManager.SaveValue("areas", areasJson);
            }

            StartCoroutine(BakeNavMeshAfterPhysicsUpdate());
        }

        // Read the saved data from disk
        void LoadData()
        {
            if (gameData.areasData.Count == 0)
            {
                string newAreasData = dataManager.LoadValue<string>("areas");

                gameData.areasData = dataConverter.ConvertJsonToArea(newAreasData);

                LogData();
            }
        }

        // Delete the saved data from disk
        void ClearData()
        {
            string folderPath = Application.persistentDataPath + "/QuickSave/Areas.json";

            if (File.Exists(folderPath))
            {
                File.Delete(folderPath);
            }

            PlayerPrefs.DeleteKey("AreaLoaded");
            PlayerPrefs.Save();
        }

        // Log the read data
        void LogData()
        {
            if (canLog)
            {
                for (int i = 0; i < gameData.areasData.Count; i++)
                {
                    if (gameData.areasData[i].isRoom)
                    {
                        Debug.Log(gameData.areasData[i].name + ", Is Room, " + gameData.areasData[i].wallLeftOrder + ", " + gameData.areasData[i].wallRightOrder + ", " + gameData.areasData[i].floorOrder);
                    }
                    else
                    {
                        Debug.Log(gameData.areasData[i].name);
                    }

                    for (int j = 0; j < gameData.areasData[i].furniture.Count; j++)
                    {
                        Debug.Log("      " + gameData.areasData[i].furniture[j].name + ", " + gameData.areasData[i].furniture[j].order);
                    }
                }
            }
        }

        // Bakes the navigation mesh after reading or writing the root
        IEnumerator BakeNavMeshAfterPhysicsUpdate()
        {
            if (!loaded)
            {
                yield return new WaitForSeconds(bakeDelay);

                navMeshManager.Bake(() =>
                {
                    if (taskManager == null || !taskManager.enabled)
                    {
                        loaded = true;
                    }
                    else
                    {
                        StartCoroutine(CheckIfThereIsATaskToComplete());
                    }
                });
            }
        }

        // Check if there is a tasks to complete
        IEnumerator CheckIfThereIsATaskToComplete()
        {
            while (!taskManager.isLoaded)
            {
                yield return null;
            }

            taskManager.CheckIfThereIsATaskToComplete(() =>
            {
                loaded = true;
            });
        }

        // Public methods
        public void SetSelectable(Selectable selectable)
        {
            for (int i = 0; i < gameData.areasData.Count; i++)
            {
                // Walls
                if (selectable.type == Selectable.Type.Wall && gameData.areasData[i].name == selectable.transform.parent.name)
                {
                    gameData.areasData[i].wallRightOrder = selectable.spriteOrder;

                    break;
                }

                // Floor
                if (selectable.type == Selectable.Type.Floor && gameData.areasData[i].name == selectable.transform.parent.name)
                {
                    gameData.areasData[i].floorOrder = selectable.spriteOrder;

                    break;
                }

                // Furniture
                if (selectable.type == Selectable.Type.Furniture && gameData.areasData[i].name == selectable.transform.parent.transform.parent.name)
                {
                    for (int j = 0; j < gameData.areasData[i].furniture.Count; j++)
                    {
                        if (gameData.areasData[i].furniture[j].name == selectable.name)
                        {
                            gameData.areasData[i].furniture[j].order = selectable.spriteOrder;

                            break;
                        }
                    }

                    break;
                }

                // Prop
                if (selectable.type == Selectable.Type.Prop && gameData.areasData[i].name == selectable.transform.parent.transform.parent.name)
                {
                    for (int j = 0; j < gameData.areasData[i].props.Count; j++)
                    {
                        if (gameData.areasData[i].props[j].name == selectable.name)
                        {
                            gameData.areasData[i].props[j].order = selectable.spriteOrder;

                            break;
                        }
                    }

                    break;
                }
            }

            SaveData();
        }

        public void SetFilth(Transform filth)
        {
            for (int i = 0; i < gameData.areasData.Count; i++)
            {
                if (gameData.areasData[i].name == filth.parent.name)
                {
                    for (int j = 0; j < gameData.areasData[i].props.Count; j++)
                    {
                        if (gameData.areasData[i].filth[j].name == filth.name)
                        {
                            gameData.areasData[i].filth[j].removed = true;

                            break;
                        }
                    }
                }
            }

            SaveData();
        }

        // Get the rooms that are initially unlocked (should be none)
        public string GetInitialUnlockedRooms()
        {
            List<string> unlockedRooms = new();

            for (int i = 0; i < worldRoot.childCount; i++)
            {
                Transform worldItem = worldRoot.GetChild(i);

                if (worldItem.name.Contains("Area"))
                {
                    RoomHandler roomHandler = worldItem.GetComponent<RoomHandler>();

                    if (roomHandler != null && !roomHandler.locked)
                    {
                        unlockedRooms.Add(worldItem.name);
                    }
                }
            }

            string[] initialUnlocked = new string[unlockedRooms.Count];

            for (int i = 0; i < unlockedRooms.Count; i++)
            {
                initialUnlocked[i] = unlockedRooms[i];
            }

            dataManager.unlockedRoomsJsonData = JsonConvert.SerializeObject(initialUnlocked);

            return dataManager.unlockedRoomsJsonData;
        }

        // Get the selectable world item for the given task
        public Transform GetWorldItem(string groupId, Types.Task task)
        {
            Transform worldArea = null;

            for (int i = 0; i < worldRoot.childCount; i++)
            {
                Transform tempArea = worldRoot.GetChild(i);

                if (tempArea.name == groupId + "Area")
                {
                    worldArea = tempArea;
                }
            }

            if (worldArea != null)
            {
                switch (task.taskRefType)
                {
                    case Types.TaskRefType.Area:
                        return worldArea;
                    case Types.TaskRefType.Wall:
                        return worldArea.Find(task.taskRefType.ToString() + (task.isTaskRefRight ? "Right" : "Left"));
                    case Types.TaskRefType.Filth:
                        return worldArea.Find("Filth").Find(task.taskRefName);
                    default:
                        // Floor, Furniture, Item and others
                        // TODO - Possibly find Furniture and Item first before getting their children
                        return worldArea.Find(task.taskRefName == "" ? task.taskRefType.ToString() : task.taskRefName);
                }
            }

            return null;
        }

        // Fine the position of a room in the world (root)
        public Transform FindRoomInWorld(string roomName)
        {
            for (int i = 0; i < worldRoot.childCount; i++)
            {
                Transform tempArea = worldRoot.GetChild(i);

                if (tempArea.name == roomName + "Area")
                {
                    return tempArea;
                }
            }

            return null;
        }

        // Doors
        public void SaveDoors(List<string> unlockedDoors)
        {
            dataManager.SaveValue(new(){
                {"doorSet", true},
                {"unlockedDoors", JsonConvert.SerializeObject(unlockedDoors)}
            });

            PlayerPrefs.SetInt("doorSet", 1);
            PlayerPrefs.Save();
        }

        public List<string> LoadDoors()
        {
            string newUnlockedDoors = dataManager.LoadValue<string>("unlockedDoors");

            return JsonConvert.DeserializeObject<List<string>>(newUnlockedDoors);
        }

        // Find selectable items collider center position
        public Vector2 GetColliderCenter(Transform taskRef, Types.TaskRefType taskRefType)
        {
            Vector2 center;

            switch (taskRefType)
            {
                case Types.TaskRefType.Area:
                    center = taskRef.transform.position;
                    break;
                case Types.TaskRefType.Wall:
                    center = taskRef.GetComponent<CompositeCollider2D>().bounds.center;
                    break;
                case Types.TaskRefType.Floor:
                    center = taskRef.GetComponent<CompositeCollider2D>().bounds.center;
                    break;
                default: // Item and others
                    center = taskRef.GetComponent<PolygonCollider2D>().bounds.center;
                    break;
            }

            return center;
        }
    }
}