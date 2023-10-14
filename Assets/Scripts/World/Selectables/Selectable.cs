using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class Selectable : MonoBehaviour
    {
        // Variables
        public Type type;
        [Header("Selection")]
        public bool canBeSelected = false;
        [Condition("canBeSelected", true)]
        public float selectSpeed = 1.8f;
        [Condition("canBeSelected", true, true)]
        public bool notifyCantBeSelected = false;

        [Header("Taps")]
        public bool canBeTapped = false;
        [Condition("canBeTapped", true)]
        public float charMoveOffset = 0f;

        [Header("States")]
        [SerializeField]
        [ReadOnly]
        public bool isPlaying = false;

        [HideInInspector]
        public string id;

        // Enums
        public enum Type
        {
            Floor,
            Wall,
            Furniture,
            Item,
        }

        // References
        private Animation anim;

        [HideInInspector]
        public ChangeFloor changeFloor;
        [HideInInspector]
        public ChangeWall changeWall;
        [HideInInspector]
        public ChangeFurniture changeFurniture;

        void Start()
        {
            // Cache
            anim = GetComponent<Animation>();

            Initialize();

            enabled = false;
        }

        void Update()
        {
            if (isPlaying && !anim.IsPlaying("SelectableSelect"))
            {
                isPlaying = false;

                enabled = false;
            }
        }

        void Initialize()
        {
            // Generate random id for comparison
            Guid guid = Guid.NewGuid();

            id = guid.ToString();

            // Get the correct changer
            switch (type)
            {
                case Type.Floor:
                    changeFloor = GetComponent<ChangeFloor>();
                    break;

                case Type.Wall:
                    changeWall = GetComponent<ChangeWall>();
                    break;

                case Type.Furniture:
                    changeFurniture = GetComponent<ChangeFurniture>();
                    break;
            }
        }

        public bool GetOld()
        {
            bool isOld = true;

            switch (type)
            {
                case Type.Floor:
                    isOld = changeFloor.isOld;
                    break;

                case Type.Wall:
                    isOld = changeWall.isOld;
                    break;

                case Type.Furniture:
                    isOld = changeFurniture.isOld;
                    break;
            }

            return isOld;
        }

        public int GetSpriteOrder()
        {
            switch (type)
            {
                case Type.Floor:
                    return changeFloor.spriteOrder;

                case Type.Wall:
                    return changeWall.spriteOrder;

                default: // Type.Furniture
                    return changeFurniture.spriteOrder;
            }
        }

        public Sprite[] GetSpriteOptions()
        {
            Sprite[] sprites = new Sprite[0];

            switch (type)
            {
                case Type.Floor:
                    sprites = changeFloor.optionSprites;
                    break;

                case Type.Wall:
                    sprites = changeWall.optionSprites;
                    break;

                case Type.Furniture:
                    sprites = changeFurniture.optionSprites;
                    break;
            }

            return sprites;
        }

        public void SetSprites(int order)
        {
            switch (type)
            {
                case Type.Floor:
                    changeFloor.SetSprites(order);
                    break;

                case Type.Wall:
                    changeWall.SetSprites(order);
                    break;

                case Type.Furniture:
                    changeFurniture.SetSprites(order);
                    break;
            }
        }

        public void CancelSpriteChange(int order)
        {
            switch (type)
            {
                case Type.Floor:
                    changeFloor.Cancel(order);
                    break;

                case Type.Wall:
                    changeWall.Cancel(order);
                    break;

                case Type.Furniture:
                    changeFurniture.Cancel(order);
                    break;
            }
        }

        public void ConfirmSpriteChange(int order)
        {
            canBeSelected = true;

            switch (type)
            {
                case Type.Floor:
                    changeFloor.Confirm();
                    break;

                case Type.Wall:
                    changeWall.Confirm();
                    break;

                case Type.Furniture:
                    changeFurniture.Confirm();
                    break;
            }
        }

        public void Select()
        {
            switch (type)
            {
                case Type.Floor:
                    changeFloor.Select();
                    break;

                case Type.Wall:
                    changeWall.Select();
                    break;

                case Type.Furniture:
                    changeFurniture.Select();
                    break;
            }
        }

        public void Unselect()
        {
            switch (type)
            {
                case Type.Floor:
                    changeFloor.Unselect();
                    break;

                case Type.Wall:
                    changeWall.Unselect();
                    break;

                case Type.Furniture:
                    changeFurniture.Unselect();
                    break;
            }
        }

        public bool Tapped()
        {
            if (canBeTapped && anim != null)
            {
                anim["SelectableSelect"].speed = selectSpeed;
                anim.Play("SelectableSelect");

                isPlaying = true;

                enabled = true;

                return true;
            }

            return false;
        }
    }
}