
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class CharRoam : MonoBehaviour
    {
        // Variables
        public bool canRoam = true;
        public float checkRoomInterval = 3f;
        public Vector2 mainRoomCheckSize = new Vector2(80, 280);
        public Vector2 secondaryRoomCheckSize = new Vector2(60, 120);
        [ReadOnly]
        public bool isRoaming = false;

        private float actualTime;

        // References
        private CharMove charMove;
        private CharOrderSetter charOrderSetter;
        private Camera cam;

        void Start()
        {
            // Cache
            cam = Camera.main;
            charMove = GetComponent<CharMove>();
            charOrderSetter = GetComponent<CharOrderSetter>();

            // Initialize
            actualTime = checkRoomInterval;
        }

        void Update()
        {
            actualTime -= Time.deltaTime;

            if (actualTime <= 0)
            {
                actualTime = checkRoomInterval;

                Roam();
            }
        }

        void OnDrawGizmosSelected()
        {
            if(Application.isPlaying)
            {
                Gizmos.color = Color.red;

                Gizmos.DrawWireCube(GetCameraCenterPos(), mainRoomCheckSize);

                Gizmos.color = Color.blue;

                Gizmos.DrawWireCube(GetCameraCenterPos(), secondaryRoomCheckSize);
            }
            else
            {
                Gizmos.color = Color.red;

                Gizmos.DrawWireCube(Vector2.zero, mainRoomCheckSize);

                Gizmos.color = Color.blue;

                Gizmos.DrawWireCube(Vector2.zero, secondaryRoomCheckSize);
            }
        }

        void Roam()
        {
            if (canRoam && charMove.canWalk && !charMove.isWalking)
            {
                if (!isRoaming)
                {
                    isRoaming = true;

                    CheckForRoom();
                }
            }
            else
            {
                isRoaming = false;
            }
        }

        void CheckForRoom()
        {
            // Check if the room the main character is in, is on the screen
            List<RaycastHit2D> hits = new();

            ContactFilter2D contactFilter2D = new();

            contactFilter2D.SetLayerMask(LayerMask.GetMask("Room"));

            // Find the rooms on the screen
            Physics2D.BoxCast(GetCameraCenterPos(), mainRoomCheckSize, 0, Vector2.zero, contactFilter2D, hits, Mathf.Infinity);

            if (hits.Count > 0 && (hits[0] || hits[0].collider != null))
            {
                bool isIn = false;

                // Check if the main character is in one of the rooms
                for (int i = 0; i < hits.Count; i++)
                {
                    if (hits[i].transform.name == charOrderSetter.currentRoomName)
                    {
                        isIn = true;

                        break;
                    }
                }

                if (!isIn)
                {
                    TryRoaming();
                }
            }
        }

        void TryRoaming()
        {
            // Check if there is just one room in the center
            List<RaycastHit2D> hits = new();

            ContactFilter2D contactFilter2D = new();

            contactFilter2D.SetLayerMask(LayerMask.GetMask("Room"));

            // Find the rooms in the center of the screen
            Physics2D.BoxCast(GetCameraCenterPos(), secondaryRoomCheckSize, 0, Vector2.zero, contactFilter2D, hits, Mathf.Infinity);

            if (hits.Count == 1 && (hits[0] || hits[0].collider != null))
            {
                isRoaming = true;

                charMove.SetDestination(hits[0].collider.bounds.center, false, true, () =>
                {
                    StopRoaming();
                });
            }
        }

        public void StopRoaming()
        {
            isRoaming = false;
        }

        Vector2 GetCameraCenterPos()
        {
            return Camera.main.ScreenToWorldPoint(new Vector2(Screen.width / 2, Screen.height / 2));
        }
    }
}
