using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ValuePop : MonoBehaviour
{
    public SafeAreaHandler safeAreaHandler;
    public float topOffset = 3f;
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

    public void PopValue(float newAmount, string newType)
    {
            StartCoroutine(HandlePopValue(newAmount, newType));
    }

    public IEnumerator HandlePopValue(float amount, string type)
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
        VisualElement valuePop= InitializePopValueElement(valuePopSprite);

        yield return new WaitForSeconds(0.1f);

        // Increase the size of the value pop
        Scale scale = new Scale(new Vector2(1f, 1f));

        valuePop.style.scale = new StyleScale(scale);

        yield return new WaitForSeconds(0.5f);

        // Move the value pop to it's intended position

        valuePop.style.left = Mathf.Ceil(valuePopOffset);
        valuePop.style.top = safeAreaHandler.topPadding + topOffset;

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
                gameData.UpdateEnergy((int)amount);
                break;
            case "Gold":
                gameData.UpdateGold((int)amount);
                break;
            case "Gems":
                gameData.UpdateGems((int)amount);
                break;
            default: // Experience
                gameData.UpdateExperience(amount);
                break;
        }
    }

    VisualElement InitializePopValueElement(Sprite sprite)
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

        // Calculate the center of the UI
        float halfWidth = popWidth / 2;
        float rootHalfWidth = root.resolvedStyle.width / 2;
        float rootHalfHeight = root.resolvedStyle.height / 2;

        // Set the value pop's position
        newValuePop.style.left = rootHalfWidth - halfWidth;
        newValuePop.style.top = rootHalfHeight - halfWidth;

        // Add the value pop to the root
        root.Add(newValuePop);

        return newValuePop;
    }

    void RemovePopFromQuery(Pop oldPop){

    }
}
