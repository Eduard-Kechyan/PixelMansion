using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class Selector : MonoBehaviour
    {
        // Variables
        public CameraPan cameraPan;
        public bool checkForOld = true;
        public float tapDuration = 0.2f;
        public float secondTapDuration = 0.1f;
        public float arrowDuration = 1f;
        public Sprite[] arrowSprites;

        [ReadOnly]
        public bool isSelecting = false;
        [ReadOnly]
        public bool isSelected = false;
        [ReadOnly]
        public bool isTapping = false;
        [ReadOnly]
        public bool triedToSelectUnselectable = false;
        private float duration;
        private Selectable selectable;
        private int lastSpriteOrder;

        private Action confirmCallback;
        private Action cancelCallback;

        // References
        private Camera cam;
        private SoundManager soundManager;
        private UIDocument worldGameUiDoc;
        private SelectorUIHandler selectorUIHandler;
        private PopupManager popupManager;
        private I18n LOCALE;
        private WorldDataManager worldDataManager;
        private CharMain charMain;

        // UI
        private VisualElement root;
        private VisualElement selectorArrow;

        void Start()
        {
            // Cache
            cam = Camera.main;
            soundManager = SoundManager.Instance;
            worldGameUiDoc = GameRefs.Instance.worldGameUIDoc;
            selectorUIHandler = worldGameUiDoc.GetComponent<SelectorUIHandler>();
            popupManager = PopupManager.Instance;
            LOCALE = I18n.Instance;
            worldDataManager = GetComponent<WorldDataManager>();
            charMain = CharMain.Instance;

            // Calc duration
            duration = arrowDuration / 6;

            // UI
            root = worldGameUiDoc.rootVisualElement;

            selectorArrow = root.Q<VisualElement>("SelectorArrow");
        }

        void Update()
        {
            if (isTapping && (selectable == null || !selectable.isPlaying))
            {
                isTapping = false;
            }
        }

        public void StartSelecting(Vector2 position, bool tapped = false, bool ignorePopup = false)
        {
            Selectable newSelectable = SelectAndReturn(position);

            if (newSelectable == null)
            {
                return;
            }

            Vector2 worldPosition = cam.ScreenToWorldPoint(position);

            // Check if the selected selectable is the same
            if (!tapped && isSelected && selectable != null && (selectable.id == newSelectable.id || (checkForOld && newSelectable.isOld)))
            {
                return;
            }
            else
            {
                if (checkForOld && selectable != null && selectable.isOld)
                {
                    return;
                }

                // There is already a selectable, so cancel it
                if (isSelected && selectable != null)
                {
                    CancelSelectingAlt();
                }

                selectable = newSelectable;
            }

            lastSpriteOrder = selectable.spriteOrder;

            // Check if we can select the selectable
            if (selectable.canBeSelected)
            {
                if (tapped)
                {
                    SelectableTapped();
                }
                else
                {
                    if (isSelected)
                    {
                        selectable.Select();

                        soundManager.PlaySound(Types.SoundType.Generate);

                        //selectorUIHandler.Open(selectable.GetSpriteOptions(), selectable.spriteOrder, false);
                        selectorUIHandler.Open(selectable.spriteOrder, false);
                    }
                    else
                    {
                        isSelected = false;
                        isSelecting = true;

                        Vector2 newUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
                            root.panel,
                            worldPosition,
                            cam
                        );

                        StartCoroutine(ShowArrow(newUIPos, selectable));
                    }
                }
            }
            else
            {
                if (tapped)
                {
                    SelectableTapped();
                }
                else
                {
                    isSelected = false;
                    isSelecting = false;

                    bool notify = selectable.notifyCantBeSelected;

                    selectable = null;

                    triedToSelectUnselectable = true;

                    if (!ignorePopup && notify)
                    {
                        popupManager.Pop(LOCALE.Get("pop_item_unselectable"), worldPosition, Types.SoundType.None, true, true);
                    }
                }
            }

        }

        void SelectableTapped()
        {
            if (selectable.Tapped())
            {
                soundManager.PlaySound(Types.SoundType.Generate);

                Vector2 newPos = new(selectable.transform.position.x, selectable.transform.position.y - selectable.charMoveOffset);

                charMain.SelectableTapped(newPos, selectable);

                isTapping = true;
            }

            isSelecting = false;
            isSelected = false;
            selectable = null;
        }

        public Selectable SelectAndReturn(Vector2 position)
        {
            List<RaycastHit2D> hits = new();

            ContactFilter2D contactFilter2D = new();

            contactFilter2D.SetLayerMask(LayerMask.GetMask("Selectable"));

            Physics2D.Raycast(
                cam.ScreenToWorldPoint(position),
                Vector2.zero,
                contactFilter2D,
                hits,
                Mathf.Infinity
            );

            hits.Sort(SortColliders);

            if (hits.Count > 0 && (hits[0] || hits[0].collider != null))
            {
                return hits[0].transform.GetComponent<Selectable>();
            }

            return null;
        }

        public void SelectAlt(Selectable newSelectable, Action newConfirmCallback = null, Action newCancelCallback = null)
        {
            confirmCallback = newConfirmCallback;
            cancelCallback = newCancelCallback;

            selectable = newSelectable;

            //selectorUIHandler.Open(selectable.GetSpriteOptions(), selectable.spriteOrder, true, true);
            selectorUIHandler.Open(selectable.spriteOrder, true, true);

            soundManager.PlaySound(Types.SoundType.Pop);

            isSelecting = false;
            isSelected = true;

            selectable.Select();
        }

        public void SelectOption(int option)
        {
            selectable.SetSprites(option);

            soundManager.PlaySound(Types.SoundType.Generate);
        }

        public void CancelSelecting(bool denied = false)
        {
            StopCoroutine("ShowArrow");

            isSelecting = false;
            isSelected = false;

            selectorArrow.style.display = DisplayStyle.None;

            selectorArrow.style.opacity = 0;

            selectable.Select(false);

            if (denied)
            {
                selectable.CancelSpriteChange(lastSpriteOrder);
            }

            selectable = null;

            if (cancelCallback != null)
            {
                cancelCallback();
            }
        }

        void CancelSelectingAlt()
        {
            selectable.Select(false);

            selectable.CancelSpriteChange(lastSpriteOrder);

            selectable = null;

            if (cancelCallback != null)
            {
                cancelCallback();
            }
        }

        public void SelectionConfirmed()
        {
            isSelecting = false;
            isSelected = false;

            selectable.ConfirmSpriteChange();

            worldDataManager.SetSelectable(selectable);

            selectable.Select(false);

            selectable = null;

            if (confirmCallback != null)
            {
                confirmCallback();
            }
        }

        IEnumerator ShowArrow(Vector2 newUIPos, Selectable selectable)
        {
            selectorArrow.style.display = DisplayStyle.Flex;

            selectorArrow.style.opacity = 1;

            selectorArrow.style.backgroundImage = new StyleBackground(arrowSprites[0]);

            selectorArrow.style.top = newUIPos.y - arrowSprites[0].rect.height;
            selectorArrow.style.left = newUIPos.x - (arrowSprites[0].rect.width / 2);

            yield return new WaitForSeconds(duration);

            selectorArrow.style.backgroundImage = new StyleBackground(arrowSprites[1]);

            soundManager.PlaySound(Types.SoundType.Pop);

            yield return new WaitForSeconds(duration);

            selectorArrow.style.backgroundImage = new StyleBackground(arrowSprites[2]);

            soundManager.PlaySound(Types.SoundType.Pop);

            yield return new WaitForSeconds(duration);

            selectorArrow.style.backgroundImage = new StyleBackground(arrowSprites[3]);

            soundManager.PlaySound(Types.SoundType.Pop);

            yield return new WaitForSeconds(duration);

            selectorArrow.style.backgroundImage = new StyleBackground(arrowSprites[4]);

            soundManager.PlaySound(Types.SoundType.Pop);

            yield return new WaitForSeconds(duration);

            selectorArrow.style.backgroundImage = new StyleBackground(arrowSprites[5]);

            soundManager.PlaySound(Types.SoundType.Pop);

            yield return new WaitForSeconds(duration);

            selectorArrow.style.opacity = 0;

            selectorArrow.style.display = DisplayStyle.None;

            //selectorUIHandler.Open(selectable.GetSpriteOptions(), selectable.spriteOrder, true);
            selectorUIHandler.Open(selectable.spriteOrder, true);

            isSelecting = false;
            isSelected = true;

            selectable.Select();

            yield return new WaitForSeconds(duration);
        }

        int SortColliders(RaycastHit2D hit1, RaycastHit2D hit2)
        {
            return hit2.transform.position.z.CompareTo(hit1.transform.position.z);
        }
    }
}