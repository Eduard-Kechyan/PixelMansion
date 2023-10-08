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
        public float gizmoRadiusGameplay = 0.65f;
        [Condition("drawGizmos", true)]
        public Color gizmoColor = Color.blue;
        private Vector2 gizmosSizeHub;
        private Vector2 gizmosSizeGameplay;
#endif

        [Header("Hub Scene")]
        [ReadOnly]
        public bool hubButtonsSet = false;
        [ReadOnly]
        public Vector2 hubPlayButtonPos;
        [ReadOnly]
        public Vector2 hubTaskButtonPos;

        [Header("Gameplay Scene")]
        [ReadOnly]
        public bool gameplayButtonsSet = false;
        [ReadOnly]
        public Vector2 gameplayBonusButtonPos;
        [ReadOnly]
        public Vector2 gameplayBonusButtonScreenPos;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (drawGizmos)
            {
                gizmosSizeHub = new Vector2(gizmoRadiusHub, gizmoRadiusHub);
                gizmosSizeGameplay = new Vector2(gizmoRadiusGameplay, gizmoRadiusGameplay);
            }
        }

        void OnDrawGizmos()
        {
            if (drawGizmos && Application.isPlaying)
            {
                Gizmos.color = gizmoColor;

                if (SceneManager.GetActiveScene().name == "Hub")
                {
                    Gizmos.DrawWireCube(hubPlayButtonPos, gizmosSizeHub);
                    Gizmos.DrawWireCube(hubTaskButtonPos, gizmosSizeHub);
                }else{
                    Gizmos.DrawWireCube(gameplayBonusButtonPos, gizmosSizeGameplay);
                }
            }
        }
#endif
    }
}
