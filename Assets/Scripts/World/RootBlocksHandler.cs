using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class RootBlocksHandler : MonoBehaviour
    {
        // Variables
        public bool fill = false;
        public bool randomize = false;
        public bool clear = false;
        public bool log = false;
        public Transform rootBlocks;
        public GameObject rootGrassPrefab;
        public Sprite[] grassSprites = new Sprite[0];

        private GameObject blocksHolder;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (fill)
            {
                fill = false;

                Glob.Validate(() =>
                {
                    Fill();
                }, this);
            }

            if (randomize)
            {
                randomize = false;

                Glob.Validate(() =>
                {
                    Randomize();
                }, this);
            }

            if (clear)
            {
                clear = false;

                Glob.Validate(() =>
                {
                    Clear();
                }, this);
            }
        }
#endif
        void Fill()
        {
            SpriteRenderer rootSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            SpriteRenderer prefabSpriteRenderer = rootGrassPrefab.GetComponent<SpriteRenderer>();

            int rootWidth = (int)rootSpriteRenderer.sprite.rect.width;
            int rootHeight = (int)rootSpriteRenderer.sprite.rect.height;
            int prefabWidth = (int)prefabSpriteRenderer.sprite.rect.width;
            int prefabHeight = (int)prefabSpriteRenderer.sprite.rect.height;

            int horizontalCount = Mathf.CeilToInt(rootWidth / prefabWidth) + 1;
            int verticalCount = Mathf.CeilToInt(rootHeight / prefabHeight) + 2;

            if (log)
            {
                Debug.Log(horizontalCount);
                Debug.Log(verticalCount);
            }

            Clear(true);

            // Instantiate new root blocks
            for (int i = 0; i < verticalCount; i++)
            {
                for (int j = 0; j < horizontalCount; j++)
                {
                    Vector3 newPosition = new Vector3(j * prefabWidth - (rootWidth / 2), i * prefabHeight - (rootHeight / 2), 0);

                    CreateNewBlock(newPosition, i);
                }
            }

            for (int i = 0; i < verticalCount - 1; i++) // Notice the - 1
            {
                for (int j = 0; j < horizontalCount; j++)
                {
                    Vector3 newPosition = new Vector3(j * prefabWidth + (prefabWidth / 2) - (rootWidth / 2), i * prefabHeight + (prefabHeight / 2) - (rootHeight / 2), 0);

                    CreateNewBlock(newPosition, i);
                }
            }
        }

        void CreateNewBlock(Vector3 newPosition, int order)
        {
            GameObject newPrefab = Instantiate(rootGrassPrefab, newPosition, Quaternion.identity);

            newPrefab.transform.SetParent(blocksHolder.transform);

            newPrefab.layer = LayerMask.NameToLayer("Unclick");

            int randomNum = 2;

            if (Random.value > 0.5)
            {
                randomNum = Random.Range(0, grassSprites.Length);
            }

            newPrefab.GetComponent<SpriteRenderer>().sprite = grassSprites[randomNum];
        }

        void Randomize()
        {
            if (blocksHolder == null)
            {
                blocksHolder = rootBlocks.GetChild(0).gameObject;
            }

            if (grassSprites.Length > 0)
            {
                for (int i = 0; i < blocksHolder.transform.childCount; i++)
                {
                    SpriteRenderer spriteRenderer = blocksHolder.transform.GetChild(i).GetComponent<SpriteRenderer>();

                    int randomNum = 2;

                    if (Random.value > 0.5)
                    {
                        randomNum = Random.Range(0, grassSprites.Length);
                    }

                    spriteRenderer.sprite = grassSprites[randomNum];
                }
            }
            else
            {
                Fill();
            }
        }

        void Clear(bool createNewOne = false)
        {
            if (rootBlocks.transform.childCount > 0)
            {
                DestroyImmediate(rootBlocks.GetChild(0).gameObject);

            }

            if (blocksHolder != null)
            {
                DestroyImmediate(blocksHolder.gameObject);
            }

            blocksHolder = null;

            if (createNewOne)
            {
                blocksHolder = new GameObject("BlocksHolder");

                blocksHolder.transform.SetParent(rootBlocks);
            }
        }
    }
}
