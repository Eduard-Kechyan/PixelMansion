using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Merge
{
    public class UIButtons : MonoBehaviour
    {
        // Variables
#if UNITY_EDITOR
        [Header("Debug")]
        public bool drawGizmos = false;
        [Condition("drawGizmos", true)]
        public float gizmoRadiusWorld = 28f;
        [Condition("drawGizmos", true)]
        public float gizmoRadiusWorldSmall = 20f;
        [Condition("drawGizmos", true)]
        public float gizmoRadiusMerge = 0.65f;
        [Condition("drawGizmos", true)]
        public Color gizmoColor = Color.blue;

        private Vector2 gizmosSizeWorld;
        private Vector2 gizmosSizeWorldSmall;
        private Vector2 gizmosSizeMerge;
#endif

        [Header("World Scene")]
        [ReadOnly]
        public bool worldButtonsSet = false;
        [ReadOnly]
        public Vector2 worldShopButtonPos;
        [ReadOnly]
        public Vector2 worldPlayButtonPos;
        [ReadOnly]
        public Vector2 worldTaskButtonPos;

        [Header("Merge Scene")]
        [ReadOnly]
        public bool mergeButtonsSet = false;
        [ReadOnly]
        public Vector2 mergeHomeButtonPos;
        [ReadOnly]
        public Vector2 mergeShopButtonPos;
        [ReadOnly]
        public Vector2 mergeTaskButtonPos;
        [ReadOnly]
        public Vector2 mergeBonusButtonPos;

        // Buttons
        public enum Button
        {
            None,
            Play,
            Home,
            Task,
            TaskMenu,
            Shop,
            Bonus,
            Settings,
            Inventory,
        };

#if UNITY_EDITOR
        void OnValidate()
        {
            if (drawGizmos)
            {
                gizmosSizeWorld = new Vector2(gizmoRadiusWorld, gizmoRadiusWorld);
                gizmosSizeWorldSmall = new Vector2(gizmoRadiusWorldSmall, gizmoRadiusWorldSmall);
                gizmosSizeMerge = new Vector2(gizmoRadiusMerge, gizmoRadiusMerge);
            }
        }

        void OnDrawGizmos()
        {
            if (drawGizmos && Application.isPlaying)
            {
                Gizmos.color = gizmoColor;

                if (SceneManager.GetActiveScene().name == SceneLoader.SceneType.World.ToString())
                {
                    Gizmos.DrawWireCube(worldShopButtonPos, gizmosSizeWorldSmall);
                    Gizmos.DrawWireCube(worldPlayButtonPos, gizmosSizeWorld);
                    Gizmos.DrawWireCube(worldTaskButtonPos, gizmosSizeWorld);
                }
                else
                {
                    Gizmos.DrawWireCube(mergeShopButtonPos, gizmosSizeMerge);
                    Gizmos.DrawWireCube(Camera.main.ScreenToWorldPoint(mergeTaskButtonPos), gizmosSizeMerge);
                    Gizmos.DrawWireCube(mergeBonusButtonPos, gizmosSizeMerge);
                }
            }
        }
#endif
    }
}
