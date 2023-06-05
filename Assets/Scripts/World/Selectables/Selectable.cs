using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    // Variables
    public Type type;
    [Header("Selection")]
    public bool canBeSelected = false;
    [Condition("canBeSelected", true)]
    public float selectSpeed = 1.8f;

    [Header("Taps")]
    public bool canBeTapped = false;

    [Header("States")]
    [SerializeField]
    [ReadOnly]
    public bool isPlaying = false;

    [HideInInspector]
    public string id;
    [HideInInspector]
    public int order = 0;

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

    private ChangeFloor changerFloor;
    private ChangeWall changerWall;
    private ChangeFurniture changeFurniture;

    void Start()
    {
        // Cache
        anim = GetComponent<Animation>();

        Initialize();
    }

    void Update()
    {
        if (isPlaying && !anim.isPlaying)
        {
            isPlaying = false;
        }
    }

    void Initialize()
    {
        // Gererate random id for comparison
        Guid guid = Guid.NewGuid();

        id = guid.ToString();

        // Get the correct changer
        switch (type)
        {
            case Type.Floor:
                changerFloor = GetComponent<ChangeFloor>();
                break;

            case Type.Wall:
                changerWall = GetComponent<ChangeWall>();
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
                isOld = changerFloor.isOld;
                break;

            case Type.Wall:
                isOld = changerWall.isOld;
                break;

            case Type.Furniture:
                isOld = changeFurniture.isOld;
                break;
        }

        return isOld;
    }

    public int GetSprites()
    {
        int order = 0;

        switch (type)
        {
            case Type.Floor:
                order = changerFloor.spriteOrder;
                break;

            case Type.Wall:
                order = changerWall.spriteOrder;
                break;

            case Type.Furniture:
                order = changeFurniture.spriteOrder;
                break;
        }

        return order;
    }

    public Sprite[] GetSpriteOptions()
    {
        Sprite[] sprites = new Sprite[0];

        switch (type)
        {
            case Type.Floor:
                sprites = changerFloor.optionSprites;
                break;

            case Type.Wall:
                sprites = changerWall.optionSprites;
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
                changerFloor.SetSprites(order);
                break;

            case Type.Wall:
                changerWall.SetSprites(order);
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
                changerFloor.Cancel(order);
                break;

            case Type.Wall:
                changerWall.Cancel(order);
                break;

            case Type.Furniture:
                changeFurniture.Cancel(order);
                break;
        }
    }

    public void ConfirmSpriteChange(int order)
    {
        switch (type)
        {
            case Type.Floor:
                changerFloor.Confirm();
                break;

            case Type.Wall:
                changerWall.Confirm();
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
                changerFloor.Select();
                break;

            case Type.Wall:
                changerWall.Select();
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
                changerFloor.Unselect();
                break;

            case Type.Wall:
                changerWall.Unselect();
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

            return true;
        }

        return false;
    }
}
