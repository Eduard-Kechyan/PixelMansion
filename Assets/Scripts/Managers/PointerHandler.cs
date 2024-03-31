using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class PointerHandler : MonoBehaviour
    {
        // Variables
        public BoardInteractions boardInteractions;

        private Scale fullScale = new(new Vector2(1f, 1f));
        private Scale tapScale = new(new Vector2(0.8f, 0.8f));

        public Action buttonCallback = null;
        private Action mergeCallback = null;

        private Types.Button currentButton;

        private Coroutine pressCoroutine;
        // private bool pressing = false;
        private bool animatePress = false;

        private Coroutine mergeCoroutine;
        [HideInInspector]
        public bool merging = false;
        private bool animateMerge = false;
        private Vector2 mergeFirstPos;
        private Vector2 mergeSecondPos;
        [HideInInspector]
        public Sprite mergeSprite;

        [HideInInspector]
        public bool waitForBoardIndication = false;

        // References
        private TaskMenu taskMenu;

        // UI
        private VisualElement root;
        private VisualElement pointer;
        private VisualElement pointerBackground;

        void Start()
        {
            // Cache 
            taskMenu = GameRefs.Instance.taskMenu;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            pointer = root.Q<VisualElement>("Pointer");

            if (GameRefs.Instance.worldUIDoc != null)
            {
                pointerBackground = GameRefs.Instance.worldUIDoc.rootVisualElement.Q<VisualElement>("PointerBackground");
            }
        }

        /* void Update()
        {if (boardInteractions.interactionsEnabled && !boardInteractions.isDragging)
            {
                pointer.style.visibility = Visibility.Visible;
            }
            else
            {
                pointer.style.visibility = Visibility.Hidden;
            }
        }*/

        //// Press ////
        public void HandlePress(Vector2 position, Types.Button button, Action callback)
        {
            StopAllAnimations();

            buttonCallback = callback;

            currentButton = button;

            if (button == Types.Button.TaskMenu)
            {
                StartCoroutine(WaitForTaskMenu());
            }
            else
            {
                ContinuePress(GetUIPos(position));
            }
        }

        IEnumerator WaitForTaskMenu()
        {
            while (taskMenu.loadingTaskMenuButton)
            {
                yield return null;
            }

            ContinuePress(taskMenu.tempTaskButtonPos);
        }

        void ContinuePress(Vector2 uiPos)
        {
            pointer.style.opacity = 1;
            pointer.style.display = DisplayStyle.Flex;

            pointer.style.left = uiPos.x;
            pointer.style.top = uiPos.y;

            if (pointerBackground != null)
            {
                pointerBackground.style.display = DisplayStyle.Flex;
            }

            // pressing = true;

            animatePress = true;
            pressCoroutine = StartCoroutine(AnimatePress());
        }

        public void ButtonPress(Types.Button button, bool alt = false, Action callback = null)
        {
            if (currentButton == button && buttonCallback != null)
            {
                StopAllAnimations();

                pointer.style.opacity = 0;
                pointer.style.display = DisplayStyle.None;

                if (pointerBackground != null)
                {
                    pointerBackground.style.display = DisplayStyle.None;
                }

                if (!alt)
                {
                    buttonCallback();
                }

                //  pressing = false;

                callback?.Invoke();
            }
            else
            {
                callback?.Invoke();
            }

            buttonCallback = null;
        }

        public void ButtonPressFinish()
        {
            buttonCallback();
        }

        IEnumerator AnimatePress()
        {
            while (animatePress)
            {
                pointer.RemoveFromClassList("pointer_tap");
                pointer.style.scale = new StyleScale(fullScale);

                yield return new WaitForSeconds(0.4f);

                pointer.AddToClassList("pointer_tap");
                pointer.style.scale = new StyleScale(tapScale);

                yield return new WaitForSeconds(0.6f);
            }
        }

        //// Merge ////
        public void HandleMerge(Sprite itemSprite, Action callback)
        {
            StopAllAnimations();

            mergeCallback = callback;

            waitForBoardIndication = true;
            mergeSprite = itemSprite;
        }

        public void IndicateMerge(Vector2 firstPos, Vector2 secondPos)
        {
            pointer.style.display = DisplayStyle.Flex;
            pointer.style.opacity = 1;

            mergeFirstPos = firstPos;
            mergeSecondPos = secondPos;

            merging = true;

            animateMerge = true;
            mergeCoroutine = StartCoroutine(AnimateMerge(firstPos, secondPos));
        }

        public void CheckMerge(Sprite itemSprite)
        {
            if (itemSprite == mergeSprite)
            {
                StopAllAnimations();

                pointer.style.opacity = 0;
                pointer.style.display = DisplayStyle.None;
                pointer.RemoveFromClassList("pointer_tap");
                pointer.RemoveFromClassList("pointer_pos_transition");
                pointer.style.scale = new StyleScale(tapScale);

                waitForBoardIndication = false;

                merging = false;

                mergeSprite = null;

                mergeFirstPos = Vector2.zero;
                mergeSecondPos = Vector2.zero;

                mergeCallback?.Invoke();
            }
        }

        IEnumerator AnimateMerge(Vector2 firstPos, Vector2 secondPos)
        {
            Vector2 firstUIPos = GetUIPos(firstPos);
            Vector2 secondsUIPos = GetUIPos(secondPos);

            while (animateMerge)
            {
                pointer.RemoveFromClassList("pointer_pos_transition");
                pointer.style.opacity = 1;
                pointer.style.left = firstUIPos.x;
                pointer.style.top = firstUIPos.y;

                yield return new WaitForSeconds(0.4f);

                pointer.AddToClassList("pointer_tap");
                pointer.style.scale = new StyleScale(tapScale);

                yield return new WaitForSeconds(0.6f);

                pointer.AddToClassList("pointer_pos_transition");

                yield return new WaitForSeconds(0.1f);

                pointer.style.left = secondsUIPos.x;
                pointer.style.top = secondsUIPos.y;

                yield return new WaitForSeconds(0.6f);

                pointer.RemoveFromClassList("pointer_tap");
                pointer.style.scale = new StyleScale(fullScale);

                yield return new WaitForSeconds(0.4f);

                pointer.style.opacity = 0;

                yield return new WaitForSeconds(0.3f);
            }
        }

        //// Other ////
        void StopAllAnimations()
        {
            pointer.RemoveFromClassList("pointer_tap");
            pointer.RemoveFromClassList("pointer_pos_transition");
            pointer.style.scale = new StyleScale(fullScale);
            pointer.style.opacity = 0;
            pointer.style.display = DisplayStyle.None;

            animatePress = false;
            animateMerge = false;

            if (pressCoroutine != null)
            {
                StopCoroutine(pressCoroutine);
                pressCoroutine = null;
            }

            if (mergeCoroutine != null)
            {
                StopCoroutine(mergeCoroutine);
                mergeCoroutine = null;
            }
        }

        public void ShowPointer()
        {
            if (merging)
            {
                animateMerge = true;
                mergeCoroutine = StartCoroutine(AnimateMerge(mergeFirstPos, mergeSecondPos));
            }
        }

        public void HidePointer()
        {
            StopAllAnimations();
        }

        Vector2 GetUIPos(Vector2 position)
        {
            return RuntimePanelUtils.CameraTransformWorldToPanel(
                root.panel,
                position,
                Camera.main
            );
        }
    }
}
