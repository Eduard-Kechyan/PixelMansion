using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleTapManager : MonoBehaviour
{
    private BoardInteractions interactions;

    void Start()
    {
        // Cache boardInteractions
        interactions = GetComponent<BoardInteractions>();
    }

    public void DoubleTapped()
    {
        if (
            interactions.currentItem.type == Item.Type.Gen
            && interactions.currentItem.generates.Length > 0
        )
        {
            Types.Generates[] generates = interactions.currentItem.generates;
            //int randomNumber = Random.Range(0, interactions.currentItem.generates.Length - 1);

            for (int i = 0; i < generates.Length; i++)
            {
                Debug.Log(generates[0].chance / 100);
                Debug.Log(50f / 100);
                /*if (Random.value > 0.5f) {

                }*/
            }
        }
    }
}
