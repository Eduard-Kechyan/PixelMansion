using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// "Uzum em poshinery haytnven kisatapancik heto anhetanan." by Lusine Harutyunyan (knik)

namespace Merge
{
    public class RemoveFilth : MonoBehaviour
    {
        // Variables
        public Transform dustCloudRoot;
        public Transform[] dustCloudPrefabs = new Transform[2];

        public int dustCloudCount = 10;
        public int dustCloudOffsetX = 10;
        public int dustCloudOffsetY = 10;
        public Color dustCloudColorHalf;
        public Color dustCloudColorNull;
        public float dustCloudSize = 1f;
        public float dustCloudScaleSpeed = 8f;
        public float finishDelay = 0.2f;
        public float scaleSpeed = 8f;
        public float targetScale = 0f;

        [Header("Debug")]
        public bool debug = false;
        [Condition("debug", true)]
        public bool remove = false;
        [Condition("debug", true)]
        public Transform dummyFilth;

        private bool filthRemoved = false;

        private Transform refFilth = null;
        private Action callback = null;

        // References
        private SoundManager soundManager;

        void Start()
        {
            // Cache
            soundManager = SoundManager.Instance;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (remove)
            {
                remove = false;

                if (dummyFilth == null)
                {
                    Debug.LogWarning("DummyFilth is null!");
                }
                else
                {
                    Remove(dummyFilth);
                }
            }
        }
#endif

        public void Remove(Transform newRefFilth, Action newCallback = null)
        {
            Glob.taskLoading = true;

            refFilth = newRefFilth;

            callback = newCallback;

            StartCoroutine(WaitForFilth());

            int filthCount = refFilth.childCount;

            soundManager.PlaySound(Types.SoundType.Generate); // TODO - Change this to Types.SoundType.RemoveFilth and add the proper sound 

            // Remove mansion parts
            for (int i = 0; i < filthCount; i++)
            {
                // Determine if this is the last mansion part
                bool last = i == filthCount - 1;

                StartCoroutine(RemoveFilthPart(refFilth.GetChild(i), last));
            }
        }

        // Make the filth part disappear
        IEnumerator RemoveFilthPart(Transform part, bool last = false)
        {
            StartCoroutine(MakeDustCloud(part.position));

            // Add a random delay before starting the removal
            float randomDelay = UnityEngine.Random.Range(0.4f, 1.4f);

            if (finishDelay < randomDelay)
            {
                finishDelay = randomDelay + 0.1f;
            }

            yield return new WaitForSeconds(randomDelay);

            // Decrease the part scale to 0
            Vector3 newTargetScale = new Vector3(targetScale, targetScale, targetScale);

            while (part.localScale != newTargetScale)
            {
                part.localScale = Vector3.MoveTowards(
                    part.localScale,
                    newTargetScale,
                    scaleSpeed * Time.deltaTime
                );

                yield return null;
            }

            // Check if this is the last part
            if (last)
            {
                yield return new WaitForSeconds(finishDelay);

                filthRemoved = true;
            }
        }

        IEnumerator MakeDustCloud(Vector2 pos)
        {
            SpriteRenderer newDustCloudSpriteRenderer;
            Vector3 newTargetScale = new Vector3(dustCloudSize, dustCloudSize, dustCloudSize);

            // Set up random values
            float randomDelay = UnityEngine.Random.Range(0f, 0.4f);
            int randomDustCloud = UnityEngine.Random.Range(0, dustCloudPrefabs.Length);
            int randomOffsetX = UnityEngine.Random.Range(-dustCloudOffsetX, dustCloudOffsetX);
            int randomOffsetY = UnityEngine.Random.Range(-dustCloudOffsetY, dustCloudOffsetY);

            Vector3 newOffsetTransform = new Vector3(
                pos.x + randomOffsetX,
                pos.y + randomOffsetY,
                0
            );

            yield return new WaitForSeconds(randomDelay);

            // Instantiate dust cloud
            Transform newDustCloud = Instantiate(dustCloudPrefabs[randomDustCloud], newOffsetTransform, Quaternion.identity);

            newDustCloud.SetParent(dustCloudRoot);

            newDustCloudSpriteRenderer = newDustCloud.GetComponent<SpriteRenderer>();

            // Increase the part scale to 1
            while (newDustCloudSpriteRenderer.color != dustCloudColorHalf)
            {
                float timeChange = dustCloudScaleSpeed * Time.deltaTime;

                newDustCloud.localScale = Vector3.MoveTowards(
                    newDustCloud.localScale,
                    newTargetScale,
                    timeChange
                );

                Color currentColor = newDustCloudSpriteRenderer.color;

                currentColor.a = Mathf.MoveTowards(currentColor.a, dustCloudColorHalf.a, timeChange);

                newDustCloudSpriteRenderer.color = currentColor;

                yield return null;
            }

            randomDelay = UnityEngine.Random.Range(0.5f, 1f);

            yield return new WaitForSeconds(randomDelay);

            newTargetScale = new Vector3(0, 0, 0);

            // Decrease the part scale to 0
            while (newDustCloudSpriteRenderer.color != dustCloudColorNull)
            {
                float timeChange = dustCloudScaleSpeed * Time.deltaTime;

                newDustCloud.localScale = Vector3.MoveTowards(
                    newDustCloud.localScale,
                    newTargetScale,
                    timeChange
                );

                Color currentColor = newDustCloudSpriteRenderer.color;

                currentColor.a = Mathf.MoveTowards(currentColor.a, dustCloudColorNull.a, timeChange);

                newDustCloudSpriteRenderer.color = currentColor;

                yield return null;
            }

            Destroy(newDustCloud.gameObject);
        }

        // Make dust clouds appear
        IEnumerator WaitForFilth()
        {
            while (!filthRemoved)
            {
                yield return null;
            }

            // Finished
            RemovingFinished();
        }

        // Removing has been finished
        void RemovingFinished()
        {
            refFilth.gameObject.SetActive(false);

            filthRemoved = false;

            Glob.taskLoading = false;

            callback?.Invoke();

            callback = null;
        }
    }
}
