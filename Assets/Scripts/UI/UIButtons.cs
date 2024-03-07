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
        public float gizmoRadiusHub = 28f;
        [Condition("drawGizmos", true)]
        public float gizmoRadiusHubSmall = 20f;
        [Condition("drawGizmos", true)]
        public float gizmoRadiusGameplay = 0.65f;
        [Condition("drawGizmos", true)]
        public Color gizmoColor = Color.blue;

        private Vector2 gizmosSizeHub;
        private Vector2 gizmosSizeHubSmall;
        private Vector2 gizmosSizeGameplay;
#endif

        [Header("Hub Scene")]
        [ReadOnly]
        public bool hubButtonsSet = false;
        [ReadOnly]
        public Vector2 hubShopButtonPos;
        [ReadOnly]
        public Vector2 hubPlayButtonPos;
        [ReadOnly]
        public Vector2 hubTaskButtonPos;

        [Header("GamePlay Scene")]
        [ReadOnly]
        public bool gamePlayButtonsSet = false;
        [ReadOnly]
        public Vector2 gamePlayHomeButtonPos;
        [ReadOnly]
        public Vector2 gamePlayShopButtonPos;
        [ReadOnly]
        public Vector2 gamePlayTaskButtonPos;
        [ReadOnly]
        public Vector2 gamePlayBonusButtonPos;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (drawGizmos)
            {
                gizmosSizeHub = new Vector2(gizmoRadiusHub, gizmoRadiusHub);
                gizmosSizeHubSmall = new Vector2(gizmoRadiusHubSmall, gizmoRadiusHubSmall);
                gizmosSizeGameplay = new Vector2(gizmoRadiusGameplay, gizmoRadiusGameplay);
            }
        }

        void OnDrawGizmos()
        {
            if (drawGizmos && Application.isPlaying)
            {
                Gizmos.color = gizmoColor;

                if (SceneManager.GetActiveScene().name == Types.Scene.Hub.ToString())
                {
                    Gizmos.DrawWireCube(hubShopButtonPos, gizmosSizeHubSmall);
                    Gizmos.DrawWireCube(hubPlayButtonPos, gizmosSizeHub);
                    Gizmos.DrawWireCube(hubTaskButtonPos, gizmosSizeHub);
                }
                else
                {
                    Gizmos.DrawWireCube(gamePlayShopButtonPos, gizmosSizeGameplay);
                    Gizmos.DrawWireCube(Camera.main.ScreenToWorldPoint(gamePlayTaskButtonPos), gizmosSizeGameplay);
                    Gizmos.DrawWireCube(gamePlayBonusButtonPos, gizmosSizeGameplay);
                }
            }
        }
#endif
    }
}
