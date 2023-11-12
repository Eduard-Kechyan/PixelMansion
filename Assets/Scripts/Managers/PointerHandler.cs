using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class PointerHandler : MonoBehaviour
    {
        private Scale fullScale = new(new Vector2(1f, 1f));
        private Scale tapScale = new(new Vector2(0.8f, 0.8f));

        private Action buttonCallback = null;
        private Action mergeCallback = null;

        private string currentButtonName = "";

        [HideInInspector]
        public bool waitForBoardIndication = false;
        public string boardIndicationItemName = "";

        // UI
        private VisualElement root;
        private VisualElement pointer;
        private VisualElement pointerBackground;

        void OnEnable()
        {

        }

        void OnDisable()
        {

        }

        void Start()
        {
            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            pointer = root.Q<VisualElement>("Pointer");

            if (GameRefs.Instance.hubUIDoc != null)
            {
                pointerBackground = GameRefs.Instance.hubUIDoc.rootVisualElement.Q<VisualElement>("PointerBackground");
            }
        }

        //// Press ////
        public void HandlePress(Vector2 position, string buttonName, Action callback)
        {
            StopAllAnimations();

            buttonCallback = callback;

            currentButtonName = buttonName;

            Vector2 uiPos = GetUIPos(position);

            pointer.style.opacity = 1;
            pointer.style.display = DisplayStyle.Flex;

            pointer.style.left = uiPos.x;
            pointer.style.top = uiPos.y;

            if (pointerBackground != null)
            {
                pointerBackground.style.display = DisplayStyle.Flex;
            }

            StartCoroutine(AnimatePress());
        }

        IEnumerator AnimatePress()
        {
            while (true)
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
        public void HandleMerge(string itemName, Action callback)
        {
            StopAllAnimations();

            mergeCallback = callback;

            waitForBoardIndication = true;
            boardIndicationItemName = itemName;
        }

        public void IndicateMerge(Vector2 firstPos, Vector2 secondPos)
        {
            pointer.style.display = DisplayStyle.Flex;

            Debug.Log(firstPos);

            StartCoroutine(AnimateMerge(firstPos, secondPos));
        }

        public void Merged()
        {
            StopAllAnimations();

            pointer.style.opacity = 0;
            pointer.style.display = DisplayStyle.None;
            pointer.RemoveFromClassList("pointer_tap");
            pointer.RemoveFromClassList("pointer_pos_transition");
            pointer.style.scale = new StyleScale(tapScale);
        }

        IEnumerator AnimateMerge(Vector2 firstPos, Vector2 secondPos)
        {
            Vector2 firstUIPos = GetUIPos(firstPos);
            Vector2 secondsUIPos = GetUIPos(secondPos);

            while (true)
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
                pointer.style.left = secondsUIPos.x;
                pointer.style.top = secondsUIPos.y;

                yield return new WaitForSeconds(0.4f);

                pointer.RemoveFromClassList("pointer_tap");
                pointer.style.scale = new StyleScale(fullScale);

                yield return new WaitForSeconds(0.3f);

                pointer.style.opacity = 0;

                yield return new WaitForSeconds(0.3f);
            }
        }

        //// Other ////
        public void ButtonPress(string buttonName, Action callback = null)
        {
            if (currentButtonName != "" && currentButtonName == buttonName && buttonCallback != null)
            {
                pointer.style.opacity = 0;
                pointer.style.display = DisplayStyle.None;

                if (pointerBackground != null)
                {
                    pointerBackground.style.display = DisplayStyle.None;
                }

                currentButtonName = "";

                buttonCallback();
                buttonCallback = null;

                callback?.Invoke();
            }
            else
            {
                callback?.Invoke();
            }
        }

        public void ShowPointer()
        {
            pointer.style.visibility = Visibility.Visible;
        }

        public void HidePointer()
        {
            pointer.style.visibility = Visibility.Hidden;
        }

        void StopAllAnimations()
        {
            pointer.RemoveFromClassList("pointer_tap");
            pointer.RemoveFromClassList("pointer_pos_transition");
            pointer.style.scale = new StyleScale(fullScale);

            StopCoroutine(AnimatePress());
            StopCoroutine(AnimateMerge(Vector2.zero, Vector2.zero));
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
