using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class WorldTypes : MonoBehaviour
    {
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
            public List<string> dirt;
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
            public string dirt;
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
    }
}
