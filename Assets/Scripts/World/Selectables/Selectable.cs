using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Merge
{
    public class Selectable : MonoBehaviour
    {
        // Variables
        public Type type;
        [Header("Selection")]
        public bool canBeSelected = false;
        [Condition("canBeSelected", true, true)]
        public bool notifyCantBeSelected = false;

        [HideInInspector]
        public bool isProp = false;
        [Condition("isProp", true, true)]
        public bool isStatic = false;
        [Condition("isProp", true, true)]
        public bool isInitiallyHidden = false;

        [Header("Taps")]
        public bool canBeTapped = false;
        [Condition("canBeTapped", true)]
        public float charMoveOffset = 0f;

        [Header("States")]
        [SerializeField]
        [ReadOnly]
        public bool isPlaying = false;
        [ReadOnly]
        public bool isOld = true;
        [ReadOnly]
        public int spriteOrder = -1;

        [HideInInspector]
        public string id;

        private Dictionary<Type, IChanger> changers = new();

        // Enums
        public enum Type
        {
            Floor,
            Wall,
            Furniture,
            Prop,
        }

        // References
        private GameData gameData;
        private Animation anim;

        [HideInInspector]
        public ChangeFloor changeFloor;
        [HideInInspector]
        public ChangeWall changeWall;
        [HideInInspector]
        public ChangeFurniture changeFurniture;
        [HideInInspector]
        public ChangeProp changeProp;

        void Start()
        {
            // Cache
            gameData = GameData.Instance;
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

        void OnValidate()
        {
            if (type == Type.Prop)
            {
                isProp = true;
            }

            if (isProp && isStatic)
            {
                isInitiallyHidden = false;
            }

            if (isProp && isInitiallyHidden)
            {
                isStatic = false;
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
                    changers.Add(type, changeFloor);
                    break;

                case Type.Wall:
                    changeWall = GetComponent<ChangeWall>();
                    changers.Add(type, changeFloor);
                    break;

                case Type.Furniture:
                    changeFurniture = GetComponent<ChangeFurniture>();
                    changers.Add(type, changeFloor);
                    break;

                case Type.Prop:
                    if (!isStatic)
                    {
                        changeProp = GetComponent<ChangeProp>();
                        changers.Add(type, changeFloor);
                    }
                    break;
            }
        }

        public Sprite GetSprite(int order)
        {
            string name = Regex.Replace(gameObject.name, $"\\(.*", "") + (order + 2);

            return gameData.GetSprite(name, type);
        }

        public Sprite[] GetSpriteOptions()
        {
            Sprite[] spriteOptions = new Sprite[3];

            for (int i = 0; i < spriteOptions.Length; i++)
            {
                string name = Regex.Replace(gameObject.name, $"\\(.*", "") + "Option" + (i + 1);

                spriteOptions[i] = gameData.GetSprite(name, type, true);
            }

            return spriteOptions;
        }

        public void SetSprites(int order)
        {
            GetChanger().SetSprites(order);
        }

        public void CancelSpriteChange(int order)
        {
            GetChanger().Cancel(order);
        }

        public void ConfirmSpriteChange()
        {
            canBeSelected = true;

            GetChanger().Confirm();
        }

        public void Select(bool select = true)
        {
            GetChanger().Select(select);
        }

        IChanger GetChanger()
        {
            if (changers.TryGetValue(type, out var changer))
            {
                return changer;
            }

            return null;
        }

        public bool Tapped(float selectSpeed = 1.8f)
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

    public interface IChanger
    {
        void Select(bool select = true);

        void SetSprites(int order, bool alt = false);

        void Cancel(int order);

        void Confirm();
    }
}