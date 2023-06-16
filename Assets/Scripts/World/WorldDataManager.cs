using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using CI.QuickSave;
using Newtonsoft.Json;

public class WorldDataManager : MonoBehaviour
{
    public Transform worldRoot;
    public int roomInHierarchyOffset = 2;

    private List<Area> areas = new List<Area>();
    private List<Area> loadedAreas = new List<Area>();
    private bool initial = true;

    public bool saveAreaData = false;
    public bool loadAreaData = false;
    public bool logAreaData = false;

    public bool clearAreaData = false;

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

    // Quick Save
    private QuickSaveSettings saveSettings;
    public QuickSaveWriter writer;
    public QuickSaveReader reader;

    void Start()
    {
        if (worldRoot == null)
        {
            ErrorManager.Instance.FindUsed("the World Root");

            worldRoot = GameObject.Find("World").transform.Find("Root");
        }

        InitData();

        Load();
    }

    void OnValidate()
    {
        if (saveAreaData)
        {
            saveAreaData = false;

            InitData();

            GetInitialData();
        }

        if (loadAreaData)
        {
            loadAreaData = false;

            InitData();

            LoadAreaData();
        }

        if (clearAreaData)
        {
            clearAreaData = false;

            string folderPath = Application.persistentDataPath + "/QuickSave/Areas.json";

            if (File.Exists(folderPath))
            {
                File.Delete(folderPath);
            }

            PlayerPrefs.DeleteKey("AreaLoaded");
        }
    }

    void InitData()
    {
        saveSettings = new QuickSaveSettings() { CompressionMode = CompressionMode.None };
        writer = QuickSaveWriter.Create("Areas", saveSettings);

        if (PlayerPrefs.HasKey("AreaLoaded") && writer.Exists("areaSet"))
        {
            reader = QuickSaveReader.Create("Areas", saveSettings);

            initial = false;
        }
    }

    void GetInitialData()
    {
        areas = new List<Area>();

        for (int i = 0; i < worldRoot.childCount; i++)
        {
            Transform worldItem = worldRoot.GetChild(i);

            if (worldItem.name.Contains("Area"))
            {
                HandleArea(worldItem);
            }
        }

        LogAreaData();

        SaveAreaData();
    }

    void HandleArea(Transform area)
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

        areas.Add(newArea);
    }

    void LogAreaData()
    {
        if (logAreaData)
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

    void SaveAreaData()
    {
        writer
        .Write("areaSet", true)
       .Write("areas", ConvertAreaToJson(areas))
        .Commit();

        if (initial)
        {
            PlayerPrefs.SetInt("AreaLoaded", 1);
        }
    }

    void LoadAreaData()
    {
        string newAreasData = "";

        reader.Read<string>("areas", r => newAreasData = r);

        loadedAreas = ConvertJsonToArea(newAreasData);

        LogAreaData();
    }

    void Load()
    {
        GetInitialData();

        LoadAreaData();

        for (int i = 0; i < areas.Count; i++)
        {
            for (int g = 0; g < loadedAreas.Count; g++)
            {
                if (areas[i].name == loadedAreas[g].name)
                {
                    if (areas[i].isRoom)
                    {
                        if (areas[i].wallLeftOrder != loadedAreas[g].wallLeftOrder)
                        {
                            worldRoot.GetChild(i + roomInHierarchyOffset).GetChild(0).GetComponent<ChangeWall>().SetSprites(loadedAreas[g].wallLeftOrder);
                        }

                        if (areas[i].wallRightOrder != loadedAreas[g].wallRightOrder)
                        {
                            worldRoot.GetChild(i + roomInHierarchyOffset).GetChild(1).GetComponent<ChangeWall>().SetSprites(loadedAreas[g].wallRightOrder);
                        }

                        if (areas[i].floorOrder != loadedAreas[g].floorOrder)
                        {
                            worldRoot.GetChild(i + roomInHierarchyOffset).GetChild(2).GetComponent<ChangeWall>().SetSprites(loadedAreas[g].floorOrder);
                        }

                        for (int j = 0; j < areas[i].furniture.Count; j++)
                        {
                            for (int h = 0; h < loadedAreas[g].furniture.Count; h++)
                            {
                                if (areas[i].furniture[j].name != loadedAreas[g].furniture[h].name)
                                {
                                    worldRoot.GetChild(i + roomInHierarchyOffset).GetChild(3).GetChild(h).GetComponent<ChangeFurniture>().SetSprites(loadedAreas[g].furniture[h].order);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

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
