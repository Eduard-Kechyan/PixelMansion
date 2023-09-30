using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using CI.QuickSave;
using Newtonsoft.Json;

namespace Merge
{
    public class WorldDataManager : MonoBehaviour
    {
        public Transform worldRoot;
        public int roomInHierarchyOffset = 2;

        public bool canLog = false;

        public bool saveData = false;
        public bool loadData = false;
        public bool clearData = false;

        private List<Area> areas = new List<Area>();
        private List<Area> loadedAreas = new List<Area>();
        private bool initial = true;

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
        }

        [Serializable]
        public class Furniture
        {
            public string name;
            public int order;
        }

        // References
        private NavMeshManager navMeshManager;

        // Quick Save
        private QuickSaveSettings saveSettings;
        public QuickSaveWriter writer;
        public QuickSaveReader reader;

        void Start()
        {
            navMeshManager = NavMeshManager.Instance;

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

            // Set up the writer
            saveSettings = new QuickSaveSettings() { CompressionMode = CompressionMode.None }; // Chage compression mode from None to Gzip on release
            writer = QuickSaveWriter.Create("Areas", saveSettings);

            if (PlayerPrefs.HasKey("areaSet") && writer.Exists("areaSet"))
            {
                // Set up the reader
                reader = QuickSaveReader.Create("Areas", saveSettings);

                initial = false;
            }

            GetDataFromRoot();

            if (!initial)
            {
                SetDataToRoot();
            }

            if (initial)
            {
                initial = false;
            }
        }

        void GetDataFromRoot(bool alt = false)
        {
            areas = new();

            for (int i = 0; i < worldRoot.childCount; i++)
            {
                Transform worldItem = worldRoot.GetChild(i);

                if (worldItem.name.Contains("Area"))
                {
                    areas.Add(HandleArea(worldItem));
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

            for (int i = 0; i < loadedAreas.Count; i++)
            {
                for (int j = 0; j < worldRoot.childCount; j++)
                {
                    Transform worldArea = worldRoot.GetChild(j);

                    if (worldArea.name == loadedAreas[i].name)
                    {
                        // Rooms
                        if (loadedAreas[i].isRoom)
                        {
                            // Wall left
                            if (loadedAreas[i].wallLeftOrder >= 0 && worldArea.GetChild(0).TryGetComponent(out ChangeWall changeWallLeft))
                            {
                                changeWallLeft.SetSprites(loadedAreas[i].wallLeftOrder, true);
                            }

                            // Wall right
                            if (loadedAreas[i].wallRightOrder >= 0 && worldArea.GetChild(1).TryGetComponent(out ChangeWall changeWallRight))
                            {
                                changeWallRight.SetSprites(loadedAreas[i].wallRightOrder, true);
                            }

                            // Floor
                            if (loadedAreas[i].floorOrder >= 0 && worldArea.GetChild(2).TryGetComponent(out ChangeFloor changeFloor))
                            {
                                changeFloor.SetSprites(loadedAreas[i].floorOrder, true);
                            }

                            // Furniture
                            for (int k = 0; k < loadedAreas[i].furniture.Count; k++)
                            {
                                Transform furniture = worldArea.GetChild(3).GetChild(k);

                                if (furniture.name == loadedAreas[i].furniture[k].name)
                                {
                                    if (loadedAreas[i].furniture[k].order >= 0 && furniture.TryGetComponent(out ChangeFurniture changeFurniture))
                                    {
                                        changeFurniture.SetSprites(loadedAreas[i].furniture[k].order, true);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            areas = loadedAreas;

            StartCoroutine(BakeNavMeshAfterPhysicsUpdate());
        }

        void SaveData()
        {
            if (initial)
            {
                writer
                .Write("areaSet", true)
                .Write("areas", ConvertAreaToJson(areas))
                .Commit();

                PlayerPrefs.SetInt("areaSet", 1);
                PlayerPrefs.Save();
            }
            else
            {
                writer
                .Write("areas", ConvertAreaToJson(areas))
                .Commit();
            }
        }

        void LoadData()
        {
            string newAreasData = "";

            reader.Read<string>("areas", r => newAreasData = r);

            loadedAreas = ConvertJsonToArea(newAreasData);

            LogData();
        }

        Area HandleArea(Transform area)
        {
            bool isRoom;
            bool isLocked;
            int wallLeftOrder = -1;
            int wallRightOrder = -1;
            int floorOrder = -1;
            List<Furniture> furniture = new List<Furniture>();

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

                    Furniture furnitureItem = new Furniture
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
                isLocked = false; // TODO - Check this here
            }

            Area newArea = new Area
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
                for (int i = 0; i < areas.Count; i++)
                {
                    if (areas[i].isRoom)
                    {
                        Debug.Log(areas[i].name + ", Is Room, " + areas[i].wallLeftOrder + ", " + areas[i].wallRightOrder + ", " + areas[i].floorOrder);
                    }
                    else
                    {
                        Debug.Log(areas[i].name);
                    }

                    for (int j = 0; j < areas[i].furniture.Count; j++)
                    {
                        Debug.Log("      " + areas[i].furniture[j].name + ", " + areas[i].furniture[j].order);
                    }
                }
            }
        }

        IEnumerator BakeNavMeshAfterPhysicsUpdate()
        {
            yield return new WaitForSeconds(1f);

            navMeshManager.Bake();
        }

        // Public methods
        public void SetSelectable(Selectable selectable)
        {
            for (int i = 0; i < areas.Count; i++)
            {
                // Walls
                if (selectable.type == Selectable.Type.Wall && areas[i].name == selectable.transform.parent.name)
                {
                    if (selectable.name.Contains("Left"))
                    {
                        areas[i].wallLeftOrder = selectable.changeWall.spriteOrder;
                    }
                    else
                    {
                        areas[i].wallRightOrder = selectable.changeWall.spriteOrder;
                    }

                    break;
                }

                // Floor
                if (selectable.type == Selectable.Type.Floor && areas[i].name == selectable.transform.parent.name)
                {
                    areas[i].floorOrder = selectable.changeFloor.spriteOrder;

                    break;
                }

                // Furniture
                if (selectable.type == Selectable.Type.Furniture && areas[i].name == selectable.transform.parent.transform.parent.name)
                {
                    for (int j = 0; j < areas[i].furniture.Count; j++)
                    {
                        if (areas[i].furniture[j].name == selectable.name)
                        {
                            areas[i].furniture[j].order = selectable.changeFurniture.spriteOrder;

                            break;
                        }
                    }

                    break;
                }
            }

            SaveData();
        }

        public GameObject GetWorldItem(Types.Task task)
        {
            Transform worldArea = null;

            for (int i = 0; i < worldRoot.childCount; i++)
            {
                Transform tempArea = worldRoot.GetChild(i);

                if (tempArea.name == task.groupId +"Area")
                {
                    worldArea = tempArea;
                }
            }

            if (worldArea != null)
            {
                switch (task.taskRefType)
                {
                    case Types.TaskRefType.Area:
                        return worldArea.gameObject;
                    case Types.TaskRefType.Wall:
                        if (worldArea.GetChild(task.isTaskRefRight ? 1 : 0).TryGetComponent(out ChangeWall changeWallRight))
                        {
                            return changeWallRight.gameObject;
                        }
                        break;
                    case Types.TaskRefType.Floor:
                        if (worldArea.GetChild(2).TryGetComponent(out ChangeFloor changeFloor))
                        {
                            return changeFloor.gameObject;
                        }
                        break;
                    case Types.TaskRefType.Furniture:
                        for (int i = 0; i < worldArea.GetChild(3).childCount; i++)
                        {
                            Transform furniture = worldArea.GetChild(3).GetChild(i);

                            if (furniture.name == task.taskRefName)
                            {
                                if (furniture.TryGetComponent(out ChangeFurniture changeFurniture))
                                {
                                    return changeFurniture.gameObject;
                                }

                                break;
                            }
                        }
                        break;
                    case Types.TaskRefType.Item:
                        Debug.LogWarning("Types.StepRefType.Item not set in WorldDataManager.cs");
                        return null;
                }
            }

            return null;
        }

        // Conversion
        string ConvertAreaToJson(List<Area> areasData)
        {
            AreaJson[] areaJson = new AreaJson[areasData.Count];

            for (int i = 0; i < areasData.Count; i++)
            {
                AreaJson newAreaJson = new AreaJson
                {
                    name = areasData[i].name,
                    isLocked = areasData[i].isLocked,
                    isRoom = areasData[i].isRoom,
                    wallLeftOrder = areasData[i].wallLeftOrder,
                    wallRightOrder = areasData[i].wallRightOrder,
                    floorOrder = areasData[i].floorOrder,
                    furniture = JsonConvert.SerializeObject(areasData[i].furniture),
                };

                areaJson[i] = newAreaJson;
            }

            return JsonConvert.SerializeObject(areaJson);
        }

        List<Area> ConvertJsonToArea(string areasString)
        {
            AreaJson[] areaJson = JsonConvert.DeserializeObject<AreaJson[]>(areasString);

            List<Area> areasData = new List<Area>();

            for (int i = 0; i < areaJson.Length; i++)
            {
                Area newAreasData = new Area
                {
                    name = areaJson[i].name,
                    isLocked = areaJson[i].isLocked,
                    isRoom = areaJson[i].isRoom,
                    wallLeftOrder = areaJson[i].wallLeftOrder,
                    wallRightOrder = areaJson[i].wallRightOrder,
                    floorOrder = areaJson[i].floorOrder,
                    furniture = JsonConvert.DeserializeObject<List<Furniture>>(areaJson[i].furniture),
                };

                areasData.Add(newAreasData);
            }

            return areasData;
        }
    }
}