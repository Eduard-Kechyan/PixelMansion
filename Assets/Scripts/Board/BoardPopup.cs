using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

public class BoardPopup : MonoBehaviour
{
    // Varaibles
    public Color textColor;
    public Color outlineColor;
    public Color shadowColor;
    public Font textFont;
    public float timeOut = 2f;
    public float positionOffset = 1.5f;

    public class Pop
    {
        public string text;
        public bool single;
    }

    private List<Pop> currentPops = new List<Pop>();

    // Instances

    private SoundManager soundManager;

    // UI
    private VisualElement root;

    private void Start()
    {
        // Cache instances
        soundManager = SoundManager.Instance;

        // UI
        root = GameRefs.Instance.gameplayUIDoc.rootVisualElement;
    }

    public void AddPop(string newText, Vector2 position, bool single = true, string sfxName = "")
    {
        if (single)
        {
            for (int i = 0; i < currentPops.Count; i++)
            {
                if (currentPops[i].single && currentPops[i].text == newText)
                {
                    return;
                }
            }

            currentPops.Add(new Pop { text = newText, single = true });
        }
        else
        {
            currentPops.Add(new Pop { text = newText, single = false });
        }

        StartCoroutine(PopTextToBoard(newText, position, single, sfxName));
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
        popLabel.style.unityFontDefinition = new StyleFontDefinition(textFont);
        popLabel.style.color = new StyleColor(textColor);
        popLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        popLabel.style.unityTextOutlineWidth = 0.1f;
        popLabel.style.unityTextOutlineColor = new StyleColor(outlineColor);
        popLabel.style.textShadow = new StyleTextShadow(textShadow);

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
        popLabel.style.fontSize = 12;

        yield return new WaitForSeconds(timeOut / 2.5f); // 0.4f

        popLabel.style.paddingBottom = 30;

        // Play audio
        if (sfxName != "")
        {
            soundManager.PlaySFX(sfxName, 0.2f);
        }

        yield return new WaitForSeconds(timeOut * 2);

        popLabel.style.opacity = 0;

        yield return new WaitForSeconds(timeOut);

        root.Remove(popLabel);

        RemovePop(newText, single);
    }
}
