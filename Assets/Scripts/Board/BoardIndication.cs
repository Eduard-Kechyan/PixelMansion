using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class BoardIndication : MonoBehaviour
    {
        // Variables
        public float indicateDelay = 2f;
        public float indicateSpeed = 1f;
        public PointerHandler pointerHandler;
        // public float getCloserSpeed = 3f;

        Pair selectedPairs = null;
        //List<Types.Board> lastSingleArray = new List<Types.Board>();

        private bool checkingPossibleMerges = false;
        private bool indicatingPossibleMerges = false;

        private Item item1 = null;
        private Item item2 = null;

        private Coroutine checkCoroutine = null;
        private Coroutine checkCoroutineAlt = null;

        // private Vector3 midPoint = Vector3.zero;

        [Serializable]
        public class Pair
        {
            public Types.Board item1;
            public Types.Board item2;
            public Vector2 pos1;
            public Vector2 pos2;
            public Vector2 desiredPos1;
            public Vector2 desiredPos2;
        }

        // References
        private BoardManager boardManager;
        private BoardInteractions boardInteractions;
        private GameData gameData;
        private DataManager dataManager;

        void Start()
        {
            // References
            boardManager = GetComponent<BoardManager>();
            boardInteractions = GetComponent<BoardInteractions>();
            gameData = GameData.Instance;
            dataManager = DataManager.Instance;
        }

        void Update()
        {
            if (pointerHandler != null && pointerHandler.mergeSprite != null && !pointerHandler.merging)
            {
                StopPossibleMergeCheck();

                if (boardInteractions.interactionsEnabled && !boardInteractions.isDragging && dataManager.loaded)
                {
                    StartAltMergeCheck();
                }
                else
                {
                    StopAltMergeCheck();
                }
            }
            else
            {
                StopAltMergeCheck();

                if (boardInteractions.interactionsEnabled && !boardInteractions.isDragging && dataManager.loaded)
                {
                    StartPossibleMergeCheck();
                }
                else
                {
                    StopPossibleMergeCheck();
                }
            }
        }

        void FixedUpdate()
        {
            if (selectedPairs != null)
            {
                if (indicatingPossibleMerges)
                {
                    if (item1 != null && !item1.isPlaying)
                    {
                        item1.Animate(indicateSpeed, true);
                    }

                    if (item2 != null && !item2.isPlaying)
                    {
                        item2.Animate(indicateSpeed, true);
                    }
                }
                else
                {
                    if (item1 != null)
                    {
                        item1.StopAnimate(true);
                    }

                    if (item2 != null)
                    {
                        item2.StopAnimate(true);
                    }
                }
            }
        }

        public void StartPossibleMergeCheck()
        {
            if (checkCoroutine == null)
            {
                checkCoroutine = StartCoroutine(CheckPossibleMerges());
            }
        }

        public void StartAltMergeCheck()
        {
            if (checkCoroutineAlt == null)
            {
                checkCoroutineAlt = StartCoroutine(CheckAltMerge());
            }

            if (checkingPossibleMerges)
            {
                checkingPossibleMerges = false;
            }
        }

        public void StopPossibleMergeCheck()
        {
            if (selectedPairs != null)
            {
                if (item1 != null)
                {
                    item1.StopAnimate(true);
                }

                if (item2 != null)
                {
                    item2.StopAnimate(true);
                }

                selectedPairs = null;
            }

            if (indicatingPossibleMerges)
            {
                indicatingPossibleMerges = false;
            }

            if (checkingPossibleMerges)
            {
                checkingPossibleMerges = false;

                if (checkCoroutine != null)
                {
                    StopCoroutine(checkCoroutine);
                    checkCoroutine = null;
                }
            }

            // StopCoroutine(GetCloser());

            // ResetDistance();
        }

        public void StopAltMergeCheck()
        {
            if (checkCoroutineAlt != null)
            {
                StopCoroutine(checkCoroutineAlt);
                checkCoroutineAlt = null;
            }
        }

        public void CheckIfShouldStop(Item currentItem)
        {
            if (currentItem != null)
            {
                if ((item1.id != null && currentItem.id == item1.id) || (item2.id != null && currentItem.id == item2.id))
                {
                    StopPossibleMergeCheck();
                }
            }
        }

        IEnumerator CheckPossibleMerges()
        {
            indicatingPossibleMerges = false;
            checkingPossibleMerges = true;

            yield return new WaitForSeconds(UnityEngine.Random.Range(2, 5));

            // Variables
            List<Types.Board> singleArray = new();

            List<Pair> pairs = new();

            // Get all items
            for (int i = 0; i < GameData.WIDTH; i++)
            {
                for (int j = 0; j < GameData.HEIGHT; j++)
                {
                    Types.Board singleItem = gameData.boardData[i, j];

                    if (singleItem.state == Types.State.Default && singleItem.sprite != null)
                    {
                        if (!boardManager.boardTiles.transform.GetChild(singleItem.order).GetChild(0).gameObject.GetComponent<Item>().isMaxLevel)
                        {
                            singleArray.Add(singleItem);
                        }
                    }
                }
            }

            // Check if the items are the same
            /* if (singleArray.Count == lastSingleArray.Count)
             {
                 bool different = false;

                 for (int i = 0; i < singleArray.Count; i++)
                 {
                     if (
                         singleArray[i].sprite.name != lastSingleArray[i].sprite.name
                         || singleArray[i].state != lastSingleArray[i].state
                         || singleArray[i].crate != lastSingleArray[i].crate
                         || singleArray[i].order != lastSingleArray[i].order
                     )
                     {
                         different = true;
                     }
                 }

                 if (different)
                 {
                     lastSingleArray = singleArray;
                 }
                 else
                 {
                     yield break;
                 }
             }
             else
             {
                 lastSingleArray = singleArray;
             }*/

            // Check if we should continue
            if (singleArray.Count == 0)
            {
                yield break;
            }

            // Sort the items
            singleArray.Sort((a, b) => a.sprite.name.CompareTo(b.sprite.name));

            // Remove last item if it's odd
            if (singleArray.Count % 2 == 1)
            {
                singleArray.RemoveAt(singleArray.Count - 1);
            }

            // Get the duplicates
            for (int i = 0; i < singleArray.Count; i += 2)
            {
                if (singleArray[i].sprite == singleArray[i + 1].sprite)
                {
                    pairs.Add(new Pair
                    {
                        item1 = singleArray[i],
                        item2 = singleArray[i + 1]
                    });
                }
            }

            // Check if we should continue
            if (pairs.Count == 0)
            {
                yield break;
            }

            // Sort the pairs by priority, by collectables first, then by the highest level
            pairs.Sort(SortPairs);

            // Select a random pair
            selectedPairs = pairs[UnityEngine.Random.Range(0, pairs.Count)];

            // Select the items
            item1 = boardManager.boardTiles.transform.GetChild(selectedPairs.item1.order).GetChild(0).gameObject.GetComponent<Item>();
            item2 = boardManager.boardTiles.transform.GetChild(selectedPairs.item2.order).GetChild(0).gameObject.GetComponent<Item>();

            // Set the initial positions
            // selectedPairs.pos1 = item1.transform.position;
            // selectedPairs.pos2 = item2.transform.position;

            // midPoint = Vector2.Lerp(selectedPairs.pos1, selectedPairs.pos2, 0.5f);

            indicatingPossibleMerges = true;

            // StartCoroutine(GetCloser());
        }

        IEnumerator CheckAltMerge()
        {
            checkingPossibleMerges = true;

            yield return new WaitForSeconds(0.3f);

            // Variables
            List<Types.Board> singleArray = new();

            List<Pair> pairs = new();

            // Get all items
            for (int i = 0; i < GameData.WIDTH; i++)
            {
                for (int j = 0; j < GameData.HEIGHT; j++)
                {
                    Types.Board singleItem = gameData.boardData[i, j];

                    if ((singleItem.state == Types.State.Default || singleItem.state == Types.State.Locker) && singleItem.sprite != null)
                    {
                        if (boardManager.boardTiles.transform.GetChild(singleItem.order).childCount > 0 && !boardManager.boardTiles.transform.GetChild(singleItem.order).GetChild(0).gameObject.GetComponent<Item>().isMaxLevel)
                        {
                            if (singleItem.sprite.name.Contains(pointerHandler.mergeSprite.name))
                            {
                                singleArray.Add(singleItem);
                            }
                        }
                    }
                }
            }

            // Check if we should continue
            if (singleArray.Count == 0)
            {
                yield break;
            }

            // Sort the items
            singleArray.Sort((a, b) => a.sprite.name.Substring(a.sprite.name.Length - 1).CompareTo(b.sprite.name.Substring(b.sprite.name.Length - 1)));
            singleArray.Sort((a, b) => a.state.CompareTo(b.state));

            // Remove last item if it's odd
            if (singleArray.Count % 2 == 1)
            {
                singleArray.RemoveAt(singleArray.Count - 1);
            }

            // Select the items
            Item itemAlt1 = boardManager.boardTiles.transform.GetChild(singleArray[0].order).GetChild(0).gameObject.GetComponent<Item>();
            Item itemAlt2 = boardManager.boardTiles.transform.GetChild(singleArray[0 + 1].order).GetChild(0).gameObject.GetComponent<Item>();

            pointerHandler.IndicateMerge(itemAlt1.transform.position, itemAlt2.transform.position);

            checkCoroutineAlt = null;
        }

        int SortPairs(Pair a, Pair b)
        {
            return a.item1.sprite.name[a.item1.sprite.name.Length - 1].CompareTo(b.item1.sprite.name[b.item1.sprite.name.Length - 1]);
        }

        /*
            IEnumerator GetCloser()
            {
                bool gettingClosers = true;

                float elapsedTime = 0;
                float waitTime = getCloserSpeed;

                while (indicatingPossibleMerges)
                {
                    while (elapsedTime < waitTime)
                    {

                        if (gettingClosers)
                        {
                            if (item1 != null && item2 != null)
                            {
                                item1.transform.position = Vector2.MoveTowards(item1.transform.position, midPoint, (elapsedTime / waitTime));
                                item2.transform.position = Vector2.MoveTowards(item2.transform.position, midPoint, (elapsedTime / waitTime));
                                elapsedTime += Time.deltaTime;
                            }

                            if (item1.transform.position.x == midPoint.x && item1.transform.position.y == midPoint.y)
                            {
                                gettingClosers = false;
                                elapsedTime = 0;
                            }
                        }
                        else
                        {
                            if (item1 != null && item2 != null)
                            {
                                item1.transform.position = Vector2.MoveTowards(item1.transform.position, selectedPairs.pos1, (elapsedTime / waitTime));
                                item2.transform.position = Vector2.MoveTowards(item2.transform.position, selectedPairs.pos2, (elapsedTime / waitTime));
                                elapsedTime += Time.deltaTime;
                            }

                            if (item1.transform.position.x == selectedPairs.pos1.x && item1.transform.position.y == selectedPairs.pos1.y)
                            {
                                gettingClosers = true;
                                elapsedTime = 0;
                            }
                        }

                        yield return null;
                    }

                    yield return null;
                }

                yield return null;
            }

            void ResetDistance()
            {
                if (item1 != null)
                {
                    item1.transform.position = selectedPairs.pos1;
                }
                if (item2 != null)
                {
                    item2.transform.position = selectedPairs.pos2;
                }
            }*/
    }
}