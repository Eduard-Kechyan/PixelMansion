using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UIElements;

namespace Merge
{
    public class PointerHandler : MonoBehaviour
    {
        // Variables
        private Scale fullScale = new(new Vector2(1f, 1f));
        private Scale tapScale = new(new Vector2(0.8f, 0.8f));

        private Types.Button currentButton;
        private bool pointerHidden;

        // Press
        private Coroutine pressCoroutine;
        private bool pressing = false;
        private bool animatePress = false;
        public Action buttonCallback = null;

        // Merge
        private Coroutine mergeCoroutine;
        [HideInInspector]
        public bool merging = false;
        private bool animateMerge = false;
        private Vector2 mergeFirstPos;
        private Vector2 mergeSecondPos;
        [HideInInspector]
        public Sprite mergeSprite;
        private Action mergeCallback = null;

        // Gen
        private Coroutine genCoroutine;
        [HideInInspector]
        public bool generating = false;
        private bool animateGen = false;
        [HideInInspector]
        public Sprite genSprite;
        [HideInInspector]
        public Sprite genItemSprite;
        [HideInInspector]
        public ItemTypes.Group genItemGroup;
        private Action genCallback = null;

        // References
        private GameRefs gameRefs;
        private TaskMenu taskMenu;
        private Camera cam;
        private BoardManager boardManager;
        private TutorialManager tutorialManager;

        // UI
        private VisualElement root;
        private VisualElement pointer;
        private VisualElement pointerBackground;

        void Start()
        {
            // Cache
            gameRefs = GameRefs.Instance;
            taskMenu = gameRefs.taskMenu;
            cam = Camera.main;
            boardManager = gameRefs.boardManager;
            tutorialManager = gameRefs.tutorialManager;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            pointer = root.Q<VisualElement>("Pointer");

            if (gameRefs.worldUIDoc != null)
            {
                pointerBackground = gameRefs.worldUIDoc.rootVisualElement.Q<VisualElement>("PointerBackground");
            }
        }

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

            pressing = true;

            animatePress = true;
            pressCoroutine = StartCoroutine(AnimatePress());
        }

        public void ButtonPress(Types.Button button, bool alt = false, Action callback = null)
        {
            if (currentButton == button && buttonCallback != null)
            {
                tutorialManager.HideButtons();

                StopAllAnimations();

                pointer.style.opacity = 0;
                pointer.style.display = DisplayStyle.None;

                if (pointerBackground != null)
                {
                    pointerBackground.style.display = DisplayStyle.None;
                }

                if (!alt)
                {
                    buttonCallback?.Invoke();
                }

                pressing = false;

                callback?.Invoke();
            }
            else
            {
                callback?.Invoke();
            }
        }

        public void ButtonPressFinish()
        {
            buttonCallback?.Invoke();
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

                merging = false;

                mergeSprite = null;

                mergeFirstPos = Vector2.zero;
                mergeSecondPos = Vector2.zero;

                mergeCallback?.Invoke();
            }
        }

        public void StopMerge()
        {
            if (merging)
            {
                merging = false;

                animateMerge = false;

                StopAllAnimations();
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

        //// Gen ////
        public void HandleGen(Sprite generatorSprite, Sprite itemSprite, ItemTypes.Group itemGroup, Action callback)
        {
            StopAllAnimations();

            genCallback = callback;

            genSprite = generatorSprite;
            genItemSprite = itemSprite;

            genItemGroup = itemGroup;

            ContinueGen();
        }

        public void ContinueGen()
        {
            pointer.style.opacity = 1;
            pointer.style.display = DisplayStyle.Flex;

            Vector2 itemPos = boardManager.UnlockAndGetItemPos(genSprite.name);

            SetGenPos(itemPos, true);

            if (pointerBackground != null)
            {
                pointerBackground.style.display = DisplayStyle.Flex;
            }

            generating = true;

            animateGen = true;
            pressCoroutine = StartCoroutine(AnimateGen());
        }

        public void SetGenPos(Vector2 newPos = default, bool useNewPos = false)
        {
            Vector2 tilePos;

            if (useNewPos)
            {
                tilePos = newPos;
            }
            else
            {
                tilePos = boardManager.GetTileItemPosBySpriteName(genSprite.name);
            }

            Vector2 tileUiPos = RuntimePanelUtils.CameraTransformWorldToPanel(
              root.panel,
              tilePos,
              cam
            );

            pointer.style.left = tileUiPos.x;
            pointer.style.top = tileUiPos.y;
        }

        public void CheckGen(Action callback = null)
        {
            StopAllAnimations();

            pointer.style.opacity = 0;
            pointer.style.display = DisplayStyle.None;
            pointer.RemoveFromClassList("pointer_tap");
            pointer.RemoveFromClassList("pointer_pos_transition");
            pointer.style.scale = new StyleScale(tapScale);

            generating = false;

            genSprite = null;
            genItemSprite = null;

            genCallback?.Invoke();

            callback?.Invoke();
        }

        IEnumerator AnimateGen()
        {
            while (animateGen)
            {
                pointer.RemoveFromClassList("pointer_tap");
                pointer.style.scale = new StyleScale(fullScale);

                yield return new WaitForSeconds(0.4f);

                pointer.AddToClassList("pointer_tap");
                pointer.style.scale = new StyleScale(tapScale);

                yield return new WaitForSeconds(0.6f);
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

            pointerHidden = false;

            if (pressing)
            {
                animatePress = false;
            }

            if (merging)
            {
                animateMerge = false;
            }

            if (generating)
            {
                animateGen = false;
            }

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

            if (genCoroutine != null)
            {
                StopCoroutine(genCoroutine);
                genCoroutine = null;
            }
        }

        public void ShowPointer()
        {
            if (pointerHidden)
            {
                pointerHidden = false;

                if (pressing)
                {
                    animatePress = true;

                    pointer.style.opacity = 1;
                    pointer.style.display = DisplayStyle.Flex;

                    pressCoroutine = StartCoroutine(AnimatePress());
                }

                if (merging)
                {
                    animateMerge = true;
                    mergeCoroutine = StartCoroutine(AnimateMerge(mergeFirstPos, mergeSecondPos));
                }

                if (generating)
                {
                    animateGen = true;

                    pointer.style.opacity = 1;
                    pointer.style.display = DisplayStyle.Flex;

                    SetGenPos();

                    genCoroutine = StartCoroutine(AnimateGen());
                }
            }
        }

        public void HidePointer()
        {
            StopAllAnimations();

            pointerHidden = true;
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
