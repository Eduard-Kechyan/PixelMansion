using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ValuePop : MonoBehaviour
{
    public SafeAreaHandler safeAreaHandler;
    public float offset = 3f;
    public float popWidth = 17f;
    public Sprite experienceSprite;
    public Sprite energySprite;
    public Sprite goldSprite;
    public Sprite gemsSprite;

    private VisualElement root;
    private SoundManager soundManager;

    private GameData gameData;
    private Values values;

    public class Pop
    {
        public float amount;
        public string type;
    }

    void Start()
    {
        gameData = GameData.Instance;

        soundManager = SoundManager.Instance;

        values = DataManager.Instance.GetComponent<Values>();

        // Cache UI
        root = MenuManager.Instance.menuUI.rootVisualElement;
    }

    public void PopValue(int amount, string type)
    {
        StartCoroutine(HandlePopValue(amount, type, Vector2.zero));
    }

    public void PopExperience(int level, string type, Vector2 position)
    {
        StartCoroutine(HandlePopValue(level, type, position, true));
    }

    public void PopBonus(Item item, Vector2 bonusButtonPosition,bool check = true)
    {
        StartCoroutine(HandlePopBonus(item, bonusButtonPosition,check));
    }

    public IEnumerator HandlePopValue(
        int amount,
        string type,
        Vector2 position,
        bool useOffset = false
    )
    {
        Sprite valuePopSprite;
        float valuePopOffset;
        string valuePopSFX;

        // Get the value pop's sprite, offset and SFX name
        switch (type)
        {
            case "Energy":
                valuePopSprite = energySprite;
                valuePopOffset = values.energyButton.layout.x;
                valuePopSFX = "Energy";
                break;
            case "Gold":
                valuePopSprite = goldSprite;
                valuePopOffset = values.goldButton.layout.x;
                valuePopSFX = "Gold";
                break;
            case "Gems":
                valuePopSprite = gemsSprite;
                valuePopOffset = values.gemsButton.layout.x;
                valuePopSFX = "Gems";
                break;
            default: // Experience
                valuePopSprite = experienceSprite;
                valuePopOffset = values.levelButton.layout.x;
                valuePopSFX = "Experience";
                break;
        }

        // Add value pop element to the root
        VisualElement valuePop = InitializePopValueElement(valuePopSprite, position);

        yield return new WaitForSeconds(0.1f);

        // Increase the size of the value pop
        Scale scale = new Scale(new Vector2(1f, 1f));

        valuePop.style.scale = new StyleScale(scale);

        yield return new WaitForSeconds(0.5f);

        // Move the value pop to it's intended position
        valuePop.style.left = Mathf.Ceil(valuePopOffset + (useOffset ? offset : 0));
        valuePop.style.top = safeAreaHandler.topPadding + offset;

        yield return new WaitForSeconds(0.5f);

        // Decrease the size of the value pop
        scale = new Scale(new Vector2(0f, 0f));

        valuePop.style.scale = new StyleScale(scale);

        yield return new WaitForSeconds(0.1f);

        // Play value pop sound
        soundManager.PlaySFX(valuePopSFX, 0.3f);

        // Hide the value pop
        valuePop.style.visibility = Visibility.Hidden;
        valuePop.style.opacity = 0;

        yield return new WaitForSeconds(0.5f);

        // Remove the value pop
        root.Remove(valuePop);

        // Update data
        switch (type)
        {
            case "Energy":
                gameData.UpdateEnergy(amount);
                break;
            case "Gold":
                gameData.UpdateGold(amount);
                break;
            case "Gems":
                gameData.UpdateGems(amount);
                break;
            default: // Experience
                gameData.UpdateExperience(amount,useOffset);
                break;
        }
    }

    public IEnumerator HandlePopBonus(Item item, Vector2 bonusButtonPosition,bool check = true)
    {
        Vector2 initialPosition = new Vector2(
            Mathf.Ceil(values.levelButton.layout.x + offset),
            safeAreaHandler.topPadding + offset
        );

        Sprite valuePopSprite = item.sprite;
        string valuePopSFX = "Experience";

        // Add value pop element to the root
        VisualElement valuePop = InitializePopValueElement(valuePopSprite, initialPosition, true);

        yield return new WaitForSeconds(0.1f);

        // Increase the size of the value pop
        Scale scale = new Scale(new Vector2(1f, 1f));

        valuePop.style.scale = new StyleScale(scale);

        yield return new WaitForSeconds(0.5f);

        // Move the value pop to it's intended position
        Vector2 newUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
            root.panel,
            bonusButtonPosition,
            Camera.main
        );

        valuePop.style.left = newUIPos.x - (28 / 4);
        valuePop.style.top = newUIPos.y - (28 / 4);

        yield return new WaitForSeconds(0.5f);

        // Decrease the size of the value pop
        scale = new Scale(new Vector2(0f, 0f));

        valuePop.style.scale = new StyleScale(scale);

        yield return new WaitForSeconds(0.1f);

        // Play value pop sound
        soundManager.PlaySFX(valuePopSFX, 0.3f);

        // Hide the value pop
        valuePop.style.visibility = Visibility.Hidden;
        valuePop.style.opacity = 0;

        yield return new WaitForSeconds(0.5f);

        // Remove the value pop
        root.Remove(valuePop);

        gameData.AddToBonus(item,check);
    }

    VisualElement InitializePopValueElement(
        Sprite sprite,
        Vector2 position,
        bool ingoreZeroes = false
    )
    {
        VisualElement newValuePop = new VisualElement { name = "ValuePop" };

        // Set the value pop's styles
        Scale scale = new Scale(new Vector2(0f, 0f));

        List<TimeValue> durations = new List<TimeValue>();
        durations.Add(new TimeValue(0.5f, TimeUnit.Second));

        newValuePop.style.width = popWidth;
        newValuePop.style.height = popWidth;
        newValuePop.style.position = Position.Absolute;
        newValuePop.style.marginBottom = 0;
        newValuePop.style.backgroundImage = new StyleBackground(sprite);
        newValuePop.style.transitionDuration = new StyleList<TimeValue>(durations);

        newValuePop.style.scale = new StyleScale(scale);

        float halfWidth = popWidth / 2;

        // Check where we should initialize the pop value
        if (position.x == 0 && position.y == 0 && !ingoreZeroes)
        {
            // Calculate the center of the UI
            float rootHalfWidth = root.resolvedStyle.width / 2;
            float rootHalfHeight = root.resolvedStyle.height / 2;

            // Set the value pop's position
            newValuePop.style.left = rootHalfWidth - halfWidth;
            newValuePop.style.top = rootHalfHeight - halfWidth;
        }
        else
        {
            if (ingoreZeroes)
            {
                // Set the value pop's position
                newValuePop.style.left = position.x;
                newValuePop.style.top = position.y;
            }
            else
            {
                // Get position on the UI from the scene
                Vector2 newUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
                    root.panel,
                    position,
                    Camera.main
                );

                // Set the value pop's position
                newValuePop.style.left = newUIPos.x - halfWidth;
                newValuePop.style.top = newUIPos.y - halfWidth;
            }
        }

        // Add the value pop to the root
        root.Add(newValuePop);

        return newValuePop;
    }
}
