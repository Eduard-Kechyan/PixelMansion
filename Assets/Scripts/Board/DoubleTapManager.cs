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
            interactions.currentItem.type == Types.Type.Gen
            && interactions.currentItem.creates.Length > 0
        )
        {
            Debug.Log("AAA");
            Types.Creates[] creates = interactions.currentItem.creates;
            //int randomNumber = Random.Range(0, interactions.currentItem.creates.Length - 1);

            for (int i = 0; i < creates.Length; i++)
            {
                Debug.Log(creates[0].chance / 100);
                Debug.Log(50f / 100);
                /*if (Random.value > 0.5f) {

                }*/
            }
        }
    }
}
