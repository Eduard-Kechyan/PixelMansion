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

        // Classes
        [Serializable]
        public class Area
        {
            public string name;
            public bool isLocked;
            public bool isRoom;
            public int wallLeftOrder;
            public int wallRightOrder;
            public int floorOrder;
            public List<Furniture> furniture;
            public List<Prop> props;
            public List<Filth> filth;
        }

        [Serializable]
        public class AreaJson
        {
            public string name;
            public bool isLocked;
            public bool isRoom;
            public int wallLeftOrder;
            public int wallRightOrder;
            public int floorOrder;
            public string furniture;
            public string props;
            public string filth;
        }

        [Serializable]
        public class Furniture
        {
            public string name;
            public int order;
        }

        [Serializable]
        public class Prop
        {
            public string name;
            public int order;
        }

        [Serializable]
        public class Filth
        {
            public string name;
            public bool removed;
        }

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
                        if (!gameData.areasData[i].isLocked)
                        {
                            // Get references
                            AreaRefs areaRefs = worldArea.GetComponent<AreaRefs>();

                            areaRefs.GetReferences(() =>
                            {
                                // Handle room
                                if (worldArea.TryGetComponent(out RoomHandler roomHandler))
                                {
                                    roomHandler.UnlockAlt();
                                }

                                // Wall left
                                if (areaRefs.wallLeft && gameData.areasData[i].wallLeftOrder >= 0 && areaRefs.wallLeft.TryGetComponent(out ChangeWall changeWallLeft))
                                {
                                    changeWallLeft.SetSprites(gameData.areasData[i].wallLeftOrder, true);

                                    Selectable selectable = changeWallLeft.GetComponent<Selectable>();

                                    selectable.canBeSelected = true;
                                    selectable.spriteOrder = gameData.areasData[i].wallLeftOrder;
                                }

                                // Wall right
                                if (areaRefs.wallRight && gameData.areasData[i].wallRightOrder >= 0 && areaRefs.wallRight.TryGetComponent(out ChangeWall changeWallRight))
                                {
                                    changeWallRight.SetSprites(gameData.areasData[i].wallRightOrder, true);

                                    Selectable selectable = changeWallRight.GetComponent<Selectable>();

                                    selectable.canBeSelected = true;
                                    selectable.spriteOrder = gameData.areasData[i].wallRightOrder;
                                }

                                // Floor
                                if (areaRefs.floor && gameData.areasData[i].floorOrder >= 0 && areaRefs.floor.TryGetComponent(out ChangeFloor changeFloor))
                                {
                                    changeFloor.SetSprites(gameData.areasData[i].floorOrder, true);

                                    Selectable selectable = changeFloor.GetComponent<Selectable>();

                                    selectable.canBeSelected = true;
                                    selectable.spriteOrder = gameData.areasData[i].floorOrder;
                                }

                                // Furniture
                                if (areaRefs.furniture)
                                {
                                    for (int k = 0; k < gameData.areasData[i].furniture.Count; k++)
                                    {
                                        Transform furniture = areaRefs.furniture.GetChild(k);

                                        if (furniture.name == gameData.areasData[i].furniture[k].name)
                                        {
                                            if (gameData.areasData[i].furniture[k].order >= 0 && furniture.TryGetComponent(out ChangeFurniture changeFurniture))
                                            {
                                                changeFurniture.SetSprites(gameData.areasData[i].furniture[k].order, true);

                                                Selectable selectable = changeFurniture.GetComponent<Selectable>();

                                                selectable.canBeSelected = true;
                                                selectable.spriteOrder = gameData.areasData[i].furniture[k].order;
                                            }
                                        }
                                    }
                                }

                                // Props
                                if (areaRefs.props)
                                {
                                    for (int k = 0; k < gameData.areasData[i].props.Count; k++)
                                    {
                                        Transform props = areaRefs.props.GetChild(k);

                                        if (props.name == gameData.areasData[i].props[k].name)
                                        {
                                            if (gameData.areasData[i].props[k].order >= 0 && props.TryGetComponent(out ChangeProp changeProp))
                                            {
                                                changeProp.SetSprites(gameData.areasData[i].props[k].order, true);

                                                Selectable selectable = changeProp.GetComponent<Selectable>();

                                                selectable.canBeSelected = true;
                                                selectable.spriteOrder = gameData.areasData[i].props[k].order;
                                            }
                                        }
                                    }
                                }

                                // Filth
                                if (areaRefs.filth)
                                {
                                    for (int k = 0; k < gameData.areasData[i].filth.Count; k++)
                                    {
                                        Transform filth = areaRefs.filth.GetChild(k);

                                        if (filth.name == gameData.areasData[i].filth[k].name)
                                        {
                                            if (gameData.areasData[i].filth[k].removed)
                                            {
                                                filth.gameObject.SetActive(false);
                                            }
                                        }
                                    }
                                }
                            });
                        }
                    }
                }
            }

            StartCoroutine(BakeNavMeshAfterPhysicsUpdate());
        }

        // Read the data from the given area
        Area HandleArea(Transform area)
        {
            bool isRoom = false;
            bool isLocked = false;
            int wallLeftOrder = -1;
            int wallRightOrder = -1;
            int floorOrder = -1;
            List<Furniture> furniture = new();
            List<Prop> props = new();
            List<Filth> filth = new();

            AreaRefs areaRefs = area.GetComponent<AreaRefs>();
            RoomHandler roomHandler = area.GetComponent<RoomHandler>();

            if (roomHandler != null)
            {
                isRoom = true;
                isLocked = roomHandler.locked;
            }

            areaRefs.GetReferences(() =>
            {
                // Wall Left
                if (areaRefs.wallLeft)
                {
                    Selectable wallLeftSelectable = areaRefs.wallLeft.GetComponent<Selectable>();

                    if (!wallLeftSelectable.isOld)
                    {
                        wallLeftOrder = wallLeftSelectable.spriteOrder;
                    }
                }

                // Wall Right
                if (areaRefs.wallRight)
                {
                    Selectable wallRightSelectable = areaRefs.wallRight.GetComponent<Selectable>();

                    if (!wallRightSelectable.isOld)
                    {
                        wallRightOrder = wallRightSelectable.spriteOrder;
                    }
                }

                // Floor
                if (areaRefs.floor)
                {
                    Selectable floorSelectable = areaRefs.floor.GetComponent<Selectable>();

                    if (!floorSelectable.isOld)
                    {
                        floorOrder = floorSelectable.spriteOrder;
                    }
                }

                // Furniture
                if (areaRefs.furniture)
                {
                    Transform areaFurniture = areaRefs.furniture;

                    for (int i = 0; i < areaFurniture.childCount; i++)
                    {
                        Selectable furnitureSelectable = areaFurniture.GetChild(i).GetComponent<Selectable>();

                        Furniture furnitureItem = new()
                        {
                            name = areaFurniture.GetChild(i).name,
                            order = furnitureSelectable.spriteOrder
                        };

                        furniture.Add(furnitureItem);
                    }
                }

                // Props
                if (areaRefs.props)
                {
                    Transform areaProps = areaRefs.props;

                    for (int i = 0; i < areaProps.childCount; i++)
                    {
                        Selectable propSelectable = areaProps.GetChild(i).GetComponent<Selectable>();

                        Prop propItem = new()
                        {
                            name = areaProps.GetChild(i).name,
                            order = propSelectable.spriteOrder
                        };

                        props.Add(propItem);
                    }
                }

                // Filth
                if (areaRefs.filth)
                {
                    Transform areaFilth = areaRefs.filth;

                    for (int i = 0; i < areaFilth.childCount; i++)
                    {
                        Transform filthItemTransform = areaFilth.GetChild(i);

                        Filth filthItem = new()
                        {
                            name = filthItemTransform.name,
                            removed = !filthItemTransform.gameObject.activeSelf
                        };

                        filth.Add(filthItem);
                    }
                }
            });

            Area newArea = new()
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
                    ChangeWall changeWall = selectable.GetComponent<ChangeWall>();

                    if (changeWall.isRight)
                    {
                        gameData.areasData[i].wallRightOrder = selectable.spriteOrder;
                    }
                    else
                    {
                        gameData.areasData[i].wallLeftOrder = selectable.spriteOrder;
                    }

                    SaveData();

                    break;
                }

                // Floor
                if (selectable.type == Selectable.Type.Floor && gameData.areasData[i].name == selectable.transform.parent.name)
                {
                    gameData.areasData[i].floorOrder = selectable.spriteOrder;

                    SaveData();

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

                            SaveData();

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

                            SaveData();

                            break;
                        }
                    }

                    break;
                }
            }
        }

        public void SetFilth(Transform filth)
        {
            for (int i = 0; i < gameData.areasData.Count; i++)
            {
                if (gameData.areasData[i].name == filth.parent.parent.name)
                {
                    for (int j = 0; j < gameData.areasData[i].filth.Count; j++)
                    {
                        if (gameData.areasData[i].filth[j].name == filth.name)
                        {
                            gameData.areasData[i].filth[j].removed = true;

                            SaveData();

                            break;
                        }
                    }

                    break;
                }
            }
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
        public void GetWorldItem(string groupId, TaskManager.Task task, Action<Transform> callback)
        {
            Transform worldArea = null;
            AreaRefs areaRefs = null;

            for (int i = 0; i < worldRoot.childCount; i++)
            {
                Transform tempArea = worldRoot.GetChild(i);

                if (tempArea.name == groupId + "Area")
                {
                    worldArea = tempArea;

                    areaRefs = worldArea.GetComponent<AreaRefs>();

                    break;
                }
            }

            if (worldArea != null && areaRefs != null)
            {
                areaRefs.GetReferences(() =>
                {
                    Transform foundWorldItem;

                    switch (task.taskRefType)
                    {
                        case TaskManager.TaskRefType.Last:
                            foundWorldItem = worldArea;
                            break;
                        case TaskManager.TaskRefType.Wall:
                            if (task.isTaskRefRight)
                            {
                                foundWorldItem = areaRefs.wallRight;
                            }
                            else
                            {
                                foundWorldItem = areaRefs.wallLeft;
                            }
                            break;
                        case TaskManager.TaskRefType.Filth:
                            foundWorldItem = areaRefs.filth.Find(task.taskRefName);
                            break;
                        default:
                            // Floor, Furniture, Item and others
                            // TODO - Possibly find Furniture and Item first before getting their children
                            foundWorldItem = worldArea.Find(task.taskRefName == "" ? task.taskRefType.ToString() : task.taskRefName);
                            break;
                    }

                    callback(foundWorldItem);
                });
            }
            else
            {
                callback(null);
            }
        }

        // Fine the position of a room in the world (root)
        public Transform FindRoomInWorld(string roomName)
        {
            Debug.Log(roomName);

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

        // Find selectable items collider center position
        public Vector2 GetColliderCenter(Transform taskRef, TaskManager.TaskRefType taskRefType)
        {
            Vector2 center;

            switch (taskRefType)
            {
                case TaskManager.TaskRefType.Last:
                    center = taskRef.transform.position;
                    break;
                case TaskManager.TaskRefType.Wall:
                    center = taskRef.GetComponent<CompositeCollider2D>().bounds.center;
                    break;
                case TaskManager.TaskRefType.Floor:
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