using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using CI.QuickSave;
using Newtonsoft.Json;
using Unity.VisualScripting;

namespace Merge
{
    public class WorldDataManager : MonoBehaviour
    {
        public Transform worldRoot;
        public TaskManager taskManager;
        public int roomInHierarchyOffset = 2;

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

        void Start()
        {
            // Cache
            navMeshManager = NavMeshManager.Instance;
            dataManager = DataManager.Instance;
            dataConverter = dataManager.GetComponent<DataConverter>();
            gameData = GameData.Instance;

            Init();
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

        void Init()
        {
            // Check world root
            if (worldRoot == null)
            {
                ErrorManager.Instance.FindUsed("the World Root");

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
                            }
                        }
                    }
                }
            }

            StartCoroutine(BakeNavMeshAfterPhysicsUpdate());

            StartCoroutine(TryCheckingForTasks());
        }

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

        void LoadData()
        {
            if (gameData.areasData.Count == 0)
            {
                string newAreasData = dataManager.LoadValue<string>("areas");

                gameData.areasData = dataConverter.ConvertJsonToArea(newAreasData);

                LogData();
            }
        }

        WorldTypes.Area HandleArea(Transform area)
        {
            bool isRoom;
            bool isLocked;
            int wallLeftOrder = -1;
            int wallRightOrder = -1;
            int floorOrder = -1;
            List<WorldTypes.Furniture> furniture = new();

            RoomHandler roomHandler = area.GetComponent<RoomHandler>();

            if (roomHandler != null)
            {
                isRoom = true;
                isLocked = roomHandler.locked;

                ChangeWall changeWallLeft = area.GetChild(0).GetComponent<ChangeWall>();
                ChangeWall changeWallRight = area.GetChild(1).GetComponent<ChangeWall>();
                ChangeFloor changeFloor = area.GetChild(2).GetComponent<ChangeFloor>();

                if (!changeWallLeft.isOld)
                {
                    wallLeftOrder = changeWallLeft.spriteOrder;
                }

                if (!changeWallRight.isOld)
                {
                    wallRightOrder = changeWallRight.spriteOrder;
                }

                if (!changeFloor.isOld)
                {
                    floorOrder = changeFloor.spriteOrder;
                }

                Transform areaFurniture = area.GetChild(3);

                for (int i = 0; i < areaFurniture.childCount; i++)
                {
                    ChangeFurniture changeFurniture = areaFurniture.GetChild(i).GetComponent<ChangeFurniture>();

                    WorldTypes.Furniture furnitureItem = new()
                    {
                        name = areaFurniture.GetChild(i).name,
                        order = changeFurniture.spriteOrder
                    };

                    furniture.Add(furnitureItem);
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
                furniture = furniture
            };

            return newArea;
        }

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

        IEnumerator BakeNavMeshAfterPhysicsUpdate()
        {
            yield return new WaitForSeconds(1f);

            navMeshManager.Bake();

            loaded = true;
        }

        IEnumerator TryCheckingForTasks()
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
                    if (selectable.name.Contains("Left"))
                    {
                        gameData.areasData[i].wallLeftOrder = selectable.changeWall.spriteOrder;
                    }
                    else
                    {
                        gameData.areasData[i].wallRightOrder = selectable.changeWall.spriteOrder;
                    }

                    break;
                }

                // Floor
                if (selectable.type == Selectable.Type.Floor && gameData.areasData[i].name == selectable.transform.parent.name)
                {
                    gameData.areasData[i].floorOrder = selectable.changeFloor.spriteOrder;

                    break;
                }

                // Furniture
                if (selectable.type == Selectable.Type.Furniture && gameData.areasData[i].name == selectable.transform.parent.transform.parent.name)
                {
                    for (int j = 0; j < gameData.areasData[i].furniture.Count; j++)
                    {
                        if (gameData.areasData[i].furniture[j].name == selectable.name)
                        {
                            gameData.areasData[i].furniture[j].order = selectable.changeFurniture.spriteOrder;

                            break;
                        }
                    }

                    break;
                }
            }

            SaveData();
        }

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
                    case Types.TaskRefType.Floor:
                    default:
                        // Floor, Furniture, Item and others
                        // TODO - Possibly find Furniture and Item first before getting their children
                        return worldArea.Find(task.taskRefName == "" ? task.taskRefType.ToString() : task.taskRefName);
                }
            }

            return null;
        }

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

        public void UnlockRoom(string roomName)
        {
            for (int i = 0; i < gameData.areasData.Count; i++)
            {
                if (gameData.areasData[i].name == roomName)
                {
                    gameData.areasData[i].isLocked = false;
                }
            }

            SaveData();
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