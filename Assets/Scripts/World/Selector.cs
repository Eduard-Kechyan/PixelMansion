using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class Selector : MonoBehaviour
    {
        // Variables
        public UIDocument hubGameUiDoc;
        public CameraPan cameraPan;
        public bool checkForOld = true;
        public float tapDuration = 0.2f;
        public float secondTapDuration = 0.1f;
        public float arrowDuration = 1f;
        public Sprite[] arrowSprites;
        public AudioClip arrowSound;
        public AudioClip swapSound;

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

        // References
        private Camera cam;
        private SoundManager soundManager;
        private SelectorUIHandler selectorUIHandler;
        private PopupManager popupManager;
        private I18n LOCALE;
        private WorldDataManager WorldDataManager;
        private CharMain charMain;

        // UI
        private VisualElement root;
        private VisualElement selectorArrow;

        void Start()
        {
            // Cache
            cam = Camera.main;
            soundManager = SoundManager.Instance;
            selectorUIHandler = hubGameUiDoc.GetComponent<SelectorUIHandler>();
            popupManager = GameRefs.Instance.popupManager;
            LOCALE = I18n.Instance;
            WorldDataManager = GetComponent<WorldDataManager>();
            charMain = CharMain.Instance;

            // Calc duration
            duration = arrowDuration / 6;

            // UI
            root = hubGameUiDoc.rootVisualElement;

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
            List<RaycastHit2D> hits = new();

            ContactFilter2D contactFilter2D = new();

            contactFilter2D.SetLayerMask(LayerMask.GetMask("RoomLocked"));

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
                if (!tapped && !ignorePopup)
                {
                    LockedRoomSelecting(position);
                }
            }
            else
            {
                DefaultSelecting(position, tapped, ignorePopup);
            }
        }

        void LockedRoomSelecting(Vector2 position)
        {
            popupManager.AddPop(LOCALE.Get("pop_room_locked"), position, true, "", true);
        }

        void DefaultSelecting(Vector2 position, bool tapped = false, bool ignorePopup = false)
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
                RaycastHit2D hit = hits[0];

                Selectable newSelectable = hit.transform.GetComponent<Selectable>();

                // Check if the selected selectable is the same
                if (!tapped && isSelected && selectable != null && (selectable.id == newSelectable.id || (checkForOld && newSelectable.GetOld())))
                {
                    return;
                }
                else
                {
                    if (checkForOld && selectable != null && selectable.GetOld())
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

                lastSpriteOrder = selectable.GetSprites();

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

                            soundManager.PlaySound("", swapSound);

                            selectorUIHandler.Open(selectable.GetSpriteOptions(), false);
                        }
                        else
                        {
                            isSelected = false;
                            isSelecting = true;

                            Vector2 newUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
                                root.panel,
                                cam.ScreenToWorldPoint(position),
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

                        selectable = null;

                        triedToSelectUnselectable = true;

                        if (!ignorePopup)
                        {
                            popupManager.AddPop(LOCALE.Get("pop_item_unselectable"), position, true, "", true);
                        }
                    }
                }
            }

        }

        void SelectableTapped()
        {
            if (selectable.Tapped())
            {
                soundManager.PlaySound("", swapSound);

                Vector2 newPos = new(selectable.transform.position.x, selectable.transform.position.y - selectable.charMoveOffset);

                charMain.SelectableTapped(newPos, selectable);

                isTapping = true;
            }

            isSelecting = false;
            isSelected = false;
            selectable = null;
        }

        public void SelectOption(int option)
        {
            selectable.SetSprites(option);

            soundManager.PlaySound("", swapSound);
        }

        public void CancelSelecting(bool denied = false)
        {
            StopCoroutine("ShowArrow");

            isSelecting = false;
            isSelected = false;

            selectorArrow.style.display = DisplayStyle.None;

            selectorArrow.style.opacity = 0;

            selectable.Unselect();

            if (denied)
            {
                selectable.CancelSpriteChange(lastSpriteOrder);
            }

            selectable = null;
        }

        void CancelSelectingAlt()
        {
            selectable.Unselect();

            selectable.CancelSpriteChange(lastSpriteOrder);

            selectable = null;
        }

        public void SelectionConfirmed()
        {
            isSelecting = false;
            isSelected = false;

            selectable.ConfirmSpriteChange(lastSpriteOrder);

            WorldDataManager.SetSelectable(selectable);

            selectable.Unselect();

            selectable = null;
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

            soundManager.PlaySound("", arrowSound);

            yield return new WaitForSeconds(duration);

            selectorArrow.style.backgroundImage = new StyleBackground(arrowSprites[2]);

            soundManager.PlaySound("", arrowSound);

            yield return new WaitForSeconds(duration);

            selectorArrow.style.backgroundImage = new StyleBackground(arrowSprites[3]);

            soundManager.PlaySound("", arrowSound);

            yield return new WaitForSeconds(duration);

            selectorArrow.style.backgroundImage = new StyleBackground(arrowSprites[4]);

            soundManager.PlaySound("", arrowSound);

            yield return new WaitForSeconds(duration);

            selectorArrow.style.backgroundImage = new StyleBackground(arrowSprites[5]);

            soundManager.PlaySound("", arrowSound);

            yield return new WaitForSeconds(duration);

            selectorArrow.style.opacity = 0;

            selectorArrow.style.display = DisplayStyle.None;

            selectorUIHandler.Open(selectable.GetSpriteOptions(), true);

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