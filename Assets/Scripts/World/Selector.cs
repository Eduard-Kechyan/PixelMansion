using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Selector : MonoBehaviour
{
    // Variables
    public UIDocument hubGameUiDoc;
    public CameraPan cameraPan;
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

    private float duration;
    private Selectable selectable;
    private int lastSpriteOrder;

    // References
    private Camera cam;
    private SoundManager soundManager;
    private SelectorUIHandler selectorUIHandler;
   /* private CharMove charMoveMain;
    private CharSpeech charSpeechMain;*/

    // UI
    private VisualElement root;
    private VisualElement selectorArrow;

    void Start()
    {
        // Cache
        cam = Camera.main;
        soundManager = SoundManager.Instance;
        selectorUIHandler = hubGameUiDoc.GetComponent<SelectorUIHandler>();
      /*  charMoveMain = CharMain.Instance.charMove;
        charSpeechMain = CharMain.Instance.charSpeech;*/

        // Calc duration
        duration = arrowDuration / 6;

        // UI
        root = hubGameUiDoc.rootVisualElement;

        selectorArrow = root.Q<VisualElement>("SelectorArrow");
    }

    void Update()
    {
        if (isTapping && (selectable == null && !selectable.isPlaying))
        {
            isTapping = false;
        }
    }

    public void StartSelecting(Vector2 position, bool tapped = false)
    {
        List<RaycastHit2D> hits = new List<RaycastHit2D>();

        ContactFilter2D contactFilter2D = new ContactFilter2D();

        contactFilter2D.SetLayerMask(LayerMask.GetMask("Selectable"));

        Physics2D.Raycast(
                       cam.ScreenToWorldPoint(position),
                       Vector2.zero,
                       contactFilter2D,
                       hits,
                       Mathf.Infinity
                   );

        hits.Sort(SortBySortingOrder);

        if (hits.Count > 0 && (hits[0] || hits[0].collider != null))
        {
            RaycastHit2D hit = hits[0];

            Selectable newSelectable = hit.transform.GetComponent<Selectable>();

            // Check if the selected selectable is the same
            if (!tapped && isSelected && selectable != null && selectable.id == newSelectable.id)
            {
                return;
            }
            else
            {
                if (selectable != null)
                {
                    selectable.Unselect();
                }

                selectable = newSelectable;
            }

            lastSpriteOrder = selectable.GetSprite();

            // Check if we can select the selectable
            if (selectable.canBeSelected)
            {
                if (tapped)
                {
                    if (selectable.Tapped())
                    {
                        soundManager.PlaySound("", 0f, swapSound);

                       /* charMoveMain.SetDestination(selectable.transform.position);

                        if (!charSpeechMain.isSpeaking && !charSpeechMain.isTimeOut)
                        {
                            SelectableSpeech selectableSpeech = selectable.GetComponent<SelectableSpeech>();

                            charSpeechMain.TryToSpeak(selectableSpeech.GetSpeech(), false);
                        }*/

                        isTapping = true;
                    }
                }
                else
                {
                    if (isSelected)
                    {
                        isSelected = true;

                        selectable.Select();

                        soundManager.PlaySound("", 0f, swapSound);

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
        }
    }

    public void SelectOption(int option)
    {
        selectable.SetSprite(option);

        soundManager.PlaySound("", 0f, swapSound);
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
            selectable.SetSprite(lastSpriteOrder);
        }
    }

    public void SelectionConfirmed()
    {
        isSelecting = false;
        isSelected = false;

        selectable.Unselect();
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

        soundManager.PlaySound("", 0f, arrowSound);

        yield return new WaitForSeconds(duration);

        selectorArrow.style.backgroundImage = new StyleBackground(arrowSprites[2]);

        soundManager.PlaySound("", 0f, arrowSound);

        yield return new WaitForSeconds(duration);

        selectorArrow.style.backgroundImage = new StyleBackground(arrowSprites[3]);

        soundManager.PlaySound("", 0f, arrowSound);

        yield return new WaitForSeconds(duration);

        selectorArrow.style.backgroundImage = new StyleBackground(arrowSprites[4]);

        soundManager.PlaySound("", 0f, arrowSound);

        yield return new WaitForSeconds(duration);

        selectorArrow.style.backgroundImage = new StyleBackground(arrowSprites[5]);

        soundManager.PlaySound("", 0f, arrowSound);

        yield return new WaitForSeconds(duration);

        selectorArrow.style.opacity = 0;

        selectorArrow.style.display = DisplayStyle.None;

        selectorUIHandler.Open(selectable.GetSpriteOptions(), true);

        isSelecting = false;
        isSelected = true;

        selectable.Select();

        if (Settings.Instance.vibrationOn)
        {
            Handheld.Vibrate();
        }

        yield return new WaitForSeconds(duration);
    }

    int SortBySortingOrder(RaycastHit2D hit1, RaycastHit2D hit2)
    {
        float hit1Order = hit1.transform.GetComponent<SpriteRenderer>().sortingOrder;
        float hit2Order = hit2.transform.GetComponent<SpriteRenderer>().sortingOrder;

        return hit2Order.CompareTo(hit1Order);
    }
}
