using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace Merge
{
    public class PopupManager : MonoBehaviour
{
    // Varaibles
    public SceneLoader sceneLoader;
    public Color textColor;
    public Color outlineColor;
    public Color shadowColor;
    public float timeOut = 2f;
    public float positionOffset = 1.5f;

    [HideInInspector]
    public bool isSelectorPopup = false;

    public class Pop
    {
        public string text;
        public bool single;
    }

    private List<Pop> currentPops = new List<Pop>();

    // Reference
    private SoundManager soundManager;
    private LocaleManager localeManager;

    // UI
    private VisualElement root;
    private Label popupLable;

    private void Start()
    {
        // Cache references
        soundManager = SoundManager.Instance;
        localeManager = Settings.Instance.GetComponent<LocaleManager>();

        // UI
        if (SceneManager.GetActiveScene().name == "Gameplay")
        {
            root = GameRefs.Instance.gameplayUIDoc.rootVisualElement;
            popupLable = root.Q<Label>("PopupLabel");
        }
        else
        {
            root = GameRefs.Instance.hubGameUIDoc.rootVisualElement;
            popupLable = root.Q<Label>("PopupLabel");
        }
    }

    public void AddPop(string newText, Vector2 position, bool single = true, string soundName = "", bool fromSelector = false)
    {
        if (single)
        {
            if (currentPops.Count > 0)
            {
                for (int i = 0; i < currentPops.Count; i++)
                {
                    if (currentPops[i].single && currentPops[i].text == newText)
                    {
                        return;
                    }
                }
            }

            currentPops.Add(new() { text = newText, single = true });
        }
        else
        {
            currentPops.Add(new() { text = newText, single = false });
        }

        isSelectorPopup = fromSelector;

        if (SceneManager.GetActiveScene().name == "Gameplay")
        {
            StartCoroutine(PopTextToBoard(newText, position, single, soundName));
        }
        else
        {
            StartCoroutine(PopText(newText, position, single, soundName, fromSelector));
        }

    }

    IEnumerator PopTextToBoard(string newText, Vector2 pos, bool single, string sfxName = "")
    {
        Label popLabel = new Label { name = "PopLabel" + newText, text = newText };

        Vector2 newPos = new Vector2(pos.x, pos.y);

        if (pos.x > 1.24f)
        {
            newPos = new Vector2(positionOffset, pos.y);
        }
        else if (pos.x < -1.24f)
        {
            newPos = new Vector2(-positionOffset, pos.y);
        }

        Vector2 newUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
            root.panel,
            newPos,
            Camera.main
        );

        List<TimeValue> durations = new List<TimeValue>();
        List<EasingFunction> easings = new List<EasingFunction>();

        TextShadow textShadow = new TextShadow
        {
            offset = new Vector2(1, 1),
            blurRadius = 1f,
            color = shadowColor
        };

        durations.Add(new TimeValue(timeOut + 0.1f, TimeUnit.Second));
        easings.Add(new EasingFunction(EasingMode.EaseInOut));

        popLabel.style.width = 100;
        popLabel.style.height = 10;
        popLabel.style.position = Position.Absolute;
        popLabel.style.left = newUIPos.x;
        popLabel.style.top = newUIPos.y;
        popLabel.style.visibility = Visibility.Visible;
        popLabel.style.opacity = 0.2f;
        popLabel.style.translate = new Translate(-50f, -20f);
        popLabel.style.transitionDuration = new StyleList<TimeValue>(durations);
        popLabel.style.transitionTimingFunction = new StyleList<EasingFunction>(easings);

        popLabel.style.fontSize = 2;
        popLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        popLabel.style.color = new StyleColor(textColor);
        popLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        popLabel.style.unityTextOutlineWidth = 0.1f;
        popLabel.style.unityTextOutlineColor = new StyleColor(outlineColor);
        popLabel.style.textShadow = new StyleTextShadow(textShadow);

        int grownFontSize = 8;

        switch (Settings.Instance.currentLocale)
        {
            case Types.Locale.Armenian:
                popLabel.style.unityFontDefinition = new StyleFontDefinition(localeManager.hyFont);
                break;
            case Types.Locale.Japanese:
                popLabel.style.unityFontDefinition = new StyleFontDefinition(localeManager.jpFont);
                break;
            case Types.Locale.Korean:
                popLabel.style.unityFontDefinition = new StyleFontDefinition(localeManager.krFont);
                break;
            case Types.Locale.Chinese:
                popLabel.style.unityFontDefinition = new StyleFontDefinition(localeManager.cnFont);
                break;
            default:
                popLabel.style.unityFontDefinition = new StyleFontDefinition(localeManager.enFont);

                if (Settings.Instance.currentLocale != Types.Locale.German)
                {
                    grownFontSize = 10;
                }
                break;
        }

        popLabel.style.marginLeft = 0;
        popLabel.style.marginTop = 0;
        popLabel.style.marginRight = 0;
        popLabel.style.marginBottom = 0;
        popLabel.style.paddingLeft = 0;
        popLabel.style.paddingTop = 0;
        popLabel.style.paddingRight = 0;
        popLabel.style.paddingBottom = 0;

        root.Add(popLabel);

        yield return new WaitForSeconds(0.1f);

        popLabel.style.opacity = 1;
        popLabel.style.fontSize = grownFontSize;

        yield return new WaitForSeconds(timeOut / 2.5f); // 0.4f

        popLabel.style.paddingBottom = 30;

        // Play audio
        if (sfxName != "")
        {
            soundManager.PlaySound(sfxName);
        }

        yield return new WaitForSeconds(timeOut * 2);

        popLabel.style.opacity = 0;

        yield return new WaitForSeconds(timeOut);

        root.Remove(popLabel);

        RemovePop(newText, single);
    }

    IEnumerator PopText(string newText, Vector2 pos, bool single, string soundName = "", bool fromSelector = false)
    {
        Vector2 newPos = new Vector2(pos.x, pos.y);

        int grownFontSize = 8;

        if (newText.Length > 14)
        {
            grownFontSize = 6;
        }

        if (fromSelector)
        {
            newPos = Camera.main.ScreenToWorldPoint(newPos);
        }

        /*if (pos.x > 1.24f)
        {
            newPos = new Vector2(positionOffset, pos.y);
        }
        else if (pos.x < -1.24f)
        {
            newPos = new Vector2(-positionOffset, pos.y);
        }*/

        Vector2 newUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
            root.panel,
            newPos,
            Camera.main
        );

        popupLable.text = newText;
        popupLable.style.visibility = Visibility.Visible;
        popupLable.style.opacity = 0.2f;
        popupLable.style.top = newUIPos.y;
        popupLable.style.left = newUIPos.x;
        popupLable.style.paddingBottom = 0;

        switch (Settings.Instance.currentLocale)
        {
            case Types.Locale.Armenian:
                popupLable.style.unityFontDefinition = new StyleFontDefinition(localeManager.hyFont);
                break;
            case Types.Locale.Japanese:
                popupLable.style.unityFontDefinition = new StyleFontDefinition(localeManager.jpFont);
                break;
            case Types.Locale.Korean:
                popupLable.style.unityFontDefinition = new StyleFontDefinition(localeManager.krFont);
                break;
            case Types.Locale.Chinese:
                popupLable.style.unityFontDefinition = new StyleFontDefinition(localeManager.cnFont);
                break;
            default:
                popupLable.style.unityFontDefinition = new StyleFontDefinition(localeManager.enFont);

                if (Settings.Instance.currentLocale != Types.Locale.German)
                {
                    grownFontSize += 2;
                }
                break;
        }

        yield return new WaitForSeconds(0.1f);

        popupLable.style.fontSize = grownFontSize;
        popupLable.style.opacity = 1f;

        yield return new WaitForSeconds(timeOut / 2.5f); // 0.4f

        popupLable.style.paddingBottom = 30;

        // Play audio
        if (soundName != "")
        {
            soundManager.PlaySound(soundName);
        }

        yield return new WaitForSeconds(timeOut * 2);

        popupLable.style.opacity = 0;

        yield return new WaitForSeconds(timeOut);

        popupLable.style.paddingBottom = 0;

        isSelectorPopup = !fromSelector;

        RemovePop(newText, single);
    }

    void RemovePop(string oldText, bool single)
    {
        int index = 0;

        for (int i = 0; i < currentPops.Count; i++)
        {
            if (currentPops[i].text == oldText && currentPops[i].single == single)
            {
                index = i;
            }
        }

        currentPops.RemoveAt(index);
    }
}
}