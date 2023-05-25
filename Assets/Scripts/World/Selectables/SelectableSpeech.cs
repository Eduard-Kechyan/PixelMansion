using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableSpeech : MonoBehaviour
{
    // Variables
    [Condition("canBeTapped", true)]
    public float speechTimeOut = 5f;
    [Condition("canBeTapped", true)]
    public string speechOld;
    [Condition("canBeTapped", true)]
    public string speechNew1;
    [Condition("canBeTapped", true)]
    public string speechNew2;
    [Condition("canBeTapped", true)]
    public string speechNew3;

    [Header("States")]
    [SerializeField]
    [ReadOnly]
    private float speechTimeOutInner;

    // References
    private Selectable selectable;

    void Start()
    {
        selectable = GetComponent<Selectable>();
    }

    public string GetSpeech()
    {
        bool canBeTapped = selectable.canBeTapped;
        int spriteOrder = selectable.GetSprites();

        if (canBeTapped && speechTimeOutInner <= 0)
        {
            if (spriteOrder == -1 && speechOld != "")
            {
                speechTimeOutInner = speechTimeOut;

                return speechOld;
            }
            if (spriteOrder == 0 && speechNew1 != "")
            {
                speechTimeOutInner = speechTimeOut;

                return speechNew1;
            }
            if (spriteOrder == 1 && speechNew2 != "")
            {
                speechTimeOutInner = speechTimeOut;

                return speechNew2;
            }
            if (spriteOrder == 2 && speechNew3 != "")
            {
                speechTimeOutInner = speechTimeOut;

                return speechNew3;
            }
            else
            {
                return "";
            }
        }
        else
        {
            return "";
        }
    }
}
