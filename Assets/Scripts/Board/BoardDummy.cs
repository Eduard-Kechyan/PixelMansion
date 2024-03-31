using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class BoardDummy : MonoBehaviour
    {
        // Variables

        // References
        private GameData gameData;

        // UI
        private VisualElement root;
        private VisualElement board;

        void Start()
        {
            // Cache
            gameData = GameData.Instance;

            // UI
            root = GameRefs.Instance.mergeUIDoc.rootVisualElement;

            board = root.Q<VisualElement>("Board");
        }
    }
}
