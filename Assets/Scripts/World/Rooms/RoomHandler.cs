using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomHandler : MonoBehaviour
{
    public bool locked = true;
    public float unlockSpeed = 3f;
    public Color lockOverlayColor;

    [Header("Debug")]
    public bool debugOn = false;
    [Condition("debugOn", true)]
    public bool unlock = false;

    void Start()
    {
        if (locked)
        {
            Lock();
        }
    }

    void OnValidate()
    {
        if (debugOn && unlock)
        {
            // Debug
            if (locked)
            {
                Unlock();
            }
            else
            {
                Lock();
            }

            unlock = false;
        }
    }

    void Lock()
    {
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        Debug.Log(spriteRenderers.Length);

        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            // renderer.color = Color.Lerp(renderer.color, lockOverlayColor, unlockSpeed * Time.deltaTime);

            renderer.color += Color.red;
        }

        locked = true;
    }

    void Unlock()
    {
        if (locked)
        {
            SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

            foreach (SpriteRenderer renderer in spriteRenderers)
            {
                // renderer.color = Color.Lerp(renderer.color, lockOverlayColor, unlockSpeed * Time.deltaTime);

                renderer.color -= lockOverlayColor;
            }
        }

        locked = false;
    }
}
